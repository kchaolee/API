# AES 加密/解密工具使用說明

## 專案概述

這是一個 Windows Forms 應用程式，用於 WMS API 的 AES 256 加密和解密測試工具。

## 功能特點

### ✅ 核心功能
- **AES 256 加密/解密** - 支援 CBC 模式和 PKCS7 填充
- **IV 自動提取** - 從 AES Key 的第 7-22 字元（索引 6-21）自動提取 IV
- **JSON 格式化** - 自動 minimize JSON 以減少加密資料量
- **RequestData/ReturnData 包裝** - 自動包裝加密資料到標準 API 格式

### ✅ 使用者介面
- **直觀的 GUI** - 清晰的輸入輸出區域
- **操作選擇** - 單選按鈕切換加密/解密模式
- **預設值** - 預設 AES Key 和範例 JSON
- **一鍵複製** - 快速複製結果到剪貼簿
- **錯誤提示** - 友善的錯誤訊息

## 使用方式

### 1. 啟動程式

```bash
cd src/AesHelperGUI
dotnet run
```

或在 Visual Studio 中開啟 `AesHelperGUI.csproj` 並執行。

### 2. 輸入 AES Key

在「AES Key」文字框中輸入您的 AES Key。

**重要規則：**
- AES Key 長度至少需要 **22 字元**
- 建議使用 **32 字元以上**
- 第 7-22 字元會自動提取作為 IV（初始化向量）

**預設值：**
```
ThisIsASecretKeyForAESEncryptionThatIs32Chars!
```

### 3. 輸入 JSON 文本

在「輸入 JSON」文字框中輸入要處理的 JSON 文本。

**加密模式輸入範例（原始 JSON）：**
```json
{
  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",
  "account": "user001",
  "password": "password123"
}
```

**解密模式輸入範例（已加上 RequestData/ReturnData 包裝）：**
```json
{
  "RequestData": "8uhB22wljs/Zqm4jfDrcVKIHQc0cBwjkgrPv3C4xM+hFXfz2X5r8T..."
}
```

或只輸入純 Base64 字串：
```
8uhB22wljs/Zqm4jfDrcVKIHQc0cBwjkgrPv3C4xM+hFXfz2X5r8T...
```

### 4. 選擇操作模式

- **加密 Encrypt** - 將 JSON 加密並包裝為 `{"RequestData": "..."}` 格式
- **解密 Decrypt** - 從加密資料中解密 JSON

### 5. 執行操作

點擊「執行 Execute」按鈕，結果會顯示在「輸出結果」文字框中。

### 6. 複製結果

點擊「複製 Copy」按鈕可將結果複製到剪貼簿。

### 7. 清除內容

點擊「清除 Clear」按鈕可清除輸入和輸出。

## 加密規格詳細說明

### 加密流程

1. **Minimize JSON** - 移除多餘空白
2. **準備 Key** - 取 AES Key 前 32 字元作為加密金鑰
3. **提取 IV** - 從 AES Key 第 7-22 字元（索引 6-21）提取 IV
4. **AES 256 加密** - 使用 CBC 模式和 PKCS7 填充
5. **Base64 編碼** - 將加密後的二進制資料轉為 Base64
6. **包裝** - 將結果包裝在 `{"RequestData": "..."}`

### 解密流程

1. **提取資料** - 從 `RequestData` 或 `ReturnData` 提取 Base64 字串
2. **Base64 解碼** - 轉回二進制資料
3. **準備 Key** - 取 AES Key 前 32 字元作為解密金鑰
4. **提取 IV** - 從 AES Key 第 7-22 字元（索引 6-21）提取 IV
5. **AES 256 解密** - 使用 CBC 模式和 PKCS7 填充
6. **返回 JSON** - 解密後的字串即為原始 JSON

### IV 提取規則

```csharp
AES Key = "ThisIsASecretKeyForAESEncryptionThatIs32Chars!"
索引        0123456789012345678901234567890123456789012345
字元        ThisIsASecretKeyForAESEncryptionThatIs32Chars!

Key (前 32 字元) = "ThisIsASecretKeyForAESEncryptionThatIs32"
IV (第 7-22 字元)  = "eKeyForAESEncry"      (索引 6-21)
```

## 實際使用範例

### 範例 1：加密登入請求

**步驟：**
1. 輸入 AES Key：`ThisIsASecretKeyForAESEncryptionThatIs32Chars!`
2. 選擇「加密 Encrypt」
3. 輸入 JSON：
```json
{
  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",
  "account": "user001",
  "password": "password123"
}
```
4. 點擊「執行 Execute」

**輸出：**
```json
{
  "RequestData": "8uhB22wljs/Zqm4jfDrcVKIHQc0cBwjkgrPv3C4xM+hFXfz2X5r8TmdvpAsz1XaQ6vkKfExbdQWnDp71pmdQYY7tu8Q2Fmdj2..." 
}
```

