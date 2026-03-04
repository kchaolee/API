# API 測試指南

本文說明如何測試 SEG WMS API 的各種功能。

## 前置準備

1. **啟動 API 伺服器**

   ```bash
   cd src/SEG.WmsAPI
   dotnet run
   ```

   API 預設在 `http://localhost:5000` 啟動。

2. **準備加密工具**

   由於所有 API 請求和回應都需要加密，建議準備一個加密解密工具。

## 測試步驟

### 1. 測試登入（A1）

#### 步驟 1：準備登入請求

```json
{
  "requestId": "{{使用 UUID 產生器}}",
  "account": "user001",
  "password": "password123"
}
```

#### 步驟 2：加密請求

使用 AES 256 加密上述 JSON，得到 `encryptedRequestData`。

**加密參數：**
- AES Key: `ThisIsASecretKeyForAESEncryptionThatIs32Chars!`
- IV: 從 AES Key 的第 7-22 字元提取 = `SecretKeyForAESC`

#### 步驟 3：發送請求

```bash
curl -X POST http://localhost:5000/wmService/v1/Auth/SignInVerification \
  -H "Content-Type: application/json" \
  -d '{
    "RequestData": "encryptedRequestData"
  }'
```

#### 步驟 4：解密回應

從 `ReturnData` 欄位提取加密內容並解密。

**預期回應（解密後）：**
```json
{
  "requestId": "{{原始 requestId}}",
  "status": "S",
  "message": "登入成功",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "expires": "2024-12-11 23:59:59"
  },
  "errors": null
}
```

**重要：** 保存 `accessToken`，後續 API 都需要使用。

### 2. 測試取得預期收貨清單（R1）

#### 步驟 1：準備請求

```json
{
  "requestId": "{{新的 UUID}}",
  "storerCode": "97286918",
  "docStatus": "OPEN"
}
```

#### 步驟 2：加密並發送

```bash
curl -X POST http://localhost:5000/wmService/v1/PO/PoHeaderData \
  -H "Authorization: Bearer {YOUR_ACCESS_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "RequestData": "encryptedRequestData"
  }'
```

#### 步驟 3：解密回應

**預期回應（解密後）：**
```json
{
  "requestId": "{{原始 requestId}}",
  "status": "S",
  "message": "成功，共3筆資料",
  "data": {
    "headerList": [
      {
        "warehouseCode": "1020",
        "wmsAsnNumber": "0000000001",
        "storerCode": "69512619",
        "externReceiptNumber": "API21052600601",
        "vendorName": "供應商名稱"
      }
    ]
  },
  "errors": null
}
```

**重要：** 保存 `wmsAsnNumber`、`storerCode`、`externReceiptNumber`，後續 API 需要使用。

### 3. 測試取得收貨明細資料（R2）

#### 準備請求

```json
{
  "requestId": "{{新的 UUID}}",
  "wmsAsnNumber": "0000000001",
  "storerCode": "69512619",
  "externReceiptNumber": "API21052600601"
}
```

#### 發送請求

```bash
curl -X POST http://localhost:5000/wmService/v1/PO/PoDetailData \
  -H "Authorization: Bearer {YOUR_ACCESS_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "RequestData": "encryptedRequestData"
  }'
```

#### 預期回應（解密後）

```json
{
  "requestId": "{{原始 requestId}}",
  "status": "S",
  "message": "成功",
  "data": {
    "itemList": [
      {
        "lineNo": "00001",
        "externLineNo": "100",
        "sku": "100048",
        "descr": "挪威鮭 6-7kg",
        "expiryDate": "2026/02/17",
        "packQty": "0",
        "fishingGroundName": "M394",
        "itemColumnConfig": [
          {
            "colName": "qty",
            "colRequired": false
          },
          {
            "colName": "batchNumber",
            "colRequired": false
          }
        ]
      }
    ]
  },
  "errors": null
}
```

**重要：** 保存 `lineNo` 和 `itemColumnConfig`，R3 需要使用。

### 4. 測試回傳收貨資料（R3）

#### 準備請求（3.2確認收貨 - 新增）

```json
{
  "requestId": "{{新的 UUID}}",
  "requestFnName": "3.2確認收貨",
  "wmsAsnNumber": "0000000001",
  "storerCode": "69512619",
  "externReceiptNumber": "API21052600601",
  "lineNo": "00001",
  "externLineNo": "100",
  "sku": "100048",
  "expiryDate": "2026/02/17",
  "packQty": "100",
  "qty": "30.5",
  "batchNumber": "BATCH001",
  "mfgDate": "2026/01/15"
}
```

**注意：** `qty`、`batchNumber`、`mfgDate` 等欄位根據 `itemColumnConfig` 決定是否必填。

#### 發送請求

```bash
curl -X POST http://localhost:5000/wmService/v1/PO/PoReceivingItem \
  -H "Authorization: Bearer {YOUR_ACCESS_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "RequestData": "encryptedRequestData"
  }'
```

