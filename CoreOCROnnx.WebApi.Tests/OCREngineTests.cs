using CoreOCROnnx.SDK;
using CoreOCROnnx.WebApi.Services;

namespace CoreOCROnnx.WebApi.Tests;

public class OCREngineTests
{
    [Fact]
    public void Constructor_ActivatesExistingLicenseBeforeInitializingGpuOcr()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            string licenseFile = Path.Combine(tempDirectory, "paddleocr.lic");
            File.WriteAllText(licenseFile, "license");
            FakeOcrService ocrService = new();
            OCRConfig config = CreateConfig(licenseFile, useGpu: true);

            _ = new OCREngine(ocrService, config);

            Assert.Equal(["ActivateLicense", "Init"], ocrService.Calls);
            Assert.Equal(licenseFile, ocrService.ActivatedLicenseFile);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void Constructor_DoesNotActivateLicenseWhenGpuDisabled()
    {
        string licenseFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "paddleocr.lic");
        FakeOcrService ocrService = new();
        OCRConfig config = CreateConfig(licenseFile, useGpu: false);

        _ = new OCREngine(ocrService, config);

        Assert.Equal(["Init"], ocrService.Calls);
        Assert.Null(ocrService.ActivatedLicenseFile);
    }

    [Fact]
    public void Constructor_DoesNotInitializeGpuOcrWhenLicenseMissing()
    {
        string licenseFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "paddleocr.lic");
        FakeOcrService ocrService = new();
        OCRConfig config = CreateConfig(licenseFile, useGpu: true);

        _ = new OCREngine(ocrService, config);

        Assert.Empty(ocrService.Calls);
        Assert.Null(ocrService.ActivatedLicenseFile);
    }

    [Fact]
    public void Constructor_DoesNotInitializeGpuOcrWhenLicenseActivationFails()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            string licenseFile = Path.Combine(tempDirectory, "paddleocr.lic");
            File.WriteAllText(licenseFile, "license");
            FakeOcrService ocrService = new() { ActivateLicenseResult = false };
            OCRConfig config = CreateConfig(licenseFile, useGpu: true);

            _ = new OCREngine(ocrService, config);

            Assert.Equal(["ActivateLicense"], ocrService.Calls);
            Assert.Equal(licenseFile, ocrService.ActivatedLicenseFile);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }


    private static OCRConfig CreateConfig(string licenseFile, bool useGpu)
    {
        return new OCRConfig
        {
            det_infer = "det.onnx",
            cls_infer = "cls.onnx",
            rec_infer = "rec.onnx",
            keyFile = "keys.txt",
            OCRLicense = licenseFile,
            use_gpu = useGpu
        };
    }

    private sealed class FakeOcrService : IOCRService
    {
        public List<string> Calls { get; } = [];

        public string? ActivatedLicenseFile { get; private set; }

        public bool ActivateLicenseResult { get; init; } = true;

        public bool ActivateLicense(string licenseFile)
        {
            Calls.Add(nameof(ActivateLicense));
            ActivatedLicenseFile = licenseFile;
            return ActivateLicenseResult;
        }

        public bool Init(InitParamater para)
        {
            Calls.Add(nameof(Init));
            return true;
        }

        public string InitDefaultOCREngine(string modelsPath) => throw new NotImplementedException();

        public string GetLicenseRequestCode() => throw new NotImplementedException();

        public string GetLicenseStatus() => throw new NotImplementedException();

        public LicenseStatus GetLicenseStatusInfo() => throw new NotImplementedException();

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

        public string GetError() => throw new NotImplementedException();

        public string FreeEngine() => throw new NotImplementedException();
    }
}
