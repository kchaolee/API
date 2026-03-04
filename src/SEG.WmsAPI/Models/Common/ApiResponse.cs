namespace SEG.WmsAPI.Models.Common;

/// <summary>
/// 通用的 API 回應格式
/// </summary>
/// <typeparam name="T">回應資料的類型</typeparam>
public class ApiResponse<T>
{
    public string RequestId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;  // S:成功, L:請重新登入, F:失敗, U:伺服器儲存失敗
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ErrorDetail>? Errors { get; set; }

    /// <summary>
    /// 建立成功的回應
    /// </summary>
    public static ApiResponse<T> Success(string requestId, string message, T data)
    {
        return new ApiResponse<T>
        {
            RequestId = requestId,
            Status = "S",
            Message = message,
            Data = data,
            Errors = null
        };
    }

    /// <summary>
    /// 建立失敗的回應
    /// </summary>
    public static ApiResponse<T> Fail(string requestId, string message, List<ErrorDetail>? errors = null)
    {
        return new ApiResponse<T>
        {
            RequestId = requestId,
            Status = "F",
            Message = message,
            Data = default,
            Errors = errors ?? new List<ErrorDetail>()
        };
    }

    /// <summary>
    /// 建立失敗的回應（帶單個錯誤）
    /// </summary>
    public static ApiResponse<T> Fail(string requestId, string message, string errorCode, string errorMessage)
    {
        return new ApiResponse<T>
        {
            RequestId = requestId,
            Status = "F",
            Message = message,
            Data = default,
            Errors = new List<ErrorDetail> { new ErrorDetail { Code = errorCode, Message = errorMessage } }
        };
    }
}

/// <summary>
/// 錯誤詳情
/// </summary>
public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 請求基類
/// </summary>
public class RequestBase
{
    public string RequestId { get; set; } = string.Empty;
}
