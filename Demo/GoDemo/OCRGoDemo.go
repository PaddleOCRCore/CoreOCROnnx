package main

import "C"
import (
	"bufio"
	"fmt"
	"io/ioutil"
	"os"
	"syscall"
	"time"
	"unsafe"
)

// 获取字符串的指针
func stringToPointer(s string) uintptr {
	return uintptr(unsafe.Pointer(syscall.StringBytePtr(s)))
}

// 获取当前工作目录的绝对路径
func getCurrentDirectory() string {
	dir, err := os.Getwd()
	if err != nil {
		fmt.Println("获取当前目录失败:", err)
	}
	return dir
}

func main() {
	// 加载DLL文件
	ocrDLL, err := syscall.LoadDLL("PaddleOCROnnx.dll")
	if err != nil {
		fmt.Println("加载DLL失败:", err)
		return
	}

	// 获取DLL中的函数
	initFunc, err := ocrDLL.FindProc("Initjson")
	if err != nil {
		fmt.Println("获取Initjson函数失败:", err)
		return
	}
	detectFunc, err := ocrDLL.FindProc("Detect")
	if err != nil {
		fmt.Println("获取Detect函数失败:", err)
		return
	}

	// 初始化OCR
	rootDir := getCurrentDirectory()
	initFunc.Call(
		stringToPointer(rootDir+"\\models\\ch_PP-OCRv5_mobile_det.onnx"),
		stringToPointer(rootDir+"\\models\\ch_ppocr_mobile_v2.0_cls_infer.onnx"),
		stringToPointer(rootDir+"\\models\\ch_PP-OCRv5_rec_mobile_infer.onnx"),
		stringToPointer(rootDir+"\\models\\ppocrv5_dict.txt"),
		stringToPointer("{\"returncharboxes\":false,\"cpu_mem\":0,\"cpu_threads\":10,\"use_gpu\":false,\"gpu_id\":-1,\"gpu_mem\":4000,\"padding\":50,\"maxSideLen\":1024,\"boxScoreThresh\":0.5,\"boxThresh\":0.3,\"unClipRatio\":1.6,\"doAngle\":true,\"mostAngle\":true,\"visualize\":false,\"enable_log\":false,\"isOutputConsole\":true}"),
	)

	// 读取图片目录
	imageDir := rootDir + "\\images"
	images, err := ioutil.ReadDir(imageDir)
	if err != nil {
		fmt.Println("读取图片目录失败:", err)
		return
	}
	// 处理每张图片
	for _, image := range images {
		fmt.Println("处理图片:", image.Name())
		startTime := time.Now()

		// 调用OCR检测函数
		imagePath := imageDir + "\\" + image.Name()
		cString, _, _ := detectFunc.Call(stringToPointer(imagePath))
		// 计算识别时间
		elapsedTime := time.Since(startTime)
		fmt.Printf("OCR耗时: %.2fms\n", float64(elapsedTime)/float64(time.Millisecond))

		// 将返回的char* 指针转换为Go字符串
		var result string = C.GoString((*C.char)(unsafe.Pointer(cString)))
		fmt.Println("识别结果:", result)
	}

	// 等待用户输入以退出程序
	fmt.Println("按回车键退出...")
	bufio.NewScanner(os.Stdin).Scan()
}
