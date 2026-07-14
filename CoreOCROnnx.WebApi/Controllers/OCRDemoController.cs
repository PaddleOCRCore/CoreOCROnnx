using System.Globalization;
using CoreOCROnnx.SDK;
using CoreOCROnnx.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace CoreOCROnnx.WebApi.Controllers
{
    /// <summary>
    /// 在线Demo解析接口。
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]/[action]")]
    public class OCRDemoController : ActionBase
    {
        private const long MaxUploadBytes = 10 * 1024 * 1024;
        private const int YoloInputWidth = 640;
        private const int YoloInputHeight = 640;
        private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"
        };

        private readonly OCREngine _ocrEngine;
        private readonly YOLOEngine _yoloEngine;
        private readonly YOLOConfig _yoloConfig;

        public OCRDemoController(OCREngine ocrEngine, YOLOEngine yoloEngine, YOLOConfig yoloConfig)
        {
            _ocrEngine = ocrEngine;
            _yoloEngine = yoloEngine;
            _yoloConfig = yoloConfig;
        }

        /// <summary>
        /// 解析上传图片。
        /// </summary>
        /// <param name="file">图片文件。</param>
        /// <param name="model">pp-ocrv6 或 yolo。</param>
        /// <returns>在线Demo响应。</returns>
        [HttpPost]
        public async Task<ActionResult> Analyze(IFormFile file, [FromForm] string model)
        {
            string normalizedModel = NormalizeModel(model);
            if (normalizedModel.Length == 0)
            {
                return BadResult("不支持的解析模型");
            }

            if (!TryValidateFile(file, out string validationMessage))
            {
                return BadResult(validationMessage);
            }

            byte[] imageData;
            await using (MemoryStream stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                imageData = stream.ToArray();
            }

            if (!TryIdentifyImage(imageData, out int imageWidth, out int imageHeight, out string imageMessage))
            {
                return BadResult(imageMessage);
            }

            try
            {
                OCRDemoAnalyzeResult result = normalizedModel == "yolo"
                    ? AnalyzeYolo(file.FileName, imageData, imageWidth, imageHeight)
                    : AnalyzePpOcr(file.FileName, imageData, imageWidth, imageHeight);
                return OKResult(result);
            }
            catch (Exception ex)
            {
                return BadResult($"解析失败:{ex.Message}");
            }
        }

        private OCRDemoAnalyzeResult AnalyzePpOcr(string fileName, byte[] imageData, int imageWidth, int imageHeight)
        {
            OCRResult ocrResult = _ocrEngine.OcrService.Detect(imageData) ?? new OCRResult();
            List<OCRDemoBox> boxes = BuildOcrBoxes(ocrResult.TextBlocks);
            string content = ocrResult.StrRes ?? string.Empty;
            string jsonText = string.IsNullOrWhiteSpace(ocrResult.JsonText)
                ? JsonConvert.SerializeObject(ocrResult)
                : ocrResult.JsonText;

            return new OCRDemoAnalyzeResult
            {
                Model = "pp-ocrv6",
                ModelName = "PP-OCRv6",
                FileName = fileName,
                Content = content,
                Markdown = content,
                JsonText = jsonText,
                ImageWidth = imageWidth,
                ImageHeight = imageHeight,
                Raw = ocrResult,
                Boxes = boxes
            };
        }

        private OCRDemoAnalyzeResult AnalyzeYolo(string fileName, byte[] imageData, int imageWidth, int imageHeight)
        {
            if (!_yoloEngine.IsInitialized)
            {
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(_yoloEngine.InitializeMessage)
                    ? "YOLO尚未初始化"
                    : _yoloEngine.InitializeMessage);
            }

            YoloTensorResult tensor = _yoloEngine.YoloService.YoloDetectByteTensor(imageData);
            List<YoloDetection> detections = YoloTensorPostProcessor.Process(tensor, new YoloPostProcessOptions
            {
                InputWidth = YoloInputWidth,
                InputHeight = YoloInputHeight,
                OriginalWidth = imageWidth,
                OriginalHeight = imageHeight,
                ConfidenceThreshold = _yoloConfig.confidence_threshold,
                IouThreshold = _yoloConfig.iou_threshold,
                EnableNms = true,
                MaxDetections = 100
            });
            YoloTensorDemoRaw raw = BuildYoloRaw(tensor, detections);
            string jsonText = JsonConvert.SerializeObject(raw, Formatting.Indented);
            string content = BuildYoloContent(raw, detections);

            return new OCRDemoAnalyzeResult
            {
                Model = "yolo",
                ModelName = "YOLO",
                FileName = fileName,
                Content = content,
                Markdown = content,
                JsonText = jsonText,
                ImageWidth = imageWidth,
                ImageHeight = imageHeight,
                Raw = raw,
                Boxes = BuildYoloBoxes(detections)
            };
        }

        private static List<OCRDemoBox> BuildOcrBoxes(IEnumerable<CoreOCROnnx.SDK.JsonResult>? textBlocks)
        {
            List<OCRDemoBox> boxes = [];
            if (textBlocks == null)
            {
                return boxes;
            }

            int order = 0;
            foreach (CoreOCROnnx.SDK.JsonResult block in textBlocks)
            {
                if (block == null || string.IsNullOrWhiteSpace(block.Text) || block.Boxes == null || block.Boxes.Count == 0)
                {
                    continue;
                }

                List<OCRDemoPoint> points = block.Boxes.Select(point => new OCRDemoPoint { X = point.x, Y = point.y }).ToList();
                double minX = points.Min(point => point.X);
                double minY = points.Min(point => point.Y);
                double maxX = points.Max(point => point.X);
                double maxY = points.Max(point => point.Y);
                _ = double.TryParse(block.BoxScore, NumberStyles.Float, CultureInfo.InvariantCulture, out double score);

                boxes.Add(new OCRDemoBox
                {
                    Label = block.Text,
                    Text = block.Text,
                    IsTextLine = true,
                    BlockId = order.ToString(CultureInfo.InvariantCulture),
                    BlockOrder = order,
                    X = minX,
                    Y = minY,
                    Width = Math.Max(0, maxX - minX),
                    Height = Math.Max(0, maxY - minY),
                    Score = score,
                    Points = points
                });
                order++;
            }

            return boxes;
        }

        private static List<OCRDemoBox> BuildYoloBoxes(IReadOnlyList<YoloDetection> detections)
        {
            List<OCRDemoBox> boxes = [];
            for (int i = 0; i < detections.Count; i++)
            {
                YoloDetection detection = detections[i];
                string label = string.IsNullOrWhiteSpace(detection.ClassName)
                    ? $"class {detection.ClassId}"
                    : detection.ClassName;
                boxes.Add(new OCRDemoBox
                {
                    Label = label,
                    Text = $"{label} {detection.Confidence:P1}",
                    IsTextLine = false,
                    BlockId = detection.BoxIndex.ToString(CultureInfo.InvariantCulture),
                    BlockOrder = i,
                    X = detection.X,
                    Y = detection.Y,
                    Width = detection.Width,
                    Height = detection.Height,
                    Score = detection.Confidence,
                    Points =
                    [
                        new OCRDemoPoint { X = detection.X, Y = detection.Y },
                        new OCRDemoPoint { X = detection.X + detection.Width, Y = detection.Y },
                        new OCRDemoPoint { X = detection.X + detection.Width, Y = detection.Y + detection.Height },
                        new OCRDemoPoint { X = detection.X, Y = detection.Y + detection.Height }
                    ]
                });
            }
            return boxes;
        }

        private static YoloTensorDemoRaw BuildYoloRaw(YoloTensorResult tensor, IReadOnlyList<YoloDetection> detections)
        {
            return new YoloTensorDemoRaw
            {
                Shape = tensor.Shape ?? [],
                ShapeLen = tensor.ShapeLen,
                ElementCount = tensor.ElementCount,
                CandidatePreview = BuildYoloCandidatePreview(tensor),
                Detections = detections.Select(detection => new YoloDetectionDemoItem
                {
                    BatchIndex = detection.BatchIndex,
                    BoxIndex = detection.BoxIndex,
                    ClassId = detection.ClassId,
                    ClassName = detection.ClassName,
                    Confidence = detection.Confidence,
                    X = detection.X,
                    Y = detection.Y,
                    Width = detection.Width,
                    Height = detection.Height,
                    CenterX = detection.CenterX,
                    CenterY = detection.CenterY,
                    RawX = detection.RawX,
                    RawY = detection.RawY,
                    RawWidth = detection.RawWidth,
                    RawHeight = detection.RawHeight
                }).ToList()
            };
        }

        private static List<YoloTensorCandidatePreview> BuildYoloCandidatePreview(YoloTensorResult tensor)
        {
            List<YoloTensorCandidatePreview> previews = [];
            if (tensor?.Data == null || tensor.Shape == null || tensor.Shape.Length < 3)
            {
                return previews;
            }

            long boxes = tensor.Shape[1];
            long channels = tensor.Shape[2];
            int candidateCount = (int)Math.Min(5, boxes);
            int channelCount = (int)Math.Min(12, channels);
            for (int boxIndex = 0; boxIndex < candidateCount; boxIndex++)
            {
                List<float> values = [];
                for (int channelIndex = 0; channelIndex < channelCount; channelIndex++)
                {
                    long index = boxIndex * channels + channelIndex;
                    values.Add(index >= 0 && index < tensor.Data.LongLength ? tensor.Data[index] : 0.0f);
                }

                previews.Add(new YoloTensorCandidatePreview
                {
                    BoxIndex = boxIndex,
                    Values = values
                });
            }
            return previews;
        }

        private static string BuildYoloContent(YoloTensorDemoRaw raw, IReadOnlyCollection<YoloDetection> detections)
        {
            string shape = raw.Shape.Length == 0 ? "[]" : $"[{string.Join(", ", raw.Shape)}]";
            return $"Tensor Shape: {shape}\nElementCount: {raw.ElementCount}\nDetections: {detections.Count}";
        }

        private static bool TryValidateFile(IFormFile file, out string message)
        {
            if (file == null || file.Length == 0)
            {
                message = "图片不存在！";
                return false;
            }
            if (file.Length > MaxUploadBytes)
            {
                message = "图片大小不能超过10MB";
                return false;
            }
            string extension = Path.GetExtension(file.FileName);
            if (!SupportedExtensions.Contains(extension))
            {
                message = "仅支持PNG/JPG/JPEG/BMP/TIF/TIFF图片";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static bool TryIdentifyImage(byte[] imageData, out int width, out int height, out string message)
        {
            width = 0;
            height = 0;
            try
            {
                using MemoryStream stream = new MemoryStream(imageData);
                var imageInfo = Image.Identify(stream);
                if (imageInfo == null || imageInfo.Width <= 0 || imageInfo.Height <= 0)
                {
                    message = "图片格式无效或无法读取尺寸";
                    return false;
                }

                width = imageInfo.Width;
                height = imageInfo.Height;
                message = string.Empty;
                return true;
            }
            catch
            {
                message = "图片格式无效或无法读取尺寸";
                return false;
            }
        }

        private static string NormalizeModel(string model)
        {
            if (string.Equals(model, "pp-ocrv6", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(model, "ppocrv6", StringComparison.OrdinalIgnoreCase))
            {
                return "pp-ocrv6";
            }
            if (string.Equals(model, "yolo", StringComparison.OrdinalIgnoreCase))
            {
                return "yolo";
            }
            return string.Empty;
        }
    }

    public class OCRDemoAnalyzeResult
    {
        public string Model { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Markdown { get; set; } = string.Empty;
        public string JsonText { get; set; } = string.Empty;
        public string PreviewImage { get; set; } = string.Empty;
        public int PageIndex { get; set; } = 1;
        public int PageCount { get; set; } = 1;
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public object? Raw { get; set; }
        public List<OCRDemoBox> Boxes { get; set; } = [];
    }

    public class OCRDemoBox
    {
        public string Label { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsTextLine { get; set; }
        public string BlockId { get; set; } = string.Empty;
        public int BlockOrder { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Score { get; set; }
        public List<OCRDemoPoint> Points { get; set; } = [];
    }

    public class OCRDemoPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class YoloTensorDemoRaw
    {
        public long[] Shape { get; set; } = [];
        public int ShapeLen { get; set; }
        public long ElementCount { get; set; }
        public List<YoloTensorCandidatePreview> CandidatePreview { get; set; } = [];
        public List<YoloDetectionDemoItem> Detections { get; set; } = [];
    }

    public class YoloTensorCandidatePreview
    {
        public int BoxIndex { get; set; }
        public List<float> Values { get; set; } = [];
    }

    public class YoloDetectionDemoItem
    {
        public int BatchIndex { get; set; }
        public int BoxIndex { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float CenterX { get; set; }
        public float CenterY { get; set; }
        public float RawX { get; set; }
        public float RawY { get; set; }
        public float RawWidth { get; set; }
        public float RawHeight { get; set; }
    }
}