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

#pragma once
#include <cstddef>
#include <cstdint>
#include <string>
#include <opencv2/opencv.hpp>//使用OpenCV4.10
#include <include/AI_Parameter.h>
#pragma comment (lib,"PaddleOCROnnx.lib")
#pragma once

extern "C" {
    __declspec(dllimport) const char* __stdcall GetLicenseRequestCode();
    __declspec(dllimport) bool __stdcall ActivateLicense(const char* licensefile);
    __declspec(dllimport) const char* __stdcall GetLicenseStatus();

    __declspec(dllimport) bool __stdcall Init(const char* det_infer, const char* cls_infer,
        const char* rec_infer, const char* keys,
        const OCRParameter parameter);
    __declspec(dllimport) bool __stdcall Initjson(const char* det_infer, const char* cls_infer,
        const char* rec_infer, const char* keys,
        const char* parameterjson);
    __declspec(dllimport) const char* __stdcall Detect(const char* imageFile);
    __declspec(dllimport) const char* __stdcall DetectMat(const cv::Mat& cvmat);
    __declspec(dllimport) const char* __stdcall DetectByte(unsigned char* imagebytedata,
        std::size_t size);
    __declspec(dllimport) const char* __stdcall DetectBase64(const char* imagebase64);
    __declspec(dllimport) int __stdcall FreeEngine();

    /// <summary>
    /// 初始化YOLO模型(JSON参数)。完整parameterjson示例:
    /// {
    ///   "model_type": 1,
    ///   "input_width": 640,
    ///   "input_height": 640,
    ///   "confidence_threshold": 0.25,
    ///   "point_score_threshold": 0.25,
    ///   "iou_threshold": 0.45,
    ///   "enable_nms": false,
    ///   "key_points_num": 17,
    ///   "num_threads": 4,
    ///   "use_gpu": false,
    ///   "gpu_id": 0,
    ///   "warmup": true,
    ///   "visualize": false,
    ///   "enable_log": false,
    ///   "class_names_preset": "auto",
    ///   "class_names": ["person", "car"],
    ///   "class_names_file": "coco.names"
    /// }
    ///
    /// 参数说明:
    /// model_type: 模型类型。1=detect,2=pose,3=classification,4=detect FP16,5=pose FP16,6=classification FP16,7=seg,8=obb,9=seg FP16,10=obb FP16。
    /// input_width/input_height: 模型输入尺寸；固定尺寸模型可自动从ONNX读取，动态尺寸未传时默认640。
    /// confidence_threshold: 目标/类别置信度阈值，低于该值的候选结果会被过滤。
    /// point_score_threshold: pose关键点绘制阈值，低于该值的关键点/骨架线不绘制。
    /// iou_threshold: NMS重叠框过滤阈值，enable_nms=true时生效。
    /// enable_nms: 是否启用NMS，默认false；false时不按重叠框过滤检测结果，Tensor接口原始输出不受该参数影响。
    /// key_points_num: pose模型关键点数量，COCO人体姿态通常为17。
    /// num_threads: CPU推理线程数，默认1。
    /// use_gpu: 是否使用GPU/加速后端；需授权允许且工程使用对应后端构建。
    /// gpu_id: GPU设备编号，默认0。
    /// warmup: 初始化后是否执行一次预热推理，默认true。
    /// visualize: 是否保存可视化图片到output目录；JSON接口会返回vis_path，Tensor接口仅作为可视化副作用。
    /// enable_log: 是否输出YOLO运行日志到控制台。
    /// class_names_preset: 类别预设；auto=按模型任务自动设置，none=不使用预设类别名。
    /// class_names: 类别名，可传JSON数组，也可传逗号/换行分隔字符串；优先级高于class_names_preset。
    /// class_names_file: 类别名文件路径，按行读取；设置后会覆盖class_names。
    /// </summary>
    __declspec(dllimport) bool __stdcall YoloInitJson(const char* modelPath, const char* parameterjson);

    /// <summary>
    /// YOLO检测图片文件，返回JSON字符串；调用方使用FreeResultBuffer释放。
    /// </summary>
    __declspec(dllimport) const char* __stdcall YoloDetect(const char* imageFile);

    /// <summary>
    /// YOLO检测cv::Mat，返回JSON字符串；调用方使用FreeResultBuffer释放。
    /// </summary>
    __declspec(dllimport) const char* __stdcall YoloDetectMat(const cv::Mat& cvmat);

    /// <summary>
    /// YOLO检测图片编码字节，返回JSON字符串；调用方使用FreeResultBuffer释放。
    /// </summary>
    __declspec(dllimport) const char* __stdcall YoloDetectByte(unsigned char* imagebytedata,
        std::size_t size);

    /// <summary>
    /// YOLO检测Base64图片，返回JSON字符串；调用方使用FreeResultBuffer释放。
    /// </summary>
    __declspec(dllimport) const char* __stdcall YoloDetectBase64(const char* imagebase64);

    /// <summary>
    /// YOLO检测图片文件，返回标准张量[bs, boxes, channels]连续float数组。
    /// outShape需至少可写入3个int64_t；调用方使用YoloFreeTensor释放outData。
    /// </summary>
    __declspec(dllimport) bool __stdcall YoloDetectTensor(const char* imageFile, float** outData,
        int64_t* outShape, int* outShapeLen, int64_t* outElementCount);

    /// <summary>
    /// YOLO检测cv::Mat，返回标准张量[bs, boxes, channels]连续float数组。
    /// </summary>
    __declspec(dllimport) bool __stdcall YoloDetectMatTensor(const cv::Mat& cvmat, float** outData,
        int64_t* outShape, int* outShapeLen, int64_t* outElementCount);

    /// <summary>
    /// YOLO检测图片编码字节，返回标准张量[bs, boxes, channels]连续float数组。
    /// </summary>
    __declspec(dllimport) bool __stdcall YoloDetectByteTensor(unsigned char* imagebytedata,
        std::size_t size, float** outData, int64_t* outShape, int* outShapeLen,
        int64_t* outElementCount);

    /// <summary>
    /// YOLO检测Base64图片，返回标准张量[bs, boxes, channels]连续float数组。
    /// </summary>
    __declspec(dllimport) bool __stdcall YoloDetectBase64Tensor(const char* imagebase64, float** outData,
        int64_t* outShape, int* outShapeLen, int64_t* outElementCount);

    /// <summary>
    /// 释放YOLO张量接口返回的outData。
    /// </summary>
    __declspec(dllimport) void __stdcall YoloFreeTensor(float* ptr);

    /// <summary>
    /// 释放YOLO模型。
    /// </summary>
    __declspec(dllimport) int __stdcall YoloFreeEngine();

    __declspec(dllimport) char* __stdcall GetError();
    __declspec(dllimport) void __stdcall FreeResultBuffer(void* ptr);
}
