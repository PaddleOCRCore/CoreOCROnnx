using System;
using System.Collections.Generic;

namespace CoreOCROnnx.SDK
{
    /// <summary>
    /// YOLO Tensor后处理参数。
    /// </summary>
    public class YoloPostProcessOptions
    {
        /// <summary>
        /// 模型输入宽度。用于将letterbox坐标映射回原图坐标。
        /// </summary>
        public int InputWidth { get; set; } = 640;

        /// <summary>
        /// 模型输入高度。用于将letterbox坐标映射回原图坐标。
        /// </summary>
        public int InputHeight { get; set; } = 640;

        /// <summary>
        /// 原图宽度。小于等于0时不做原图坐标映射。
        /// </summary>
        public int OriginalWidth { get; set; }

        /// <summary>
        /// 原图高度。小于等于0时不做原图坐标映射。
        /// </summary>
        public int OriginalHeight { get; set; }

        /// <summary>
        /// 置信度阈值。
        /// </summary>
        public float ConfidenceThreshold { get; set; } = 0.25f;

        /// <summary>
        /// NMS IoU阈值。
        /// </summary>
        public float IouThreshold { get; set; } = 0.45f;

        /// <summary>
        /// 是否启用NMS。
        /// </summary>
        public bool EnableNms { get; set; } = true;

        /// <summary>
        /// 处理第几个batch。当前YOLO检测通常为0。
        /// </summary>
        public int BatchIndex { get; set; }

        /// <summary>
        /// 最大输出数量。小于等于0表示不限制。
        /// </summary>
        public int MaxDetections { get; set; }

        /// <summary>
        /// 类别名称列表。可为空，按class id输出。
        /// </summary>
        public IList<string> ClassNames { get; set; }
    }

    /// <summary>
    /// YOLO检测框结果，坐标为映射后的左上角宽高。
    /// </summary>
    public class YoloDetection
    {
        public int BatchIndex { get; set; }
        public int BoxIndex { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
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

        public float X1 => X;
        public float Y1 => Y;
        public float X2 => X + Width;
        public float Y2 => Y + Height;
    }

    /// <summary>
    /// YOLO Tensor后处理工具：Decode -> 阈值过滤 -> NMS -> 坐标映射。
    /// 输入Tensor格式为[bs, boxes, channels]，channels通常为nc+4。
    /// </summary>
    public static class YoloTensorPostProcessor
    {
        public static List<YoloDetection> Process(YoloTensorResult tensor, YoloPostProcessOptions options)
        {
            if (tensor == null)
            {
                throw new ArgumentNullException(nameof(tensor));
            }
            if (options == null)
            {
                options = new YoloPostProcessOptions();
            }
            ValidateTensor(tensor);

            long batch = tensor.Shape[0];
            long boxes = tensor.Shape[1];
            long channels = tensor.Shape[2];
            if (options.BatchIndex < 0 || options.BatchIndex >= batch)
            {
                throw new ArgumentOutOfRangeException(nameof(options.BatchIndex), "BatchIndex超出Tensor batch范围");
            }

            List<YoloDetection> candidates = DecodeAndFilter(tensor, options, boxes, channels);
            candidates.Sort(CompareByConfidenceDesc);

            List<YoloDetection> results = options.EnableNms
                ? ApplyClassAwareNms(candidates, options.IouThreshold)
                : candidates;

            if (options.MaxDetections > 0 && results.Count > options.MaxDetections)
            {
                results.RemoveRange(options.MaxDetections, results.Count - options.MaxDetections);
            }
            return results;
        }

        private static List<YoloDetection> DecodeAndFilter(
            YoloTensorResult tensor,
            YoloPostProcessOptions options,
            long boxes,
            long channels)
        {
            List<YoloDetection> detections = new List<YoloDetection>();
            for (int boxIndex = 0; boxIndex < boxes; boxIndex++)
            {
                int classId = -1;
                float confidence = float.MinValue;
                for (int c = 4; c < channels; c++)
                {
                    float score = GetTensorValue(tensor.Data, boxes, channels, options.BatchIndex, boxIndex, c);
                    if (score > confidence)
                    {
                        confidence = score;
                        classId = c - 4;
                    }
                }

                if (classId < 0 || confidence < options.ConfidenceThreshold)
                {
                    continue;
                }

                float centerX = GetTensorValue(tensor.Data, boxes, channels, options.BatchIndex, boxIndex, 0);
                float centerY = GetTensorValue(tensor.Data, boxes, channels, options.BatchIndex, boxIndex, 1);
                float width = GetTensorValue(tensor.Data, boxes, channels, options.BatchIndex, boxIndex, 2);
                float height = GetTensorValue(tensor.Data, boxes, channels, options.BatchIndex, boxIndex, 3);
                float rawWidth = width;
                float rawHeight = height;
                if (width <= 0.0f || height <= 0.0f)
                {
                    continue;
                }

                float left = centerX - width * 0.5f;
                float top = centerY - height * 0.5f;
                MapLetterboxRect(options, ref left, ref top, ref width, ref height);
                if (width <= 0.0f || height <= 0.0f)
                {
                    continue;
                }

                detections.Add(new YoloDetection
                {
                    BatchIndex = options.BatchIndex,
                    BoxIndex = boxIndex,
                    ClassId = classId,
                    ClassName = GetClassName(options.ClassNames, classId),
                    Confidence = confidence,
                    X = left,
                    Y = top,
                    Width = width,
                    Height = height,
                    CenterX = left + width * 0.5f,
                    CenterY = top + height * 0.5f,
                    RawX = centerX,
                    RawY = centerY,
                    RawWidth = rawWidth,
                    RawHeight = rawHeight
                });
            }

            return detections;
        }

