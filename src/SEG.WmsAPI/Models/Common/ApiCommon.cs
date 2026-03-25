using Microsoft.Extensions.Logging;
using NLog;
using System.ComponentModel.DataAnnotations;

namespace SEG.WmsAPI.Models.Common;

/// <summary>
/// 通用的 API 回應格式
/// </summary>
/// <typeparam name="T">回應資料的類型</typeparam>
public class ApiResponse<T>
{
    public string requestId { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;  // S:成功, L:請重新登入, F:失敗, U:伺服器儲存失敗
    public string message { get; set; } = string.Empty;
    public T? data { get; set; }
    public List<ErrorDetail>? errors { get; set; }

    /// <summary>
    /// 建立成功的回應
    /// </summary>
    public static ApiResponse<T> Success(string requestId, string message, T data)
    {
        return new ApiResponse<T>
        {
            requestId = requestId,
            status = "S",
            message = message,
            data = data,
            errors = null
        };
    }

    /// <summary>
    /// 建立失敗的回應
    /// </summary>
    public static ApiResponse<T> Fail(string requestId, string message, List<ErrorDetail>? errors = null)
    {
        return new ApiResponse<T>
        {
            requestId = requestId,
            status = "F",
            message = message,
            data = default,
            errors = errors ?? new List<ErrorDetail>()
        };
    }

    /// <summary>
    /// 建立失敗的回應（帶單個錯誤）
    /// </summary>
    public static ApiResponse<T> Fail(string requestId, string message, string errorCode, string errorMessage)
    {
        return new ApiResponse<T>
        {
            requestId = requestId,
            status = "F",
            message = message,
            data = default,
            errors = new List<ErrorDetail> { new ErrorDetail { code = errorCode, message = errorMessage } }
        };
    }
}

/// <summary>
/// 錯誤詳情
/// </summary>
public class ErrorDetail
{
    public string code { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
}

/// <summary>
/// 請求基類
/// </summary>
public class RequestBase
{
    public string requestId { get; set; } = string.Empty;
}


public static class ValidateHelper
{
    internal static Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 檢查數據是否符合 Model 定義
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="toValidateData"></param>
    /// <returns></returns>
    public static string ValidateModel<T>(T toValidateData)
    {
        var validationContext = new ValidationContext(toValidateData, null, null);
        var validationResults = new List<ValidationResult>();
        var isValidate = Validator.TryValidateObject(toValidateData, validationContext, validationResults, true);
        if (validationResults.Count > 0)
            return string.Join("、", validationResults.Select(r => r.ErrorMessage)); //修改為回傳所有訊息 validationResults[0].ErrorMessage.ToString();

        return "";
    }
}