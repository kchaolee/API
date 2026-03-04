using System.Text.Json.Serialization;

namespace SEG.WmsAPI.Models.Requests;

/// <summary>
/// 登入請求（內層，加密前）
/// </summary>
public class LoginRequest : Models.Common.RequestBase
{
    [JsonPropertyName("account")]
    public string Account { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 外層加密請求包裝
/// </summary>
public class EncryptedRequest
{
    [JsonPropertyName("RequestData")]
    public string RequestData { get; set; } = string.Empty;
}

/// <summary>
/// 外層加密回應包裝
/// </summary>
public class EncryptedResponse
{
    [JsonPropertyName("ReturnData")]
    public string ReturnData { get; set; } = string.Empty;
}