### 範例 2：解密 API 回應

**步驟：**
1. 輸入 AES Key：`ThisIsASecretKeyForAESEncryptionThatIs32Chars!`
2. 選擇「解密 Decrypt」
3. 輸入加密資料（可以是完整 JSON 或只輸入 RequestData 值）：
```json
{
  "ReturnData": "8uhB22wljs/Zqm4jfDrcVKIHQc0cBwjkgrPv3C4xM+hFXfz2X5r8TmdvpAsz1XaQ6vkKfExbdQWnDp71pmdQYY7tu8Q2Fmdj2..."
}
```
4. 點擊「執行 Execute」

**輸出：**
```json
{
  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",
  "status": "S",
  "message": "登入成功",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expires": "2024-12-11 23:59:59"
  },
  "errors": null
}
```

## 錯誤處理

程式會檢查以下錯誤情況：

| 錯誤情況 | 說明 | 解决方式 |
|---------|------|---------|
| AES Key 為空 | 未輸入 AES Key | 請輸入至少 22 字元的 AES Key |
| AES Key 太短 | 少於 22 字元 | 請輸入至少 22 字元的 AES Key |
| JSON 為空 | 未輸入要處理的 JSON | 請輸入要加密或解密的 JSON 文本 |
| 加密失敗 | 可能在加密過程中發生錯誤 | 檢查 AES Key 是否正確，JSON 格式是否有效 |
| 解密失敗 | 可能在解密過程中發生錯誤 | 檢查 AES Key 是否正確，加密資料是否完整 |
| JSON 解析錯誤 | 輸入的 JSON 格式錯誤 | 檢查 JSON 格式是否正確 |

## 注意事項

1. **AES Key 安全性**
   - 請妥善保管 AES Key
   - 不要將 AES Key 提交到版本控制系統
   - 生產環境請使用強密碼

2. **IV 提取規則**
   - IV 從 AES Key 的第 7-22 字元提取
   - 必須與 API 端使用相同的規則
   - 不要嘗試手動輸入 IV

3. **JSON 格式**
   - 確保 JSON 格式正確（括號配對、引號正確）
   - 加密前會自動 minimize（移除多餘空白）
   - 支援中文和其他 Unicode 字元

4. **Base64 格式**
   - 加密結果為 Base64 字串
   - 解密時會自動 Base64 解碼
   - 無需手動處理 Base64

5. **RequestData vs ReturnData**
   - 請求加密會產生 `{"RequestData": "..."}` 格式
   - 回應解密會解析 `{"ReturnData": "..."}` 格式
   - 也可以只輸入/輸出純 Base64 字串

## 系統需求

- .NET 8.0 或更高版本
- Windows 作業系統
- 至少 50 MB 可用磁碟空間
- 至少 512 MB RAM

## 建置專案

```bash
cd src/AesHelperGUI
dotnet build
```

## 執行專案

```bash
cd src/AesHelperGUI
dotnet run
```

或使用 Visual Studio：
1. 開啟 `AesHelperGUI.csproj`
2. 按 F5 或點擊「開始」按鈕

## 技術細節

### 專案配置

```xml
<TargetFramework>net8.0-windows</TargetFramework>
<OutputType>WinExe</OutputType>
<UseWindowsForms>true</UseWindowsForms>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

### 加密演算法

| 項目 | 設定 |
|------|------|
| 演算法 | AES |
| 金鑰長度 | 256 bits (32 bytes) |
| 模式 | CBC (Cipher Block Chaining) |
| 填充 | PKCS7 |
| 區塊大小 | 128 bits (16 bytes) |
| 編碼 | Base64 |

### 核心方法

```csharp
// 加密
private string ExecuteEncrypt()

// 解密
private string ExecuteDecrypt()

// 最小化 JSON
private string MinimizeJson(string json)

// 加密
private string Encrypt(string plainText, byte[] key, byte[] iv)

// 解密
private string Decrypt(string cipherText, byte[] key, byte[] iv)

// 提取 IV
private byte[] ExtractIvFromKey(byte[] key)

// 確保 Key 長度
private byte[] EnsureKeyLength(byte[] key)
```

## 相關文件

- `docs/api/encryption/加密說明.md` - 完整的加密規格說明
- `src/SEG.WmsAPI/Services/AesService.cs` - API 使用的加密服務
- `src/SEG.WmsAPI/Tests/AesServiceTests.cs` - 加解密測試案例

## 支援與反饋

如有問題或建議，請聯繫開發團隊。

---

**版本：** 1.0  
**更新日期：** 2026-03-03  
**適用 API 版本：** v1.01
