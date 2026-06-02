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
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreOCROnnx.SDK
{
    public interface IOCRService
    {
        /// <summary>
        /// 初始化OCR引擎默认V4模型，使用CPU及mkldnn
        /// </summary>
        /// <param name="modelsPath"></param>
        /// <returns>返回初始化结果</returns>
        string InitDefaultOCREngine(string modelsPath);
        /// <summary>
        /// 初如化OCR
        /// </summary>
        /// <param name="para"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool Init(InitParamater para);

        /// <summary>
        /// 获取当前机器的加密授权申请码
        /// </summary>
        /// <returns>授权申请码</returns>
        string GetLicenseRequestCode();

        /// <summary>
        /// 激活授权文件
        /// </summary>
        /// <param name="licenseFile">授权文件路径</param>
        /// <returns>激活成功返回true，失败返回false</returns>
        bool ActivateLicense(string licenseFile);

        /// <summary>
        /// 获取当前授权状态JSON
        /// </summary>
        /// <returns>授权状态JSON字符串</returns>
        string GetLicenseStatus();

        /// <summary>
        /// 获取当前授权状态
        /// </summary>
        /// <returns>授权状态对象</returns>
        LicenseStatus GetLicenseStatusInfo();

        /// <summary>
        /// 对图像文件进行文本识别
        /// </summary>
        /// <param name="imagefile">图像文件</param>
        /// <returns>OCR识别结果</returns>
        OCRResult Detect(string imagefile);

        /// <summary>
        /// 对图像文件进行文本识别
        /// </summary>
        /// <param name="imagebyte">图像文件</param>
        /// <returns>OCR识别结果</returns>
        OCRResult Detect(byte[] imagebyte);
        /// <summary>
        /// 对Mat进行文本识别
        /// </summary>
        /// <param name="ptr_cvmat">Mat</param>
        /// <returns></returns>
        OCRResult DetectMat(IntPtr ptr_cvmat);
        /// <summary>
        /// 对base64图像进行识别
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        OCRResult DetectBase64(string base64);

        /// <summary>
        /// 初始化YOLO模型
        /// </summary>
        /// <param name="modelPath">YOLO ONNX模型路径</param>
        /// <param name="parameterJson">YOLO初始化参数JSON</param>
        /// <returns>初始化成功返回true</returns>
        bool YoloInitJson(string modelPath, string parameterJson);

        /// <summary>
        /// YOLO检测图片文件，返回YOLO JSON v2字符串
        /// </summary>
        /// <param name="imagefile">图像文件</param>
        /// <returns>YOLO JSON结果</returns>
        string YoloDetect(string imagefile);

        /// <summary>
        /// YOLO检测图片字节，返回YOLO JSON v2字符串
        /// </summary>
        /// <param name="imagebyte">图像字节</param>
        /// <returns>YOLO JSON结果</returns>
        string YoloDetect(byte[] imagebyte);

        /// <summary>
        /// YOLO检测Mat，返回YOLO JSON v2字符串
        /// </summary>
        /// <param name="ptr_cvmat">Mat指针</param>
        /// <returns>YOLO JSON结果</returns>
        string YoloDetectMat(IntPtr ptr_cvmat);

        /// <summary>
        /// YOLO检测Base64图片，返回YOLO JSON v2字符串
        /// </summary>
        /// <param name="base64">Base64图片</param>
        /// <returns>YOLO JSON结果</returns>
        string YoloDetectBase64(string base64);

        /// <summary>
        /// 释放YOLO模型
        /// </summary>
        /// <returns>错误信息，成功为空字符串</returns>
        string YoloFreeEngine();

        /// <summary>
        /// 获取错误原因
        /// </summary>
        /// <returns></returns>
        string GetError();
        /// <summary>
        /// 释放OCR引擎
        /// </summary>
        /// <returns></returns>
        string FreeEngine();
    }
}
