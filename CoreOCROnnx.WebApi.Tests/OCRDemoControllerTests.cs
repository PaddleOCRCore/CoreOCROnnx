using System.Net;
using CoreOCROnnx.SDK;
using CoreOCROnnx.WebApi.Controllers;
using CoreOCROnnx.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CoreOCROnnx.WebApi.Tests;

public class OCRDemoControllerTests
{
    [Fact]
    public async Task Analyze_ReturnsBadRequest_WhenModelIsUnsupported()
    {
        OCRDemoController controller = CreateController(new OCRResult());

        ActionResult actionResult = await controller.Analyze(CreateFile("demo.png", CreatePngBytes()), "unknown");

        ApiResult result = AssertApiResult(actionResult);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("不支持的解析模型", result.ErrorMessage);
    }

    [Fact]
    public async Task Analyze_ReturnsBadRequest_WhenFileIsEmpty()
    {
        OCRDemoController controller = CreateController(new OCRResult());

        ActionResult actionResult = await controller.Analyze(CreateFile("demo.png", []), "pp-ocrv6");

        ApiResult result = AssertApiResult(actionResult);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("图片不存在！", result.ErrorMessage);
    }

    [Fact]
    public async Task Analyze_ReturnsBadRequest_WhenExtensionIsUnsupported()
    {
        OCRDemoController controller = CreateController(new OCRResult());

        ActionResult actionResult = await controller.Analyze(CreateFile("demo.gif", [1, 2, 3]), "ppocrv6");

        ApiResult result = AssertApiResult(actionResult);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("仅支持PNG/JPG/JPEG/BMP/TIF/TIFF图片", result.ErrorMessage);
    }

    [Fact]
    public async Task Analyze_ReturnsBadRequest_WhenImageCannotBeRead()
    {
        OCRDemoController controller = CreateController(new OCRResult());

        ActionResult actionResult = await controller.Analyze(CreateFile("demo.png", [1, 2, 3]), "pp-ocrv6");

        ApiResult result = AssertApiResult(actionResult);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("图片格式无效或无法读取尺寸", result.ErrorMessage);
    }

    [Fact]
    public async Task Analyze_ReturnsPpOcrResult_WithImageSizeAndTextBoxes()
    {
        OCRResult ocrResult = new()
        {
            StrRes = "测试文本",
            JsonText = "{\"ok\":true}",
            TextBlocks =
            [
                new CoreOCROnnx.SDK.JsonResult
                {
                    Text = "测试文本",
                    BoxScore = "0.95",
                    Boxes =
                    [
                        new OCRLocation { x = 10, y = 20 },
                        new OCRLocation { x = 110, y = 20 },
                        new OCRLocation { x = 110, y = 60 },
                        new OCRLocation { x = 10, y = 60 }
                    ]
                }
            ]
        };
        OCRDemoController controller = CreateController(ocrResult);

        ActionResult actionResult = await controller.Analyze(CreateFile("demo.png", CreatePngBytes()), "ppocrv6");

        ApiResult result = AssertApiResult(actionResult);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        OCRDemoAnalyzeResult data = Assert.IsType<OCRDemoAnalyzeResult>(result.Data);
        Assert.Equal("pp-ocrv6", data.Model);
        Assert.Equal("PP-OCRv6", data.ModelName);
        Assert.Equal("demo.png", data.FileName);
        Assert.Equal("测试文本", data.Content);
        Assert.Equal("{\"ok\":true}", data.JsonText);
        Assert.Equal(1, data.ImageWidth);
        Assert.Equal(1, data.ImageHeight);

        OCRDemoBox box = Assert.Single(data.Boxes);
        Assert.True(box.IsTextLine);
        Assert.Equal("测试文本", box.Text);
        Assert.Equal(10, box.X);
        Assert.Equal(20, box.Y);
        Assert.Equal(100, box.Width);
        Assert.Equal(40, box.Height);
        Assert.Equal(0.95, box.Score, precision: 2);
        Assert.Equal(4, box.Points.Count);
    }

    private static OCRDemoController CreateController(OCRResult ocrResult)
    {
        FakeOcrService ocrService = new() { DetectResult = ocrResult };
        OCRConfig ocrConfig = new()
        {
            models_root = "models",
            det_infer = "det.onnx",
            cls_infer = "cls.onnx",
            rec_infer = "rec.onnx",
            keyFile = "keys.txt",
            use_gpu = false
        };
        YOLOConfig yoloConfig = new()
        {
            models_root = "models",
            model_path = "missing-yolo.onnx",
            use_gpu = false
        };
        OCREngine ocrEngine = new(ocrService, ocrConfig);
        YOLOEngine yoloEngine = new(ocrService, yoloConfig, ocrEngine);
        return new OCRDemoController(ocrEngine, yoloEngine, yoloConfig);
    }

    private static ApiResult AssertApiResult(ActionResult actionResult)
    {
        ObjectResult objectResult = Assert.IsType<OkObjectResult>(actionResult);
        return Assert.IsType<ApiResult>(objectResult.Value);
    }

    private static IFormFile CreateFile(string fileName, byte[] bytes)
    {
        MemoryStream stream = new(bytes);
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
    }

    private static byte[] CreatePngBytes()
    {
        using Image<Rgba32> image = new(1, 1);
        using MemoryStream stream = new();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    private sealed class FakeOcrService : IOCRService
    {
        public OCRResult DetectResult { get; init; } = new();

        public bool ActivateLicense(string licenseFile) => true;

        public bool Init(InitParamater para) => true;

        public string InitDefaultOCREngine(string modelsPath) => string.Empty;

        public string GetLicenseRequestCode() => string.Empty;

        public string GetLicenseStatus() => string.Empty;

        public LicenseStatus GetLicenseStatusInfo() => new();

        public OCRResult Detect(string imagefile) => DetectResult;

        public OCRResult Detect(byte[] imagebyte) => DetectResult;

        public OCRResult DetectMat(IntPtr ptr_cvmat) => DetectResult;

        public OCRResult DetectBase64(string base64) => DetectResult;

        public bool YoloInitJson(string modelPath, string parameterJson) => true;

        public string YoloDetect(string imagefile) => string.Empty;

        public string YoloDetect(byte[] imagebyte) => string.Empty;

        public string YoloDetectMat(IntPtr ptr_cvmat) => string.Empty;

        public string YoloDetectBase64(string base64) => string.Empty;

        public YoloTensorResult YoloDetectTensor(string imagefile) => new();

        public YoloTensorResult YoloDetectByteTensor(byte[] imagebyte) => new();

        public YoloTensorResult YoloDetectMatTensor(IntPtr ptr_cvmat) => new();

        public YoloTensorResult YoloDetectBase64Tensor(string base64) => new();

        public string YoloFreeEngine() => string.Empty;

        public string GetError() => string.Empty;

        public string FreeEngine() => string.Empty;
    }
}