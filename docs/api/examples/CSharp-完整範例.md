# C# API 整合範例

本文提供 C# 完整的 API 整合範例,從登入到完成收貨確認。

## 1. 專案結構

```
WmsApiClient/
├── Models/                         # 資料模型
│   ├── ApiResponseModel.cs
│   ├── LoginResponseModel.cs
│   ├── ReceivingHeaderModel.cs
│   ├── ReceivingDetailModel.cs
│   └── ConfirmReceiptModel.cs
├── Services/                       # 服務類別
│   ├── ApiService.cs               # API 呼叫服務
│   └── AesHelper.cs                # AES 加解密工具
├── Data/                           # 資料存取
│   └── TokenManager.cs             # Token 管理
└── Program.cs                      # 主程式
```

## 2. AES 加解密工具 (AesHelper.cs)

```csharp
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WmsApiClient.Services
{
    public static class AesHelper
    {
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

        public static byte[] HexStringToBytes(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}
```

## 3. 資料模型 (Models/ApiResponseModel.cs)

```csharp
using System.Collections.Generic;

namespace WmsApiClient.Models
{
    public class ApiResponse<T>
    {
        public string RequestId { get; set; }
        public string Status { get; set; }  // S = 成功, F = 失敗
        public string Message { get; set; }
        public T Data { get; set; }
        public List<ApiError> Errors { get; set; }
    }

    public class ApiError
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
```

## 4. 登入回應模型 (Models/LoginResponseModel.cs)

```csharp
namespace WmsApiClient.Models
{
    public class LoginData
    {
        public string AccessToken { get; set; }
        public string Expires { get; set; }
    }
}
```

## 5. 收貨表頭模型 (Models/ReceivingHeaderModel.cs)

```csharp
namespace WmsApiClient.Models
{
    public class ReceivingHeader
    {
        public string WarehouseCode { get; set; }
        public string WmsAsnNumber { get; set; }
        public string StorerCode { get; set; }
        public string ExternReceiptNumber { get; set; }
        public string VendorName { get; set; }
    }

    public class R1Request
    {
        public string RequestId { get; set; }
        public string StorerCode { get; set; }
        public string DocStatus { get; set; }
    }

    public class R1Data
    {
        public List<ReceivingHeader> HeaderList { get; set; }
    }
}
```

## 6. 收貨明細模型 (Models/ReceivingDetailModel.cs)

```csharp
namespace WmsApiClient.Models
{
    public class ReceivingDetail
    {
        public string LineNo { get; set; }
        public string ExternLineNo { get; set; }
        public string Sku { get; set; }
        public string Descr { get; set; }
        public string ExpiryDate { get; set; }
        public string PackQty { get; set; }
        public string FishingGroundName { get; set; }
        public List<ColumnConfig> ItemColumnConfig { get; set; }
    }

    public class ColumnConfig
    {
        public string ColName { get; set; }
        public bool ColRequired { get; set; }
    }

    public class R2Request
    {
        public string RequestId { get; set; }
        public string WmsAsnNumber { get; set; }
        public string StorerCode { get; set; }
        public string ExternReceiptNumber { get; set; }
    }

    public class R2Data
    {
        public List<ReceivingDetail> ItemList { get; set; }
    }
}
```

## 7. 收貨輸入模型 (Models/ReceivingInputModel.cs)

```csharp
namespace WmsApiClient.Models
{
    public class R3Request
    {
        public string RequestId { get; set; }
        public string RequestFnName { get; set; }  // "3.2確認收貨" 或 "3.3修改"
        public string WmsAsnNumber { get; set; }
        public string StorerCode { get; set; }
        public string ExternReceiptNumber { get; set; }
        public string LineNo { get; set; }
        public string ExternLineNo { get; set; }
        public string Sku { get; set; }
        public string ExpiryDate { get; set; }
        public string PackQty { get; set; }
        public string Qty { get; set; }
        public string BatchNumber { get; set; }
        public string MfgDate { get; set; }
        public string StorageStatus { get; set; }
        public string StockType { get; set; }
        public string Other { get; set; }
        public string Other1 { get; set; }
    }
}
```

## 8. 核對資料模型 (Models/VerifyDataModel.cs)

