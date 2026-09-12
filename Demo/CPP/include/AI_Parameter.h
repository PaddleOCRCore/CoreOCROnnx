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
#pragma pack(push,1)
#include <vector>
using namespace std;

/// <summary>
/// PaddleOCROnnx.dll C++识别参数，不可修改顺序
/// </summary>

struct OCRParameter {
	int cpu_mem = 0; //CPU内存占用上限，单位MB。-1表示不限制
	int cpu_threads = 8; //CPU预测时的线程数，在机器核数充足的情况下，该值越大，预测速度越快，默认10

	bool use_gpu = false; //是否使用GPU
	int gpu_id = -1;         //GPU id，使用GPU时有效
	int gpu_mem = 4000;   //使用GPU时内存 
	int padding = 50; //图像预处理，在图片外周添加白边，用于提升识别率，文字框没有正确框住所有文字时，增加此值。
	int maxSideLen = 1024; //按图片最长边的长度，此值为0代表不缩放，例：1024，如果图片长边大于1024则把图像整
	float boxScoreThresh = 0.5f; //文字框置信度门限，文字框没有正确框住所有文字时，减小此值。
	float boxThresh = 0.3f; //自行实验
	float unClipRatio = 1.6f; //单个文字框大小倍率，越大时单个文字框越大。此项与图片的大小相关，越大的图片此值应该越大。
	bool doAngle = true;  // 只有图片倒置的情况下(旋转90~270度的图片)，才需要启用文字方向检测。
	bool mostAngle = true; //启用(1) / 禁用(0) 角度投票(整张图片以最大可能文字方向来识别)，当禁用文字方向检测时，此项也不起作用。
	bool visualize = false; //是否对结果进行可视化，为true时，预测结果会保存在output文件夹下和输入图像同名的图像上。
	bool enable_log = false; //是否输出到文件日志，在log目录下
	bool isOutputConsole = true;//是否输出到控制台日志
};
#pragma pack(pop)  
