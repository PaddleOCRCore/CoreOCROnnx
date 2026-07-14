using CoreOCROnnx.WebApi;
using CoreOCROnnx.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreOCROnnx.WebApi.Tests;

public class OCRDemoControllerTests
{
    [Fact]
    public async Task Analyze_ReturnsBadResultWhenModelUnsupported()
    {
        OCRDemoController controller = CreateController();
        IFormFile file = CreateFormFile([1, 2, 3], "test.jpg");

        ActionResult actionResult = await controller.Analyze(file, "pp-structure");

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("不支持的解析模型", result.ErrorMessage);
    }

    [Fact]
    public async Task Analyze_ReturnsBadResultWhenFileMissing()
    {
        OCRDemoController controller = CreateController();

        ActionResult actionResult = await controller.Analyze(null!, "pp-ocrv6");

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("图片不存在！", result.ErrorMessage);
    }

    [Fact]
    public async Task Analyze_ReturnsBadResultWhenExtensionUnsupported()
    {
        OCRDemoController controller = CreateController();
        IFormFile file = CreateFormFile([1, 2, 3], "test.gif");

        ActionResult actionResult = await controller.Analyze(file, "yolo");

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("仅支持PNG/JPG/JPEG/BMP/TIF/TIFF图片", result.ErrorMessage);
    }

    [Fact]
    public async Task Analyze_ReturnsBadResultWhenImageDataInvalid()
    {
        OCRDemoController controller = CreateController();
        IFormFile file = CreateFormFile([1, 2, 3], "test.jpg");

        ActionResult actionResult = await controller.Analyze(file, "ppocrv6");

        ApiResult result = GetApiResult(actionResult);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("图片格式无效或无法读取尺寸", result.ErrorMessage);
    }

    private static OCRDemoController CreateController()
    {
        return new OCRDemoController(null!, null!, new YOLOConfig());
    }

    private static ApiResult GetApiResult(ActionResult actionResult)
    {
        OkObjectResult objectResult = Assert.IsType<OkObjectResult>(actionResult);
        return Assert.IsType<ApiResult>(objectResult.Value);
    }

    private static IFormFile CreateFormFile(byte[] data, string fileName)
    {
        return new FormFile(new MemoryStream(data), 0, data.Length, "file", fileName);
    }
}