```csharp
namespace WmsApiClient.Models
{
    public class VerifyData
    {
        public string WarehouseCode { get; set; }
        public string WmsAsnNumber { get; set; }
        public string StorerCode { get; set; }
        public string ExternReceiptNumber { get; set; }
        public string PoTotalQty { get; set; }
        public string RecTotalPackQty { get; set; }
        public List<VerifyDetail> VerifyList { get; set; }
        public List<Location> LocList { get; set; }
    }

    public class VerifyDetail
    {
        public string LineNo { get; set; }
        public string ExternLineNo { get; set; }
        public string Sku { get; set; }
        public string ItemName { get; set; }
        public string ExpiryDate { get; set; }
        public string RecPackQty { get; set; }
    }

    public class Location
    {
        public string LocCode { get; set; }
    }

    public class R4Request
    {
        public string RequestId { get; set; }
        public string WmsAsnNumber { get; set; }
        public string StorerCode { get; set; }
        public string ExternReceiptNumber { get; set; }
    }
}
```

## 9. 確認收貨模型 (Models/ConfirmReceiptModel.cs)

```csharp
namespace WmsApiClient.Models
{
    public class ConfirmReceiptResult
    {
        public string WarehouseCode { get; set; }
        public string WmsAsnNumber { get; set; }
        public string StorerCode { get; set; }
        public string ExternReceiptNumber { get; set; }
        public string VendorName { get; set; }
        public string AsnFishingGroundName { get; set; }
        public string RecPalletQty { get; set; }
        public string AsnTotalQty { get; set; }
        public string AsnTotalUom { get; set; }
        public string RecTotalPackQty { get; set; }
        public List<PalletLabel> PalletLabelList { get; set; }
    }

    public class PalletLabel
    {
        public string LblExternReceiptNumber { get; set; }
        public string LblVendorName { get; set; }
        public string LblPalletQty { get; set; }
        public string LblLocCode { get; set; }
        public string LblSku { get; set; }
        public string LblItemName { get; set; }
    }

    public class R5Request
    {
        public string RequestId { get; set; }
        public string WmsAsnNumber { get; set; }
        public string StorerCode { get; set; }
        public string ExternReceiptNumber { get; set; }
        public string RecPalletQty { get; set; }
        public string RecLocCode { get; set; }
    }
}
```

## 10. Token 管理 (Data/TokenManager.cs)

```csharp
using System;
using System.IO;

namespace WmsApiClient.Data
{
    public static class TokenManager
    {
        private static readonly string TokenFilePath = "token.json";

        public static string AccessToken { get; private set; }
        public static string Expires { get; private set; }

        public static void SaveToken(string accessToken, string expires)
        {
            AccessToken = accessToken;
            Expires = expires;
            File.WriteAllText(TokenFilePath, $"{{\"accessToken\":\"{accessToken}\",\"expires\":\"{expires}\"}}");
        }

        public static void LoadToken()
        {
            if (File.Exists(TokenFilePath))
            {
                var json = File.ReadAllText(TokenFilePath);
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                AccessToken = data.accessToken;
                Expires = data.expires;
            }
        }

        public static bool IsTokenExpired()
        {
            if (string.IsNullOrEmpty(Expires))
                return true;

            if (DateTime.TryParse(Expires, out DateTime expiryDate))
            {
                return DateTime.Now >= expiryDate;
            }
            return true;
        }
    }
}
```

## 11. API 服務 (Services/ApiService.cs)

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WmsApiClient.Data;
using WmsApiClient.Models;

