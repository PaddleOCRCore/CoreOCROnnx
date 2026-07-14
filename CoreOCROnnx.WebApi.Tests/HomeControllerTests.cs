using CoreOCROnnx.SDK;
using CoreOCROnnx.WebApi;
using CoreOCROnnx.WebApi.Controllers;
using CoreOCROnnx.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreOCROnnx.WebApi.Tests;

public class HomeControllerTests
{
    [Fact]
    public void GetLicenseRequestCode_ReturnsRequestCode()
    {
        FakeOcrService ocrService = new() { RequestCode = "request-code" };
        HomeController controller = CreateController(ocrService);

        IActionResult actionResult = controller.GetLicenseRequestCode();

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.OK, result.Status);
        Assert.Equal("request-code", ReadStringProperty(result.Data, "requestCode"));
    }

    [Fact]
    public async Task UploadLicense_ReturnsBadResultWhenFileMissing()
    {
        HomeController controller = CreateController(new FakeOcrService());

        IActionResult actionResult = await controller.UploadLicense(null!);

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("授权文件不能为空。", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadLicense_ReturnsBadResultWhenExtensionUnsupported()
    {
        HomeController controller = CreateController(new FakeOcrService());
        IFormFile file = CreateFormFile([1, 2, 3], "license.txt");

        IActionResult actionResult = await controller.UploadLicense(file);

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("请上传.lic授权文件。", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadLicense_ReturnsBadResultWhenLicenseInvalid()
    {
        FakeOcrService ocrService = new() { ActivateLicenseResult = false, Error = "invalid" };
        HomeController controller = CreateController(ocrService);
        IFormFile file = CreateFormFile([1, 2, 3], "paddleocr.lic");

        IActionResult actionResult = await controller.UploadLicense(file);

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("授权文件无效，未保存到Models目录。", result.ErrorMessage);
        Assert.Equal(["Init", "ActivateLicense", "GetLicenseStatusInfo", "GetError"], ocrService.Calls);
    }

    private static HomeController CreateController(FakeOcrService ocrService)
    {
        OCRConfig config = new()
        {
            det_infer = "det.onnx",
            cls_infer = "cls.onnx",
            rec_infer = "rec.onnx",
            keyFile = "keys.txt",
            OCRLicense = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "paddleocr.lic"),
            use_gpu = false
        };
        OCREngine ocrEngine = new(ocrService, config);
        return new HomeController(NullLogger<HomeController>.Instance, ocrService, ocrEngine, config);
    }

    private static ApiResult GetApiResult(IActionResult actionResult)
    {
        Microsoft.AspNetCore.Mvc.JsonResult jsonResult = Assert.IsType<Microsoft.AspNetCore.Mvc.JsonResult>(actionResult);
        return Assert.IsType<ApiResult>(jsonResult.Value);
    }

    private static IFormFile CreateFormFile(byte[] data, string fileName)
    {
        return new FormFile(new MemoryStream(data), 0, data.Length, "file", fileName);
    }

    private static string? ReadStringProperty(object data, string propertyName)
    {
        return data.GetType().GetProperty(propertyName)?.GetValue(data)?.ToString();
    }

    private sealed class FakeOcrService : IOCRService
    {
        public List<string> Calls { get; } = [];

        public string RequestCode { get; init; } = string.Empty;

        public bool ActivateLicenseResult { get; init; } = true;

        public string Error { get; init; } = string.Empty;

        public bool ActivateLicense(string licenseFile)
        {
            Calls.Add(nameof(ActivateLicense));
            return ActivateLicenseResult;
        }

        public bool Init(InitParamater para)
        {
            Calls.Add(nameof(Init));
            return true;
        }

        public string GetLicenseRequestCode()
        {
            Calls.Add(nameof(GetLicenseRequestCode));
            return RequestCode;
        }

        public LicenseStatus GetLicenseStatusInfo()
        {
            Calls.Add(nameof(GetLicenseStatusInfo));
            return new LicenseStatus { Activated = ActivateLicenseResult };
        }

        public string GetError()
        {
            Calls.Add(nameof(GetError));
            return Error;
        }

        public string InitDefaultOCREngine(string modelsPath) => throw new NotImplementedException();

        public string GetLicenseStatus() => throw new NotImplementedException();

        public OCRResult Detect(string imagefile) => throw new NotImplementedException();

        public OCRResult Detect(byte[] imagebyte) => throw new NotImplementedException();

        public OCRResult DetectMat(IntPtr ptr_cvmat) => throw new NotImplementedException();

        public OCRResult DetectBase64(string base64) => throw new NotImplementedException();

        public bool YoloInitJson(string modelPath, string parameterJson) => throw new NotImplementedException();

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