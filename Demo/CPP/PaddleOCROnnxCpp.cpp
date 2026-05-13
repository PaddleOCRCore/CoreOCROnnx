#include <iostream>
#include <string.h>
#include <windows.h>
#include <PaddleOCROnnx.h>
#include <locale>
#include <codecvt>
#include <string>
#include <filesystem>
#include <vector>
#include <chrono>
using namespace std;

void GetFileList(string directoryPath, vector<string>& files)
{
    WIN32_FIND_DATA ffd;
    HANDLE hFind = INVALID_HANDLE_VALUE;
    string pt;
    // 打开目录句柄
    hFind = FindFirstFile(pt.assign(directoryPath).append("/*").c_str(), &ffd);
    if (INVALID_HANDLE_VALUE == hFind)
    {
        cerr << "FindFirstFile failed (" << GetLastError() << ")" << endl;
    }
    // 遍历目录
    do
    {
        if (ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
        {
            // 跳过"."和".."目录
            if (strcmp(ffd.cFileName, ".") != 0 && strcmp(ffd.cFileName, "..") != 0)
            {
                GetFileList(pt.assign(directoryPath).append("\\").append(ffd.cFileName), files);
            }
            else
            {
                continue;
            }
        }
        else
        {
            files.push_back(pt.assign(directoryPath).append("\\") + ffd.cFileName);
        }
    } while (FindNextFile(hFind, &ffd) != 0);
    // 关闭目录句柄
    FindClose(hFind);
}
int main()
{
    SetConsoleOutputCP(CP_UTF8);//解决控制台中文乱码，使用UTF-8编码
    char path[MAX_PATH];
    GetCurrentDirectoryA(MAX_PATH, path);
    string det_infer(path);
    //请将PaddleOCRRuntime下面所有文件复制到c++的生成Release运行目录
    det_infer += "/models/ch_PP-OCRv5_mobile_det.onnx";
    string rec_infer(path);
    rec_infer += "/models/ch_PP-OCRv5_rec_mobile_infer.onnx";
    string cls_infer(path);
    cls_infer += "/models/ch_ppocr_mobile_v2.0_cls_infer.onnx";
    string keys(path);
    keys += "/models/ppocrv5_dict.txt";
    OCRParameter parameter;
    parameter.use_gpu = false;//是否使用GPU
    parameter.cpu_threads = 30;//CPU预测时的线程数，在机器核数充足的情况下，该值越大，预测速度越快，默认10
    parameter.cpu_mem = 0;//CPU内存占用上限，单位MB。 - 1表示不限制
    parameter.padding = 10; //图像预处理，在图片外周添加白边，用于提升识别率，文字框没有正确框住所有文字时，增加此值。
    parameter.maxSideLen = 512; //按图片最长边的长度，此值为0代表不缩放，例：1024，如果图片长边大于1024则把图像整
    parameter.boxScoreThresh = 0.5f; //文字框置信度门限，文字框没有正确框住所有文字时，减小此值。
    parameter.boxThresh = 0.3f; //自行实验。
    parameter.unClipRatio = 1.6f; //单个文字框大小倍率，越大时单个文字框越大。此项与图片的大小相关，越大的图片此值应该越大。
    parameter.doAngle = true;  // 只有图片倒置的情况下(旋转90~270度的图片)，才需要启用文字方向检测。
    parameter.mostAngle = true; //启用(1) / 禁用(0) 角度投票(整张图片以最大可能文字方向来识别)，当禁用文字方向检测时，此项也不起作用。
    parameter.visualize = false; //是否对结果进行可视化
    parameter.enable_log = false;//是否输出控制台日志

    string imagespath(path);
    imagespath += "\\images";//请将图片放至此目录
    vector<string> images;
    GetFileList(imagespath, images);

    Init(const_cast<char*>(det_infer.c_str()), const_cast<char*>(cls_infer.c_str()), 
        const_cast<char*>(rec_infer.c_str()), const_cast<char*>(keys.c_str()), parameter);
    for (const auto image : images) 
    {
        for (int i = 0; i < 10; i++) {//模拟单张图片循环识别
            cout << "images:" << image << endl;
            auto	starttime = chrono::steady_clock::now();
            cv::Mat imgMat = cv::imread(const_cast<char*>(image.c_str()), cv::IMREAD_COLOR);
            const char* result = DetectMat(imgMat);
            string cstr = result ? result : "";
            FreeResultBuffer((void*)result);
            //string cstr = Detect(const_cast<char*>(image.c_str()));
            auto	endtime = chrono::steady_clock::now();
            auto duration = chrono::duration_cast<chrono::milliseconds>(endtime - starttime);
            std::cout << "Detect:" << duration.count() << "ms" << endl;
            cout << cstr << endl;
        }
    }
    try
    {
        FreeEngine();
    }
    catch (const exception& e)
    {
        wcout << e.what();
    }
    system("pause");
    return 0;
}
