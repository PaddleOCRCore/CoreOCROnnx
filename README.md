[<img src="https://img.shields.io/badge/Language-简体中文-red.svg">](README.md)
# CoreOCR离线OCR组件，支持onnx及yolo,支持C#/C++/java/Python/Go语言开发
<p align="center">
    <a href="./LICENSE"><img src="https://img.shields.io/badge/license-Apache%202-dfd.svg"></a>
    <a href="https://github.com/PaddleOCRCore/CoreOCROnnx/releases"><img src="https://github.com/PaddleOCRCore/CoreOCROnnx?color=ffa"></a>
    <a href="https://github.com/PaddleOCRCore/CoreOCROnnx/stargazers"><img src="https://img.shields.io/github/stars/PaddleOCRCore/CoreOCROnnx?color=ccf"></a>
</p>

## 一、简介
免费离线极速版OCR组件，支持 ONNX Runtime、OpenVINO 和 TensorRT 运行时包三选一。不同后端的设备支持范围不同：ONNX Runtime/OpenVINO 包可按发布说明使用 CPU 或对应 GPU，TensorRT 包仅支持 NVIDIA GPU。支持C#/C++/java/Python/Go语言开发及多线程并发。
喜欢的请给本项目点一个免费的Star

支持最新PP-OCRv6/PP-OCRv5模型，向下兼容V4/V3模型

