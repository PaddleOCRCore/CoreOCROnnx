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
using System.Data.Common;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreOCROnnx.SDK
{
    public class OCRService : IOCRService
    {
        /// <summary>
        /// 初始化OCR引擎默认V4模型，使用CPU及mkldnn
        /// </summary>
        /// <param name="modelsPath"></param>
        /// <returns></returns>
        public string InitDefaultOCREngine(string modelsPath)
        {
            string det_infer = "ch_PP-OCRv5_mobile_det.onnx";//OCR检测模型
            string rec_infer = "ch_PP-OCRv5_rec_mobile_infer.onnx";//OCR识别模型
            string cls_infer = "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx";
            string keys = "ppocrv5_dict.txt";
            bool use_gpu = false;//是否使用GPU
            int cpu_mem = 0;//CPU内存占用上限，单位MB。-1表示不限制，达到上限将自动回收
            int gpu_id = 0;//GPUId
            int cpu_threads = 30; //CPU预测时的线程数
            InitParamater para = new InitParamater();
            para.det_infer = Path.Combine(modelsPath, det_infer);
            para.cls_infer = Path.Combine(modelsPath, cls_infer);
            para.rec_infer = Path.Combine(modelsPath, rec_infer);
            para.keyFile = Path.Combine(modelsPath, keys);

            OCRParameter oCRParameter = OCRParameter.CreateDefault();
            oCRParameter.use_gpu = use_gpu;
            oCRParameter.gpu_id = gpu_id;
            oCRParameter.gpu_mem = 4000;
            oCRParameter.cpu_mem = cpu_mem;
            oCRParameter.cpu_threads = cpu_threads;//提升CPU速度，优化此参数
            oCRParameter.padding = 10; //图像预处理，在图片外周添加白边，用于提升识别率，文字框没有正确框住所有文字时，增加此值。
            oCRParameter.maxSideLen = 512; //按图片最长边的长度，此值为0代表不缩放，例：1024，如果图片长边大于1024则把图像整
            oCRParameter.boxScoreThresh = 0.5f; //文字框置信度门限，文字框没有正确框住所有文字时，减小此值。
            oCRParameter.boxThresh = 0.3f; //文字框置信度门限，文字框没有正确框住所有文字时，减小此值。
            oCRParameter.unClipRatio = 1.6f; //单个文字框大小倍率，越大时单个文字框越大。此项与图片的大小相关，越大的图片此值应该越大。
            oCRParameter.doAngle = true;  // 只有图片倒置的情况下(旋转90~270度的图片)，才需要启用文字方向检测。
            oCRParameter.mostAngle = true; //启用(1) / 禁用(0) 角度投票(整张图片以最大可能文字方向来识别)，当禁用文字方向检测时，此项也不起作用。
            oCRParameter.visualize = false; //是否对结果进行可视化
            oCRParameter.enable_log = false;//是否输出控制台日志
            para.ocrpara = oCRParameter;
            para.paraType = EnumParaType.Class;
            string msg = "OCR初始化成功";
            try
            {
                Init(para);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return msg;
        }
        /// <summary>
        /// 初始化OCR引擎
        /// </summary>
        /// <param name="para"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Init(InitParamater para)
        {
            try
            {
                bool ret;
                if (para.paraType == EnumParaType.Class)
                {
                    ret = OCRSDK.Init(para.det_infer, para.cls_infer, para.rec_infer, para.keyFile, para.ocrpara);
                }
                else if (para.paraType == EnumParaType.Json)
                {
                    ret = OCRSDK.Initjson(para.det_infer, para.cls_infer, para.rec_infer, para.keyFile, para.json);
                }
                else
                {
                    throw new OCRException("不支持的参数类型");
                }

                if (!ret)
                {
                    var error = GetError();
                    throw new OCRException($"初始化失败: {error}");
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new OCRException($"初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取当前机器的加密授权申请码
        /// </summary>
        /// <returns></returns>
        public string GetLicenseRequestCode()
        {
            return OcrServiceHelper.ReadNativeString(OCRSDK.GetLicenseRequestCode, OCRSDK.FreeResultBuffer);
        }

        /// <summary>
        /// 激活授权文件
        /// </summary>
        /// <param name="licenseFile">授权文件路径</param>
        /// <returns></returns>
        public bool ActivateLicense(string licenseFile)
        {
            if (string.IsNullOrWhiteSpace(licenseFile))
            {
                return false;
            }

            return OCRSDK.ActivateLicense(licenseFile);
        }

        /// <summary>
        /// 获取当前授权状态JSON
        /// </summary>
        /// <returns></returns>
        public string GetLicenseStatus()
        {
            return OcrServiceHelper.ReadNativeString(OCRSDK.GetLicenseStatus, OCRSDK.FreeResultBuffer);
        }

        /// <summary>
        /// 获取当前授权状态对象
        /// </summary>
        /// <returns></returns>
        public LicenseStatus GetLicenseStatusInfo()
        {
            string json = GetLicenseStatus();
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return OcrServiceHelper.DeserializeLicenseStatus(json);
        }

        /// <summary>
        /// 对图像文件进行文本识别
        /// </summary>
        /// <param name="imagefile">图像文件</param>
        /// <returns>OCR识别结果</returns>
        public OCRResult Detect(string imagefile)
        {
            var ptrResult = OCRSDK.Detect(imagefile);
            return GetResult(ptrResult);
        }
        /// <summary>
        /// 对图像文件进行文本识别
        /// </summary>
        /// <param name="imagebyte">图像文件</param>
        /// <returns>OCR识别结果</returns>
        public OCRResult Detect(byte[] imagebyte)
        {
            try
            {
                var ptrResult = OCRSDK.DetectByte(imagebyte, imagebyte.LongLength);
                return GetResult(ptrResult);
            }
            catch (Exception ex) {
                throw ex;
            }

        }
        /// <summary>
        /// 对Mat进行文本识别
        /// </summary>
        /// <param name="ptr_cvmat">Mat</param>
        /// <returns>OCR识别结果</returns>
        public OCRResult DetectMat(IntPtr ptr_cvmat)
        {
            try
            {
                var ptrResult = OCRSDK.DetectMat(ptr_cvmat);
                return GetResult(ptrResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public OCRResult DetectBase64(string base64)
        {
            try
            {
                var ptrResult = OCRSDK.DetectBase64(base64);
                return GetResult(ptrResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 初始化YOLO模型
        /// </summary>
        /// <param name="modelPath">YOLO ONNX模型路径</param>
        /// <param name="parameterJson">YOLO初始化参数JSON</param>
        /// <returns>初始化成功返回true，失败抛出OCRException</returns>
        public bool YoloInitJson(string modelPath, string parameterJson)
        {
            try
            {
                bool ret = OCRSDK.YoloInitJson(modelPath, parameterJson);
                if (!ret)
                {
                    var error = GetError();
                    throw new OCRException($"YOLO初始化失败: {error}");
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new OCRException($"YOLO初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// YOLO检测图片文件，返回YOLO JSON v2字符串
        /// </summary>
        /// <param name="imagefile">图像文件</param>
        /// <returns>YOLO JSON结果</returns>
        public string YoloDetect(string imagefile)
        {
            var ptrResult = OCRSDK.YoloDetect(imagefile);
            return GetNativeStringResult(ptrResult, "YOLO检测");
        }

        /// <summary>
        /// YOLO检测图片字节，返回YOLO JSON v2字符串
        /// </summary>
        /// <param name="imagebyte">图像字节</param>
        /// <returns>YOLO JSON结果</returns>
        public string YoloDetect(byte[] imagebyte)
        {
            if (imagebyte == null)
            {
                throw new OCRException("YOLO检测失败: imagebyte不能为空");
            }
            var ptrResult = OCRSDK.YoloDetectByte(imagebyte, imagebyte.LongLength);
            return GetNativeStringResult(ptrResult, "YOLO检测");
        }

        /// <summary>
        /// YOLO检测Mat，返回YOLO JSON v2字符串
        /// </summary>
        /// <param name="ptr_cvmat">Mat指针</param>
        /// <returns>YOLO JSON结果</returns>
        public string YoloDetectMat(IntPtr ptr_cvmat)
        {
            var ptrResult = OCRSDK.YoloDetectMat(ptr_cvmat);
            return GetNativeStringResult(ptrResult, "YOLO检测");
        }

        /// <summary>
        /// YOLO检测Base64图片，返回YOLO JSON v2字符串
        /// </summary>
        /// <param name="base64">Base64图片</param>
        /// <returns>YOLO JSON结果</returns>
        public string YoloDetectBase64(string base64)
        {
            var ptrResult = OCRSDK.YoloDetectBase64(base64);
            return GetNativeStringResult(ptrResult, "YOLO检测");
        }

        /// <summary>
        /// YOLO检测图片文件，返回标准张量[bs, boxes, channels]。
        /// </summary>
        /// <param name="imagefile">图像文件</param>
        /// <returns>YOLO张量结果</returns>
        public YoloTensorResult YoloDetectTensor(string imagefile)
        {
            IntPtr dataPtr;
            long[] shape = new long[3];
            int shapeLen;
            long elementCount;
            bool ok = OCRSDK.YoloDetectTensor(imagefile, out dataPtr, shape, out shapeLen, out elementCount);
            return OcrServiceHelper.GetYoloTensorResult(ok, dataPtr, shape, shapeLen, elementCount,
                GetError, OCRSDK.YoloFreeTensor, "YOLO张量检测", message => new OCRException(message));
        }

        /// <summary>
        /// YOLO检测图片字节，返回标准张量[bs, boxes, channels]。
        /// </summary>
        /// <param name="imagebyte">图像字节</param>
        /// <returns>YOLO张量结果</returns>
        public YoloTensorResult YoloDetectByteTensor(byte[] imagebyte)
        {
            if (imagebyte == null)
            {
                throw new OCRException("YOLO张量检测失败: imagebyte不能为空");
            }
            IntPtr dataPtr;
            long[] shape = new long[3];
            int shapeLen;
            long elementCount;
            bool ok = OCRSDK.YoloDetectByteTensor(imagebyte, imagebyte.LongLength, out dataPtr, shape, out shapeLen, out elementCount);
            return OcrServiceHelper.GetYoloTensorResult(ok, dataPtr, shape, shapeLen, elementCount,
                GetError, OCRSDK.YoloFreeTensor, "YOLO张量检测", message => new OCRException(message));
        }

        /// <summary>
        /// YOLO检测Mat，返回标准张量[bs, boxes, channels]。
        /// </summary>
        /// <param name="ptr_cvmat">Mat指针</param>
        /// <returns>YOLO张量结果</returns>
        public YoloTensorResult YoloDetectMatTensor(IntPtr ptr_cvmat)
        {
            IntPtr dataPtr;
            long[] shape = new long[3];
            int shapeLen;
            long elementCount;
            bool ok = OCRSDK.YoloDetectMatTensor(ptr_cvmat, out dataPtr, shape, out shapeLen, out elementCount);
            return OcrServiceHelper.GetYoloTensorResult(ok, dataPtr, shape, shapeLen, elementCount,
                GetError, OCRSDK.YoloFreeTensor, "YOLO张量检测", message => new OCRException(message));
        }

        /// <summary>
        /// YOLO检测Base64图片，返回标准张量[bs, boxes, channels]。
        /// </summary>
        /// <param name="base64">Base64图片</param>
        /// <returns>YOLO张量结果</returns>
        public YoloTensorResult YoloDetectBase64Tensor(string base64)
        {
            IntPtr dataPtr;
            long[] shape = new long[3];
            int shapeLen;
            long elementCount;
            bool ok = OCRSDK.YoloDetectBase64Tensor(base64, out dataPtr, shape, out shapeLen, out elementCount);
            return OcrServiceHelper.GetYoloTensorResult(ok, dataPtr, shape, shapeLen, elementCount,
                GetError, OCRSDK.YoloFreeTensor, "YOLO张量检测", message => new OCRException(message));
        }

        /// <summary>
        /// 释放YOLO模型
        /// </summary>
        /// <returns>错误信息，成功为空字符串</returns>
        public string YoloFreeEngine()
        {
            string lastErr = "";
            try
            {
                OCRSDK.YoloFreeEngine();
            }
            catch (Exception e)
            {
                lastErr = e.Message;
            }
            return lastErr;
        }

        private OCRResult GetResult(IntPtr ptrResult)
        {
            OCRResult result = new OCRResult();
            if (ptrResult == IntPtr.Zero)
            {
                var lastErr = GetError();
                if (!string.IsNullOrEmpty(lastErr))
                {
                    throw new OCRException("OCR内部错误：" + lastErr);
                }
                return result;
            }
            string json = string.Empty;
            try
            {
                json = MarshalUtf8.PtrToStringUTF8(ptrResult);
                try
                {
                    result = OcrServiceHelper.DeserializeObject<OCRResult>(json);
                    result.JsonText = json;
                }
                catch (Exception e)
                {
                    result.StrRes = json + e.Message;
                }
            }
            catch (Exception ex)
            {
                throw new OCRException("OCR结果Json反序列化失败:" + ex.Message);
            }
            finally
            {
                if (ptrResult != IntPtr.Zero)
                {
                    OCRSDK.FreeResultBuffer(ptrResult);
                }
            }
            return result;
        }

        private string GetNativeStringResult(IntPtr ptrResult, string operationName)
        {
            return OcrServiceHelper.GetNativeStringResult(ptrResult, GetError, OCRSDK.FreeResultBuffer,
                operationName, message => new OCRException(message));
        }

        /// <summary>
        /// 获取错误原因
        /// </summary>
        /// <returns></returns>
        public string GetError()
        {
            string lastErr = "";
            try
            {
                var ret = OCRSDK.GetError();
                if (ret != IntPtr.Zero)
                {
                    lastErr = MarshalUtf8.PtrToStringUTF8(ret);
                    OCRSDK.FreeResultBuffer(ret);
                }
            }
            catch (Exception e)
            {
                lastErr = e.Message;
            }
            return lastErr;
        }

        /// <summary>
        /// 释放OCR引擎
        /// </summary>
        /// <returns></returns>
        public string FreeEngine()
        {
            string lastErr = "";
            try
            {
                var ret = OCRSDK.FreeEngine();
            }
            catch (Exception e)
            {
                lastErr = e.Message;
            }
            return lastErr;
        }
    }
}
