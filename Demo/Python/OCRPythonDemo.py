import atexit
import ctypes
import json
import os
import sys
import time
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
        project_root / "runtimes" / f"win--{architecture}" / "bin",
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


def to_c_string(path: Path) -> ctypes.c_char_p:
    return ctypes.c_char_p(str(path.resolve()).encode("utf-8"))


def read_result_string(result_ptr: int, free_result_buffer) -> str:
    if not result_ptr:
        return ""
    try:
        return ctypes.string_at(result_ptr).decode("utf-8")
    finally:
        free_result_buffer(result_ptr)


project_root = find_project_root()
library_path = find_library(project_root)
ocr_dll = load_library(library_path)

activate_license = ocr_dll.ActivateLicense
activate_license.argtypes = [ctypes.c_char_p]
activate_license.restype = ctypes.c_bool

get_error = ocr_dll.GetError
get_error.argtypes = []
get_error.restype = ctypes.c_void_p

init_func = ocr_dll.Initjson
init_func.argtypes = [
    ctypes.c_char_p,
    ctypes.c_char_p,
    ctypes.c_char_p,
    ctypes.c_char_p,
    ctypes.c_char_p,
]
init_func.restype = ctypes.c_bool

detect_func = ocr_dll.Detect
detect_func.argtypes = [ctypes.c_char_p]
detect_func.restype = ctypes.c_void_p

free_result_buffer = ocr_dll.FreeResultBuffer
free_result_buffer.argtypes = [ctypes.c_void_p]
free_result_buffer.restype = None

free_engine = ocr_dll.FreeEngine
free_engine.argtypes = []
free_engine.restype = ctypes.c_int

model_dir = project_root / "models"
image_dir = project_root / "images"
license_path = model_dir / "paddleocr.lic"

if not license_path.is_file():
    raise FileNotFoundError(f"License file not found: {license_path}")

if not activate_license(to_c_string(license_path)):
    error_text = read_result_string(get_error(), free_result_buffer)
    raise RuntimeError(f"ActivateLicense failed: {error_text}")

parameter_json = json.dumps(
    {
        "cpu_mem": 0,
        "cpu_threads": 8,
        "use_gpu": False,
        "gpu_id": 0,
        "gpu_mem": 4000,
        "padding": 0,
        "maxSideLen": 1024,
        "boxScoreThresh": 0.5,
        "boxThresh": 0.3,
        "unClipRatio": 1.6,
        "doAngle": True,
        "mostAngle": True,
        "visualize": False,
        "enable_log": False,
        "isOutputConsole": False,
        "ocr_instance_count": 1,
        "returnCharBoxes": False,
    },
    separators=(",", ":"),
).encode("utf-8")

init_ok = init_func(
    to_c_string(model_dir / "PP-OCRv6_tiny_det.onnx"),
    to_c_string(model_dir / "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx"),
    to_c_string(model_dir / "PP-OCRv6_tiny_rec.onnx"),
    to_c_string(model_dir / "ppocrv6tiny_dict.txt"),
    ctypes.c_char_p(parameter_json),
)

if not init_ok:
    error_text = read_result_string(get_error(), free_result_buffer)
    raise RuntimeError(f"Initjson failed: {error_text}")
atexit.register(free_engine)

if not image_dir.is_dir():
    raise FileNotFoundError(f"Image directory not found: {image_dir}")

image_paths = sorted(path for path in image_dir.iterdir() if path.is_file())
if not image_paths:
    print(f"No images found in: {image_dir}")
    sys.exit(0)

for image_path in image_paths:
    print("Processing image:", image_path.name)
    start_time = time.perf_counter()

    result_ptr = detect_func(to_c_string(image_path))
    result_text = read_result_string(result_ptr, free_result_buffer)

    elapsed_time = time.perf_counter() - start_time
    print(f"OCR elapsed: {elapsed_time * 1000:.2f}ms")

    if result_text:
        print("OCR result:", result_text)
    else:
        error_text = read_result_string(get_error(), free_result_buffer)
        print(f"OCR failed: {error_text or 'empty result'}")

if sys.stdin.isatty():
    input("Press Enter to exit...")