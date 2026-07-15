[<img src="https://img.shields.io/badge/Language-简体中文-red.svg">](README.md) [<img src="https://img.shields.io/badge/Language-English-blue.svg">](README_EN.md)
# CoreOCR Offline OCR Component with ONNX and YOLO Support for C#, C++, Java, Python, and Go

<p align="center">
    <a href="./LICENSE"><img src="https://img.shields.io/badge/license-Apache%202-dfd.svg"></a>
    <a href="https://github.com/PaddleOCRCore/CoreOCROnnx/releases"><img src="https://github.com/PaddleOCRCore/CoreOCROnnx?color=ffa"></a>
    <a href="https://github.com/PaddleOCRCore/CoreOCROnnx/stargazers"><img src="https://img.shields.io/github/stars/PaddleOCRCore/CoreOCROnnx?color=ccf"></a>
</p>

## 1. Introduction

CoreOCR is a free, fast, offline OCR component supporting ONNX Runtime, DirectML, OpenVINO, and TensorRT backends. Platform, device, and licensing requirements vary by backend; an application only needs one runtime package matching its target environment. The component supports multithreaded concurrent development in C#, C++, Java, Python, and Go.

If you find this project useful, please give it a free Star.

Supports the latest PP-OCRv6 and PP-OCRv5 models and is backward compatible with V4 and V3 models.

