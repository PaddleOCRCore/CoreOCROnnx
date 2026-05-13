[<img src="https://img.shields.io/badge/Language-简体中文-red.svg">](README.md)
# PaddleOCROnnx离线OCR组件 支持C#/C++/java/Python/Go语言开发
<p align="center">
    <a href="./LICENSE"><img src="https://img.shields.io/badge/license-Apache%202-dfd.svg"></a>
    <a href="https://github.com/PaddleOCRCore/CoreOCROnnx/releases"><img src="https://github.com/PaddleOCRCore/CoreOCROnnx?color=ffa"></a>
    <a href="https://github.com/PaddleOCRCore/CoreOCROnnx/stargazers"><img src="https://img.shields.io/github/stars/PaddleOCRCore/CoreOCROnnx?color=ccf"></a>
</p>

## 一、简介
免费离线极速版OCR组件,采用Onnx模型,支持CPU/GPU，支持C#/C++/java/Python/Go语言开发，支持多线程并发，基于OnnxRuntime封装的C++动态链接库。
喜欢的请给本项目点一个免费的Star

支持最新PP-OCRv5_mobile/PP-OCRv5_server模型，向下兼容V4/V3模型

Paddle推理库版本请移步：[PaddleOCRCore/PaddleOCRApi](https://github.com/PaddleOCRCore/PaddleOCRApi)

## 二、运行环境
项目运行环境为VS2022+.net8.0：

1、核心文件PaddleOCROnnx.dll为C++动态链接库，支持CPU模式(GPU及Linux环境请入群)

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
| use_gpu                      | false  | 是否使用GPU                                                                              |
| gpu_id                       | 0      | GPU id，使用GPU时有效                                                                    |
| gpu_mem                      | 4000   | 使用GPU时内存                                                                            |
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


## 开发交流群

欢迎加入QQ群475159576交流,或者添加QQ：2380243976,若您喜欢本项目，请点击免费的Star

<img src="./CoreOCROnnx.SDK/OCRRuntime/qq.png" width="382px;" />

## 捐助

如果这个项目对您有所帮助，请扫下方二维码打赏一杯咖啡。

<img src="./CoreOCROnnx.SDK/OCRRuntime/donate.jpg" width="382px;" />

## 更新日志
### v1.0 `2026.1.18`
- 初版发行: CoreOCROnnx.WebApi

## ⭐️ Star

[![Star History Chart](https://api.star-history.com/svg?repos=PaddleOCRCore/CoreOCROnnx&type=Date)](https://star-history.com/#PaddleOCRCore/CoreOCROnnx&Date)

## 📄 许可证书

本项目的发布受 [Apache License Version 2.0](./LICENSE) 许可认证, 欢迎大家使用和贡献。