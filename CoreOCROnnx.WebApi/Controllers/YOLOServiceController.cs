using CoreOCROnnx.SDK;
using CoreOCROnnx.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreOCROnnx.WebApi.Controllers
{
    /// <summary>
    /// YOLO服务接口
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]/[action]")]
    public class YOLOServiceController : ActionBase
    {
        private readonly ILogger<YOLOServiceController> logger;
        private readonly YOLOEngine yoloEngine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_logger"></param>
        /// <param name="_yoloEngine"></param>
        public YOLOServiceController(ILogger<YOLOServiceController> _logger, YOLOEngine _yoloEngine)
        {
            logger = _logger;
            yoloEngine = _yoloEngine;
        }

        /// <summary>
        /// 获取YOLO初始化状态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Get()
        {
            return OKResult(new
            {
                initialized = yoloEngine.IsInitialized,
                modelPath = yoloEngine.ModelPath,
                message = yoloEngine.InitializeMessage
            });
        }

        /// <summary>
        /// YOLO识别，直接上传图片即可，无需保存图片，返回原始Tensor
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetYOLOFileTensor(IFormFile request)
        {
            if (!EnsureInitialized(out ObjectResult errorResult))
            {
                return errorResult;
            }
            if (request == null || request.Length == 0)
            {
                return BadResult("识别失败:图片不存在！");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                await request.CopyToAsync(ms);
                byte[] imageByte = ms.ToArray();
                logger.LogTrace($"获取到YOLO图片:{imageByte.Length}字节");
                YoloTensorResult result = yoloEngine.YoloService.YoloDetectByteTensor(imageByte);
                return OKResult(result);
            }
        }

        /// <summary>
        /// YOLO识别，上传图片Base64编码，返回原始Tensor
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetYOLOBase64Tensor([FromBody] RequestYoloBase64 request)
        {
            if (!EnsureInitialized(out ObjectResult errorResult))
            {
                return errorResult;
            }
            if (request == null || string.IsNullOrWhiteSpace(request.Base64String))
            {
                return BadResult("识别失败:图片不存在！");
            }

            YoloTensorResult result = yoloEngine.YoloService.YoloDetectBase64Tensor(request.Base64String);
            return OKResult(result);
        }

        private bool EnsureInitialized(out ObjectResult errorResult)
        {
            if (yoloEngine.IsInitialized)
            {
                errorResult = null!;
                return true;
            }

            string message = string.IsNullOrWhiteSpace(yoloEngine.InitializeMessage)
                ? "YOLO未初始化"
                : yoloEngine.InitializeMessage;
            errorResult = BadResult(message);
            return false;
        }
    }
}
