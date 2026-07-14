using CoreOCROnnx.SDK;
using CoreOCROnnx.WebApi;
using CoreOCROnnx.WebApi.Controllers;
using CoreOCROnnx.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreOCROnnx.WebApi.Tests;

public class YOLOServiceControllerTests
{
    [Fact]
    public async Task GetYOLOFileTensor_ReturnsBadResultWhenFileMissing()
    {
        YOLOServiceController controller = CreateController(new FakeOcrService(), initialized: true);

        ActionResult actionResult = await controller.GetYOLOFileTensor(null!);

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("识别失败:图片不存在！", result.ErrorMessage);
    }

    [Fact]
    public void GetYOLOBase64Tensor_ReturnsBadResultWhenBase64Missing()
    {
        YOLOServiceController controller = CreateController(new FakeOcrService(), initialized: true);

        ActionResult actionResult = controller.GetYOLOBase64Tensor(new RequestYoloBase64 { Base64String = "" });

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("识别失败:图片不存在！", result.ErrorMessage);
    }

    [Fact]
    public async Task GetYOLOFileTensor_ReturnsBadResultWhenEngineNotInitialized()
    {
        YOLOServiceController controller = CreateController(new FakeOcrService(), initialized: false);
        IFormFile file = CreateFormFile([1, 2, 3]);

        ActionResult actionResult = await controller.GetYOLOFileTensor(file);

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("YOLO模型文件不存在", result.ErrorMessage);
    }

    [Fact]
    public async Task GetYOLOFileTensor_CallsByteTensorAndReturnsResult()
    {
        FakeOcrService ocrService = new();
        YOLOServiceController controller = CreateController(ocrService, initialized: true);
        IFormFile file = CreateFormFile([1, 2, 3]);

        ActionResult actionResult = await controller.GetYOLOFileTensor(file);

        ApiResult result = GetApiResult(actionResult);
        YoloTensorResult tensor = Assert.IsType<YoloTensorResult>(result.Data);
        Assert.Equal(System.Net.HttpStatusCode.OK, result.Status);
        Assert.Equal(["Init", "YoloInitJson", "YoloDetectByteTensor"], ocrService.Calls);
        Assert.Equal([1, 2, 3], ocrService.LastImageBytes);
        Assert.Equal([1, 2, 3, 4], tensor.Data);
        Assert.Equal([1, 2, 2], tensor.Shape);
        Assert.Equal(3, tensor.ShapeLen);
        Assert.Equal(4, tensor.ElementCount);
    }

    [Fact]
    public void GetYOLOBase64Tensor_CallsBase64TensorAndReturnsResult()
    {
        FakeOcrService ocrService = new();
        YOLOServiceController controller = CreateController(ocrService, initialized: true);

        ActionResult actionResult = controller.GetYOLOBase64Tensor(new RequestYoloBase64 { Base64String = "base64" });

        ApiResult result = GetApiResult(actionResult);
        YoloTensorResult tensor = Assert.IsType<YoloTensorResult>(result.Data);
        Assert.Equal(System.Net.HttpStatusCode.OK, result.Status);
        Assert.Equal(["Init", "YoloInitJson", "YoloDetectBase64Tensor"], ocrService.Calls);
        Assert.Equal("base64", ocrService.LastBase64);
        Assert.Equal([1, 2, 3, 4], tensor.Data);
        Assert.Equal([1, 2, 2], tensor.Shape);
    }

    private static YOLOServiceController CreateController(FakeOcrService ocrService, bool initialized)
    {
        string modelPath = initialized ? CreateModelFile() : Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.onnx");
        OCREngine ocrEngine = new(ocrService, new OCRConfig
        {
            det_infer = "det.onnx",
            cls_infer = "cls.onnx",
            rec_infer = "rec.onnx",
            keyFile = "keys.txt",
            use_gpu = false
        });
        YOLOEngine yoloEngine = new(ocrService, new YOLOConfig { model_path = modelPath }, ocrEngine);
        return new YOLOServiceController(NullLogger<YOLOServiceController>.Instance, yoloEngine);
    }

    private static ApiResult GetApiResult(ActionResult actionResult)
    {
        OkObjectResult objectResult = Assert.IsType<OkObjectResult>(actionResult);
        return Assert.IsType<ApiResult>(objectResult.Value);
    }

    private static IFormFile CreateFormFile(byte[] data)
    {
        return new FormFile(new MemoryStream(data), 0, data.Length, "request", "test.jpg");
    }

    private static string CreateModelFile()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        string modelFile = Path.Combine(tempDirectory, "yolov8s.onnx");
        File.WriteAllText(modelFile, "model");
        return modelFile;
    }

    private sealed class FakeOcrService : IOCRService
    {
        private readonly YoloTensorResult tensorResult = new YoloTensorResult
        {
            Data = [1, 2, 3, 4],
            Shape = [1, 2, 2],
            ShapeLen = 3,
            ElementCount = 4
        };

        public List<string> Calls { get; } = [];

        public byte[]? LastImageBytes { get; private set; }

        public string? LastBase64 { get; private set; }

        public bool Init(InitParamater para)
        {
            Calls.Add(nameof(Init));
            return true;
        }

        public bool YoloInitJson(string modelPath, string parameterJson)
        {
            Calls.Add(nameof(YoloInitJson));
            return true;
        }

        public YoloTensorResult YoloDetectByteTensor(byte[] imagebyte)
        {
            Calls.Add(nameof(YoloDetectByteTensor));
            LastImageBytes = imagebyte;
            return tensorResult;
        }

        public YoloTensorResult YoloDetectBase64Tensor(string base64)
        {
            Calls.Add(nameof(YoloDetectBase64Tensor));
            LastBase64 = base64;
            return tensorResult;
        }

        public bool ActivateLicense(string licenseFile) => throw new NotImplementedException();

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

        public YoloTensorResult YoloDetectMatTensor(IntPtr ptr_cvmat) => throw new NotImplementedException();

        public string YoloFreeEngine() => throw new NotImplementedException();

        public string GetError() => string.Empty;

        public string FreeEngine() => throw new NotImplementedException();
    }
}
