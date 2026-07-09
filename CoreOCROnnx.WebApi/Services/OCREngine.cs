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

using Microsoft.AspNetCore.Cors.Infrastructure;
using System;
using CoreOCROnnx.SDK;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using System.IO;

namespace CoreOCROnnx.WebApi.Services
{
    /// <summary>
    /// OCR引擎依赖注入
    /// </summary>
    public class OCREngine
    {
        private readonly IOCRService _ocrService;
        private readonly OCRConfig _ocrConfig;
        private bool _licenseActivated;
        public IOCRService OcrService => _ocrService;

        public OCREngine(IOCRService ocrService, OCRConfig ocrConfig)
        {
            _ocrService = ocrService;
            _ocrConfig = ocrConfig;
            GetOCREngine();
        }

        /// <summary>
        /// 初始化前自动激活授权文件
        /// </summary>
        /// <returns></returns>
        public bool ActivateLicenseIfExists()
        {
            if (_licenseActivated)
            {
                return true;
            }

            string licensePath = ResolvePath(_ocrConfig.OCRLicense);
            if (string.IsNullOrWhiteSpace(licensePath) || !File.Exists(licensePath))
            {
                return false;
            }

            Console.WriteLine($"licensePath:{licensePath}");
            _licenseActivated = _ocrService.ActivateLicense(licensePath);
            Console.WriteLine(_licenseActivated ? "License activated!" : "License activation failed!");
            return _licenseActivated;
        }

        /// <summary>
        /// GPU初始化前自动激活授权文件，CPU模式不强制授权。
        /// </summary>
        /// <returns></returns>
        public bool ActivateGpuLicenseIfExists()
        {
            return !_ocrConfig.use_gpu || ActivateLicenseIfExists();
        }

        private static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppContext.BaseDirectory, path);
        }

        /// <summary>
        /// 初始化OCR引擎
        /// </summary>
        /// <returns></returns>
        public string GetOCREngine()
        {
            //自带轻量版中英文模型V4模型
            InitParamater para=new InitParamater();
            //string root = AppDomain.CurrentDomain.BaseDirectory;
            string root = AppContext.BaseDirectory;
            string modelPathroot = Path.Combine(root, "models");
            para.det_infer = Path.Combine(modelPathroot, _ocrConfig.det_infer);
            para.cls_infer = Path.Combine(modelPathroot, _ocrConfig.cls_infer);
            para.rec_infer = Path.Combine(modelPathroot, _ocrConfig.rec_infer);
            para.keyFile = Path.Combine(modelPathroot, _ocrConfig.keyFile);
            OCRParameter oCRParameter = OCRParameter.CreateDefault();
            oCRParameter.use_gpu = _ocrConfig.use_gpu;
            oCRParameter.gpu_id = _ocrConfig.gpu_id;
            oCRParameter.gpu_mem = _ocrConfig.gpu_mem;
            oCRParameter.cpu_mem = _ocrConfig.cpu_mem;
            oCRParameter.cpu_threads = _ocrConfig.cpu_threads;//提升CPU速度，优化此参数
            oCRParameter.padding = _ocrConfig.padding; //图像预处理，在图片外周添加白边，用于提升识别率，文字框没有正确框住所有文字时，增加此值。
            oCRParameter.maxSideLen = _ocrConfig.maxSideLen; //按图片最长边的长度，此值为0代表不缩放，例：1024，如果图片长边大于1024则把图像整
            oCRParameter.boxScoreThresh = _ocrConfig.boxScoreThresh; //文字框置信度门限，文字框没有正确框住所有文字时，减小此值。
            oCRParameter.boxThresh = _ocrConfig.boxThresh; //文字框置信度门限，文字框没有正确框住所有文字时，减小此值。
            oCRParameter.unClipRatio = _ocrConfig.unClipRatio; //单个文字框大小倍率，越大时单个文字框越大。此项与图片的大小相关，越大的图片此值应该越大。
            oCRParameter.doAngle = _ocrConfig.doAngle;  // 只有图片倒置的情况下(旋转90~270度的图片)，才需要启用文字方向检测。
            oCRParameter.mostAngle = _ocrConfig.mostAngle; //启用(1) / 禁用(0) 角度投票(整张图片以最大可能文字方向来识别)，当禁用文字方向检测时，此项也不起作用。
            oCRParameter.visualize = _ocrConfig.visualize; //是否对结果进行可视化
            oCRParameter.enable_log = _ocrConfig.enable_log;//是否输出日志
            oCRParameter.isOutputConsole = _ocrConfig.isOutputConsole;//是否输出日志

            para.ocrpara = oCRParameter;
            para.paraType = EnumParaType.Class;
            //string ocrJson = "{\"cpu_mem\":0,\"cpu_threads\":10,\"use_gpu\":false,\"gpu_id\":-1,\"gpu_mem\":4000,\"padding\":50,\"maxSideLen\":1024,\"boxScoreThresh\":0.5,\"boxThresh\":0.3,\"unClipRatio\":1.6,\"doAngle\":true,\"mostAngle\":true,\"visualize\":false,\"enable_log\":false,\"isOutputConsole\":true}";
            //初始化通用文字引擎
            string msg = "";
            try
            {
                if (!ActivateGpuLicenseIfExists())
                {
                    return "授权文件未激活，无法初始化GPU模式";
                }

                _ocrService.Init(para);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return msg;
        }
    }
}
