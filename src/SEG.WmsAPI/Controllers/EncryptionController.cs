using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEG.WmsAPI.Services;
using System.Text.Json;

namespace SEG.WmsAPI.Controllers;

/// <summary>
/// 加密/解密工具控制器 - 提供前端工具頁面使用的 API
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class EncryptionController : ControllerBase
{
    private readonly IAesService _aesService;
    private readonly ILogger<EncryptionController> _logger;

    public EncryptionController(IAesService aesService, ILogger<EncryptionController> logger)
    {
        _aesService = aesService;
        _logger = logger;
    }

    /// <summary>
    /// 加密 JSON 文本
    /// </summary>
    /// <param name="request">加密請求</param>
    /// <returns>加密結果</returns>
    [HttpPost("encrypt")]
    public IActionResult Encrypt([FromBody] EncryptionRequest request)
    {
        try
        {
            _logger.LogInformation("開始加密文本，長度: {Length}", request.Text?.Length ?? 0);

            // 1. 驗證 AES Key
            if (string.IsNullOrEmpty(request.AesKey))
            {
                return BadRequest(new EncryptionResponse
                {
                    Success = false,
                    Message = "AES Key 不能為空"
                });
            }

            if (request.AesKey.Length < 22)
            {
                return BadRequest(new EncryptionResponse
                {
                    Success = false,
                    Message = "AES Key 長度至少需要 22 字元"
                });
            }

            // 2. 準備 AES Key 和 IV
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(request.AesKey);
            var key = EnsureKeyLength(keyBytes);
            var iv = ExtractIvFromKey(keyBytes);

            // 3. Minimize JSON
            string minimizedJson = MinimizeJson(request.Text ?? "");

            // 4. 加密
            string encryptedString = _aesService.Encrypt(minimizedJson);

            // 5. 包裝成 RequestData 格式
            //var wrappedData = new { RequestData = encryptedString };
            //string resultJson = JsonSerializer.Serialize(wrappedData, new JsonSerializerOptions
            //{
            //    WriteIndented = false
            //});

            _logger.LogInformation("加密成功，輸出長度: {Length}", encryptedString.Length);

            return Ok(new EncryptionResponse
            {
                Success = true,
                Message = "加密成功",
                Data = encryptedString
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加密失敗");
            return StatusCode(500, new EncryptionResponse
            {
                Success = false,
                Message = $"加密失敗: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 解密 JSON 文本
    /// </summary>
    /// <param name="request">解密請求</param>
    /// <returns>解密結果</returns>
    [HttpPost("decrypt")]
    public IActionResult Decrypt([FromBody] EncryptionRequest request)
    {
        try
        {
            _logger.LogInformation("開始解密文本，長度: {Length}", request.Text?.Length ?? 0);

            // 1. 驗證 AES Key
            if (string.IsNullOrEmpty(request.AesKey))
            {
                return BadRequest(new EncryptionResponse
                {
                    Success = false,
                    Message = "AES Key 不能為空"
                });
            }

            if (request.AesKey.Length < 22)
            {
                return BadRequest(new EncryptionResponse
                {
                    Success = false,
                    Message = "AES Key 長度至少需要 22 字元"
                });
            }

            // 2. 準備 AES Key 和 IV
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(request.AesKey);
            var key = EnsureKeyLength(keyBytes);
            var iv = ExtractIvFromKey(keyBytes);

            // 3. 提取加密資料
            string encryptedData = ExtractEncryptedData(request.Text ?? "");

            // 4. 解密
            string decryptedString = _aesService.Decrypt(encryptedData);

            _logger.LogInformation("解密成功，輸出長度: {Length}", decryptedString.Length);

            return Ok(new EncryptionResponse
            {
                Success = true,
                Message = "解密成功",
                Data = decryptedString
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解密失敗");
            return StatusCode(500, new EncryptionResponse
            {
                Success = false,
                Message = $"解密失敗: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 確保 AES Key 至少有 32 bytes
    /// </summary>
    private static byte[] EnsureKeyLength(byte[] key)
    {
        byte[] fullKey = new byte[32];
        int copyLength = Math.Min(key.Length, 32);
        Array.Copy(key, 0, fullKey, 0, copyLength);
        return fullKey;
    }

    /// <summary>
    /// 從 AES Key 中提取 IV (第 7~22 字元，索引 6~21)
    /// </summary>
    private static byte[] ExtractIvFromKey(byte[] key)
    {
        byte[] iv = new byte[16];
        int keyLength = Math.Min(key.Length, 22);
        int start = 6;

        for (int i = 0; i < 16 && (start + i) < keyLength; i++)
        {
            iv[i] = key[start + i];
        }

        // 如果 key 長度不足 22，用 0 補足
        for (int i = (keyLength - start); i < 16; i++)
        {
            iv[i] = 0;
        }

        return iv;
    }

    /// <summary>
    /// Minimize JSON（移除多餘空白）
    /// </summary>
    private static string MinimizeJson(string json)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            string minimized = JsonSerializer.Serialize(doc, new JsonSerializerOptions
            {
                WriteIndented = false
            });
            return minimized;
        }
        catch
        {
            // JSON 解析失敗，直接返回原始字串
            return json;
        }
    }

    /// <summary>
    /// 提取加密資料
    /// </summary>
    private static string ExtractEncryptedData(string inputText)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(inputText);

            // 檢查是否有 RequestData
            if (doc.RootElement.TryGetProperty("RequestData", out var requestData))
            {
                return requestData.GetString() ?? string.Empty;
            }

            // 檢查是否有 ReturnData
            if (doc.RootElement.TryGetProperty("ReturnData", out var returnData))
            {
                return returnData.GetString() ?? string.Empty;
            }

            // 如果都沒有，假設輸入就是純 Base64 字串
            return inputText;
        }
        catch
        {
            // JSON 解析失敗，假設輸入就是純 Base64 字串
            return inputText;
        }
    }
}

/// <summary>
/// 加密/解密請求
/// </summary>
public class EncryptionRequest
{
    public string AesKey { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// 加密/解密回應
/// </summary>
public class EncryptionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; }
}
