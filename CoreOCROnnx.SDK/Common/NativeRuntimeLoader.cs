using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace CoreOCROnnx.SDK
{
    internal static class NativeRuntimeLoader
    {
        private static readonly object SyncRoot = new object();
        private static string _nativeDirectory;

        internal static string EnsureLoaded()
        {
            if (_nativeDirectory != null)
            {
                return _nativeDirectory;
            }

            lock (SyncRoot)
            {
                if (_nativeDirectory != null)
                {
                    return _nativeDirectory;
                }

                _nativeDirectory = ResolveNativeDirectory(
                    AppDomain.CurrentDomain.BaseDirectory,
                    Directory.GetCurrentDirectory());

                if (IsWindows() && !string.IsNullOrWhiteSpace(_nativeDirectory))
                {
                    RegisterWindowsNativeDirectory(_nativeDirectory);
                }

                return _nativeDirectory;
            }
        }

        internal static string ResolveNativeDirectory(string baseDirectory, string currentDirectory)
        {
            foreach (string candidate in GetCandidateDirectories(baseDirectory, currentDirectory))
            {
                if (Directory.Exists(candidate) && ContainsCoreOcrNativeLibrary(candidate))
                {
                    return Path.GetFullPath(candidate);
                }
            }

            return string.Empty;
        }

        private static IEnumerable<string> GetCandidateDirectories(string baseDirectory, string currentDirectory)
        {
            if (!string.IsNullOrWhiteSpace(baseDirectory))
            {
                yield return Path.Combine(baseDirectory, "runtimes", "win-x64", "native");
                yield return baseDirectory;
            }

            if (!string.IsNullOrWhiteSpace(currentDirectory))
            {
                yield return Path.Combine(currentDirectory, "runtimes", "win-x64", "native");
                yield return currentDirectory;
            }
        }

        private static bool ContainsCoreOcrNativeLibrary(string directory)
        {
            return File.Exists(Path.Combine(directory, OCRSDK.dllFileName + ".dll"));
        }

        private static bool IsWindows()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            return platform == PlatformID.Win32NT
                || platform == PlatformID.Win32S
                || platform == PlatformID.Win32Windows
                || platform == PlatformID.WinCE;
        }

        private static void RegisterWindowsNativeDirectory(string nativeDirectory)
        {
            string currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            if (!PathContainsDirectory(currentPath, nativeDirectory))
            {
                Environment.SetEnvironmentVariable("PATH", nativeDirectory + Path.PathSeparator + currentPath);
            }

            SetDllDirectory(nativeDirectory);
        }

        private static bool PathContainsDirectory(string pathValue, string directory)
        {
            string[] parts = pathValue.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string normalizedPart;
                try
                {
                    normalizedPart = Path.GetFullPath(part.Trim());
                }
                catch
                {
                    continue;
                }

                if (string.Equals(
                    normalizedPart.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        [DllImport("kernel32", EntryPoint = "SetDllDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}