### 5. 測試取得收貨核對明細（R4）

#### 準備請求

```json
{
  "requestId": "{{新的 UUID}}",
  "wmsAsnNumber": "0000000001",
  "storerCode": "69512619",
  "externReceiptNumber": "API21052600601"
}
```

#### 發送請求

```bash
curl -X POST http://localhost:5000/wmService/v1/PO/PoVerifying \
  -H "Authorization: Bearer {YOUR_ACCESS_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "RequestData": "encryptedRequestData"
  }'
```

#### 預期回應（解密後）

```json
{
  "requestId": "{{原始 requestId}}",
  "status": "S",
  "message": "成功",
  "data": {
    "warehouseCode": "1020",
    "wmsAsnNumber": "0000000001",
    "storerCode": "69512619",
    "externReceiptNumber": "API21052600601",
    "poTotalQty": "500.000",
    "recTotalPackQty": "300",
    "verifyList": [...],
    "locList": [
      {
        "locCode": "A01"
      },
      {
        "locCode": "A02"
      }
    ]
  },
  "errors": null
}
```

**重要：** 從 `locList` 中選擇一個 `locCode`，R5 需要使用。

### 6. 測試回傳全收確認（R5）

#### 準備請求

```json
{
  "requestId": "{{新的 UUID}}",
  "wmsAsnNumber": "0000000001",
  "storerCode": "69512619",
  "externReceiptNumber": "API21052600601",
  "recPalletQty": "5",
  "recLocCode": "A01"
}
```

#### 發送請求

```bash
curl -X POST http://localhost:5000/wmService/v1/PO/PoCfmReceipt \
  -H "Authorization: Bearer {YOUR_ACCESS_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "RequestData": "encryptedRequestData"
  }'
```

#### 預期回應（解密後）

```json
{
  "requestId": "{{原始 requestId}}",
  "status": "S",
  "message": "收貨確認成功",
  "data": {
    "warehouseCode": "1020",
    "wmsAsnNumber": "0000000001",
    "storerCode": "69512619",
    "externReceiptNumber": "API21052600601",
    "vendorName": "供應商名稱",
    "asnFishingGroundName": "漁廠A、漁廠B",
    "recPalletQty": "5",
    "asnTotalQty": "500.000",
    "asnTotalUom": "KG",
    "recTotalPackQty": "300",
    "palletLabelList": [
      {
        "lblExternReceiptNumber": "採購單號 API21052600601",
        "lblPallet": "5-1",
        "lblFishingGroundName": "M394,H107"
      }
    ]
  },
  "errors": null
}
```

## 常見問題

### 1. 登入失敗（F119）

**原因：** 帳號或密碼錯誤

**解決：** 檢查帳號密碼，重新登入

### 2. Token 過期（L）

**原因：** JWT Token 已過期（24小時後）

**解決：** 重新登入取得新 Token

### 3. 解密失敗（F009）

**原因：** 加密金鑰或 IV 不正確

**解決：**
- 確認 AES Key 正確（至少 32 字元）
- 確認 IV 從 AES Key 的第 7-22 字元提取
- 確認加密演算法為 AES 256 CBC 模式

### 4. 格式錯誤（F981）

**原因：** 請求資料格式不符合要求

**解決：**
- `packQty` 必須為整數
- `recPalletQty` 必須為整數
- 日期格式必須為 "yyyy/MM/dd"

### 5. 業務邏輯錯誤（F983）

**原因：** 資料不符合業務規則

**解決：**
- R5 前需確認所有項次都有件數
- 採購單已過帳時無法再次確認

## 使用 Postman 測試

建議使用 Postman 進行 API 測試，可以設定環境變數和預請求腳本：

### Postman 環境變數設定

```
base_url = http://localhost:5000
aes_key = ThisIsASecretKeyForAESEncryptionThatIs32Chars!
access_token = {登入後自動更新}
```

### Postman Pre-request Script（自動加密）

```javascript
// 讀取 AES Key
const aesKey = pm.environment.get("aes_key");

// 提取 IV（第 7-22 字元）
const iv = aesKey.substring(6, 22);

// 加密請求體（需要實作 AES 256 加密）
// ... 加密邏輯 ...

pm.request.body.update({
  mode: 'raw',
  raw: JSON.stringify({ RequestData: encryptedData })
});
```

### Postman Test Script（自動解密回應）

```javascript
const response = pm.response.json();
const encryptedData = response.ReturnData;

// 解密回應（需要實作 AES 256 解密）
// ... 解密邏輯 ...

pm.environment.set("last_response", decryptedData);
```

## 自動化測試

建議建立單元測試或整合測試來自動化測試流程。

相關檔案：
- `tests/` - 測試專案目錄
- `test-api.bat` - 基本測試腳本
- `docs/api/test-guide.md` - 本測試指南