Paddle推理库版本请移步：[PaddleOCRCore/PaddleOCRApi](https://github.com/PaddleOCRCore/PaddleOCRApi)

支持YOLO模型

## 二、运行环境
项目运行环境为VS2022+.net10.0：

1、核心文件PaddleOCROnnx.dll为C++动态链接库。其 CPU/GPU 能力取决于所使用的后端运行时包；TensorRT 版仅支持 Windows/Linux x64 NVIDIA GPU。

### [WebApi接口文档](./PaddleOCROnnxApi/README.md)
WebApi部署后可供前端调用。

### WinFormDemo预览：

<img src="./CoreOCROnnx.SDK/OCRRuntime/ocrDemo.png" width="800px;" />

依赖库列表参考：

## 三、调用参数说明
| 参数名称                     | 默认值 | 值说明                                                                                   |
| ---------------------------- | ------ | ---------------------------------------------------------------------------------------- |
| det_model_dir                | -      | 检测模型inference model地址                                                              |
| cls_model_dir                | -      | 方向分类器inference model地址                                                            |
| rec_infer                    | -      | 文字识别模型inference model地址                                                          |
| keys                         | -      | 文字识别字典文件,V5和V4的不通用                                                          |
| 通用参数                     | --     | -- |
| cpu_mem                      | 4000   | CPU内存占用上限，单位MB。-1表示不限制                                                    |
| cpu_math_library_num_threads | 10     | CPU预测时的线程数，在机器核数充足的情况下，该值越大，预测速度越快                        |
| use_gpu                      | false  | 是否使用GPU；TensorRT 后端始终使用 GPU，此参数仅为 ABI 兼容保留                          |
| gpu_id                       | 0      | GPU id；TensorRT 后端表示 CUDA 设备编号                                                   |
| gpu_mem                      | 4000   | GPU 内存参数；实际含义及支持情况取决于所使用的后端                                       |
| padding                      | 20     | 图像预处理，在图片外周添加白边，用于提升识别率，文字框没有正确框住所有文字时，增加此值。 | 
| maxSideLen                   | 1024   | 按图片最长边的长度，此值为0代表不缩放，例：1024，如果图片长边大于1024则把图像整          |
| boxScoreThresh               | 0.5    | 文字框置信度门限，文字框没有正确框住所有文字时，减小此值。                               |
| boxThresh                    | 0.3    | 请自行试验                                                                               |
| unClipRatio                  | 1.6    | 单个文字框大小倍率，越大时单个文字框越大。此项与图片的大小相关，越大的图片此值应该越大。 |
| doAngle                      | true   | 只有图片倒置的情况下(旋转90~270度的图片)，才需要启用文字方向检测。                       |
| mostAngle                    | true   | 启用(1) / 禁用(0) 角度投票(整张图片以最大可能文字方向来识别)，当禁用文字方向检测时，此项也不起作用。|
| visualize                    | false  | 是否对结果进行可视化，为true时，预测结果会保存在output文件夹下和输入图像同名的图像上。   |
| enable_log                   | false  | 是否输出到文件日志，在log目录下                                                          |
| isOutputConsole              | true   | 是否输出到控制台日志                                                                     |

## 四、OpenVINO 后端说明

当前 C# WebApi/WinForms 仍然通过 `PaddleOCROnnx.dll` 调用 CoreOCR 运行时。ONNX Runtime 后端和 OpenVINO 后端的 DLL 文件名相同，导出的 C API 函数名、调用约定和参数保持一致，C# 端的 `DllImport` 声明通常不需要修改，只需要按需替换对应后端的运行时包。

OpenVINO 后端支持：

- PaddleOCR：检测、方向分类、文字识别
- YOLO：detect、pose、classification、seg、obb 以及现有 Tensor 接口

### 运行时包下载

请在 GitHub Release 中下载对应后端的运行时包，二选一：

- ONNX Runtime 后端：[OCRRuntimeOnnx_v4.0.0.zip](https://github.com/PaddleOCRCore/CoreOCROnnx/releases/download/v4.0.0/OCRRuntimeOnnx_v4.0.0.zip)
- OpenVINO 后端：[OCRRuntimeOpenVino_v4.0.0.zip](https://github.com/PaddleOCRCore/CoreOCROnnx/releases/download/v4.0.0/OCRRuntimeOpenVino_v4.0.0.zip)
- TensorRT 后端：因TensorRT较大，百度网盘下载：[CoreOCROnnxApi4.1-TensorRT.zip](https://pan.baidu.com/s/1h8tyeGzG0dukzMVDUwPqWQ?pwd=bejk)

将压缩包中的 `PaddleOCROnnx.dll` 及其同目录依赖文件复制到 C# 程序运行目录即可。当前发布包为 Windows x64，因此 C# 项目也应使用 x64 运行。

OpenVINO 后端的 GPU 表示 OpenVINO Intel GPU，不是 CUDA，也不是 DirectML。

### 部署文件

部署 OpenVINO 后端时不要只复制 `PaddleOCROnnx.dll`，需要将 OpenVINO 运行时包中的依赖文件一起复制到 C# 程序运行目录。

CPU 版至少包含：

```text
PaddleOCROnnx.dll
openvino.dll
openvino_c.dll
openvino_intel_cpu_plugin.dll
openvino_ir_frontend.dll
openvino_onnx_frontend.dll
plugins.xml
```

如果发布包包含 GPU 插件，还会包含：

```text
openvino_intel_gpu_plugin.dll
cache.json
```

包含 GPU 插件的 OpenVINO 包可同时支持 CPU/GPU。`use_gpu=false` 时走 CPU，`use_gpu=true` 时走 OpenVINO `GPU` 设备。未包含 GPU 插件的包如果传 `use_gpu=true` 会初始化失败。

### 模型格式

当前 OpenVINO 包包含的 frontend 为：

```text
ir
onnx
```

因此当前项目支持两类模型路径：

- ONNX：传入 `.onnx` 文件路径
- OpenVINO IR：传入 `.xml` 文件路径，旁边必须有同名 `.bin`

使用 OpenVINO IR 的 PaddleOCR 示例：

```text
det_infer = models\PP-OCRv5_mobile_det_ov\inference.xml
cls_infer = models\PP-OCRv5_mobile_cls_ov\inference.xml
rec_infer = models\PP-OCRv5_mobile_rec_ov\inference.xml
keys      = models\keys.txt
```

注意：不要把模型目录直接传给 `Init` / `Initjson`。例如下面这种路径会失败：

```text
models\PP-OCRv5_mobile_det_ov
```

应传具体的 `.xml` 文件：

```text
models\PP-OCRv5_mobile_det_ov\inference.xml
```

如果错误信息中出现 `model format: ""`，通常表示传入的是目录或没有扩展名的路径。OpenVINO 无法从目录名判断模型格式。

YOLO 的 `YoloInitJson` 同样支持传入 `.onnx` 或 OpenVINO IR 的 `.xml` 文件路径。

### C# 端兼容性

OpenVINO 后端保持以下导出函数不变：

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

因此 C# 端主要需要确认：

- DLL 和 OpenVINO 依赖文件在同一运行目录
- 进程为 x64
- IR 模型传入的是 `.xml` 文件路径，不是模型目录
- GPU 参数的含义为 OpenVINO Intel GPU

## 五、TensorRT 后端说明

TensorRT 后端通过 `PaddleOCROnnx.dll` 提供 PaddleOCR 与 YOLO 推理，保持现有 C API、调用约定和 C# `DllImport` 声明不变。调用方需要替换 TensorRT 版 DLL 及其运行时依赖，不需要修改接口名称。

### 环境要求

- Windows 10/11 x64 或 Linux x64
- 64 位应用进程，不支持 x86/Win32
- NVIDIA GPU，不支持 CPU、Intel GPU、AMD GPU、DirectML 或 OpenVINO 设备
- TensorRT 11.1
- CUDA 12.9 Runtime
- 与 CUDA 12.9 和目标 GPU 兼容的 NVIDIA 显卡驱动
- C# WebApi/WinForms 项目必须以 x64 运行，不能使用 `Any CPU` 选择 32 位进程

TensorRT 始终在 NVIDIA GPU 上执行。OCR 与 YOLO 初始化都会进行 GPU 授权检查。历史参数 `use_gpu` 仅为 ABI 兼容保留，设置为 `false` 不会切换到 CPU；`gpu_id` 用于选择 CUDA 设备，默认值为 `0`。

### Windows 部署文件

部署时不要只复制 `PaddleOCROnnx.dll`。需要将构建输出目录中的 TensorRT 11.1 和 CUDA 12.9 Runtime DLL 一起复制到 WebApi/WinForms 程序运行目录，其中至少包括：

```text
PaddleOCROnnx.dll
nvinfer_11.dll
nvonnxparser_11.dll
nvinfer_builder_resource_*.dll
cudart64_*.dll
```

实际发布时应以 TensorRT 构建输出目录中的 DLL 集合为准，并保持这些文件位于 `PaddleOCROnnx.dll` 同目录或系统可搜索路径中。缺少依赖时，C# 通常会报告无法加载 DLL 或找不到指定模块。

### 模型格式与 engine 缓存

TensorRT 后端支持以下模型路径：

- `.onnx`：首次初始化时解析 ONNX、选择适合当前 GPU 的 CUDA tactic，并在模型同目录生成 `.fp32.engine` 缓存。
- `.engine` / `.plan`：直接加载 TensorRT 序列化 engine，不再执行 ONNX 构建。

第一次从 ONNX 初始化可能需要数十秒，并占用较多 GPU 显存；缓存生成后，后续初始化会直接加载 engine，启动速度更快。源 ONNX 文件更新后，旧缓存会自动失效并重新构建。模型目录必须允许当前进程写入，否则无法保存 engine 缓存。

TensorRT engine 与生成它的 TensorRT 版本、操作系统、GPU 架构及构建配置相关，不是可跨机器任意复用的通用模型格式。更换 GPU、升级 TensorRT/CUDA 或修改动态输入范围后，应删除旧 `.engine` 并在目标机器上重新生成。直接传入 `.engine`/`.plan` 时，如果反序列化失败，初始化会直接返回失败，不会回退到 ONNX。

### 与其他后端的区别

| 后端 | 设备 | 常用模型格式 | `use_gpu=false` |
| ---- | ---- | ------------ | --------------- |
| ONNX Runtime | 取决于发布包和执行提供程序 | `.onnx` | 可使用 CPU |
| OpenVINO | CPU 或 Intel GPU，取决于插件 | `.onnx`、`.xml` + `.bin` | 使用 CPU |
| TensorRT | 仅 NVIDIA GPU | `.onnx`、`.engine`、`.plan` | 仍使用 NVIDIA GPU |


## 开发交流群

欢迎加入QQ群475159576交流,或者添加QQ：2380243976,若您喜欢本项目，请点击免费的Star

<img src="./CoreOCROnnx.SDK/OCRRuntime/qq.png" width="300px;" />

## 捐助

如果这个项目对您有所帮助，请扫下方二维码打赏一杯咖啡。

<img src="./CoreOCROnnx.SDK/OCRRuntime/donate.jpg" width="300px;" />

## 更新日志
### v4.1.0 `2026.6.7`
- 增加后端TensorRT支持，入群下载运行时。
### v4.0.0 `2026.6.7`
- 增加Yolo支持，增加OpenVino支持
### v1.0.0 `2026.1.18`
- 初版发行: CoreOCROnnx.WebApi

## ⭐️ Star

[![Star History Chart](https://api.star-history.com/svg?repos=PaddleOCRCore/CoreOCROnnx&type=Date)](https://star-history.com/#PaddleOCRCore/CoreOCROnnx&Date)

## 📄 许可证书

本项目的发布受 [Apache License Version 2.0](./LICENSE) 许可认证, 欢迎大家使用和贡献。
