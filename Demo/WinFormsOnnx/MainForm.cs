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

using CoreOCROnnx.SDK;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WinFormsApp.Services;
using WinFormsApp.Utils;

namespace WinFormsApp
{
    public partial class MainForm : Form
    {
        StringBuilder message = new StringBuilder();
        private readonly IOCRService ocrService;
        private bool yoloInitialized = false;
        private string yoloModelPath = "";
        public static bool use_gpu = false;//是否使用GPU
        public static int gpu_id = 0;//GPUId
        public static int cpu_threads = 30; //CPU预测时的线程数
        public static int cpu_mem = 4000;//CPU内存占用上限，单位MB。-1表示不限制，达到上限将自动回收
        public static string RecFilepath = "";
        public static bool outPutJson = false;//是否输出JSON
        public static int recCount = 1; //OCR识别时同一张图片模拟调用接口次数
        public static int model_type = 0;//模型类型：01是V6，1是V5，2是V4
        private bool isOCRBusy;
        public MainForm()
        {
            InitializeComponent();
            ocrService = OCREngine.ocrService;
        }

        private void ToolStripMenuItemGetLicenseRequestCode_Click(object? sender, EventArgs e)
        {
            try
            {
                textBoxResult.Clear();
                string code = ocrService.GetLicenseRequestCode();
                LogMessage($"{DateTime.Now:HH:mm:ss.fff}:授权申请码");
                LogMessage(code);
            }
            catch (Exception ex)
            {
                LogMessage($"{DateTime.Now:HH:mm:ss.fff}:生成授权申请码失败: {ex.Message}");
                string error = ocrService.GetError();
                if (!string.IsNullOrWhiteSpace(error))
                {
                    LogMessage($"DLL错误信息: {error}");
                }
            }
        }

        private void ToolStripMenuItemApplyGPUTrial_Click(object? sender, EventArgs e)
        {
            try
            {
                textBoxResult.Clear();
                string licensePath = OCREngine.ResolveLicensePath();
                LogMessage($"{DateTime.Now:HH:mm:ss.fff}:尝试激活授权文件: {licensePath}");
                if (string.IsNullOrWhiteSpace(licensePath) || !File.Exists(licensePath))
                {
                    LogMessage($"{DateTime.Now:HH:mm:ss.fff}:未找到默认授权文件，无法免费试用GPU。请将授权文件放到 models/paddleocr.lic");
                    return;
                }

                bool activated = ocrService.ActivateLicense(licensePath);
                LogMessage(activated ? $"{DateTime.Now:HH:mm:ss.fff}:授权文件激活成功" : $"{DateTime.Now:HH:mm:ss.fff}:授权文件激活失败");
            }
            catch (Exception ex)
            {
                LogMessage($"{DateTime.Now:HH:mm:ss.fff}:免费试用GPU失败: {ex.Message}");
                string error = ocrService.GetError();
                if (!string.IsNullOrWhiteSpace(error))
                {
                    LogMessage($"DLL错误信息: {error}");
                }
            }
        }

        private void ToolStripMenuItemCheckLicense_Click(object? sender, EventArgs e)
        {
            try
            {
                textBoxResult.Clear();
                string licensePath = OCREngine.ResolveLicensePath();
                bool licenseFileActivated = false;
                if (!string.IsNullOrWhiteSpace(licensePath) && File.Exists(licensePath))
                {
                    licenseFileActivated = ocrService.ActivateLicense(licensePath);
                }

                LicenseStatus? status = ocrService.GetLicenseStatusInfo();
                if (status == null)
                {
                    string error = ocrService.GetError();
                    LogMessage($"{DateTime.Now:HH:mm:ss.fff}: 未获取到授权状态。{error}");
                    return;
                }

                LogMessage(BuildLicenseStatusText(status, licenseFileActivated));
            }
            catch (Exception ex)
            {
                LogMessage($"{DateTime.Now:HH:mm:ss.fff}: 查看GPU授权失败: {ex.Message}");
                string error = ocrService.GetError();
                if (!string.IsNullOrWhiteSpace(error))
                {
                    LogMessage($"DLL错误信息: {error}");
                }
            }
        }

