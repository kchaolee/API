using Azure.Core;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using NLog;
using SEG.WmsAPI.Models.Requests;
using SEG.WmsAPI.Models.Responses;
using SEG.WmsAPI.Services;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace SEG.WmsAPI.Middleware;

/// <summary>
/// 加密中介軟體 - 自動處理回應的加密包裝（ReturnData）
/// 請求的解密由各別控制器自行處理
/// </summary>
public class EncryptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly NLog.ILogger _logger;

    public EncryptionMiddleware(RequestDelegate next, ILogger<EncryptionMiddleware> logger)
    {
        _next = next;
        _logger = LogManager.GetLogger("EncryptionMiddleware");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userAccount = "";
        var requestId = "";

        // AesTest.Html
        if (context.Request.Path.Value?.Contains("/api/Encryption/encrypt") == true ||
            context.Request.Path.Value?.Contains("/api/Encryption/decrypt") == true)
        {
            await _next(context);
            return;
        }

        // 攔截並處理回應，包裝在 ReturnData 中並加密
        var encryptionSettings = context.RequestServices.GetRequiredService<Services.EncryptionSettings>();
        var aesService = new Services.AesService(context.RequestServices.GetRequiredService<IConfiguration>());

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var originalResponse = await new StreamReader(memoryStream).ReadToEndAsync();

            // 回應加密
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode <= 500)
            {
                try
                {
                    // 獲取 requestId
                    requestId = context.Items["requestId"]?.ToString() ?? "";
                    var tokenValidationResult = context.Items["TokenValidationResult"] as SEG.WmsAPI.Services.TokenValidationResult;
                    userAccount = tokenValidationResult?.Account ?? "";
                    Services.LogHelper.CreateLog<LoginResponseData>(NLog.LogLevel.Trace, _logger, requestId, originalResponse, userAccount, "", "加解密中間件 EncryptionMiddleware", "", "", null, "", "", "", "", "", "加密前的回應內容");

                    // 加密原始回應
                    var encryptedData = aesService.Encrypt(originalResponse);

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

                    Services.LogHelper.CreateLog<LoginResponseData>(NLog.LogLevel.Trace, _logger, requestId, encryptedData, userAccount, "", "加解密中間件 EncryptionMiddleware", "", "", null, "", "", "", "", "", "加密後的回應內容");

                    context.Response.ContentLength = Encoding.UTF8.GetByteCount(responseJson);
                    context.Response.Body = originalBodyStream;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(responseJson);
                     
                }
                catch (Exception ex)
                {
                    var strErrMsg = $"回應資料加密失敗: {ex.Message}";
                    Services.LogHelper.CreateLog<LoginResponseData>(NLog.LogLevel.Error, _logger, requestId, originalResponse, userAccount, "A1", "SignInVerification", "", "", null, "", "", "", "", "", strErrMsg);

                    // 加密失敗時，返回原始回應
                    context.Response.Body = originalBodyStream;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(originalResponse);
                }
            }
            else
            {
                // 錯誤回應直接返回，不加密
                context.Response.Body = originalBodyStream;
                await context.Response.WriteAsync(originalResponse);
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}
