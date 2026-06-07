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
using CoreOCROnnx.SDK;

namespace WinFormsApp.Services
{
    /// <summary>
    /// OCR引擎依赖注入
    /// </summary>
    public class OCREngine
    {
        private static readonly Lazy<IOCRService> _ocrService = new Lazy<IOCRService>(() => new OCRService());
        public static IOCRService ocrService => _ocrService.Value;
        public static string det_infer = "ch_PP-OCRv5_mobile_det.onnx";//OCR检测模型
        public static string rec_infer = "ch_PP-OCRv5_rec_mobile_infer.onnx";//OCR识别模型
        public static string cls_infer = "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx";
        public static string keys = "ppocrv5_dict.txt";
        public static int cpu_threads = 30; //CPU预测时的线程数
        private static bool visualize = true;//是否对结果进行可视化，为true时，预测结果会保存在output文件夹下。

        public static bool use_gpu = false;//是否使用GPU
        public static int cpu_mem = 0;//CPU内存占用上限，单位MB。-1表示不限制，达到上限将自动回收
        public static int gpu_id = 0;//GPUId
        public static int gpu_mem = 4000;//GPU显存上限
        public static bool doAngle = true;//是否执行文字方向分类
        public static bool mostAngle = true;//是否使用方向分类器

        public static int padding = 20; //图像预处理，在图片外周添加白边，用于提升识别率，文字框没有正确框住所有文字时，增加此值。
        public static int maxSideLen = 1024; //按图片最长边的长度，此值为0代表不缩放，例：1024，如果图片长边大于1024则把图像整
        public static float boxScoreThresh = 0.5f; //文字框置信度门限，文字框没有正确框住所有文字时，减小此值。
        public static float boxThresh = 0.3f; //自行实验
        public static float unClipRatio = 1.6f; //单个文字框大小倍率，越大时单个文字框越大。此项与图片的大小相关，越大的图片此值应该越大。

        /// <summary>
        /// 初始化OCR引擎
        /// </summary>
        /// <returns></returns>
        public static string GetOCREngine()
        {
            InitParamater para = new InitParamater();
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string modelsPath = Path.Combine(root, "models");
            para.det_infer = Path.Combine(modelsPath, det_infer);
            para.cls_infer = Path.Combine(modelsPath, cls_infer);
            para.rec_infer = Path.Combine(modelsPath, rec_infer);
            para.keyFile = Path.Combine(modelsPath, keys);

            OCRParameter oCRParameter = OCRParameter.CreateDefault();
            oCRParameter.use_gpu = use_gpu;
            oCRParameter.gpu_id = gpu_id;
            oCRParameter.gpu_mem = gpu_mem;
            oCRParameter.cpu_mem = cpu_mem;
            oCRParameter.cpu_threads = cpu_threads;//提升CPU速度，优化此参数
            oCRParameter.padding = padding; //图像预处理，在图片外周添加白边，用于提升识别率，文字框没有正确框住所有文字时，增加此值。
            oCRParameter.maxSideLen = maxSideLen; //按图片最长边的长度，此值为0代表不缩放，例：1024，如果图片长边大于1024则把图像整
            oCRParameter.boxScoreThresh = boxScoreThresh; //文字框置信度门限，文字框没有正确框住所有文字时，减小此值。
            oCRParameter.boxThresh = boxThresh; //文字框置信度门限，文字框没有正确框住所有文字时，减小此值。
            oCRParameter.unClipRatio = unClipRatio; //单个文字框大小倍率，越大时单个文字框越大。此项与图片的大小相关，越大的图片此值应该越大。
            oCRParameter.doAngle = doAngle;  // 只有图片倒置的情况下(旋转90~270度的图片)，才需要启用文字方向检测。
            oCRParameter.mostAngle = mostAngle; //启用(1) / 禁用(0) 角度投票(整张图片以最大可能文字方向来识别)，当禁用文字方向检测时，此项也不起作用。
            oCRParameter.visualize = visualize; //是否对结果进行可视化
            oCRParameter.enable_log = true;//是否输出日志
            oCRParameter.isOutputConsole = false;

            para.ocrpara = oCRParameter;
            para.paraType = EnumParaType.Class;
            //初始化通用文字引擎
            string msg = "文本识别初始化成功";
            try
            {
                ocrService.Init(para);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return msg;
        }
    }
}

