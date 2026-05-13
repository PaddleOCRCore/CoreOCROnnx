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
using System.Runtime.InteropServices;
using System.Text;

namespace CoreOCROnnx.SDK
{
    public enum EnumParaType
    {
        /// <summary>
        /// 实体参数类型
        /// </summary>
        Class,
        /// <summary>
        /// Json类型
        /// </summary>
        Json,
    }
    public class InitParamater
    {
        /// <summary>
        /// det_infer模型路径
        /// </summary>
        public string det_infer { get; set; }
        /// <summary>
        /// cls_infer模型路径
        /// </summary>
        public string cls_infer { get; set; }
        /// <summary>
        /// rec_infer模型路径
        /// </summary>
        public string rec_infer { get; set; }
        /// <summary>
        /// ppocr_keys.txt文件名全路径
        /// </summary>
        public string keyFile { get; set; }
        /// <summary>
        /// 参数类型
        /// </summary>
        public EnumParaType paraType { get; set; }
        /// <summary>
        /// 参数列表
        /// </summary>
        public OCRParameter ocrpara { get; set; }
        /// <summary>
        /// json字符串
        /// </summary>
        public string json { get; set; }
    }
    /// <summary>
    /// OCR识别参数，OCRParameter类的属性定义顺序不可随便更改，与PdddleOCROnnx.dll传入参数保持一致
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class OCRParameter
    {
        /// <summary>
        /// CPU内存占用上限，单位MB。-1表示不限制，达到上限将自动回收
        /// </summary>
        [field: MarshalAs(UnmanagedType.I4)]
        public int cpu_mem { get; set; } = 2000;
        /// <summary>
        /// CPU预测时的线程数，在机器核数充足的情况下，该值越大，预测速度越快，默认10
        /// </summary>
        public int cpu_threads { get; set; } = 10;
        /// <summary>
        /// 是否使用GPU
        /// </summary>
        [field: MarshalAs(UnmanagedType.I1)]
        public bool use_gpu { get; set; } = false;
        /// <summary>
        /// GPU id，使用GPU时有效
        /// </summary>
        [field: MarshalAs(UnmanagedType.I4)]
        public int gpu_id { get; set; } = 0;
        /// <summary>
        /// 使用GPU时内存
        /// </summary>
        [field: MarshalAs(UnmanagedType.I4)]
        public int gpu_mem { get; set; } = 4000;
        /// <summary>
        /// 图像预处理，在图片外周添加白边，用于提升识别率，文字框没有正确框住所有文字时，增加此值。
        /// </summary>
        public int padding { get; set; } = 20;
        /// <summary>
        /// 按图片最长边的长度，此值为0代表不缩放，例：1024，如果图片长边大于1024则把图像整
        /// </summary>
        public int maxSideLen { get; set; } = 1024;
        /// <summary>
        /// 文字框置信度门限，文字框没有正确框住所有文字时，减小此值。
        /// </summary>
        public float boxScoreThresh { get; set; } = 0.5f;
        /// <summary>
        /// 请自行试验
        /// </summary>
        public float boxThresh { get; set; } = 0.3f;
        /// <summary>
        /// 单个文字框大小倍率，越大时单个文字框越大。此项与图片的大小相关，越大的图片此值应该越大。
        /// </summary>
        public float unClipRatio { get; set; } = 1.6f;
        /// <summary>
        /// 只有图片倒置的情况下(旋转90~270度的图片)，才需要启用文字方向检测。
        /// </summary>
        [field: MarshalAs(UnmanagedType.I1)]
        public bool doAngle { get; set; } = true;
        /// <summary>
        /// 启用(1) / 禁用(0) 角度投票(整张图片以最大可能文字方向来识别)，当禁用文字方向检测时，此项也不起作用。
        /// </summary>
        [field: MarshalAs(UnmanagedType.I1)]
        public bool mostAngle { get; set; } = true;
        /// <summary>
        /// 是否对结果进行可视化
        /// </summary>

        [field: MarshalAs(UnmanagedType.I1)]
        public bool visualize { get; set; } = false;
        /// <summary>
        /// 是否输出到文件日志，在log目录下
        /// </summary>
        [field: MarshalAs(UnmanagedType.I1)]
        public bool enable_log { get; set; } = false;
        /// <summary>
        /// 是否输出到控制台日志
        /// </summary>
        [field: MarshalAs(UnmanagedType.I1)]
        public bool isOutputConsole { get; set; } = true;
    }
}
