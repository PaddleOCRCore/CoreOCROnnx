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
#include <string>
#include <opencv2/opencv.hpp>//使用OpenCV4.10
#include <include/AI_Parameter.h>
#pragma comment (lib,"PaddleOCROnnx.lib")
#pragma once

extern "C" {
    __declspec(dllimport) bool __stdcall Init(const char* det_infer, const char* cls_infer,
        const char* rec_infer, const char* keys,
        const OCRParameter parameter);
    __declspec(dllimport) bool __stdcall Initjson(const char* det_infer, const char* cls_infer,
        const char* rec_infer, const char* keys,
        const char* parameterjson);
    __declspec(dllimport) const char* __stdcall Detect(const char* imageFile);
    __declspec(dllimport) const char* __stdcall DetectMat(const cv::Mat& cvmat);
    __declspec(dllimport) const char* __stdcall DetectByte(const unsigned char* imagebytedata,
        size_t size);
    __declspec(dllimport) const char* __stdcall DetectBase64(const char* imagebase64);
    __declspec(dllimport) int __stdcall FreeEngine();
    __declspec(dllimport) char* __stdcall GetError();
    __declspec(dllimport) void __stdcall FreeResultBuffer(void* ptr);
}