        private static List<YoloDetection> ApplyClassAwareNms(List<YoloDetection> detections, float iouThreshold)
        {
            List<YoloDetection> kept = new List<YoloDetection>();
            bool[] removed = new bool[detections.Count];
            for (int i = 0; i < detections.Count; i++)
            {
                if (removed[i])
                {
                    continue;
                }

                YoloDetection current = detections[i];
                kept.Add(current);
                for (int j = i + 1; j < detections.Count; j++)
                {
                    if (removed[j] || current.ClassId != detections[j].ClassId)
                    {
                        continue;
                    }
                    if (CalculateIou(current, detections[j]) > iouThreshold)
                    {
                        removed[j] = true;
                    }
                }
            }
            return kept;
        }

        private static void MapLetterboxRect(YoloPostProcessOptions options, ref float left, ref float top, ref float width, ref float height)
        {
            if (options.OriginalWidth <= 0 || options.OriginalHeight <= 0 ||
                options.InputWidth <= 0 || options.InputHeight <= 0)
            {
                return;
            }

            float scale = Math.Min(
                options.InputWidth / (float)options.OriginalWidth,
                options.InputHeight / (float)options.OriginalHeight);
            float resizedWidth = options.OriginalWidth * scale;
            float resizedHeight = options.OriginalHeight * scale;
            float padX = (options.InputWidth - resizedWidth) * 0.5f;
            float padY = (options.InputHeight - resizedHeight) * 0.5f;

            float right = left + width;
            float bottom = top + height;
            left = (left - padX) / scale;
            top = (top - padY) / scale;
            right = (right - padX) / scale;
            bottom = (bottom - padY) / scale;

            left = Clamp(left, 0.0f, options.OriginalWidth - 1.0f);
            top = Clamp(top, 0.0f, options.OriginalHeight - 1.0f);
            right = Clamp(right, 0.0f, options.OriginalWidth);
            bottom = Clamp(bottom, 0.0f, options.OriginalHeight);
            width = right - left;
            height = bottom - top;
        }

        private static float CalculateIou(YoloDetection a, YoloDetection b)
        {
            float x1 = Math.Max(a.X1, b.X1);
            float y1 = Math.Max(a.Y1, b.Y1);
            float x2 = Math.Min(a.X2, b.X2);
            float y2 = Math.Min(a.Y2, b.Y2);
            float intersectionWidth = Math.Max(0.0f, x2 - x1);
            float intersectionHeight = Math.Max(0.0f, y2 - y1);
            float intersection = intersectionWidth * intersectionHeight;
            float areaA = Math.Max(0.0f, a.Width) * Math.Max(0.0f, a.Height);
            float areaB = Math.Max(0.0f, b.Width) * Math.Max(0.0f, b.Height);
            float union = areaA + areaB - intersection;
            return union <= 0.0f ? 0.0f : intersection / union;
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

        private static int CompareByConfidenceDesc(YoloDetection left, YoloDetection right)
        {
            return right.Confidence.CompareTo(left.Confidence);
        }

        private static string GetClassName(IList<string> classNames, int classId)
        {
            if (classNames == null || classId < 0 || classId >= classNames.Count)
            {
                return string.Empty;
            }
            return classNames[classId] ?? string.Empty;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

        private static void ValidateTensor(YoloTensorResult tensor)
        {
            if (tensor.Data == null)
            {
                throw new ArgumentException("Tensor Data不能为空", nameof(tensor));
            }
            if (tensor.Shape == null || tensor.Shape.Length < 3)
            {
                throw new ArgumentException("Tensor Shape必须为[bs, boxes, channels]", nameof(tensor));
            }
            if (tensor.Shape[0] <= 0 || tensor.Shape[1] <= 0 || tensor.Shape[2] < 5)
            {
                throw new ArgumentException("Tensor Shape无效，channels至少需要包含x,y,w,h和一个类别分数", nameof(tensor));
            }
            long expectedCount = tensor.Shape[0] * tensor.Shape[1] * tensor.Shape[2];
            if (expectedCount > tensor.Data.LongLength)
            {
                throw new ArgumentException("Tensor Data长度小于Shape需要的元素数量", nameof(tensor));
            }
        }
    }
}
