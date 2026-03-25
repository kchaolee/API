using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;
using SEG.WmsAPI.Models.Common;
using SEG.WmsAPI.Models.Requests;
using SEG.WmsAPI.Models.Responses;
using SEG.WmsAPI.Services;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace SEG.WmsAPI.Middleware;

/// <summary>
/// Token 驗證中間件 - 檢查 Bearer Token 是否存在且有效
/// </summary>
public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly NLog.ILogger _logger;
   
    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
        _logger = LogManager.GetLogger("TokenValidationMiddleware");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var strErrMsg = "";
        var path = context.Request.Path.Value ?? "";

        // 登入接口和加密工具接口不需要 Token 驗證
        if (path.Contains("/Auth/") || path.Contains("/api/Encryption/"))
        {
            await _next(context);
            return;
        }

        // 檢查 Authorization Header
        var token = context.Request.Headers.Authorization.FirstOrDefault();
        var authService = context.RequestServices.GetRequiredService<IAuthService>();
        if (authService == null)
        {
            strErrMsg = $"Token authService 為空";
            Services.LogHelper.CreateLog<LoginResponseData>(NLog.LogLevel.Error, _logger, "", token, "", "", "Token驗證中間件 TokenValidationMiddleware", "", "", null, "", "", "", "", "", strErrMsg);
            await ReturnUnauthorized(context, strErrMsg);
            return;
        }

        // 檢查 Token 是否為空
        if (string.IsNullOrEmpty(token))
        {
            strErrMsg = $"Token 為空";
            Services.LogHelper.CreateLog<LoginResponseData>(NLog.LogLevel.Error, _logger, "", token, "", "", "Token驗證中間件 TokenValidationMiddleware", "", "", null, "", "", "", "", "", strErrMsg);
            await ReturnUnauthorized(context, strErrMsg);
            return;
        }
        if(token.StartsWith("Bearer "))
            token = token.Substring("Bearer ".Length).Trim();

 
        var tokenValidationResult = authService.ValidateJwtToken(token);
        if (tokenValidationResult.IsValid == false)
        {
            strErrMsg = $"token 驗證失敗: {tokenValidationResult.ErrorMessage}";
            Services.LogHelper.CreateLog<LoginResponseData>(NLog.LogLevel.Error, _logger, "", token, "", "", "Token驗證中間件 TokenValidationMiddleware", "", "", null, "", "", "", "", "", strErrMsg);
            await ReturnUnauthorized(context, strErrMsg);
            return;
        }

        context.Items["TokenValidationResult"] = tokenValidationResult;

        // 通過通過檢查，繼續處理請求
        await _next(context);
    }

    /// <summary>
    /// 返回未授權的錯誤回應
    /// </summary>
    private static async Task ReturnUnauthorized(HttpContext context, string errorMessage)
    {
        var aesService = new Services.AesService(context.RequestServices.GetRequiredService<IConfiguration>());
        var originalBodyStream = context.Response.Body;

        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        // 使用自定義 API 回應格式
        var errorResponse = ApiResponse<object>.Fail(
            "",
            "失敗-驗證失效",
            "F119",
            errorMessage
        );

        // 加密原始回應
        var errorResponseString = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = false
        });
        var encryptedData = aesService.Encrypt(errorResponseString);

        // 包裝在 ReturnData 結構中
        var encryptedResponse = new EncryptedResponse
        {
            ReturnData = encryptedData
        };

        var responseJson = JsonSerializer.Serialize(encryptedResponse, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = false
        });

        context.Response.ContentLength = Encoding.UTF8.GetByteCount(responseJson);
        context.Response.Body = originalBodyStream;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(responseJson);
         
        return;
    }
}
