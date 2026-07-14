using CoreOCROnnx.SDK;
using CoreOCROnnx.WebApi;
using CoreOCROnnx.WebApi.Services;
using Newtonsoft.Json.Linq;

namespace CoreOCROnnx.WebApi.Tests;

public class YOLOEngineTests
{
    [Fact]
    public void Constructor_InitializesYoloWithConfiguredModelAndParameters()
    {
        string modelFile = CreateModelFile();
        FakeOcrService ocrService = new();
        OCREngine ocrEngine = new(ocrService, CreateOcrConfig(useGpu: false));
        YOLOConfig config = CreateYoloConfig(modelFile);

        YOLOEngine engine = new(ocrService, config, ocrEngine);

        Assert.True(engine.IsInitialized);
        Assert.Equal(modelFile, engine.ModelPath);
        Assert.Equal(["Init", "YoloInitJson"], ocrService.Calls);
        Assert.Equal(modelFile, ocrService.YoloModelPath);

        JObject parameter = JObject.Parse(ocrService.YoloParameterJson!);
        Assert.Equal(1, parameter.Value<int>("model_type"));
        Assert.False(parameter.Value<bool>("use_gpu"));
        Assert.Equal(0, parameter.Value<int>("gpu_id"));
        Assert.Equal(30, parameter.Value<int>("num_threads"));
        Assert.Equal(0.25f, parameter.Value<float>("confidence_threshold"));
        Assert.Equal(0.45f, parameter.Value<float>("iou_threshold"));
        Assert.True(parameter.Value<bool>("visualize"));
        Assert.False(parameter.Value<bool>("enable_log"));
    }

    [Fact]
    public void Constructor_DoesNotInitializeWhenModelMissing()
    {
        FakeOcrService ocrService = new();
        OCREngine ocrEngine = new(ocrService, CreateOcrConfig(useGpu: false));
        YOLOConfig config = CreateYoloConfig(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.onnx"));

        YOLOEngine engine = new(ocrService, config, ocrEngine);

        Assert.False(engine.IsInitialized);
        Assert.Contains("YOLO模型文件不存在", engine.InitializeMessage);
        Assert.Equal(["Init"], ocrService.Calls);
    }

    [Fact]
    public void Constructor_InitializesYoloWithRelativeModelsRoot()
    {
        string modelsRoot = Path.Combine("models", Guid.NewGuid().ToString("N"));
        string modelsDirectory = Path.Combine(AppContext.BaseDirectory, modelsRoot);
        Directory.CreateDirectory(modelsDirectory);
        string modelFile = Path.Combine(modelsDirectory, "relative-yolo.onnx");
        File.WriteAllText(modelFile, "model");
        FakeOcrService ocrService = new();
        OCREngine ocrEngine = new(ocrService, CreateOcrConfig(useGpu: false));
        YOLOConfig config = CreateYoloConfig("relative-yolo.onnx", modelsRoot: modelsRoot);

        YOLOEngine engine = new(ocrService, config, ocrEngine);

        Assert.True(engine.IsInitialized);
        Assert.Equal(modelFile, engine.ModelPath);
        Assert.Equal(modelFile, ocrService.YoloModelPath);
    }

    [Fact]
    public void Constructor_CapturesYoloInitializationFailure()
    {
        string modelFile = CreateModelFile();
        FakeOcrService ocrService = new() { YoloInitResult = false, Error = "init failed" };
        OCREngine ocrEngine = new(ocrService, CreateOcrConfig(useGpu: false));
        YOLOConfig config = CreateYoloConfig(modelFile);

        YOLOEngine engine = new(ocrService, config, ocrEngine);

        Assert.False(engine.IsInitialized);
        Assert.Equal("init failed", engine.InitializeMessage);
        Assert.Equal(["Init", "YoloInitJson", "GetError"], ocrService.Calls);
    }

    [Fact]
    public void Constructor_ActivatesLicenseBeforeInitializingGpuYolo()
    {
        string modelFile = CreateModelFile();
        string licenseFile = CreateLicenseFile();
        FakeOcrService ocrService = new();
        OCREngine ocrEngine = new(ocrService, CreateOcrConfig(useGpu: false, licenseFile));
        YOLOConfig config = CreateYoloConfig(modelFile, useGpu: true);

        YOLOEngine engine = new(ocrService, config, ocrEngine);

        Assert.True(engine.IsInitialized);
        Assert.Equal(["Init", "ActivateLicense", "YoloInitJson"], ocrService.Calls);
        Assert.Equal(licenseFile, ocrService.ActivatedLicenseFile);
    }

    private static YOLOConfig CreateYoloConfig(string modelPath, bool useGpu = false, string modelsRoot = "models")
    {
        return new YOLOConfig
        {
            models_root = modelsRoot,
            model_path = modelPath,
            model_type = 1,
            use_gpu = useGpu,
            gpu_id = 0,
            num_threads = 30,
            confidence_threshold = 0.25f,
            iou_threshold = 0.45f,
            visualize = true,
            enable_log = false
        };
    }

    private static OCRConfig CreateOcrConfig(bool useGpu, string? licenseFile = null)
    {
        return new OCRConfig
        {
            det_infer = "det.onnx",
            cls_infer = "cls.onnx",
            rec_infer = "rec.onnx",
            keyFile = "keys.txt",
            OCRLicense = licenseFile ?? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "paddleocr.lic"),
            use_gpu = useGpu
        };
    }