namespace WmsApiClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly byte[] _aesKey;
        private readonly byte[] _aesIv;

        public ApiService(string baseUrl, string keyHex, string ivHex)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _aesKey = AesHelper.HexStringToBytes(keyHex);
            _aesIv = AesHelper.HexStringToBytes(ivHex);
        }

        // A1 - 登入驗證
        public async Task<ApiResponse<LoginData>> LoginAsync(string account, string password)
        {
            var requestId = Guid.NewGuid().ToString();
            var requestBody = new
            {
                requestId,
                account,
                password
            };

            return await PostAsync<LoginData>("/Login", null, requestBody);
        }

        // R1 - 取得預期收貨清單
        public async Task<ApiResponse<R1Data>> GetReceivingHeadersAsync()
        {
            var requestId = Guid.NewGuid().ToString();
            var requestBody = new R1Request
            {
                RequestId = requestId,
                StorerCode = "97286918",
                DocStatus = "OPEN"
            };

            return await PostAsync<R1Data>("/PO/PoHeaderData", TokenManager.AccessToken, requestBody);
        }

        // R2 - 取得收貨明細資料
        public async Task<ApiResponse<R2Data>> GetReceivingDetailsAsync(string wmsAsnNumber, string storerCode, string externReceiptNumber)
        {
            var requestId = Guid.NewGuid().ToString();
            var requestBody = new R2Request
            {
                RequestId = requestId,
                WmsAsnNumber = wmsAsnNumber,
                StorerCode = storerCode,
                ExternReceiptNumber = externReceiptNumber
            };

            return await PostAsync<R2Data>("/PO/PoDetailData", TokenManager.AccessToken, requestBody);
        }

        // R3 - 回傳收貨資料
        public async Task<ApiResponse<object>> SaveReceivingItemAsync(R3Request request)
        {
            if (string.IsNullOrEmpty(request.RequestId))
                request.RequestId = Guid.NewGuid().ToString();

            return await PostAsync<object>("/PO/PoReceivingItem", TokenManager.AccessToken, request);
        }

        // R4 - 取得收貨核對明細
        public async Task<ApiResponse<VerifyData>> GetReceivingVerifyDataAsync(string wmsAsnNumber, string storerCode, string externReceiptNumber)
        {
            var requestId = Guid.NewGuid().ToString();
            var requestBody = new R4Request
            {
                RequestId = requestId,
                WmsAsnNumber = wmsAsnNumber,
                StorerCode = storerCode,
                ExternReceiptNumber = externReceiptNumber
            };

            return await PostAsync<VerifyData>("/PO/PoVerifying", TokenManager.AccessToken, requestBody);
        }

        // R5 - 回傳全收確認
        public async Task<ApiResponse<ConfirmReceiptResult>> ConfirmReceiptAsync(R5Request request)
        {
            if (string.IsNullOrEmpty(request.RequestId))
                request.RequestId = Guid.NewGuid().ToString();

            return await PostAsync<ConfirmReceiptResult>("/PO/PoCfmReceipt", TokenManager.AccessToken, request);
        }

        private async Task<ApiResponse<T>> PostAsync<T>(string endpoint, string token, object requestBody)
        {
            try
            {
                // 序列化請求
                var jsonBody = JsonConvert.SerializeObject(requestBody);

                // AES 加密
                var encryptedBody = AesHelper.Encrypt(jsonBody, _aesKey, _aesIv);

                // 建立請求
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Content = new StringContent(encryptedBody, Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                // 傳送請求
                var response = await _httpClient.SendAsync(request);

                // 讀取回應
                var responseContent = await response.Content.ReadAsStringAsync();

                // AES 解密
                var decryptedContent = AesHelper.Decrypt(responseContent, _aesKey, _aesIv);

                // 反序列化回應
                var result = JsonConvert.DeserializeObject<ApiResponse<T>>(decryptedContent);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API 呼叫失敗: {ex.Message}");
                throw;
            }
        }
    }
}
```

## 12. 主程式範例 (Program.cs)

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using WmsApiClient.Models;
using WmsApiClient.Services;

namespace WmsApiClient
{
    class Program
    {
        // 配置資訊 (請依照實際環境修改)
        private const string BaseUrl = "http://192.168.x.x/wmService/v1";
        private const string Account = "user001";
        private const string Password = "password123";

        // AES Key 和 IV (請依照系統提供的值設定)
        private const string AesKeyHex = "Your32ByteKeyHere1234567890123456";  // 64個16進位字元
        private const string AesIvHex = "Your16ByteIVHere";                  // 32個16進位字元

        static async Task Main(string[] args)
        {
            var apiService = new ApiService(BaseUrl, AesKeyHex, AesIvHex);

            try
            {
                // 1. 登入取得 Token
                Console.WriteLine("=== 步驟 1: 登入 ===");
                var loginResult = await apiService.LoginAsync(Account, Password);
                if (loginResult.Status != "S")
                {
                    Console.WriteLine($"登入失敗: {loginResult.Message}");
                    return;
                }

                Console.WriteLine($"登入成功! Token 有效期至: {loginResult.Data.Expires}");
                Data.TokenManager.SaveToken(loginResult.Data.AccessToken, loginResult.Data.Expires);

                // 2. 取得預期收貨清單 (R1)
                Console.WriteLine("\n=== 步驟 2: 取得預期收貨清單 ===");
                var headersResult = await apiService.GetReceivingHeadersAsync();
                if (headersResult.Status != "S" || headersResult.Data?.HeaderList == null || headersResult.Data.HeaderList.Count == 0)
                {
                    Console.WriteLine("無待收貨採購單");
                    return;
                }

                Console.WriteLine($"找到 {headersResult.Data.HeaderList.Count} 筆採購單:");
                foreach (var header in headersResult.Data.HeaderList)
                {
                    Console.WriteLine($"  - 採購單號: {header.ExternReceiptNumber}, 供應商: {header.VendorName}");
                }

                // 選擇第一個採購單
                var selectedHeader = headersResult.Data.HeaderList[0];
                Console.WriteLine($"\n選擇採購單: {selectedHeader.ExternReceiptNumber}");

                // 3. 取得收貨明細 (R2)
                Console.WriteLine("\n=== 步驟 3: 取得收貨明細 ===");
                var detailsResult = await apiService.GetReceivingDetailsAsync(
                    selectedHeader.WmsAsnNumber,
                    selectedHeader.StorerCode,
                    selectedHeader.ExternReceiptNumber
                );

                if (detailsResult.Status != "S" || detailsResult.Data?.ItemList == null)
                {
                    Console.WriteLine("取得收貨明細失敗");
                    return;
                }

                Console.WriteLine($"找到 {detailsResult.Data.ItemList.Count} 筆明細:");
                foreach (var detail in detailsResult.Data.ItemList)
                {
                    Console.WriteLine($"  - 項次: {detail.LineNo}, 貨號: {detail.Sku}, 品名: {detail.Descr}");
                }

                // 4. 確認收貨 (R3)
                Console.WriteLine("\n=== 步驟 4: 輸入收貨資料 ===");
                foreach (var detail in detailsResult.Data.ItemList)
                {
                    var receivingRequest = new R3Request
                    {
                        WmsAsnNumber = selectedHeader.WmsAsnNumber,
                        StorerCode = selectedHeader.StorerCode,
                        ExternReceiptNumber = selectedHeader.ExternReceiptNumber,
                        LineNo = detail.LineNo,
                        ExternLineNo = detail.ExternLineNo,
                        Sku = detail.Sku,
                        ExpiryDate = detail.ExpiryDate ?? "2025/12/31",
                        PackQty = "100",
                        RequestFnName = "3.2確認收貨"
                    };

                    // 根據 itemColumnConfig 決定哪些欄位需要填寫
                    foreach (var config in detail.ItemColumnConfig)
                    {
                        if (config.ColRequired)
                        {
                            switch (config.ColName)
                            {
                                case "qty":
                                    receivingRequest.Qty = "50.5";
                                    break;
                                case "batchNumber":
                                    receivingRequest.BatchNumber = "BATCH001";
                                    break;
                            }
                        }
                    }

                    var receivingResult = await apiService.SaveReceivingItemAsync(receivingRequest);
                    if (receivingResult.Status == "S")
                    {
                        Console.WriteLine($"✓ 項次 {detail.LineNo} 收貨資料已儲存");
                    }
                    else
                    {
                        Console.WriteLine($"✗ 項次 {detail.LineNo} 儲存失敗: {receivingResult.Message}");
                    }
                }

                // 5. 取得核對明細 (R4)
                Console.WriteLine("\n=== 步驟 5: 取得核對明細 ===");
                var verifyResult = await apiService.GetReceivingVerifyDataAsync(
                    selectedHeader.WmsAsnNumber,
                    selectedHeader.StorerCode,
                    selectedHeader.ExternReceiptNumber
                );

                if (verifyResult.Status != "S" || verifyResult.Data == null)
                {
                    Console.WriteLine("取得核對明細失敗");
                    return;
                }

                Console.WriteLine($"PO 總需求量: {verifyResult.Data.PoTotalQty} KG");
                Console.WriteLine($"已收總件數: {verifyResult.Data.RecTotalPackQty} 件");
                Console.WriteLine($"\n可選儲位:");
                foreach (var loc in verifyResult.Data.LocList)
                {
                    Console.WriteLine($"  - {loc.LocCode}");
                }

                // 選擇第一個儲位
                var selectedLoc = verifyResult.Data.LocList[0].LocCode;
                Console.WriteLine($"\n選擇儲位: {selectedLoc}");

                // 6. 確認全收 (R5)
                Console.WriteLine("\n=== 步驟 6: 確認全收 ===");
                var confirmRequest = new R5Request
                {
                    WmsAsnNumber = selectedHeader.WmsAsnNumber,
                    StorerCode = selectedHeader.StorerCode,
                    ExternReceiptNumber = selectedHeader.ExternReceiptNumber,
                    RecPalletQty = "5",
                    RecLocCode = selectedLoc
                };

                var confirmResult = await apiService.ConfirmReceiptAsync(confirmRequest);
                if (confirmResult.Status == "S" && confirmResult.Data != null)
                {
                    Console.WriteLine("✓ 收貨確認成功!");
                    Console.WriteLine($"總板數: {confirmResult.Data.RecPalletQty}");
                    Console.WriteLine($"單據總數量: {confirmResult.Data.AsnTotalQty} {confirmResult.Data.AsnTotalUom}");
                    Console.WriteLine($"已收總件數: {confirmResult.Data.RecTotalPackQty}");
                    Console.WriteLine($"\n板標籤資料:");
                    foreach (var label in confirmResult.Data.PalletLabelList)
                    {
                        Console.WriteLine($"  - 採購單號: {label.LblExternReceiptNumber}");
                        Console.WriteLine($"    供應商: {label.LblVendorName}");
                        Console.WriteLine($"    板數: {label.LblPalletQty}");
                        Console.WriteLine($"    儲位: {label.LblLocCode}");
                        Console.WriteLine($"    貨號: {label.LblSku}");
                        Console.WriteLine($"    品名: {label.LblItemName}");
                    }
                }
                else
                {
                    Console.WriteLine($"✓ 收貨確認失敗: {confirmResult.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程式執行失敗: {ex.Message}");
                Console.WriteLine($"詳細錯誤: {ex.StackTrace}");
            }
        }
    }
}
```

