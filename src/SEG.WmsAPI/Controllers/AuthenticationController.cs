using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using NLog;
using SEG.WmsAPI.Models.Common;
using SEG.WmsAPI.Models.Requests;
using SEG.WmsAPI.Models.Responses;
using SEG.WmsAPI.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogLevel = NLog.LogLevel;

namespace SEG.WmsAPI.Controllers;

/// <summary>
/// 驗證控制器 - 處理登入驗證相關接口
/// </summary>
[ApiController]
[Route("")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAesService _aesService;
    private readonly NLog.ILogger _logger;

    public AuthenticationController(IAuthService authService, IAesService aesService)
    {
        _authService = authService;
        _aesService = aesService;
        _logger = LogManager.GetLogger("A1-SignInVerification");
    }

    /// <summary>
    /// A1 - 登入驗證
    /// POST /wmService/v1/Auth/SignInVerification
    /// </summary>
    [HttpPost("wmService/v1/Auth/SignInVerification")]
    public async Task<IActionResult> SignInVerification([FromBody] JsonElement jsonElement)
    {
        string requestId = "";
        string userAccount = "";
        string strDecryptedRequest = "";
        string strErrMsg = string.Empty;
        Models.Requests.LoginRequest loginRequest = null;
        try
        {
            //記錄初始請求內容
            Services.LogHelper.CreateLog<LoginResponseData>(LogLevel.Trace, _logger, requestId, jsonElement, userAccount, "A1", "SignInVerification", "", "", null, "", "", "", "", "", "請求原始加密資料");

            #region 解密請求內容
            try
            {
                loginRequest = _aesService.DecryptRequest<Models.Requests.LoginRequest>(jsonElement, ref strDecryptedRequest);

                if (loginRequest == null || string.IsNullOrEmpty(loginRequest.requestId))
                {
                    strErrMsg = "請求資料解密異常";
                    Services.LogHelper.CreateLog<LoginResponseData>(LogLevel.Error, _logger, requestId, jsonElement, "", "A1", "SignInVerification", "", "", null, "", "", "", "", "", strErrMsg);
                    return BadRequest(ApiResponse<object>.Fail(requestId, "請求格式錯誤", "F981", strErrMsg));
                }

                requestId = loginRequest.requestId;
                userAccount = loginRequest.Account;
                Services.LogHelper.CreateLog<LoginResponseData>(LogLevel.Trace, _logger, requestId, JsonSerializer.Serialize(loginRequest), userAccount, "A1", "SignInVerification", "", "", null, "", "", "", "", "", "請求解密後資料");

            }
            catch (Exception ex)
            {
                strErrMsg = $"請求資料解密異常 {ex.Message}";
                Services.LogHelper.CreateLog<LoginResponseData>(LogLevel.Error, _logger, requestId, jsonElement, userAccount, "A1", "SignInVerification", "", "", null, "", "", "", "", "", strErrMsg);
                return BadRequest(ApiResponse<object>.Fail(requestId, "解密失敗", "F009", strErrMsg));
            }
            #endregion

            #region 驗證用戶
            if (!_authService.ValidateUserAccount(loginRequest.Account, loginRequest.Password))
            {
                strErrMsg = "失敗-驗證失敗";
                Services.LogHelper.CreateLog<LoginResponseData>(LogLevel.Error, _logger, requestId, JsonSerializer.Serialize(loginRequest), loginRequest.Account, "A1", "SignInVerification", "", "", null, "", "", "", "", "", strErrMsg);
                return BadRequest(ApiResponse<object>.Fail(
                    loginRequest.requestId,
                    "失敗-驗證失敗",
                    new List<ErrorDetail> { new ErrorDetail { code = "F119", message = strErrMsg } }
                ));
            }
            #endregion

            #region 產生 JWT Token
            var token = _authService.GenerateToken(loginRequest.Account);
            var tokenValidationResult = _authService.ValidateJwtToken(token);
            string expires = "";
            if (tokenValidationResult != null && tokenValidationResult.IsValid)
            {
                expires = tokenValidationResult.Expiration?.ToString("yyyy/MM/dd HH:mm:ss") ?? "";
            }
            else
            {
                strErrMsg = "JWT Token 產生發生異常";
                Services.LogHelper.CreateLog<LoginResponseData>(LogLevel.Error, _logger, requestId, JsonSerializer.Serialize(loginRequest), userAccount, "A1", "SignInVerification", "", "", null, "", "", "", "", "", strErrMsg);

                return BadRequest(ApiResponse<object>.Fail(
                   loginRequest.requestId,
                   "失敗-驗證失敗",
                   new List<ErrorDetail> { new ErrorDetail { code = "F119", message = strErrMsg } }
               ));
            }
            #endregion

            // 登入回應資料
            var responseData = new LoginResponseData
            {
                AccessToken = token,
                Expires = expires
            };

            Services.LogHelper.CreateLog<LoginResponseData>(LogLevel.Trace, _logger, requestId, $"Token:{token}", userAccount, "A1", "SignInVerification", "", "", null, "", "", "", "", "", "使用者驗證成功取得 Token.");
            
            HttpContext.Items["requestId"] = loginRequest.requestId;
            return Ok(ApiResponse<LoginResponseData>.Success(
                loginRequest.requestId,
                "登入成功",
                responseData
            ));
        }
        catch (Exception ex)
        {
            strErrMsg = $"驗證異常: {ex.Message}";
            Services.LogHelper.CreateLog<LoginResponseData>(LogLevel.Error, _logger, requestId, jsonElement, userAccount, "A1", "SignInVerification", "", "", null, "", "", "", "", "", strErrMsg);
            return StatusCode(500, ApiResponse<object>.Fail("", "伺服器內部錯誤", "F999", "伺服器處理異常"));
        }
    }

    public partial class ReturnDataWrapper
    {
        [JsonPropertyName("ReturnData")]
        public string ReturnData { get; set; } = string.Empty;
    }
}
