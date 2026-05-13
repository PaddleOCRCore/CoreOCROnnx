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
using Newtonsoft.Json;

namespace CoreOCROnnx.SDK
{
    /// <summary>
    /// 跨版本 Marshal.UTF8 工具类
    /// .NET Framework 没有 PtrToStringUTF8，这里统一提供。
    /// </summary>
    public static class MarshalUtf8
    {
        /// <summary>
        /// 将 UTF-8 零结尾字节序列转换为托管字符串。
        /// 支持 .NET Framework 2.0+ / .NET Core 1.x+ / .NET 5+
        /// </summary>
        public static string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return null;

            // .NET 5+ 原生支持 PtrToStringUTF8
#if NET5_0_OR_GREATER
        return Marshal.PtrToStringUTF8(ptr);
#else
            return PtrToStringUTF8_Manual(ptr);
#endif
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        /// .NET Framework / Core 的手动实现
        /// </summary>
        private static string PtrToStringUTF8_Manual(IntPtr ptr)
        {
            // 1. 计算长度（到零字节为止）
            int len = 0;
            while (Marshal.ReadByte(ptr, len) != 0) len++;

            // 2. 复制到托管数组
            byte[] bytes = new byte[len];
            Marshal.Copy(ptr, bytes, 0, len);

            // 3. UTF-8 解码
            return Encoding.UTF8.GetString(bytes);
        }
#endif
    }
    public class OCRException : Exception
    {
        public OCRException(string message) : base(message)
        {
        }
    }
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
            string cls_infer = "ch_ppocr_mobile_v2.0_cls_infer.onnx";
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

            OCRParameter oCRParameter = new OCRParameter();
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
                    result = DeObject<OCRResult>(json);
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
        /// <summary>
        /// Json反序列化
        /// </summary>
        /// <typeparam name="T">反序列化的目标类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <returns>反序列化后的对象</returns>
        private static T DeObject<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default(T);
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            };
            return (T)JsonConvert.DeserializeObject(json, typeof(T), settings);
        }
    }
}
