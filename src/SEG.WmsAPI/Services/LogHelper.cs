using NLog;
using SEG.WmsAPI.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ILogger = NLog.ILogger;
using LogLevel = NLog.LogLevel;

namespace SEG.WmsAPI.Services
{
    public class LogHelper
    {

        public static void CreateLog<T>(LogLevel loglevel, ILogger logger, string requestId, string content, string userId, string sysCode, string functionName,
                                        string versionNo, string tranType, ApiResponse<T> responseObject, string orderNo, string SystemNo,
                                        string warehouseCode, string storerCode, string externNumber, string logMessage)
        {
            var errorCode = "";

            if (responseObject != null && responseObject.errors != null && responseObject.errors.Count > 0)
            {
                errorCode = responseObject.errors[0].code;

                if (string.IsNullOrEmpty(logMessage))
                    logMessage = responseObject.errors[0].message;
            }


            logger.Log(loglevel, $"{sysCode} {functionName} {versionNo}, {tranType}, {requestId}, {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}, {content}, {userId}," +
                                 $"{warehouseCode},{storerCode}, {responseObject?.status ?? ""}, {logMessage}, {errorCode},{orderNo}, {SystemNo}");

        }

        public static void CreateLog<T>(LogLevel loglevel, ILogger logger, string requestId, JsonElement jsonElement, string userId, string sysCode, string functionName, 
                                        string versionNo, string tranType, ApiResponse<T> responseObject, string orderNo, string SystemNo, 
                                        string warehouseCode, string storerCode, string externNumber, string logMessage)
        {
            var errorCode = "";

            if (responseObject != null && responseObject.errors != null && responseObject.errors.Count > 0)
            {
                errorCode = responseObject.errors[0].code;

                if (string.IsNullOrEmpty(logMessage))
                    logMessage = responseObject.errors[0].message;
            }


            logger.Log(loglevel, $"{sysCode} {functionName} {versionNo}, {tranType}, {requestId}, {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}, {jsonElement}, {userId}, " +
                                 $"{warehouseCode},{storerCode}, {responseObject?.status??""}, {logMessage}, {errorCode},{orderNo}, {SystemNo}");
        }
    }
}