For the Paddle Inference version, see [PaddleOCRCore/PaddleOCRApi](https://github.com/PaddleOCRCore/PaddleOCRApi).

YOLO models are also supported.

## 2. Runtime Environment

The project requires Visual Studio 2022 or later and .NET 10.0.

1. The core file, `PaddleOCROnnx.dll` (`PaddleOCROnnx.so` on Linux), is a C++ dynamic library. Its CPU/GPU capabilities depend on the selected backend runtime package. The application platform and process architecture must match the runtime package.

### Runtime Package Downloads

Installing through [NuGet](https://www.nuget.org/packages?q=CoreOCRRuntime) is recommended. A C# application normally needs:

1. `CoreOCROnnx.SDK` for the C# interfaces, data models, and service implementations.
2. One `CoreOCRRuntime.*` native runtime package matching the target operating system, process architecture, and inference device.

> Install only one runtime package per application. These packages contain the same `PaddleOCROnnx.dll` or `PaddleOCROnnx.so` filename, so installing multiple runtimes causes files to overwrite one another. Use separate project configurations or publish directories when distributing multiple backends.

For example, a Windows x64 CPU project can use:

```bash
dotnet add package CoreOCROnnx.SDK
dotnet add package CoreOCRRuntime.Onnx.CPU.win-x64
```

This repository already references `CoreOCROnnx.SDK` as a source project. The Web API project therefore only needs one desired `CoreOCRRuntime.*` package reference.

#### Runtime Packages

| NuGet package | Platform/architecture | Inference device | Main model formats | License requirement |
| --- | --- | --- | --- | --- |
| `CoreOCRRuntime.Onnx.CPU.win-x86` | Windows x86 | CPU | `.onnx` | Not required |
| `CoreOCRRuntime.Onnx.CPU.win-x64` | Windows x64 | CPU | `.onnx` | Not required |
| `CoreOCRRuntime.Onnx.CPU.linux-x64` | Linux x64 | CPU | `.onnx` | Required |
| `CoreOCRRuntime.DirectML.win-x86` | Windows x86 | DirectML GPU; CPU is also available | `.onnx` | Not required for CPU; required for GPU |
| `CoreOCRRuntime.DirectML.win-x64` | Windows x64 | DirectML GPU; CPU is also available | `.onnx` | Not required for CPU; required for GPU |
| `CoreOCRRuntime.OpenVino.CPU.win-x64` | Windows x64 | CPU | `.onnx`, `.xml` + `.bin` | Not required |
| `CoreOCRRuntime.OpenVino.GPU.win-x64` | Windows x64 | Intel GPU; also includes the CPU plugin | `.onnx`, `.xml` + `.bin` | Not required for CPU; required for GPU |
| `CoreOCRRuntime.TensorRT.GPU.win-x64` | Windows x64 | NVIDIA GPU | `.onnx`, `.engine`, `.plan` | GPU license required |
| `CoreOCRRuntime.TensorRT.GPU.linux-x64` | Linux x64 | NVIDIA GPU | `.onnx`, `.engine`, `.plan` | GPU license required |

Selection guide:

- For general CPU deployments or minimal external dependencies, choose the ONNX Runtime CPU package for the target platform.
- For AMD, Intel, or NVIDIA GPUs on Windows, choose DirectML.
- For Intel CPUs or Intel GPUs on Windows, choose the corresponding OpenVINO package.
- For maximum performance on NVIDIA GPUs on Windows or Linux, choose TensorRT.

Runtime packages include PP-OCRv6 ONNX sample models, recognition dictionaries, and related resources. These files are placed in the application output after installation or publishing. Windows packages also provide a license request-code utility; on Linux, obtain the request code through the SDK's `GetLicenseRequestCode()` method. You may replace the sample models with your own compatible models; keep every model and dictionary used by the deployed application.

#### Licensing and Native Dependencies

- Windows CPU mode is free and does not require license activation.
- Windows GPU modes (DirectML, OpenVINO GPU, and TensorRT) require a license that permits GPU use.
- Linux requires license activation for both CPU and GPU use. NuGet packages do not include an activated commercial license.
- When a license is required, generate the request code on the final deployment machine and apply for a license matching that machine, platform, and product version at [CoreOCR Online Licensing](http://ocr.axinw.com).
- DirectML and OpenVINO dependencies are copied by their runtime packages. Do not deploy only `PaddleOCROnnx.dll`.
- TensorRT runtime packages do not include NVIDIA TensorRT, CUDA, or a graphics driver. Install TensorRT 11.1, CUDA 12.9 Runtime, and a compatible NVIDIA driver on the target machine.

For manual native-library deployment, download the matching version from [GitHub Releases](https://github.com/PaddleOCRCore/CoreOCROnnx/releases). Copy the complete dependency set from the archive, and never mix files from different backends or architectures.

### [Web API Documentation](./CoreOCROnnx.WebApi/README.md)

After deployment, the Web API can be called by frontend applications.

### WinForms Demo Preview

<img src="./CoreOCROnnx.SDK/OCRRuntime/ocrDemo.png" width="800px;" />

## 3. Parameters

| Parameter | Default | Description |
| --------- | ------- | ----------- |
| det_model_dir | - | Path to the text detection inference model. |
| cls_model_dir | - | Path to the text direction classifier inference model. |
| rec_infer | - | Path to the text recognition inference model. |
| keys | - | Path to the text recognition dictionary. V5 and V4 dictionaries are not interchangeable. |
| General parameters | -- | -- |
| cpu_mem | 4000 | CPU memory limit in MB. Use `-1` for no limit. |
| cpu_math_library_num_threads | 10 | Number of CPU inference threads. More threads generally improve inference speed when sufficient CPU cores are available. |
| use_gpu | false | Whether to use a GPU. The TensorRT backend always uses a GPU; this parameter is retained only for ABI compatibility. |
| gpu_id | 0 | GPU ID. For TensorRT, this selects the CUDA device. |
| gpu_mem | 4000 | GPU memory parameter. Its actual meaning and availability depend on the selected backend. |
| padding | 20 | Adds a white border during image preprocessing to improve recognition. Increase this value when text boxes do not fully enclose the text. |
| maxSideLen | 1024 | Maximum length of the image's longest side. Use `0` to disable resizing. For example, a value of `1024` resizes images whose longest side exceeds 1024 pixels. |
| boxScoreThresh | 0.5 | Text-box confidence threshold. Lower this value when text boxes do not fully enclose the text. |
| boxThresh | 0.3 | Detection box threshold. Tune this value for your use case. |
| unClipRatio | 1.6 | Scale factor for individual text boxes. Larger values produce larger boxes. The ideal value depends on image dimensions and may need to be increased for larger images. |
| doAngle | true | Enable text direction detection only when images may be rotated by 90-270 degrees. |
| mostAngle | true | Enable (`1`) or disable (`0`) angle voting, which recognizes the whole image using the most probable text direction. This option has no effect when direction detection is disabled. |
| visualize | false | Visualize results. When `true`, annotated images are saved under the `output` directory using the input image names. |
| enable_log | false | Write logs to files under the `log` directory. |
| isOutputConsole | true | Write logs to the console. |

### YoloInitJson Parameters

`YoloInitJson` initializes a YOLO model from a JSON parameter string. Complete `parameterjson` example:

```json
{
    "model_type": 1,
    "input_width": 640,
    "input_height": 640,
    "confidence_threshold": 0.25,
    "point_score_threshold": 0.25,
    "iou_threshold": 0.45,
    "enable_nms": false,
    "key_points_num": 17,
    "num_threads": 4,
    "use_gpu": false,
    "gpu_id": 0,
    "warmup": true,
    "visualize": false,
    "enable_log": false,
    "class_names_preset": "auto",
    "class_names": ["person", "car"],
    "class_names_file": "coco.names"
}
```

| Parameter | Example | Description |
| --- | --- | --- |
| `model_type` | `1` | Model type: `1`=detect, `2`=pose, `3`=classification, `4`=detect FP16, `5`=pose FP16, `6`=classification FP16, `7`=seg, `8`=obb, `9`=seg FP16, and `10`=obb FP16. |
| `input_width` / `input_height` | `640` | Model input dimensions. Fixed dimensions can be read automatically from an ONNX model; dynamic dimensions default to 640 when omitted. |
| `confidence_threshold` | `0.25` | Object or class confidence threshold. Candidates below this value are filtered out. |
| `point_score_threshold` | `0.25` | Pose keypoint drawing threshold. Keypoints and skeleton lines below this value are not drawn. |
| `iou_threshold` | `0.45` | Overlapping-box threshold for NMS. It applies only when `enable_nms=true`. |
| `enable_nms` | `false` | Enable NMS. The default is `false`. When disabled, detection results are not filtered by overlap; raw Tensor API output is unaffected. |
| `key_points_num` | `17` | Number of keypoints in a pose model. COCO human-pose models normally use 17. |
| `num_threads` | `4` | Number of CPU inference threads. The default is 1. |
| `use_gpu` | `false` | Use a GPU or accelerated backend. The license must permit it, and the corresponding backend build is required. TensorRT always uses a GPU. |
| `gpu_id` | `0` | GPU device ID. The default is 0. |
| `warmup` | `true` | Run one warm-up inference after initialization. The default is `true`. |
| `visualize` | `false` | Save visualized images under `output`. JSON APIs return `vis_path`; Tensor APIs perform visualization only as a side effect. |
| `enable_log` | `false` | Write YOLO runtime logs to the console. |
| `class_names_preset` | `"auto"` | Class-name preset: `auto` selects names based on the model task, while `none` disables preset class names. |
| `class_names` | `["person", "car"]` | Class names supplied as a JSON array or a comma/newline-delimited string. This takes precedence over `class_names_preset`. |
| `class_names_file` | `"coco.names"` | Path to a class-name file containing one name per line. When set, it overrides `class_names`. |

API declaration:

```cpp
Paddle_API bool CALL_CONV YoloInitJson(const char* modelPath, const char* parameterjson);
```

- `modelPath`: path to a YOLO ONNX model. Formats supported by the selected OpenVINO or TensorRT backend may also be used.
- `parameterjson`: YOLO initialization parameters as a JSON string.
- Return value: `true` on success or `false` on failure. Call `GetError` for error details.

## 4. OpenVINO Backend

The C# Web API and WinForms applications call the CoreOCR runtime through `PaddleOCROnnx.dll`. The ONNX Runtime and OpenVINO backends use the same DLL name, exported C API function names, calling conventions, and parameters. The C# `DllImport` declarations generally require no changes; replace only the backend runtime package as needed.

The OpenVINO backend supports:

- PaddleOCR: detection, direction classification, and text recognition
- YOLO: detect, pose, classification, segmentation, OBB, and the existing Tensor APIs

An OpenVINO GPU means an Intel GPU supported by OpenVINO, not CUDA or DirectML.

### Deployment Files

Do not deploy `PaddleOCROnnx.dll` by itself. Copy its OpenVINO dependencies from the runtime package to the C# application output directory as well.

The CPU package includes at least:

```text
PaddleOCROnnx.dll
openvino.dll
openvino_c.dll
openvino_intel_cpu_plugin.dll
openvino_ir_frontend.dll
openvino_onnx_frontend.dll
plugins.xml
```

Packages that include the GPU plugin also contain:

```text
openvino_intel_gpu_plugin.dll
cache.json
```

An OpenVINO package containing the GPU plugin supports both CPU and GPU execution. Set `use_gpu=false` for the CPU or `use_gpu=true` for the OpenVINO `GPU` device. Initialization fails if `use_gpu=true` is used with a package that does not contain the GPU plugin.

### Model Formats

The current OpenVINO package includes the following frontends:

```text
ir
onnx
```

The project therefore accepts two model path formats:

- ONNX: a path to an `.onnx` file
- OpenVINO IR: a path to an `.xml` file with a matching `.bin` file beside it

Example PaddleOCR configuration using OpenVINO IR:

```text
det_infer = models\PP-OCRv5_mobile_det_ov\inference.xml
cls_infer = models\PP-OCRv5_mobile_cls_ov\inference.xml
rec_infer = models\PP-OCRv5_mobile_rec_ov\inference.xml
keys      = models\keys.txt
```

Do not pass a model directory directly to `Init` or `Initjson`. For example, this path fails:

```text
models\PP-OCRv5_mobile_det_ov
```

Pass the specific `.xml` file instead:

```text
models\PP-OCRv5_mobile_det_ov\inference.xml
```

An error containing `model format: ""` usually means that a directory or a path without a file extension was provided. OpenVINO cannot determine a model format from a directory name.

YOLO's `YoloInitJson` also accepts either an `.onnx` file or an OpenVINO IR `.xml` file.

### C# Compatibility

The OpenVINO backend preserves the following exports:

```text
Init
Initjson
Detect
DetectMat
DetectByte
DetectBase64
FreeEngine
GetError
FreeResultBuffer
YoloInitJson
YoloDetect
YoloDetectMat
YoloDetectByte
YoloDetectBase64
YoloDetectTensor
YoloDetectMatTensor
YoloDetectByteTensor
YoloDetectBase64Tensor
YoloFreeTensor
YoloFreeEngine
```

On the C# side, verify that:

- The DLL and all OpenVINO dependencies are in the same output directory.
- The process runs as x64.
- An IR model path points to an `.xml` file rather than a model directory.
- GPU parameters refer to an OpenVINO Intel GPU.

## 5. TensorRT Backend

The TensorRT backend provides PaddleOCR and YOLO inference through `PaddleOCROnnx.dll` while preserving the existing C API, calling conventions, and C# `DllImport` declarations. Replace the DLL and its runtime dependencies with the TensorRT versions; no interface names need to change.

### Requirements

- Windows 10/11 x64 or Linux x64
- A 64-bit application process; x86/Win32 is not supported
- An NVIDIA GPU; CPU, Intel GPU, AMD GPU, DirectML, and OpenVINO devices are not supported
- TensorRT 11.1
- CUDA 12.9 Runtime
- An NVIDIA graphics driver compatible with CUDA 12.9 and the target GPU
- C# Web API and WinForms projects must run as x64 and must not use an `Any CPU` configuration that selects a 32-bit process

TensorRT always runs on an NVIDIA GPU. OCR and YOLO initialization both perform a GPU license check. The legacy `use_gpu` parameter is retained only for ABI compatibility; setting it to `false` does not switch to the CPU. `gpu_id` selects the CUDA device and defaults to `0`.

### Windows Deployment Files

Do not deploy `PaddleOCROnnx.dll` by itself. Copy the TensorRT 11.1 and CUDA 12.9 Runtime DLLs from the build output directory to the Web API or WinForms application output directory. At minimum, these include:

```text
PaddleOCROnnx.dll
nvinfer_11.dll
nvonnxparser_11.dll
nvinfer_builder_resource_*.dll
cudart64_*.dll
```

For a release, use the complete DLL set from the TensorRT build output. Keep these files beside `PaddleOCROnnx.dll` or in a system search path. If a dependency is missing, C# typically reports that the DLL could not be loaded or that the specified module could not be found.

### Model Formats and Engine Cache

The TensorRT backend supports the following model paths:

- `.onnx`: on first initialization, TensorRT parses the ONNX model, selects CUDA tactics for the current GPU, and creates an `.fp32.engine` cache beside the model.
- `.engine` / `.plan`: loads a serialized TensorRT engine directly without building it from ONNX.

The first initialization from ONNX may take tens of seconds and use substantial GPU memory. Later initializations load the generated engine cache and start faster. If the source ONNX file changes, the old cache is invalidated and rebuilt automatically. The process must have write access to the model directory so that the engine cache can be saved.

A TensorRT engine depends on the TensorRT version, operating system, GPU architecture, and build configuration used to create it. It is not a generally portable model format. After changing the GPU, upgrading TensorRT/CUDA, or modifying the dynamic input range, delete the old `.engine` file and rebuild it on the target machine. If a directly supplied `.engine` or `.plan` file cannot be deserialized, initialization fails without falling back to ONNX.

### Backend Comparison

| Backend | Device | Common model formats | Behavior when `use_gpu=false` |
| ------- | ------ | -------------------- | ----------------------------- |
| ONNX Runtime CPU | CPU | `.onnx` | Uses the CPU |
| DirectML | AMD/Intel/NVIDIA GPU on Windows; CPU is also available | `.onnx` | Uses the CPU |
| OpenVINO | CPU or Intel GPU, depending on installed plugins | `.onnx`, `.xml` + `.bin` | Uses the CPU |
| TensorRT | NVIDIA GPU only | `.onnx`, `.engine`, `.plan` | Still uses the NVIDIA GPU |

## Developer Community

Join QQ group 475159576 or add QQ contact 2380243976 to discuss the project. If you find the project useful, please give it a free Star.

<img src="./CoreOCROnnx.SDK/OCRRuntime/qq.png" width="300px;" />

## Donate

If this project has helped you, scan the QR code below to buy the author a coffee.

<img src="./CoreOCROnnx.SDK/OCRRuntime/donate.jpg" width="300px;" />

## Changelog

### v4.1.0 `2026.7.15`

- Added TensorRT backend support.

### v4.0.0 `2026.6.7`

- Added YOLO and OpenVINO support.

### v1.0.0 `2026.1.18`

- Initial release: CoreOCROnnx.WebApi.

## Star

[![Star History Chart](https://api.star-history.com/svg?repos=PaddleOCRCore/CoreOCROnnx&type=Date)](https://star-history.com/#PaddleOCRCore/CoreOCROnnx&Date)

## License

This project is released under the [Apache License Version 2.0](./LICENSE). Contributions are welcome.