using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace CoreOCROnnx.SDK
{
    internal static class OcrServiceHelper
    {
        internal static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include
        };

        internal static string ReadNativeString(Func<IntPtr> getResult, Action<IntPtr> freeResult)
        {
            IntPtr ptrResult = IntPtr.Zero;
            try
            {
                ptrResult = getResult();
                return PtrToString(ptrResult);
            }
            finally
            {
                FreeNativeBuffer(ptrResult, freeResult);
            }
        }

        internal static string PtrToString(IntPtr ptr)
        {
            return ptr == IntPtr.Zero ? string.Empty : MarshalUtf8.PtrToStringUTF8(ptr);
        }

        internal static string GetLastError(Func<IntPtr> getError, Action<IntPtr> freeResult)
        {
            try
            {
                return ReadNativeString(getError, freeResult);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        internal static LicenseStatus DeserializeLicenseStatus(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<LicenseStatus>(json, JsonSettings);
        }

        internal static T DeserializeObject<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }

            return (T)JsonConvert.DeserializeObject(json, typeof(T), JsonSettings);
        }

        internal static string GetNativeStringResult(
            IntPtr ptrResult,
            Func<string> getLastError,
            Action<IntPtr> freeResult,
            string operationName,
            Func<string, Exception> createException)
        {
            if (ptrResult == IntPtr.Zero)
            {
                string lastErr = getLastError();
                if (!string.IsNullOrEmpty(lastErr))
                {
                    throw createException(operationName + "内部错误：" + lastErr);
                }
                return string.Empty;
            }

            try
            {
                return PtrToString(ptrResult);
            }
            catch (Exception ex)
            {
                throw createException(operationName + "结果转换失败:" + ex.Message);
            }
            finally
            {
                FreeNativeBuffer(ptrResult, freeResult);
            }
        }

        internal static YoloTensorResult GetYoloTensorResult(
            bool ok,
            IntPtr dataPtr,
            long[] shape,
            int shapeLen,
            long elementCount,
            Func<string> getLastError,
            Action<IntPtr> freeTensor,
            string operationName,
            Func<string, Exception> createException)
        {
            try
            {
                if (!ok || dataPtr == IntPtr.Zero)
                {
                    string lastErr = getLastError();
                    if (string.IsNullOrEmpty(lastErr))
                    {
                        lastErr = "未知错误";
                    }
                    throw createException(operationName + "失败: " + lastErr);
                }
                if (shape == null || shapeLen < 0 || shapeLen > shape.Length)
                {
                    throw createException(operationName + "失败: 张量Shape长度无效");
                }
                if (elementCount < 0 || elementCount > int.MaxValue)
                {
                    throw createException(operationName + "失败: 张量元素数量超出托管数组支持范围");
                }

                float[] data = new float[(int)elementCount];
                if (elementCount > 0)
                {
                    Marshal.Copy(dataPtr, data, 0, (int)elementCount);
                }

                long[] resultShape = new long[shapeLen];
                Array.Copy(shape, resultShape, shapeLen);
                return new YoloTensorResult
                {
                    Data = data,
                    Shape = resultShape,
                    ShapeLen = shapeLen,
                    ElementCount = elementCount
                };
            }
            finally
            {
                FreeNativeBuffer(dataPtr, freeTensor);
            }
        }

        private static void FreeNativeBuffer(IntPtr ptr, Action<IntPtr> freeResult)
        {
            if (ptr != IntPtr.Zero)
            {
                freeResult(ptr);
            }
        }
    }
}