## 13. 專案配置 (WmsApiClient.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <RootNamespace>WmsApiClient</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>

</Project>
```

## 14. 建置與執行

### 安裝依賴

```bash
dotnet add package Newtonsoft.Json
```

### 執行程式

```bash
dotnet run
```

## 15. 配置說明

在執行程式前,請修改以下配置:

1. **BaseUrl**: WMS API 的基礎 URL
2. **Account / Password**: 登入帳號密碼
3. **AesKeyHex**: AES 金鑰 (64個16進位字元,即 32 bytes)
4. **AesIvHex**: 初始化向量 (32個16進位字元,即 16 bytes)

```csharp
private const string BaseUrl = "http://192.168.x.x/wmService/v1";
private const string Account = "your_account";
private const string Password = "your_password";
private const string AesKeyHex = "Your32ByteKeyHere1234567890123456";
private const string AesIvHex = "Your16ByteIVHere";
```

## 16. 錯誤處理

程式已包含基本的錯誤處理機制:

```csharp
try
{
    // API 呼叫
}
catch (Exception ex)
{
    Console.WriteLine($"發生錯誤: {ex.Message}");
    // 應用回應邏輯
}
```

常見錯誤處理:

- **F119**: Token 失效 → 重新登入
- **F981**: 資料格式錯誤 → 檢查輸入資料
- **F983**: 業務邏輯錯誤 → 檢查狀態邏輯

## 17. 注意事項

1. **AES 金鑰管理**: 金鑰和 IV 應妥善保管,建議使用配置文件或環境變數
2. **安全性**: 使用 HTTPS 傳輸加密資料
3. **Token 過期**: 檢查 Token 有效期,過期時重新登入
4. **RequestId**: 每個請求使用唯一的 GUID
5. **條件式必填欄位**: 依 R2 回傳的 `itemColumnConfig` 決定哪些欄位必填

## 18. 相關文件

- [加密說明](../encryption/加密說明.md) - 詳細加密規格
- [A1 - 登入驗證](../authentication/A1-登入驗證.md) - 登入接口詳情
- [R1-R5 進貨作業](../receiving/R1-R5-進貨作業總覽.md) - 進貨作業流程
