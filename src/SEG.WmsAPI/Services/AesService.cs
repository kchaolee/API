using Azure.Core;
using SEG.WmsAPI.Models.Common;
using SEG.WmsAPI.Models.Requests;
using SEG.WmsAPI.Models.Responses;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace SEG.WmsAPI.Services;

/// <summary>
/// AES 加解密服務介面
/// </summary>
public interface IAesService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    T DecryptRequest<T>(JsonElement jsonElement, ref string strDecryptedRequest) where T : class;
}

/// <summary>
/// AES 256 加解密服務實作
/// </summary>
public class AesService : IAesService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    /// <summary>
    /// 建構函式，從配置讀取 AES Key
    /// </summary>
    public AesService(IConfiguration configuration)
    {
        if (configuration != null)
        {
            var aesKeyString = configuration["Encryption:AesKey"]
                               ?? throw new InvalidOperationException("AES Key not configured");

            _key = EnsureKeyLength(StringToBytes(aesKeyString));
            _iv = ExtractIvFromKey(_key);
        }
    }

    /// <summary>
    /// 加密字串 (使用配置的 Key 和 IV)
    /// </summary>
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// 加密字串 (使用指定的 Key 和 IV)
    /// </summary>
    public static string Encrypt(string plainText, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// 解密字串 (使用配置的 Key 和 IV)
    /// </summary>
    public string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// 解密字串 (使用指定的 Key 和 IV)
    /// </summary>
    public static string Decrypt(string cipherText, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
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
            iv[i] = 0;
        return iv;
    }

    /// <summary>
    /// 確保 AES Key 至少有 32 bytes (256 bits)
    /// </summary>
    private byte[] EnsureKeyLength(byte[] key)
    {
        byte[] fullKey = new byte[32];
        int copyLength = Math.Min(key.Length, 32);
        Array.Copy(key, 0, fullKey, 0, copyLength);
        return fullKey;
    }

    /// <summary>
    /// 字串轉 byte[]
    /// </summary>
    private static byte[] StringToBytes(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    /// <summary>
    /// 將 JSON 物件加密並塞入 RequestData 結構
    /// </summary>
    public string EncryptRequest<T>(T jsonData, string aesKey) where T : class
    {
        var jsonString = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions
        {
            WriteIndented = false
        });

        var key = EnsureKeyLength(StringToBytes(aesKey));
        var iv = ExtractIvFromKey(key);

        var encryptedString = Encrypt(jsonString, key, iv);

        var outerRequest = new { RequestData = encryptedString };
        return JsonSerializer.Serialize(outerRequest, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    /// <summary>
    /// 從 RequestData 結構中解密回應
    /// </summary>
    public T DecryptRequest<T>(JsonElement jsonElement, ref string strDecryptedRequest) where T : class
    {
         
        var encryptedRequest = JsonSerializer.Deserialize<EncryptedRequest>(jsonElement.ToString(), new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNameCaseInsensitive = true
        });

        strDecryptedRequest = Decrypt(encryptedRequest.RequestData);
        T request = JsonSerializer.Deserialize<T>(strDecryptedRequest, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNameCaseInsensitive = true
        });

        // 將 Unicode 轉 中文
        strDecryptedRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNameCaseInsensitive = true
        });

        if (request == null)
        {
            throw new Exception("AES 加密資料解密異常");
        } 

        return request;
    }


    /// <summary>
    /// 從 ReturnData 結構中解密回應
    /// </summary>
    public T? DeserializeResponse<T>(string responseJson, string aesKey) where T : class
    {
        var outerResponse = JsonSerializer.Deserialize<ReturnDataWrapper>(responseJson);
        if (outerResponse?.ReturnData == null)
            throw new Exception("Invalid response format");

        var key = EnsureKeyLength(StringToBytes(aesKey));
        var iv = ExtractIvFromKey(key);

        var decryptedString = Decrypt(outerResponse.ReturnData, key, iv);

        return JsonSerializer.Deserialize<T>(decryptedString);
    }

    public class ReturnDataWrapper
    {
        public string ReturnData { get; set; } = string.Empty;
    }
}
