# SEG WMS API

用於倉儲管理系統的 API，提供認證與進貨作業功能。

## 專案資訊

- .NET 8.0 Web API
- 使用 AES 256 加密的請求/回應
- JWT Bearer Token 認證
- 支援的介面：A1（登入）、R1-R5（進貨作業）

## 功能特點

1. **AES 256 加密**
   - 所有 API 請求和回應都經過加密處理
   - IV（初始化向量）從 AES Key 的第 7-22 字元提取
   - 使用 `RequestData` 和 `ReturnData` 欄位包裝加密封裝

2. **JWT 認證**
   - 登入成功後返回 JWT Token
   - Token 有效期預設為 24 小時
   - 所有 API（除登入外）都需要在 Header 中攜帶 `Authorization: Bearer {token}`

3. **回應格式**
   - 狀態碼：S（成功）、L（需重新登入）、F（失敗）、U（伺服器錯誤）
   - 統一的 API 回應結構

## 專案結構

```
src/SEG.WmsAPI/
├── Controllers/
│   ├── AuthenticationController.cs    # A1 登入認證
│   └── POController.cs                  # R1-R5 進貨作業
├── Middleware/
│   └── EncryptionMiddleware.cs          # 加密處理中介軟體
├── Models/
│   ├── Common/
│   │   └── ApiResponse.cs               # 統一回應模型
│   ├── Requests/
│   │   ├── CommonRequests.cs            # 通用請求模型
│   │   └── ReceivingRequests.cs         # R1-R5 請求模型
│   └── Responses/
│       ├── LoginResponses.cs            # A1 回應模型
│       ├── R1Responses.cs               # R1 回應模型
│       ├── R2Responses.cs               # R2 回應模型
│       ├── R3Responses.cs               # R3 回應模型
│       ├── R4Responses.cs               # R4 回應模型
│       └── R5Responses.cs               # R5 回應模型
├── Services/
│   ├── AesService.cs                    # AES 加解密服務
│   └── AuthService.cs                   # JWT 認證服務
├── appsettings.json                     # 設定檔
└── Program.cs                           # 程式進入點
```

## API 端點

### A1 - 登入認證

```
POST /wmService/v1/Auth/SignInVerification
```

**請求格式：**
```json
{
  "RequestData": "AES加密後的數據"
}
```

**內層請求（加密前）：**
```json
{
  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",
  "account": "user001",
  "password": "password123"
}
```

**回應（成功）：**
```json
{
  "status": "S",
  "message": "登入成功",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "expires": "2024-12-11 23:59:59"
  }
}
```

### R1 - 取得預期收貨清單

```
POST /wmService/v1/PO/PoHeaderData
Authorization: Bearer {token}
```

### R2 - 取得收貨明細資料

```
POST /wmService/v1/PO/PoDetailData
Authorization: Bearer {token}
```

### R3 - 回傳收貨資料

```
POST /wmService/v1/PO/PoReceivingItem
Authorization: Bearer {token}
```

### R4 - 取得收貨核對明細

```
POST /wmService/v1/PO/PoVerifying
Authorization: Bearer {token}
```

### R5 - 回傳全收確認

```
POST /wmService/v1/PO/PoCfmReceipt
Authorization: Bearer {token}
```

## 設定說明

編輯 `appsettings.json` 檔案：

```json
{
  "Jwt": {
    "Secret": "YourVeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong!",
    "Issuer": "SEG.WmsAPI",
    "Audience": "SEG.WmsAPI",
    "ExpiryHours": 24
  },
  "Encryption": {
    "AesKey": "ThisIsASecretKeyForAESEncryptionThatIs32Chars!"
  }
}
```

**重要：**
- AES Key 至少需要 32 字元
- IV 會從 AES Key 的第 7-22 字元自動提取
- JWT Secret 至少需要 32 字元

## 建置與執行

### 1. 還原套件

```bash
dotnet restore
```

### 2. 建置專案

```bash
dotnet build
```

### 3. 執行專案

開發環境：
```bash
dotnet run
```

生產環境：
```bash
dotnet run --environment Production
```

API 預設會在 `http://localhost:5000` 啟動。

### 4. 存取 Swagger UI

開發環境下，開啟瀏覽器存取：
```
https://localhost:5001/swagger
```

## 加密範例

### C# 範例

```csharp
using System.Security.Cryptography;
using System.Text;

public class AesHelper
{
    public static string Encrypt(string plainText, string aesKey)
    {
        // 提取 IV：從 AES Key 的第 7-22 字元（索引 6-21）
        byte[] keyBytes = Encoding.UTF8.GetBytes(aesKey.PadRight(32, '0').Substring(0, 32));
        byte[] ivBytes = Encoding.UTF8.GetBytes(aesKey.Substring(6, 16));

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = ivBytes;
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

    public static string Decrypt(string cipherText, string aesKey)
    {
        // 提取 IV：從 AES Key 的第 7-22 字元（索引 6-21）
        byte[] keyBytes = Encoding.UTF8.GetBytes(aesKey.PadRight(32, '0').Substring(0, 32));
        byte[] ivBytes = Encoding.UTF8.GetBytes(aesKey.Substring(6, 16));

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = ivBytes;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
```

## 錯誤代碼

| 代碼 | 說明 |
|------|------|
| F009 | JSON 解析失敗 |
| F119 | 認證失敗 / Token 驗證失效 |
| F981 | 資料格式錯誤 |
| F982 | 資料不存在 |
| F983 | 業務邏輯錯誤 |
| F984 | 欄位必填錯誤 |
| F999 | 伺服器內部錯誤 |

## 技術文件

完整的 API 規格文件請參考 `docs/api/` 目錄：

- [README.md](../docs/api/README.md) - API 總覽
- [加密說明](../docs/api/encryption/加密說明.md) - 詳細加密規格
- [A1-登入驗證](../docs/api/authentication/A1-登入驗證.md)
- [R1-取得預期收貨清單](../docs/api/receiving/R1-取得預期收貨清單.md)
- [R2-取得收貨明細資料](../docs/api/receiving/R2-取得收貨明細資料.md)
- [R3-回傳收貨資料](../docs/api/receiving/R3-回傳收貨資料.md)
- [R4-取得收貨核對明細](../docs/api/receiving/R4-取得收貨核對明細.md)
- [R5-回傳全收確認](../docs/api/receiving/R5-回傳全收確認.md)

## 注意事項

1. **安全性**
   - 生產環境請將 AES Key 和 JWT Secret 更換為強密碼
   - 使用 HTTPS 傳輸
   - 妥善保管 JWT Token，避免洩漏

2. **加密要求**
   - 所有 API 請求（除登入外）必須將資料加密並包裝在 `RequestData` 欄位中
   - 所有 API 回應會自動包裝在 `ReturnData` 欄位中並加密

3. **Token 管理**
   - Token 過期後需重新登入
   - 接收到 F119 錯誤或 L 狀態碼時，應觸發重新登入

4. **請求重試**
   - 同一操作的重試請求應使用相同的 `requestId`
   - 避免重複提交造成資料不一致

## 開發與測試

### 執行測試

```bash
dotnet test
```

### 查看 Code Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 授權

© 2024 SEG WMS API. All rights reserved.
