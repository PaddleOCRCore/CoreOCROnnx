# PaddleOCRWebOnnxAPI接口文档
## 简介
实现在线调用OCR识别的WebAPI服务

## 运行环境
项目运行环境为.net10.0：

1、使用IIS：服务器环境推荐，建议操作系统Windows Server2016 Data Center，
安装IIS，及.net10 环境，下载地址：
https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0，找到ASP.NET Core
运行时 10.0，点击Windows 平台Hosting Bundle 下载并安装。
2、独立运行服务：建议操作系统Win10 以上64 位，
安装ASP.NET Core 运行时 10.0：
https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0
安装.NET 桌面运行时 10.0：
https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0
创建一个批处理文件：StartOCRApi.bat，输入以下内容：
@echo off
set CURRENT_DIR=%~dp0
CHCP 65001
echo Starting CoreOCROnnx.WebApi.dll..
dotnet "%CURRENT_DIR%CoreOCROnnx.WebApi.dll" --urls http://*:5000
pause
并将批处理发送至桌面快捷方式
双击批处理文件StartOCRApi.bat，启动服务，默认端口5000(批处理中可修改)，浏览器打开http://localhost:5000 提示服务正在运行即正常。

打开http://localhost:5000 可使用在线Demo，打开http://localhost:5000/scalar 可查看接口及在线调试。

开发调试可在项目根目录执行：
`
dotnet run --project CoreOCROnnx.WebApi/CoreOCROnnx.WebApi.csproj --no-launch-profile --urls http://localhost:5050
`
然后打开http://localhost:5050 使用在线Demo。

### 修改Web.Config 配置文件，将hostingModel="inprocess"改为hostingModel=" OutOfProcess "

## 请求与响应协议
接口采用Post请求，具体依所访问接口定义为准。

请求Content-Type设定 application/json 

## 接口返回结果说明
请求的返回参数格式为 JSON，编码为UTF-8 

`
{
 "status": 200,
 "data": object
 "errorMessage": ""
}`

| 参数名      | 描述   | 
| ----------  | ------ |
| status      | 接口请求校验结果代码  如：200 表示成功 ,其它为失败| 
| data        | 返回数据 文字或 Json 数据| 
| errorMessage|  调用接口返回的说明| 

## 接口清单

|序号| 类型| 接口地址| 接口名称| 创建日期| 最后发布日期| 备注|
| -- | --- |-------- | ------- |---------| ------------|-----|
|1| OCR| /OCRService/GetOCRText| 图片OCR识别| 2025/03/28| 2025/03/28| 上传Base64|
|2| OCR| /OCRService/GetOCRFile| 图片OCR识别| 2025/04/27| 2025/04/27| 上传图片|
|3| YOLO| /YOLOService/GetYOLOFileTensor| YOLO Tensor识别| 2026/07/14| 2026/07/14| 上传图片|
|4| YOLO| /YOLOService/GetYOLOBase64Tensor| YOLO Tensor识别| 2026/07/14| 2026/07/14| 上传Base64|
|5| Demo| /OCRDemo/Analyze| 在线Demo图片解析| 2026/07/14| 2026/07/14| 上传图片，支持PP-OCRv6/YOLO|
|6| License| /Home/GetLicenseRequestCode| 获取授权请求码| 2026/07/14| 2026/07/14| 在线Demo授权功能|
|7| License| /Home/GetLicenseStatus| 获取授权状态| 2026/07/14| 2026/07/14| 在线Demo授权功能|
|8| License| /Home/UploadLicense| 上传授权文件| 2026/07/14| 2026/07/14| 上传.lic文件|

## 在线Demo

浏览器访问：http://localhost:5000

在线Demo支持上传图片并选择解析模型：

| 模型 | 说明 |
| ---- | ---- |
| PP-OCRv6 | 调用OCR识别并返回文字、Json和文本框坐标 |
| YOLO | 调用YOLO Tensor识别，服务端按640x640输入尺寸做后处理并返回检测框 |

图片限制：仅支持PNG/JPG/JPEG/BMP/TIF/TIFF，单文件大小不超过10MB。

### 在线Demo解析接口

接口地址：/OCRDemo/Analyze

提交方式：POST

Content-Type：multipart/form-data

表单字段：

| 参数名称 | 描述 | 类型 | 是否必填 | 备注 |
| -------- | ---- | ---- | -------- | ---- |
| file | 图片文件 | file | 必填 | PNG/JPG/JPEG/BMP/TIF/TIFF |
| model | 解析模型 | string | 必填 | pp-ocrv6 或 yolo |