        private static string BuildLicenseStatusText(LicenseStatus status, bool licenseFileActivated)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{DateTime.Now:HH:mm:ss.fff}: GPU授权状态");
            builder.AppendLine("===============================================");
            builder.AppendLine($"授权文件自动激活: {(licenseFileActivated ? "成功" : "未激活或未找到默认授权文件")}");
            builder.AppendLine($"授权状态: {(status.Activated ? "已授权" : "未授权")}");
            builder.AppendLine($"产品名称: {(string.IsNullOrWhiteSpace(status.ProductName) ? "-" : status.ProductName)}");
            builder.AppendLine($"授权编号: {(string.IsNullOrWhiteSpace(status.LicenseId) ? "-" : status.LicenseId)}");
            builder.AppendLine($"授权版本: {(string.IsNullOrWhiteSpace(status.Version) ? "-" : status.Version)}");
            builder.AppendLine($"授权状态描述: {(string.IsNullOrWhiteSpace(status.LicenseState) ? "-" : status.LicenseState)}");
            builder.AppendLine($"GPU权限: {(status.AllowGpu ? "允许" : "不允许")}");
            builder.AppendLine($"设备绑定: {(status.MachineBound ? "已绑定" : "未绑定")}");
            builder.AppendLine($"授权机器码: {(string.IsNullOrWhiteSpace(status.MachineCode) ? "-" : status.MachineCode)}");
            builder.AppendLine($"当前机器码: {(string.IsNullOrWhiteSpace(status.CurrentMachineCode) ? "-" : status.CurrentMachineCode)}");
            builder.AppendLine($"绑定模式: {(string.IsNullOrWhiteSpace(status.BindMode) ? "-" : status.BindMode)}");
            builder.AppendLine($"开始时间: {(string.IsNullOrWhiteSpace(status.StartTime) ? "-" : status.StartTime)}");
            builder.AppendLine($"到期时间: {(string.IsNullOrWhiteSpace(status.ExpireTime) ? "-" : status.ExpireTime)}");
            builder.AppendLine("===============================================");
            return builder.ToString();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                comboBoxuse_gpu.SelectedIndex = 0;
                comboBoxJson.SelectedIndex = 0;
                comboBoxModel.SelectedIndex = 0;
                comboBoxYoloModelType.SelectedIndex = 0;
                comboBoxYoloUseGpu.SelectedIndex = use_gpu ? 1 : 0;


                yoloModelPath = Path.Combine(Application.StartupPath, "models", "yolov8s.onnx");
                textBoxYoloModel.Text = yoloModelPath;

