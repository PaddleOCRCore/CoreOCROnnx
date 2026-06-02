import argparse
import ctypes
import json
import os
import time
from pathlib import Path


def load_library(dll_path: Path):
    if os.name == "nt":
        if hasattr(os, "add_dll_directory"):
            os.add_dll_directory(str(dll_path.parent))
        return ctypes.WinDLL(str(dll_path))
    return ctypes.CDLL(str(dll_path))


def to_c_string(value: str):
    return ctypes.c_char_p(value.encode("utf-8"))


def read_result(dll, ptr):
    if not ptr:
        return ""
    try:
        return ctypes.string_at(ptr).decode("utf-8", errors="replace")
    finally:
        dll.FreeResultBuffer(ptr)


def configure_api(dll):
    dll.ActivateLicense.argtypes = [ctypes.c_char_p]
    dll.ActivateLicense.restype = ctypes.c_bool

    dll.YoloInitJson.argtypes = [ctypes.c_char_p, ctypes.c_char_p]
    dll.YoloInitJson.restype = ctypes.c_bool

    dll.YoloDetect.argtypes = [ctypes.c_char_p]
    dll.YoloDetect.restype = ctypes.c_void_p

    dll.YoloFreeEngine.argtypes = []
    dll.YoloFreeEngine.restype = ctypes.c_int

    dll.GetError.argtypes = []
    dll.GetError.restype = ctypes.c_void_p

    dll.FreeResultBuffer.argtypes = [ctypes.c_void_p]
    dll.FreeResultBuffer.restype = None


def get_error(dll):
    return read_result(dll, dll.GetError())


def iter_images(image_dir: Path):
    suffixes = {".jpg", ".jpeg", ".png", ".bmp", ".webp"}
    for path in sorted(image_dir.iterdir()):
        if path.is_file() and path.suffix.lower() in suffixes:
            yield path


def main():
    cwd = Path.cwd()
    parser = argparse.ArgumentParser(description="PaddleOCROnnx YOLO Python demo")
    parser.add_argument("--dll", default=str(cwd / "PaddleOCROnnx.dll"), help="Path to PaddleOCROnnx.dll")
    parser.add_argument("--model", default=str(cwd / "models" / "yolov8s.onnx"), help="Path to YOLO ONNX model")
    parser.add_argument("--images", default=str(cwd / "images"), help="Directory containing images")
    parser.add_argument("--license", default="", help="Optional license file path")
    parser.add_argument("--use-gpu", action="store_true", help="Use GPU if the DLL and license allow it")
    parser.add_argument("--gpu-id", type=int, default=0, help="GPU device id")
    parser.add_argument("--input-width", type=int, default=640, help="YOLO model input width")
    parser.add_argument("--input-height", type=int, default=640, help="YOLO model input height")
    parser.add_argument("--conf", type=float, default=0.25, help="Confidence threshold")
    parser.add_argument("--iou", type=float, default=0.45, help="NMS IOU threshold")
    parser.add_argument("--threads", type=int, default=1, help="ONNX Runtime CPU thread count")
    parser.add_argument("--class-names-file", default="", help="Optional class names text file")
    args = parser.parse_args()

    dll_path = Path(args.dll).resolve()
    model_path = Path(args.model).resolve()
    image_dir = Path(args.images).resolve()

    if not dll_path.exists():
        raise FileNotFoundError(f"DLL not found: {dll_path}")
    if not model_path.exists():
        raise FileNotFoundError(f"YOLO model not found: {model_path}")
    if not image_dir.exists():
        raise FileNotFoundError(f"Image directory not found: {image_dir}")

    dll = load_library(dll_path)
    configure_api(dll)

    if args.license:
        ok = dll.ActivateLicense(to_c_string(str(Path(args.license).resolve())))
        if not ok:
            raise RuntimeError(f"ActivateLicense failed: {get_error(dll)}")

    init_param = {
        "model_type": 1,
        "input_width": args.input_width,
        "input_height": args.input_height,
        "confidence_threshold": args.conf,
        "iou_threshold": args.iou,
        "num_threads": args.threads,
        "use_gpu": args.use_gpu,
        "gpu_id": args.gpu_id,
        "warmup": True,
    }
    if args.class_names_file:
        init_param["class_names_file"] = str(Path(args.class_names_file).resolve())

    ok = dll.YoloInitJson(
        to_c_string(str(model_path)),
        to_c_string(json.dumps(init_param, ensure_ascii=False)),
    )
    if not ok:
        raise RuntimeError(f"YoloInitJson failed: {get_error(dll)}")

    try:
        images = list(iter_images(image_dir))
        if not images:
            print(f"No images found in: {image_dir}")
            return

        for image_path in images:
            print(f"Detect image: {image_path.name}")
            start = time.time()
            result_ptr = dll.YoloDetect(to_c_string(str(image_path)))
            result = read_result(dll, result_ptr)
            elapsed_ms = (time.time() - start) * 1000
            print(f"YOLO time: {elapsed_ms:.2f} ms")
            print(result if result else "Detect failed or returned empty result")
    finally:
        dll.YoloFreeEngine()

    input("Press Enter to exit...")


if __name__ == "__main__":
    main()
