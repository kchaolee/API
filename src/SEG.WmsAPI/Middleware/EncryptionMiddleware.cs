using System.Text;
using System.Text.Json;
using SEG.WmsAPI.Models.Requests;

namespace SEG.WmsAPI.Middleware;

/// <summary>
/// 加密中介軟體 - 自動處理回應的加密包裝（ReturnData）
/// 請求的解密由各別控制器自行處理
/// </summary>
public class EncryptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EncryptionMiddleware> _logger;

    public EncryptionMiddleware(RequestDelegate next, ILogger<EncryptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 登入接口由控制器自行處理完整流程
        //if (context.Request.Path.Value?.Contains("/Auth/SignInVerification") == true)
        //{
        //    await _next(context);
        //    return;
        //}

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

            // 只有成功的回應（狀態碼 2xx）才需要加密
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                try
                {
                    // 加密原始回應
                    var encryptedData = aesService.Encrypt(originalResponse);

                    // 包裝在 ReturnData 結構中
                    var encryptedResponse = new EncryptedResponse
                    {
                        ReturnData = encryptedData
                    };

                    var responseJson = JsonSerializer.Serialize(encryptedResponse, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    context.Response.ContentLength = Encoding.UTF8.GetByteCount(responseJson);
                    context.Response.Body = originalBodyStream;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(responseJson);

                    _logger.LogInformation("Response encrypted and wrapped in ReturnData");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to encrypt or wrap response");
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