                RecFilepath = Path.Combine(Application.StartupPath, "output");
                if (!Directory.Exists(RecFilepath))
                {
                    Directory.CreateDirectory(RecFilepath);
                }
            }
            catch (Exception ex)
            {
                message.Append(ex.Message);
                textBoxResult.Text = message.ToString();
            }
        }

        private async void buttonInit_Click(object sender, EventArgs e)
        {
            try
            {
                SetOCRBusy(true);

                string initmsg = await Task.Run(() =>
                {
                    OCREngine.use_gpu = use_gpu;
                    OCREngine.gpu_id = gpu_id;
                    OCREngine.cpu_threads = cpu_threads;
                    switch (model_type)
                    {
                        case 0:
                            OCREngine.det_infer = "PP-OCRv6_tiny_det.onnx";//OCR V6检测模型
                            OCREngine.rec_infer = "PP-OCRv6_tiny_rec.onnx";//OCR V6识别模型
                            OCREngine.cls_infer = "ch_PP-LCNet_x1_0_textline_ori_cls_server.onnx";
                            OCREngine.keys = "ppocrv6tiny_dict.txt";
                            break;
                        case 1:
                            OCREngine.det_infer = "PP-OCRv6_small_det.onnx";//OCR V6检测模型
                            OCREngine.rec_infer = "PP-OCRv6_small_rec.onnx";//OCR V6识别模型
                            OCREngine.cls_infer = "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx";
                            OCREngine.keys = "ppocrv6small_dict.txt";
                            break;
                        case 2:
                            OCREngine.det_infer = "ch_PP-OCRv5_mobile_det.onnx";//OCR V5检测模型
                            OCREngine.rec_infer = "ch_PP-OCRv5_rec_mobile_infer.onnx";//OCR V5识别模型
                            OCREngine.cls_infer = "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx";
                            OCREngine.keys = "ppocrv5_dict.txt";
                            break;
                        case 3:
                            OCREngine.det_infer = "ch_PP-OCRv5_det_server.onnx";//OCR V5检测模型
                            OCREngine.rec_infer = "ch_PP-OCRv5_rec_server.onnx";//OCR V5识别模型
                            OCREngine.cls_infer = "ch_PP-LCNet_x1_0_textline_ori_cls_server.onnx";
                            OCREngine.keys = "ppocrv5_dict.txt";
                            break;
                        case 4:
                            OCREngine.det_infer = "ch_PP-OCRv4_det_infer.onnx";//OCR V4检测模型
                            OCREngine.rec_infer = "ch_PP-OCRv4_rec_infer.onnx";//OCR V4识别模型
                            OCREngine.cls_infer = "ch_ppocr_mobile_v2.0_cls_infer.onnx";
                            OCREngine.keys = "ppocr_keys_v1.txt";
                            break;
                    }

                    return OCREngine.GetOCREngine();
                });

                if (string.IsNullOrEmpty(initmsg))
                {
                    LogMessage($"{DateTime.Now:HH:mm:ss.fff}:OCR初始化成功");
                }
                else
                {
                    LogMessage($"{DateTime.Now:HH:mm:ss.fff}:{initmsg}");
                }

                if (initmsg.IndexOf("初始化成功") >= 0)
                {
                    LogMessage($"{DateTime.Now:HH:mm:ss.fff}:更换模型请先释放OCR");
                    SetOCRBusy(true);
                }
                else
                {
                    SetOCRBusy(false);
                }
            }
            catch (Exception ex)
            {
                SetOCRBusy(false);
                LogMessage($"{DateTime.Now:HH:mm:ss.fff}:{ex.Message}");
            }
        }
        private string RecOCR(string filePath)
        {
            string result = "";
            try
            {
                var stopwatch = new Stopwatch();
                var startTime = DateTime.Now;
                LogMessage($"Image: {filePath}");
                LogMessage($"开始时间: {startTime:HH:mm:ss.fff}");
                stopwatch.Start();
                //Mat image = Cv2.ImRead(filePath, ImreadModes.Color);
                OCRResult ocrResult = ocrService.Detect(filePath);
                //OCRResult ocrResult = ocrService.DetectMat(image.CvPtr);使用OpenCvSharp4时,可传入DetectMat(image.CvPtr)
                result = ocrResult.StrRes.Replace("\n", Environment.NewLine);
                var endTime = DateTime.Now;
                LogMessage($"结束时间: {endTime:HH:mm:ss.fff}");
                LogMessage($"OCR总用时: {ocrResult.DetectTime} 毫秒");
                if (outPutJson)
                    LogMessage($"结果: {ocrResult.JsonText}");
                else
                    LogMessage($"结果: {result}");

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return result;
        }

        private void buttonBrowseYolo_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "ONNX模型(*.onnx)|*.onnx|所有文件(*.*)|*.*";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            yoloModelPath = dialog.FileName;
            if (textBoxYoloModel != null)
            {
                textBoxYoloModel.Text = yoloModelPath;
            }
        }

        private int GetYoloModelType()
        {
            return comboBoxYoloModelType?.SelectedIndex switch
            {
                0 => 1,
                1 => 2,
                2 => 3,
                3 => 7,
                4 => 8,
                _ => 1
            };
        }

        private string BuildYoloParameterJson()
        {
            bool yoloUseGpu = comboBoxYoloUseGpu?.SelectedIndex == 1;
            int yoloGpuId = Convert.ToInt32(numericUpDownYoloGpuId?.Value ?? 0);
            int yoloThreads = Convert.ToInt32(numericUpDownYoloThreads?.Value ?? 1);
            decimal confidence = numericUpDownYoloConfidence?.Value ?? 0.25M;
            decimal iou = numericUpDownYoloIou?.Value ?? 0.45M;
            bool visualize = checkBoxYoloVisualize?.Checked ?? true;
            bool enableLog = checkBoxYoloLog?.Checked ?? false;

            return JsonConvert.SerializeObject(new
            {
                model_type = GetYoloModelType(),
                use_gpu = yoloUseGpu,
                gpu_id = yoloGpuId,
                num_threads = yoloThreads,
                confidence_threshold = Convert.ToSingle(confidence, CultureInfo.InvariantCulture),
                iou_threshold = Convert.ToSingle(iou, CultureInfo.InvariantCulture),
                visualize,
                enable_log = enableLog
            });
        }

        private void buttonYoloInit_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(yoloModelPath) || !File.Exists(yoloModelPath))
                {
                    buttonBrowseYolo_Click(sender, e);
                }
                if (string.IsNullOrWhiteSpace(yoloModelPath) || !File.Exists(yoloModelPath))
                {
                    LogYoloMessage("请选择正确的YOLO ONNX模型");
                    return;
                }

                string parameterJson = BuildYoloParameterJson();
                ocrService.YoloInitJson(yoloModelPath, parameterJson);
                yoloInitialized = true;
                if (buttonYoloInit != null) buttonYoloInit.Enabled = false;
                if (buttonYoloDetect != null) buttonYoloDetect.Enabled = true;
                if (buttonYoloDetectTensor != null) buttonYoloDetectTensor.Enabled = true;
                if (buttonYoloFree != null) buttonYoloFree.Enabled = true;
                LogYoloMessage($"{DateTime.Now:HH:mm:ss.fff}:YOLO初始化成功");
                LogYoloMessage(parameterJson);
            }
            catch (Exception ex)
            {
                LogYoloMessage($"{DateTime.Now:HH:mm:ss.fff}:{ex.Message}");
            }
        }

        private void buttonYoloDetect_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!yoloInitialized)
                {
                    LogYoloMessage("YOLO未初始化");
                    return;
                }

                using OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "图片文件(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|所有文件(*.*)|*.*";
                dialog.Multiselect = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                foreach (string file in dialog.FileNames)
                {
                    DetectYolo(file);
                }
            }
            catch (Exception ex)
            {
                LogYoloMessage($"{DateTime.Now:HH:mm:ss.fff}:{ex.Message}");
            }
        }
        private void DetectYolo(string filePath)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            LogYoloMessage($"开始时间: {DateTime.Now:HH:mm:ss.fff}");
            string json = ocrService.YoloDetect(filePath);
            stopwatch.Stop();
            LogYoloMessage($"Image: {Path.GetFileName(filePath)}");
            LogYoloMessage($"结束时间: {DateTime.Now:HH:mm:ss.fff}");
            LogYoloMessage($"YOLO识别耗时 {stopwatch.ElapsedMilliseconds}毫秒");
            LogYoloMessage(FormatJsonSafe(json));
            LogMessage("===============================================");
            string imageToShow = filePath;
            try
            {
                string? visPath = Newtonsoft.Json.Linq.JObject.Parse(json)["vis_path"]?.ToString();
                if (!string.IsNullOrWhiteSpace(visPath))
                {
                    string fullVisPath = Path.IsPathRooted(visPath)
                        ? visPath
                        : Path.Combine(Application.StartupPath, visPath);
                    if (File.Exists(fullVisPath))
                    {
                        imageToShow = fullVisPath;
                    }
                }
            }
            catch
            {
                string outputImage = Path.Combine(Application.StartupPath, "output", Path.GetFileName(filePath));
                if (File.Exists(outputImage))
                {
                    imageToShow = outputImage;
                }
            }
            var image = ImageTools.LoadImage(imageToShow);
            pictureBoxYolo?.Invoke((MethodInvoker)delegate
            {
                pictureBoxYolo.Image = image;
            });
        }

        private void buttonYoloDetectTensor_Click(object sender, EventArgs e)
        {
            try
            {
                if (!yoloInitialized)
                {
                    LogYoloMessage("YOLO未初始化");
                    return;
                }

                using OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "图片文件(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|所有文件(*.*)|*.*";
                dialog.Multiselect = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                foreach (string file in dialog.FileNames)
                {
                    DetectYoloTensor(file);
                }
            }
            catch (Exception ex)
            {
                LogYoloMessage($"{DateTime.Now:HH:mm:ss.fff}:{ex.Message}");
            }
        }

        private void DetectYoloTensor(string filePath)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            DateTime startTime = DateTime.Now;
            LogYoloMessage($"开始时间: {startTime:HH:mm:ss.fff}");
            YoloTensorResult tensor = ocrService.YoloDetectTensor(filePath);
            stopwatch.Stop();

            LogYoloMessage($"Image: {Path.GetFileName(filePath)}");
            LogYoloMessage($"结束时间: {DateTime.Now:HH:mm:ss.fff}");
            LogYoloMessage($"YOLO Tensor识别耗时 {stopwatch.ElapsedMilliseconds}毫秒");

            List<YoloDetection> detections;
            using (var sourceImage = System.Drawing.Image.FromFile(filePath))
            {
                detections = YoloTensorPostProcessor.Process(tensor, new YoloPostProcessOptions
                {
                    InputWidth = 640,
                    InputHeight = 640,
                    OriginalWidth = sourceImage.Width,
                    OriginalHeight = sourceImage.Height,
                    ConfidenceThreshold = Convert.ToSingle(numericUpDownYoloConfidence?.Value ?? 0.25M, CultureInfo.InvariantCulture),
                    IouThreshold = Convert.ToSingle(numericUpDownYoloIou?.Value ?? 0.45M, CultureInfo.InvariantCulture),
                    EnableNms = true,
                    MaxDetections = 100
                });
            }

            LogYoloMessage(FormatYoloTensorResult(tensor, detections));
            LogYoloMessage("===============================================");

            string imageToShow = FindLatestYoloTensorVisualization(startTime) ?? filePath;
            var image = ImageTools.LoadImage(imageToShow);
            pictureBoxYolo?.Invoke((MethodInvoker)delegate
            {
                pictureBoxYolo.Image = image;
            });
        }

        private static string FormatYoloTensorResult(YoloTensorResult tensor, List<YoloDetection> detections)
        {
            StringBuilder builder = new StringBuilder();
            if (tensor == null)
            {
                return "Tensor结果为空";
            }

            string shapeText = tensor.Shape == null ? "" : string.Join(", ", tensor.Shape);
            builder.AppendLine($"Shape: [{shapeText}]");
            builder.AppendLine($"ShapeLen: {tensor.ShapeLen}");
            builder.AppendLine($"ElementCount: {tensor.ElementCount}");
            if (tensor.Data == null || tensor.Shape == null || tensor.Shape.Length < 3)
            {
                builder.AppendLine("Tensor数据或Shape无效");
                return builder.ToString();
            }

            long batch = tensor.Shape[0];
            long boxes = tensor.Shape[1];
            long channels = tensor.Shape[2];
            builder.AppendLine($"Batch: {batch}, Boxes: {boxes}, Channels: {channels}");
            builder.AppendLine("访问方式: data[(b * boxes + boxIndex) * channels + channelIndex]");
            builder.AppendLine();
            builder.AppendLine("前5个候选框(raw):");

            int previewRows = (int)Math.Min(boxes, 5);
            int previewChannels = (int)Math.Min(channels, 12);
            for (int i = 0; i < previewRows; i++)
            {
                builder.Append($"[{i}] ");
                for (int c = 0; c < previewChannels; c++)
                {
                    builder.Append(GetTensorValue(tensor.Data, boxes, channels, 0, i, c).ToString("0.####", CultureInfo.InvariantCulture));
                    if (c < previewChannels - 1)
                    {
                        builder.Append(", ");
                    }
                }
                if (channels > previewChannels)
                {
                    builder.Append(", ...");
                }
                builder.AppendLine();
            }

            builder.AppendLine();
            builder.AppendLine($"Decode -> 阈值过滤 -> NMS -> 坐标映射 后检测数量: {detections.Count}");
            builder.AppendLine("前20个检测框:");
            int detectionPreviewCount = Math.Min(detections.Count, 20);
            for (int i = 0; i < detectionPreviewCount; i++)
            {
                YoloDetection item = detections[i];
                builder.AppendLine(
                    $"{i + 1}. box={item.BoxIndex}, class={item.ClassId}, score={item.Confidence.ToString("0.####", CultureInfo.InvariantCulture)}, " +
                    $"x={item.X.ToString("0.##", CultureInfo.InvariantCulture)}, y={item.Y.ToString("0.##", CultureInfo.InvariantCulture)}, " +
                    $"w={item.Width.ToString("0.##", CultureInfo.InvariantCulture)}, h={item.Height.ToString("0.##", CultureInfo.InvariantCulture)}");
            }

            return builder.ToString();
        }

        private static float GetTensorValue(float[] data, long boxes, long channels, long batchIndex, long boxIndex, long channelIndex)
        {
            long index = (batchIndex * boxes + boxIndex) * channels + channelIndex;
            if (index < 0 || index >= data.LongLength)
            {
                return 0.0f;
            }
            return data[index];
        }

        private string? FindLatestYoloTensorVisualization(DateTime startTime)
        {
            if (!(checkBoxYoloVisualize?.Checked ?? false))
            {
                return null;
            }

            string[] outputDirs =
            {
                Path.Combine(Application.StartupPath, "output"),
                Path.Combine(Directory.GetCurrentDirectory(), "output")
            };

            string? latestPath = null;
            DateTime latestTime = startTime;
            foreach (string outputDir in outputDirs)
            {
                if (!Directory.Exists(outputDir))
                {
                    continue;
                }

                foreach (string file in Directory.GetFiles(outputDir, "yolo_tensor_*.png"))
                {
                    DateTime writeTime = File.GetLastWriteTime(file);
                    if (writeTime >= latestTime)
                    {
                        latestTime = writeTime;
                        latestPath = file;
                    }
                }
            }

            return latestPath;
        }

        private void buttonYoloFree_Click(object? sender, EventArgs e)
        {
            string msg = ocrService.YoloFreeEngine();
            yoloInitialized = false;
            if (buttonYoloInit != null) buttonYoloInit.Enabled = true;
            if (buttonYoloDetect != null) buttonYoloDetect.Enabled = false;
            if (buttonYoloDetectTensor != null) buttonYoloDetectTensor.Enabled = false;
            if (buttonYoloFree != null) buttonYoloFree.Enabled = false;
            LogYoloMessage(string.IsNullOrEmpty(msg)
                ? $"{DateTime.Now:HH:mm:ss.fff}:YOLO引擎释放成功"
                : $"{DateTime.Now:HH:mm:ss.fff}:{msg}");
        }

        private void LogYoloMessage(string infoValue)
        {
            TextBox? target = textBoxYoloResult;
            if (target == null)
            {
                LogMessage(infoValue);
                return;
            }

            if (target.InvokeRequired)
            {
                target.BeginInvoke(new Action(() =>
                {
                    target.AppendText(infoValue);
                    target.AppendText(Environment.NewLine);
                    target.SelectionStart = target.Text.Length;
                    target.ScrollToCaret();
                }));
            }
            else
            {
                target.AppendText(infoValue);
                target.AppendText(Environment.NewLine);
                target.SelectionStart = target.Text.Length;
                target.ScrollToCaret();
            }
        }

        private void buttonRec_Click(object sender, EventArgs e)
        {
            try
            {
                textBoxResult.Text = "";
                message = new StringBuilder();
                string result = "";
                string recFileName = "";
                OpenFileDialog OpenFileDialog1 = new OpenFileDialog();
                OpenFileDialog1.Filter = "图片文件(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|所有文件(*.*)|*.*";
                OpenFileDialog1.Multiselect = true;
                if (DialogResult.OK == OpenFileDialog1.ShowDialog())
                {
                    for (int i = 0; i < recCount; i++)//模拟循环OCR识别
                    {
                        foreach (var regfile in OpenFileDialog1.FileNames)
                        {
                            string filePath = Path.GetFullPath(regfile);
                            recFileName = Path.Combine(RecFilepath, Path.GetFileName(filePath + "-result.jpg"));
                            result = RecOCR(filePath);
                            // 在工作线程中
                            var image = ImageTools.LoadImage(recFileName);
                            pictureBoxImg.Invoke((MethodInvoker)delegate
                            {
                                pictureBoxImg.Image = image;
                            });
                        }
                    }
                }
                OpenFileDialog1.Dispose();

            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
            }
        }

        private void buttonDownModels_Click(object sender, EventArgs e)
        {
            // 定义要打开的 URL
            string url = "https://www.modelscope.cn/models/RapidAI/RapidOCR/files";
            try
            {
                LogMessage($"PP-OCRv4/V5-onnx模型下载地址：{url}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开网页：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonGetBase64_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog OpenFileDialog1 = new OpenFileDialog();
                OpenFileDialog1.Filter = "图片文件(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|所有文件(*.*)|*.*";
                OpenFileDialog1.Multiselect = false;
                if (DialogResult.OK == OpenFileDialog1.ShowDialog())
                {
                    string filePath = OpenFileDialog1.FileName;
                    string base64 = ImageTools.GetBase64FromImage(filePath);
                    textBoxResult.Text = base64;
                }
                OpenFileDialog1.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"图片格式异常：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonPostFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(this.textBoxApiAddress.Text.Trim()))
                {
                    LogMessage($"{DateTime.Now:HH:mm:ss.fff}:WebApi地址不能为空");
                }
                OpenFileDialog OpenFileDialog1 = new OpenFileDialog();
                OpenFileDialog1.Filter = "图片文件(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|所有文件(*.*)|*.*";
                OpenFileDialog1.Multiselect = false;
                if (DialogResult.OK == OpenFileDialog1.ShowDialog())
                {
                    string filePath = OpenFileDialog1.FileName;
                    textBoxResult.Text = HttpHelper.PostFile(this.textBoxApiAddress.Text.Trim(), filePath);
                }
                OpenFileDialog1.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"调用接口异常：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void comboBoxuse_gpu_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.comboBoxuse_gpu.SelectedIndex)
            {
                case 0:
                    use_gpu = false;
                    break;
                case 1:
                    use_gpu = true;
                    //StringBuilder sb = new StringBuilder();
                    //sb.Append($"{DateTime.Now:HH:mm:ss.fff}:使用GPU时请下载对应的paddle_inference解压" + Environment.NewLine);
                    //sb.Append($"解压后将以下dll文件复制到程序运行文件夹中：" + Environment.NewLine);
                    //sb.Append($"paddle\\lib目录下的common.dll和paddle_inference.dll" + Environment.NewLine);
                    //sb.Append($"third_party\\install\\mkldnn\\lib目录下的mkldnn.dll" + Environment.NewLine);
                    //sb.Append($"third_party\\install\\mklml\\lib目录下的libiomp5md.dll和mklml.dll" + Environment.NewLine);
                    //sb.Append($"安装指定版本的CUDA以及CUDNN" + Environment.NewLine);
                    //sb.Append($"复制对应的CUDNN中的cudnn64_8.dll(CUDNN8的文件名)或cudnn64_9.dll(CUDNN9的文件名)到程序运行文件夹中" + Environment.NewLine);
                    //sb.Append($"C:\\Program Files\\NVIDIA GPU Computing Toolkit\\CUDA\\v12.4\\bin\\cudnn64_8.dll" + Environment.NewLine);
                    //LogMessage(sb.ToString());
                    break;
                default:
                    use_gpu = false;
                    break;
            }
        }

        private void numDowngpu_id_ValueChanged(object sender, EventArgs e)
        {
            if (this.numDowngpu_id.Value > 0)
            {
                gpu_id = Convert.ToInt32(this.numDowngpu_id.Value);
            }
        }

        private void numDowncpu_threads_ValueChanged(object sender, EventArgs e)
        {
            if (this.numDowncpu_threads.Value > 0)
            {
                cpu_threads = Convert.ToInt32(this.numDowncpu_threads.Value);
            }
        }
        private void numericUpDowncpu_mem_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDowncpu_mem.Value > 0)
                cpu_mem = Convert.ToInt32(numericUpDowncpu_mem.Value);
        }

        private void numericUpDownThread_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownThread.Value > 0)
                recCount = Convert.ToInt32(numericUpDownThread.Value);
        }

        private void comboBoxJson_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.comboBoxJson.SelectedIndex)
            {
                case 0:
                    outPutJson = false;
                    break;
                case 1:
                    outPutJson = true;
                    break;
                default:
                    outPutJson = false;
                    break;
            }

        }

        #region LogMessage
        public void LogMessage(string infoValue)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    textBoxResult.AppendText(infoValue);
                    textBoxResult.AppendText(Environment.NewLine);
                    textBoxResult.SelectionStart = textBoxResult.Text.Length;
                    textBoxResult.ScrollToCaret();
                }));
            }
            else
            {
                textBoxResult.AppendText(infoValue);
                textBoxResult.AppendText(Environment.NewLine);
                textBoxResult.SelectionStart = textBoxResult.Text.Length;
                textBoxResult.ScrollToCaret();
            }
        }

        #endregion

        private void comboBoxModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            model_type = comboBoxModel.SelectedIndex;
        }

        private void buttonFreeEngine_Click(object sender, EventArgs e)
        {
            string initmsg = ocrService.FreeEngine();
            if (string.IsNullOrEmpty(initmsg))
            {
                SetOCRBusy(false);
                LogMessage($"{DateTime.Now:HH:mm:ss.fff}:OCR释放成功");
            }
            else
            {
                LogMessage($"{DateTime.Now:HH:mm:ss.fff}:{initmsg}");
            }
        }
        public static string FormatJsonSafe(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                return System.Text.Json.JsonSerializer.Serialize(doc, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            catch
            {
                return Regex.Replace(
                    json,
                    @"\\u([0-9a-fA-F]{4})",
                    match => ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString())
                    .Replace("\\r\\n", Environment.NewLine)
                    .Replace("\\n", Environment.NewLine)
                    .Replace("\\r", Environment.NewLine);
            }
        }
        private void SetOCRBusy(bool busy)
        {
            isOCRBusy = busy;
            buttonInit.Enabled = !busy;
            buttonFreeEngine.Enabled = busy;
            buttonRec.Enabled = busy;
            buttonPostFile.Enabled = busy;
            buttonGetBase64.Enabled = !busy;
            comboBoxModel.Enabled = !busy;
            comboBoxuse_gpu.Enabled = !busy;
            numericUpDownThread.Enabled = !busy;
            numDowncpu_threads.Enabled = !busy;
            numDowngpu_id.Enabled = !busy ;
            numericUpDowncpu_mem.Enabled = !busy;
            buttonGetBase64.Enabled = true;
        }
    }
}

