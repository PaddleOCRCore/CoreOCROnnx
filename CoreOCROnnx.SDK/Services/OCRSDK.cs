// Copyright (c) 2025 PaddleOCRCore All Rights Reserved.
// https://github.com/PaddleOCRCore/PaddleOCRApi.git
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.IO;
#if NETCOREAPP3_0_OR_GREATER
using System.Reflection;
#endif
using System.Runtime.InteropServices;

namespace CoreOCROnnx.SDK
{
    /// <summary>
    /// 调用PaddleOCROnnx.dll动态链接库
    /// </summary>
    internal class OCRSDK
    {
        internal const string dllFileName = "PaddleOCROnnx";
        private static readonly List<IntPtr> nativeHandles = new List<IntPtr>();

        static OCRSDK()
        {
            ConfigureNativeDllLoading();
        }

        private static void ConfigureNativeDllLoading()
        {
            string nativeDirectory = NativeRuntimeLoader.EnsureLoaded();
            if (string.IsNullOrWhiteSpace(nativeDirectory))
            {
                nativeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            TryLoadLocalDll(nativeDirectory, "onnxruntime");

#if NETCOREAPP3_0_OR_GREATER
            try
            {
                NativeLibrary.SetDllImportResolver(typeof(OCRSDK).Assembly, ResolveNativeLibrary);
            }
            catch (InvalidOperationException)
            {
                // A resolver can only be registered once per assembly.
            }
#else
        TryLoadLocalDll(nativeDirectory, dllFileName);
#endif
        }

#if NETCOREAPP3_0_OR_GREATER
        private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (!libraryName.Equals(dllFileName, StringComparison.OrdinalIgnoreCase))
            {
                return IntPtr.Zero;
            }

            string nativeDirectory = NativeRuntimeLoader.EnsureLoaded();
            if (string.IsNullOrWhiteSpace(nativeDirectory))
            {
                nativeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            string dllPath = Path.Combine(nativeDirectory, dllFileName + ".dll");
            if (!File.Exists(dllPath))
            {
                return IntPtr.Zero;
            }

            IntPtr handle = NativeLibrary.Load(dllPath);
            nativeHandles.Add(handle);
            return handle;
        }
#endif

        private static void TryLoadLocalDll(string baseDirectory, string dllName)
        {
            string dllPath = Path.Combine(baseDirectory, dllName + ".dll");
            if (!File.Exists(dllPath))
            {
                return;
            }

#if NETCOREAPP3_0_OR_GREATER
            IntPtr handle = NativeLibrary.Load(dllPath);
#else
            IntPtr handle = NativeMethods.LoadLibrary(dllPath);
#endif
            if (handle == IntPtr.Zero)
            {
                return;
            }
            nativeHandles.Add(handle);
        }

#if !NETCOREAPP3_0_OR_GREATER
        private static class NativeMethods
        {
            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LoadLibrary(string lpFileName);
        }
#endif

        /// <summary>
        /// 获取错误提示
        /// </summary>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr GetError();

        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern void FreeResultBuffer(IntPtr ptr);

        /// <summary>
        /// 获取加密授权申请码，返回值需调用FreeResultBuffer释放
        /// </summary>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr GetLicenseRequestCode();

        /// <summary>
        /// 激活授权文件
        /// </summary>
        /// <param name="licensefile">授权文件路径</param>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool ActivateLicense(string licensefile);

        /// <summary>
        /// 获取当前授权状态JSON，返回值需调用FreeResultBuffer释放
        /// </summary>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr GetLicenseStatus();

        /// <summary>
        /// 初始化OCR文字识别
        /// </summary>
        /// <param name="det_infer"></param>
        /// <param name="cls_infer"></param>
        /// <param name="rec_infer"></param>
        /// <param name="keyfile"></param>
        /// <param name="ocrpara"></param>
        /// <returns></returns>

        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool Init(string det_infer, string cls_infer, string rec_infer, string keyfile, OCRParameter ocrpara);
        /// <summary>
        /// 初始化OCR文字识别
        /// </summary>
        /// <param name="det_infer"></param>
        /// <param name="cls_infer"></param>
        /// <param name="rec_infer"></param>
        /// <param name="keyfile"></param>
        /// <param name="parjson">json参数</param>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool Initjson(string det_infer, string cls_infer, string rec_infer, string keyfile, string parjson);
        /// <summary>
        /// OCR识别
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr Detect(string filename);
        /// <summary>
        /// OCR识别Mat
        /// </summary>
        /// <param name="cvmat">Mat</param>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr DetectMat(IntPtr cvmat);
        /// <summary>
        /// OCR文字识别
        /// </summary>
        /// <param name="imagebyte">图片字节码</param>
        /// <param name="size">大小</param>
        /// <returns></returns>

        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr DetectByte(byte[] imagebyte, long size);
        /// <summary>
        /// OCR文字识别
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>

        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr DetectBase64(string base64);
        /// <summary>
        /// 释放OCR实例
        /// </summary>
        /// <returns></returns>

        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern int FreeEngine();

        /// <summary>
        /// 初始化YOLO模型，parameterJson为YOLO初始化参数JSON
        /// </summary>
        /// <param name="modelPath">YOLO ONNX模型路径</param>
        /// <param name="parameterJson">YOLO初始化参数JSON</param>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool YoloInitJson(string modelPath, string parameterJson);

        /// <summary>
        /// YOLO检测图片文件，返回JSON字符串，返回值需调用FreeResultBuffer释放
        /// </summary>
        /// <param name="filename">图片文件路径</param>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr YoloDetect(string filename);

        /// <summary>
        /// YOLO检测Mat，返回JSON字符串，返回值需调用FreeResultBuffer释放
        /// </summary>
        /// <param name="cvmat">Mat指针</param>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr YoloDetectMat(IntPtr cvmat);

        /// <summary>
        /// YOLO检测图片字节，返回JSON字符串，返回值需调用FreeResultBuffer释放
        /// </summary>
        /// <param name="imagebyte">图片字节码</param>
        /// <param name="size">大小</param>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr YoloDetectByte(byte[] imagebyte, long size);

        /// <summary>
        /// YOLO检测Base64图片，返回JSON字符串，返回值需调用FreeResultBuffer释放
        /// </summary>
        /// <param name="base64">Base64图片</param>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr YoloDetectBase64(string base64);

        /// <summary>
        /// YOLO检测图片文件，返回标准张量[bs, boxes, channels]连续float数组。
        /// 调用成功后outData指向非托管内存，shape通常为[1, 8400, nc + 4]，使用完必须调用YoloFreeTensor释放。
        /// </summary>
        /// <param name="filename">图片文件路径</param>
        /// <param name="outData">输出张量数据指针，按[bs, boxes, channels]连续展开</param>
        /// <param name="outShape">输出shape数组，调用方需传入长度至少为3的long数组</param>
        /// <param name="outShapeLen">输出shape维度数量，当前为3</param>
        /// <param name="outElementCount">输出float元素总数</param>
        /// <returns>成功返回true，失败返回false，可通过GetError获取错误信息</returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool YoloDetectTensor(string filename, out IntPtr outData,
            [Out] long[] outShape, out int outShapeLen, out long outElementCount);

