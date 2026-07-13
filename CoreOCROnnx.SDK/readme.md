# CoreOCROnnx.SDK

CoreOCROnnx.SDK 是 CoreOCROnnx 的 .NET 调用库，提供离线 PaddleOCR 文字识别和 YOLO 推理接口。SDK 只包含托管代码；实际推理还需要安装一个 `CoreOCRRuntime.*` 原生运行时包，并准备对应模型文件。

项目主页：[PaddleOCRCore/CoreOCROnnx](https://github.com/PaddleOCRCore/CoreOCROnnx)

## 支持范围

- .NET Standard 2.0
- .NET Framework 4.5、4.6.1、4.7、4.8
- .NET 6、7、8、9
- PaddleOCR PP-OCRv6、PP-OCRv5，并兼容 V4/V3 模型
- YOLO detect、pose、classification、seg、obb 和 Tensor 接口
- ONNX Runtime、OpenVINO、TensorRT 三类推理后端

当前 NuGet 运行时包按操作系统、架构和后端拆分。应用进程的平台必须与运行时包一致。

## 安装

先安装托管 SDK：

```shell
dotnet add package CoreOCROnnx.SDK
```

再根据部署环境选择且只选择一个运行时包，例如 Windows x64 CPU：

```shell
dotnet add package CoreOCRRuntime.Onnx.CPU.win-x64
```

可用运行时包包括：

| NuGet 包 | 平台 | 设备 |
| --- | --- | --- |
| `CoreOCRRuntime.Onnx.CPU.win-x64` | Windows x64 | CPU |
| `CoreOCRRuntime.Onnx.CPU.win-x86` | Windows x86 | CPU |
| `CoreOCRRuntime.Onnx.CPU.linux-x64` | Linux x64 | CPU |
| `CoreOCRRuntime.DirectML.win-x64` | Windows x64 | DirectML GPU |
| `CoreOCRRuntime.DirectML.win-x86` | Windows x86 | DirectML GPU |
| `CoreOCRRuntime.OpenVino.CPU.win-x64` | Windows x64 | CPU |
| `CoreOCRRuntime.OpenVino.GPU.win-x64` | Windows x64 | Intel GPU |
| `CoreOCRRuntime.TensorRT.GPU.win-x64` | Windows x64 | NVIDIA GPU |
| `CoreOCRRuntime.TensorRT.GPU.linux-x64` | Linux x64 | NVIDIA GPU |

不要在同一应用中同时安装多个后端运行时包，否则同名原生库可能产生冲突。TensorRT 运行时还要求兼容的 NVIDIA 驱动、CUDA 和 TensorRT 环境。

## 快速开始

准备检测、方向分类、文字识别模型和字典文件，然后初始化一次 `OCRService`：

```csharp
using System;
using System.IO;
using CoreOCROnnx.SDK;

string modelDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models");

var parameter = new InitParamater
{
    det_infer = Path.Combine(modelDirectory, "PP-OCRv6_tiny_det.onnx"),
    cls_infer = Path.Combine(modelDirectory, "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx"),
    rec_infer = Path.Combine(modelDirectory, "PP-OCRv6_tiny_rec.onnx"),
    keyFile = Path.Combine(modelDirectory, "ppocrv6tiny_dict.txt"),
    paraType = EnumParaType.Class,
    ocrpara = OCRParameter.CreateDefault()
};

var ocrService = new OCRService();
ocrService.Init(parameter);

OCRResult result = ocrService.Detect("test.jpg");
Console.WriteLine(result.StrRes);
Console.WriteLine(result.JsonText);
```

`OCRService` 也支持通过 `byte[]`、Base64 字符串和 OpenCV Mat 指针识别图片。

## 运行时与模型说明

- Windows NuGet 运行时通常位于输出目录的 `runtimes/<rid>/native` 下，SDK 会自动查找并注册原生库目录。
- 发布应用时请检查原生运行时文件是否随发布结果一起输出，不要只复制 `CoreOCROnnx.SDK.dll`。
- ONNX Runtime 使用 `.onnx` 模型。
- OpenVINO 支持 `.onnx`，也支持 `.xml` 与同名 `.bin` 组成的 IR 模型；初始化时应传具体 `.xml` 文件，不能传模型目录。
- TensorRT 支持 `.onnx`、`.engine` 和 `.plan`。首次从 ONNX 初始化可能生成与当前 GPU 和 TensorRT 环境绑定的 engine 缓存。
- TensorRT 后端始终使用 NVIDIA GPU，`use_gpu=false` 不会切换到 CPU。

如需调整运行时文件复制布局，可在应用项目中设置：

```xml
<PropertyGroup>
  <CoreOCRRuntimeKeepNativeInRidFolder>true</CoreOCRRuntimeKeepNativeInRidFolder>
  <CoreOCRRuntimeCopyNativeDllsToOutDir>true</CoreOCRRuntimeCopyNativeDllsToOutDir>
</PropertyGroup>
```

## 常见问题

### 无法加载 `PaddleOCROnnx` 或提示找不到指定模块

确认应用位数与运行时包一致，并检查 `PaddleOCROnnx.dll` 及该后端的依赖库都存在于发布输出中。Windows 下还可以使用 Dependencies 工具检查缺失 DLL。

### OpenVINO 提示模型格式为空

请传入带扩展名的 `.onnx` 或 `.xml` 文件路径，不要传模型目录。使用 IR 模型时，确保 `.xml` 旁边存在同名 `.bin`。

### TensorRT 初始化较慢

首次解析 ONNX 会构建 engine，耗时通常明显高于后续启动。确保模型目录可写，以便保存 engine 缓存。

## 更多资料

- [完整项目文档](https://github.com/PaddleOCRCore/CoreOCROnnx)
- [Release 与运行时下载](https://github.com/PaddleOCRCore/CoreOCROnnx/releases)
- 技术交流群：QQ群 475159576

本项目使用 [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0) 许可证。