    private static string CreateModelFile()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        string modelFile = Path.Combine(tempDirectory, "yolov8s.onnx");
        File.WriteAllText(modelFile, "model");
        return modelFile;
    }

    private static string CreateLicenseFile()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        string licenseFile = Path.Combine(tempDirectory, "paddleocr.lic");
        File.WriteAllText(licenseFile, "license");
        return licenseFile;
    }

    private sealed class FakeOcrService : IOCRService
    {
        public List<string> Calls { get; } = [];

        public string? ActivatedLicenseFile { get; private set; }

        public string? YoloModelPath { get; private set; }

        public string? YoloParameterJson { get; private set; }

        public bool YoloInitResult { get; init; } = true;

        public string Error { get; init; } = string.Empty;

        public bool ActivateLicense(string licenseFile)
        {
            Calls.Add(nameof(ActivateLicense));
            ActivatedLicenseFile = licenseFile;
            return true;
        }

        public bool Init(InitParamater para)
        {
            Calls.Add(nameof(Init));
            return true;
        }

        public bool YoloInitJson(string modelPath, string parameterJson)
        {
            Calls.Add(nameof(YoloInitJson));
            YoloModelPath = modelPath;
            YoloParameterJson = parameterJson;
            return YoloInitResult;
        }

        public string GetError()
        {
            Calls.Add(nameof(GetError));
            return Error;
        }

        public string InitDefaultOCREngine(string modelsPath) => throw new NotImplementedException();

        public string GetLicenseRequestCode() => throw new NotImplementedException();

        public string GetLicenseStatus() => throw new NotImplementedException();

        public LicenseStatus GetLicenseStatusInfo() => throw new NotImplementedException();

        public OCRResult Detect(string imagefile) => throw new NotImplementedException();

        public OCRResult Detect(byte[] imagebyte) => throw new NotImplementedException();

        public OCRResult DetectMat(IntPtr ptr_cvmat) => throw new NotImplementedException();

        public OCRResult DetectBase64(string base64) => throw new NotImplementedException();

        public string YoloDetect(string imagefile) => throw new NotImplementedException();

        public string YoloDetect(byte[] imagebyte) => throw new NotImplementedException();

        public string YoloDetectMat(IntPtr ptr_cvmat) => throw new NotImplementedException();

        public string YoloDetectBase64(string base64) => throw new NotImplementedException();

        public YoloTensorResult YoloDetectTensor(string imagefile) => throw new NotImplementedException();

        public YoloTensorResult YoloDetectByteTensor(byte[] imagebyte) => throw new NotImplementedException();

        public YoloTensorResult YoloDetectMatTensor(IntPtr ptr_cvmat) => throw new NotImplementedException();

        public YoloTensorResult YoloDetectBase64Tensor(string base64) => throw new NotImplementedException();

        public string YoloFreeEngine() => throw new NotImplementedException();

        public string FreeEngine() => throw new NotImplementedException();
    }
}
