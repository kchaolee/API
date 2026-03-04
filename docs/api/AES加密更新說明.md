# 🎉 AES 加密機制更新完成

## 變更摘要

已成功更新 `docs/api` 目錄下的所有 Markdown 文件，將 AES 加密中的 IV 參數改為從 AES Key 的第 7~22 字元提取。

## 主要變更

### 1. ✅ 加密規格更新

- **AES Key**: 從「32 bytes (64個16進位字元)」改為「至少 22 bytes」
- **IV 參數**: 移除單獨提供，改為從 AES Key 的第 7~22 字元（索引 6~21）提取 16 個字元

### 2. ✅ 更新的文件列表

| 文件 | 更新內容 |
|------|---------|
| **README.md** | 更新加密欄位說明、移除 IV 參數 |
| **encryption/加密說明.md** | 重寫整個加密說明，添加 IV 提取邏輯、更新所有實作範例 |
| **authentication/A1-登入驗證.md** | 更新加密說明、移除 IV 參數 |
| **receiving/R1-取得預期收貨清單.md** | 更新加密參數表格說明 |
| **receiving/R2-取得收貨明細資料.md** | 更新加密參數表格說明 |
| **receiving/R3-回傳收貨資料.md** | 更新加密參數表格說明 |
| **receiving/R4-取得收貨核對明細.md** | 更新加密參數表格說明 |
| **receiving/R5-回傳全收確認.md** | 更新加密參數表格說明 |

## 加密機制說明

### IV 生成規則

```
AES Key = "Your32ByteKeyHere1234567890123456"
         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ ^^^^^^^^
         前 32 bytes 作為金鑰          第 7~22 字元作為 IV
         (索引 0~31)                   (索引 6~21，共 16 bytes)

AES Key (金鑰): "Your32ByteKeyHere12345678901"
IV (初始化向量):   "yKeyHere123456789"
```

### 實作範例變更

#### C# 變更前後

```csharp
// ❌ 舊版 - 需要單獨提供 IV
byte[] aesKey = AesHelper.HexStringToBytes(keyHex);  // 32 bytes
byte[] aesIv = AesHelper.HexStringToBytes(ivHex);    // 16 bytes (已移除)
string encryptedRequest = AesHelper.EncryptRequest(jsonData, aesKey, aesIv);

// ✅ 新版 - IV 從 AES Key 提取
byte[] aesKey = AesHelper.StringToBytes(aesKeyString);  // 至少 22 bytes
// IV 會自動從 aesKey 的第 7~22 字元提取
string encryptedRequest = AesHelper.EncryptRequest(jsonData, aesKey);
```

#### Kotlin 變更前後

```kotlin
// ❌ 舊版 - 需要單獨提供 IV
val aesKey = hexStringToBytes(keyHex)    // 32 bytes
val aesIv = hexStringToBytes(ivHex)      // 16 bytes (已移除)
val encryptedRequest = AesUtil.encryptRequest(jsonData, aesKey, aesIv)

// ✅ 新版 - IV 從 AES Key 提取
val aesKey = stringToBytes(aesKeyString)  // 至少 22 bytes
// IV 會自動從 aesKey 的第 7~22 字元提取
val encryptedRequest = AesUtil.encryptRequest(jsonData, aesKey)
```

## 加密參數說明表格更新

所有接口文件的加密參數表格已更新為：

| 項次 | 參數名稱 | 參數說明 | 備註 |
|------|---------|--------|------|
| 1 | RequestData | AES256加密後的數據 | AES Key (IV 從 AES Key 的第 7~22 字元提取) |
| 1 | ReturnData | AES256加密後的數據 | AES Key (IV 從 AES Key 的第 7~22 字元提取) |

## 未提供資訊更新

**現狀**：
- **AES Key**: 待系統提供 (至少 22 bytes 的字串，建議 32 bytes 以上)
- **IV**: ~~待系統提供 (16 bytes 的 16 進位字串)~~ → 改為從 AES Key 提取

## 技術細節

### IV 提取函數邏輯

**原 api傳輸加解密sample.txt 中的邏輊**：
```csharp
byte[] akey = copyOfRange(key, 32);  // 前 32 bytes 作為金鑰
byte[] aiv = copyOfRange(key, 16);  // 前 16 bytes 作為 IV
```

**新規格**：
```csharp
byte[] akey = copyOfRange(key, 32);           // 前 32 bytes 作為金鑰
byte[] aiv = key.Skip(6).Take(16).ToArray();   // 第 7~22 字元作為 IV
```

這樣的設計簡化了金鑰管理，只需提供一個 AES Key 字串，IV 就可以自動從中提取。
