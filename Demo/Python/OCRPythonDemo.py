import ctypes
import os
import time

# 获取当前工作目录的绝对路径
def get_current_directory():
    return os.getcwd()

# 加载DLL文件
root_dir = get_current_directory()
dll_path = os.path.join(root_dir, "PaddleOCROnnx.dll")
ocr_dll = ctypes.CDLL(dll_path)

# 定义DLL中的函数
init_func = ocr_dll.Initjson
detect_func = ocr_dll.Detect

# 初始化OCR
root_dir = get_current_directory()
init_func(
    ctypes.c_char_p((root_dir + "\\models\\PP-OCRv6_tiny_det.onnx").encode('utf-8')),
    ctypes.c_char_p((root_dir + "\\models\\ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx").encode('utf-8')),
    ctypes.c_char_p((root_dir + "\\models\\PP-OCRv6_tiny_rec.onnx").encode('utf-8')),
    ctypes.c_char_p((root_dir + "\\models\\ppocrv6tiny_dict.txt").encode('utf-8')),
    ctypes.c_char_p(b'{\"cpu_mem\":0,\"cpu_threads\":30,\"use_gpu\":false,\"gpu_id\":-1,\"gpu_mem\":4000,\"padding\":50,\"maxSideLen\":1024,\"boxScoreThresh\":0.5,\"boxThresh\":0.3,\"unClipRatio\":1.6,\"doAngle\":true,\"mostAngle\":true,\"visualize\":false,\"enable_log\":false,\"isOutputConsole\":true}')
)


# 读取图片目录
image_dir = root_dir + "\\images"
images = os.listdir(image_dir)

# 处理每张图片
for image_name in images:
    print("处理图片:", image_name)
    start_time = time.time()

    # 调用OCR检测函数
    image_path = image_dir + "\\" + image_name
    detect_func.restype = ctypes.c_char_p
    c_string = detect_func(ctypes.c_char_p(image_path.encode('utf-8'))).decode('utf-8')
    # 计算识别时间
    elapsed_time = time.time() - start_time
    print(f"OCR耗时: {elapsed_time * 1000:.2f}ms")
    # 检查返回值是否为空指针
    if c_string:
        print("识别结果:", c_string)
    else:
        print("识别失败，返回空指针。")

# 等待用户输入以退出程序
input("按回车键退出...")