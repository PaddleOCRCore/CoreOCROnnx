using CoreOCROnnx.SDK;
using Newtonsoft.Json;

namespace CoreOCROnnx.WebApi.Services
{
    /// <summary>
    /// YOLO引擎依赖注入
    /// </summary>
    public class YOLOEngine
    {
        private readonly IOCRService _yoloService;
        private readonly YOLOConfig _yoloConfig;
        private readonly OCREngine _ocrEngine;

        public IOCRService YoloService => _yoloService;

        public bool IsInitialized { get; private set; }

        public string InitializeMessage { get; private set; } = "YOLO尚未初始化";

        public string ModelPath { get; private set; } = string.Empty;

        public YOLOEngine(IOCRService yoloService, YOLOConfig yoloConfig, OCREngine ocrEngine)
        {
            _yoloService = yoloService;
            _yoloConfig = yoloConfig;
            _ocrEngine = ocrEngine;
            GetYOLOEngine();
        }

        /// <summary>
        /// 初始化YOLO引擎
        /// </summary>
        /// <returns></returns>
        public string GetYOLOEngine()
        {
            IsInitialized = false;
            ModelPath = ResolveModelPath(_yoloConfig.models_root, _yoloConfig.model_path);
            if (string.IsNullOrWhiteSpace(ModelPath) || !File.Exists(ModelPath))
            {
                InitializeMessage = $"YOLO模型文件不存在:{ModelPath}";
                return InitializeMessage;
            }

            try
            {
                if (_yoloConfig.use_gpu && !_ocrEngine.ActivateLicenseIfExists())
                {
                    InitializeMessage = "授权文件未激活，无法初始化GPU模式";
                    return InitializeMessage;
                }

                string parameterJson = BuildParameterJson();
                IsInitialized = _yoloService.YoloInitJson(ModelPath, parameterJson);
                InitializeMessage = IsInitialized ? string.Empty : _yoloService.GetError();
            }
            catch (Exception ex)
            {
                InitializeMessage = ex.Message;
            }
            return InitializeMessage;
        }

        private string BuildParameterJson()
        {
            return JsonConvert.SerializeObject(new
            {
                model_type = _yoloConfig.model_type,
                use_gpu = _yoloConfig.use_gpu,
                gpu_id = _yoloConfig.gpu_id,
                num_threads = _yoloConfig.num_threads,
                confidence_threshold = _yoloConfig.confidence_threshold,
                iou_threshold = _yoloConfig.iou_threshold,
                visualize = _yoloConfig.visualize,
                enable_log = _yoloConfig.enable_log
            });
        }

        private static string ResolveModelPath(string modelsRoot, string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                return string.Empty;
            }

            if (Path.IsPathRooted(modelPath))
            {
                return modelPath;
            }

            string modelRoot = string.IsNullOrWhiteSpace(modelsRoot)
                ? AppContext.BaseDirectory
                : Path.IsPathRooted(modelsRoot)
                    ? modelsRoot
                    : Path.Combine(AppContext.BaseDirectory, modelsRoot);
            return Path.Combine(modelRoot, modelPath);
        }
    }
}
