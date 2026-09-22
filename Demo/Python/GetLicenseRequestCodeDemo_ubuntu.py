import ctypes
import sys
from pathlib import Path


def find_project_root() -> Path:
    script_dir = Path(__file__).resolve().parent
    if script_dir.name.lower() == "demo":
        return script_dir.parent
    return Path.cwd().resolve()


def find_library(project_root: Path) -> Path:
    names = ("PaddleOCROnnx.so", "libPaddleOCROnnx.so")
    search_dirs = (
        Path.cwd().resolve(),
        Path(__file__).resolve().parent,
        project_root,
        project_root / "runtimes" / "linux-x64" / "bin",
        project_root / "build_Onnx.CPU.linux-x64" / "lib",
    )

    for directory in search_dirs:
        for name in names:
            candidate = directory / name
            if candidate.is_file():
                return candidate

    searched = "\n".join(str(directory / name) for directory in search_dirs for name in names)
    raise FileNotFoundError(
        "Could not find PaddleOCROnnx shared library. Searched:\n" + searched
    )


project_root = find_project_root()
library_path = find_library(project_root)
ocr_dll = ctypes.CDLL(str(library_path))

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
