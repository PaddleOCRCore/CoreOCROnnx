import ctypes
import os
import sys
from pathlib import Path


dll_directory_handles = []


def find_project_root() -> Path:
    script_dir = Path(__file__).resolve().parent
    if script_dir.name.lower() == "demo":
        return script_dir.parent
    return Path.cwd().resolve()


def find_library(project_root: Path) -> Path:
    architecture = "x64" if ctypes.sizeof(ctypes.c_void_p) == 8 else "x86"
    search_dirs = (
        Path.cwd().resolve(),
        Path(__file__).resolve().parent,
        project_root,
        project_root / "runtimes" / f"win-{architecture}" / "bin",
        project_root / "build_windows" / "bin",
        project_root / f"build_Onnx.CPU.win-{architecture}" / "bin",
        project_root / f"build_Onnx.DirectML.win-{architecture}" / "bin",
        project_root / f"build_OpenVino.CPU.win-{architecture}" / "bin",
        project_root / f"build_OpenVino.GPU.win-{architecture}" / "bin",
        project_root / f"build_TensorRT.GPU.win-{architecture}" / "bin",
    )

    candidate_dirs = tuple(
        candidate_dir
        for directory in search_dirs
        for candidate_dir in (directory, directory / "Release")
    )
    for directory in candidate_dirs:
        candidate = directory / "PaddleOCROnnx.dll"
        if candidate.is_file():
            return candidate

    searched = "\n".join(
        str(directory / "PaddleOCROnnx.dll") for directory in candidate_dirs
    )
    raise FileNotFoundError(
        "Could not find PaddleOCROnnx.dll. Searched:\n" + searched
    )


def load_library(library_path: Path):
    library_dir = library_path.resolve().parent
    if hasattr(os, "add_dll_directory"):
        dll_directory_handles.append(os.add_dll_directory(str(library_dir)))
    return ctypes.WinDLL(str(library_path.resolve()))


project_root = find_project_root()
library_path = find_library(project_root)
ocr_dll = load_library(library_path)

get_license_request_code = ocr_dll.GetLicenseRequestCode
get_license_request_code.argtypes = []
get_license_request_code.restype = ctypes.c_void_p

free_result_buffer = ocr_dll.FreeResultBuffer
free_result_buffer.argtypes = [ctypes.c_void_p]
free_result_buffer.restype = None

result_ptr = get_license_request_code()
if not result_ptr:
    raise RuntimeError("GetLicenseRequestCode returned null")

try:
    request_code = ctypes.string_at(result_ptr).decode("utf-8")
finally:
    free_result_buffer(result_ptr)

print("License request code:")
print(request_code)

output_path = project_root / "license_request_code.txt"
output_path.write_text(request_code, encoding="utf-8")

print(f"Saved to: {output_path}")

if sys.stdin.isatty():
    input("Press Enter to exit...")
