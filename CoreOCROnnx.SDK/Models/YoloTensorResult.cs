using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoreOCROnnx.SDK
{
    /// <summary>
    /// YOLO张量检测结果。
    /// Data按Shape指定的[bs, boxes, channels]顺序连续展开。
    /// </summary>
    public class YoloTensorResult
    {
        /// <summary>
        /// 张量数据，按[bs, boxes, channels]连续展开。
        /// 访问索引: Data[(b * Shape[1] + boxIndex) * Shape[2] + channelIndex]。
        /// </summary>
        public float[] Data { get; set; }

        /// <summary>
        /// 张量形状，通常为[1, 8400, nc + 4]。
        /// </summary>
        public long[] Shape { get; set; }

        /// <summary>
        /// Shape维度数量，当前为3。
        /// </summary>
        public int ShapeLen { get; set; }

        /// <summary>
        /// Data元素总数。
        /// </summary>
        public long ElementCount { get; set; }
    }
}