        /// <summary>
        /// YOLO检测Mat，返回标准张量[bs, boxes, channels]连续float数组。
        /// 调用成功后outData指向非托管内存，使用完必须调用YoloFreeTensor释放。
        /// </summary>
        /// <param name="cvmat">Mat指针</param>
        /// <param name="outData">输出张量数据指针，按[bs, boxes, channels]连续展开</param>
        /// <param name="outShape">输出shape数组，调用方需传入长度至少为3的long数组</param>
        /// <param name="outShapeLen">输出shape维度数量，当前为3</param>
        /// <param name="outElementCount">输出float元素总数</param>
        /// <returns>成功返回true，失败返回false，可通过GetError获取错误信息</returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool YoloDetectMatTensor(IntPtr cvmat, out IntPtr outData,
            [Out] long[] outShape, out int outShapeLen, out long outElementCount);

        /// <summary>
        /// YOLO检测图片字节，返回标准张量[bs, boxes, channels]连续float数组。
        /// 调用成功后outData指向非托管内存，使用完必须调用YoloFreeTensor释放。
        /// </summary>
        /// <param name="imagebyte">图片字节码</param>
        /// <param name="size">大小</param>
        /// <param name="outData">输出张量数据指针，按[bs, boxes, channels]连续展开</param>
        /// <param name="outShape">输出shape数组，调用方需传入长度至少为3的long数组</param>
        /// <param name="outShapeLen">输出shape维度数量，当前为3</param>
        /// <param name="outElementCount">输出float元素总数</param>
        /// <returns>成功返回true，失败返回false，可通过GetError获取错误信息</returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool YoloDetectByteTensor(byte[] imagebyte, long size, out IntPtr outData,
            [Out] long[] outShape, out int outShapeLen, out long outElementCount);

        /// <summary>
        /// YOLO检测Base64图片，返回标准张量[bs, boxes, channels]连续float数组。
        /// 调用成功后outData指向非托管内存，使用完必须调用YoloFreeTensor释放。
        /// </summary>
        /// <param name="base64">Base64图片</param>
        /// <param name="outData">输出张量数据指针，按[bs, boxes, channels]连续展开</param>
        /// <param name="outShape">输出shape数组，调用方需传入长度至少为3的long数组</param>
        /// <param name="outShapeLen">输出shape维度数量，当前为3</param>
        /// <param name="outElementCount">输出float元素总数</param>
        /// <returns>成功返回true，失败返回false，可通过GetError获取错误信息</returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool YoloDetectBase64Tensor(string base64, out IntPtr outData,
            [Out] long[] outShape, out int outShapeLen, out long outElementCount);

        /// <summary>
        /// 释放YOLO张量接口返回的非托管float数组内存。
        /// </summary>
        /// <param name="ptr">YoloDetectTensor系列接口返回的outData指针</param>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern void YoloFreeTensor(IntPtr ptr);

        /// <summary>
        /// 释放YOLO实例
        /// </summary>
        /// <returns></returns>
        [DllImport(dllFileName, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern int YoloFreeEngine();

    }
}
