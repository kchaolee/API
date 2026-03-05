using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using SEG.WmsAPI.Models.Common;
using SEG.WmsAPI.Models.Requests;
using SEG.WmsAPI.Models.Responses;
using SEG.WmsAPI.Services;

namespace SEG.WmsAPI.Controllers;

/// <summary>
/// 驗證控制器 - 處理登入驗證相關接口
/// </summary>
[ApiController]
[Route("")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IAuthService authService, ILogger<AuthenticationController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// A1 - 登入驗證
    /// POST /wmService/v1/Auth/SignInVerification
    /// </summary>
    [HttpPost("wmService/v1/Auth/SignInVerification")]
    public async Task<IActionResult> SignInVerification([FromBody] JsonElement jsonElement)
    {
        try
        {
            // 注意：登入接口也使用加密，需要先解密
            var encryptedRequest = JsonSerializer.Deserialize<EncryptedRequest>(jsonElement.ToString(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (encryptedRequest == null || string.IsNullOrEmpty(encryptedRequest.RequestData))
            {
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", "RequestData 為失"));
            }

            // 解密內層請求
            string decryptedRequest;
            try
            {
                var aesService = HttpContext.RequestServices.GetRequiredService<IAesService>();
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                decryptedRequest = aesService.Decrypt(encryptedRequest.RequestData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " decryption failed");
                return BadRequest(ApiResponse<object>.Fail("", "解密失敗", "F009", "JSON 解析失敗"));
            }

            var loginRequest = JsonSerializer.Deserialize<LoginRequest>(decryptedRequest, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.requestId))
            {
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", "requestId 為失"));
            }

            // 測試使用
            if (!(loginRequest.Account == "user001" &&  loginRequest.Password == "password123"))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    loginRequest.requestId,
                    "失敗-驗證失敗",
                    new List<ErrorDetail> { new ErrorDetail { code = "F119",message = "驗證失敗" } }
                ));
            }


            //if (!_authService.ValidateAccount(loginRequest.Account, loginRequest.Password))
            //{
            //    return BadRequest(ApiResponse<object>.Fail(
            //        loginRequest.requestId,
            //        "失敗-驗證失敗",
            //        new List<ErrorDetail> { new ErrorDetail { code = "F119", message = "驗證失敗" } }
            //    ));
            //}

            var token = _authService.GenerateToken(loginRequest.Account);
            var expires = DateTime.UtcNow.AddHours(24).ToString("yyyy/MM/dd HH:mm:ss");

            // 構造登入回應資料
            var responseData = new LoginResponseData
            {
                AccessToken = token,
                Expires = expires
            };

            return Ok(ApiResponse<LoginResponseData>.Success(
                loginRequest.requestId,
                "登入成功",
                responseData
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignInVerification error");
            return StatusCode(500, ApiResponse<object>.Fail("", "伺服器內部錯誤", "F999", "伺服器處理異常"));
        }
    }

    public partial class ReturnDataWrapper
    {
        [JsonPropertyName("ReturnData")]
        public string ReturnData { get; set; } = string.Empty;
    }
}
