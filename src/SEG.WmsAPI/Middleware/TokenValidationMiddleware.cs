using System.Text.Json;
using SEG.WmsAPI.Models.Common;

namespace SEG.WmsAPI.Middleware;

/// <summary>
/// Token 驗證中間件 - 檢查 Bearer Token 是否存在且有效
/// </summary>
public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;

    public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // 登入接口和加密工具接口不需要 Token 驗證
        if (path.Contains("/Auth/") || path.Contains("/api/Encryption/"))
        {
            await _next(context);
            return;
        }

        // 檢查 Authorization Header
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader))
        {
            _logger.LogWarning("請求缺少 Authorization Header: {Path}", path);
            await ReturnUnauthorized(context, ".Authorization Header 不存在");
            return;
        }

        // 檢查是否有 "Bearer " 前綴
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Authorization Header 格式不正確: {Path}", path);
            await ReturnUnauthorized(context, "Authorization Header 格式應為 'Bearer {token}'");
            return;
        }

        // 檢查 Token 是否為空
        var token = authHeader.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Token 為空: {Path}", path);
            await ReturnUnauthorized(context, "Token 為空");
            return;
        }

        // 通過檢查，繼續處理請求
        await _next(context);
    }

    /// <summary>
    /// 返回未授權的錯誤回應
    /// </summary>
    private static async Task ReturnUnauthorized(HttpContext context, string errorMessage)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        // 使用自定義 API 回應格式
        var errorResponse = ApiResponse<object>.Fail(
            "",
            "失敗-驗證失效",
            "F119",
            errorMessage
        );

        var responseJson = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(responseJson);
    }
}