返回数据示例：

`
{
 "status": 200,
 "data": {
  "model": "pp-ocrv6",
  "modelName": "PP-OCRv6",
  "fileName": "demo.jpg",
  "content": "识别文本",
  "markdown": "识别文本",
  "jsonText": "{}",
  "imageWidth": 1024,
  "imageHeight": 768,
  "raw": {},
  "boxes": []
 },
 "errorMessage": ""
}
`

YOLO模式下raw返回裁剪后的Tensor摘要和后处理检测结果，不返回完整Tensor data数组；如需完整Tensor，请使用/YOLOService/GetYOLOFileTensor或/YOLOService/GetYOLOBase64Tensor。

### 在线Demo授权接口

在线Demo保留license-actions授权相关功能，接口如下：

| 接口地址 | 提交方式 | 说明 |
| -------- | -------- | ---- |
| /Home/GetLicenseRequestCode | GET | 获取当前机器授权请求码 |
| /Home/GetLicenseStatus | GET | 获取授权状态 |
| /Home/UploadLicense | POST multipart/form-data | 上传.lic授权文件 |

/Home/UploadLicense表单字段名为file，文件大小不超过1MB。授权验证成功后会保存到OCRConfig.OCRLicense配置的位置。

图片OCR识别：/OCRService/GetOCRText 

提交方式：POST

传入参数：

`
{
 "Base64String ":"",
 " ResultType ":"text"
} `

| 序号|  参数名称   | 描述  | 类型   |  是否必填 |  备注  | 
| --- | ----------  |-------| ------ |-----------| ------ |
| 1   | Base64String|  图片Base6 编码|  字符串|  必填|
| 2   | ResultType | text/json|  字符串 | 必填 | Text仅返回文字| 

### 返回结果示例：

`
{
 "status": 200,
 "data": "纯臻营养护发素\r\n 产品信息/参数\r\n（45 元/每公斤，100 公斤起订）
\r\n 每瓶 22 元，1000 瓶起订）\r\n【品牌】：代加工方式/OEMODM\r\n【品名】：纯
臻营养护发素\r\n【产品编号】：YM-X-3011\r\nODMOEM\r\n【净含量】：220ml\r\n
【适用人群】：适合所有肤质\r\n【主要成分】：鲸蜡硬脂醇、燕麦 β-葡聚\r\n 糖、椰
油酰胺丙基甜菜碱、泛醌\r\n（成品包材）\r\n【主要功能】：可紧致头发磷层，从而
达到\r\n 即时持久改善头发光泽的效果，给干燥的头\r\n 发足够的滋养",
 "errorMessage": ""
}
`

## YOLO Tensor识别接口

YOLO模型参数在appsettings.json的YOLOConfig节点配置。默认模型文件名为yolov8s.onnx，服务运行时会从程序目录下的models文件夹加载，例如：

`
CoreOCROnnx.WebApi.dll
models/yolov8s.onnx
`

### 上传图片返回Tensor

接口地址：/YOLOService/GetYOLOFileTensor

提交方式：POST

Content-Type：multipart/form-data

表单字段名：request

返回数据为完整YoloTensorResult：

`
{
 "status": 200,
 "data": {
  "data": [0.1, 0.2],
  "shape": [1, 8400, 84],
  "shapeLen": 3,
  "elementCount": 705600
 },
 "errorMessage": ""
}
`

### 上传Base64返回Tensor

接口地址：/YOLOService/GetYOLOBase64Tensor

提交方式：POST

Content-Type：application/json

传入参数：

`
{
 "Base64String": ""
}
`

返回数据同/GetYOLOFileTensor，data为原始Tensor展开数据，shape通常为[batch, boxes, channels]。

## 模型路径配置

OCRConfig和YOLOConfig都支持models_root配置，表示模型根目录，支持绝对路径或相对程序目录路径。

示例：

`
"OCRConfig": {
    "models_root": "models",
    "det_infer": "PP-OCRv6_tiny_det.onnx",
    "rec_infer": "PP-OCRv6_tiny_rec.onnx",
    "cls_infer": "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx",
    "keyFile": "ppocrv6tiny_dict.txt",
    "OCRLicense": "models/paddleocr.lic"
},
"YOLOConfig": {
    "models_root": "models",
    "model_path": "yolov8s.onnx"
}
`
