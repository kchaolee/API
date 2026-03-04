# API 技術規格文件

## 概述

本文档定義機場 WMS（倉儲管理系統）的 API 技術規格，供 Android App 客戶端進行整合開發。

**版本**: v1.01  
**更新日期**: 2024-12-10

## 目錄

- [通用規範](#通用規範)
- [加密機制](encryption/加密說明.md)
- [認證機制](authentication/A1-登入驗證.md)
- [進貨作業](receiving/R1-R5-進貨作業總覽.md)
- [出貨作業](shipping/README.md) (待開發)
- [整合範例](examples/Android-Kotlin-完整範例.md)

## 通用規範

### 基礎資訊

| 項目 | 說明 |
|------|------|
| 基礎 URL | `http://Host IP:HostPort/wmService/v1` |
| 通訊協定 | HTTP/HTTPS |
| 請求格式 | JSON (需 AES 256 加密) |
| 回應格式 | JSON (需 AES 256 加密) |
| 字符編碼 | UTF-8 |

### API 請求與回應結構

**請求結構**（外層）：
```json
{
  "RequestData": "AES256加密後的數據 (AES Key (IV 從 AES Key 的第 7~22 字元提取))"
}
```

**回應結構**（外層）：
```json
{
  "ReturnData": "AES256加密後的數據 (AES Key (IV 從 AES Key 的第 7~22 字元提取))"
}
```

**重要說明**：
- 所有 API 請求（除登入接口外）的數據需先 JSON 序列化後，經過 AES 256 加密，再放置於 `RequestData` 欄位中
- 所有 API 回應的數據經過 AES 256 加密後，放置於 `ReturnData` 欄位中
- AES Key 需由系統另行提供 (IV 從 AES Key 的第 7~22 字元提取)

**登入接口請求範例**：
```json
{
  "RequestData": "8uhB22wljs/Zqm4jfDrcVKIHQc0cBwjkgrPv3C4xM+hFXfz2X5r8TmdvpAsz1XaQRrXbXTPO0Lxexum4D2wu/tsZ14kQScSb5FH04DmgFxCDVPjSzchbrSeB3GYZYLyXkdEsTfhsvC3xs/Ar1wc1kQ=="
}
```

**登入接口回應範例**：
```json
{
  "ReturnData": "8uhB22wljs/Zqm4jfDrcVKIHQc0cBwjkgrPv3C4xM+hFXfz2X5r8TmdvpAsz1XaQ6vkKfExbdQWnDp71pmdQYY7tu8Q2Fmdj2j96eezi3wkjxpztdLmk0jYfCsrQSLxA9EOeXrlOtqklHqE1sClRiZ/Kuj8BhzJgD05X3HczT/zo4rAn8jZpdcP2L7Mqwt6VjcnvELLOKOIKymQyPgPa/irAo2FBV1Pwz4fz8oBMeGjYCXNZ7zOPw49tRnjGGtkVM3Ps6ef2Cqy+NMo5GPKOig=="
}
```

**實際請求數據（加密前）**：
```json
{
  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",
  "account": "user001",
  "password": "password123"
}
```

**實際回應數據（解密後）**：
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

### 全域回應格式標準

所有 API 統一使用 JSON 格式回應（解密後）：

**成功回應**:
```json
{
  "requestId": "請求編號 (GUID)",
  "status": "S",
  "message": "成功訊息描述",
  "data": { ... },
  "errors": null
}
```

**失敗回應**:
```json
{
  "requestId": "請求編號 (GUID)",
  "status": "F",
  "message": "失敗訊息描述",
  "data": null,
  "errors": [
    {
      "code": "錯誤代碼",
      "message": "錯誤說明"
    }
  ]
}
```

**Status 代碼說明**：
| Status | 說明 |
|--------|------|
| S | 處理完成 |
| L | 請重新登入 |
| F | 處理失敗 |
| U | 伺服器儲存失敗、物件存取錯誤時不會回應錯誤 |

### HTTP 狀態碼

| HTTP 狀態碼 | 說明 | API Status |
|------------|------|------------|
| 200 | 請求成功 | S / F (依據 status 欄位) |
| 500 | 伺服器儲存失敗、物件存取錯誤時不會回應錯誤 | U |
| 200以外 | 有錯誤、回傳errors | F |

### 錯誤代碼體系

| 錯誤代碼 | HTTP 狀態 | 類別 | 說明 | 因應方式 |
|---------|---------|------|------|--------|
| F119 | 401 | 驗證失效 | Token 驗證失效、錯誤的 Token | 重新驗證取得 Token |
| F009 | 500 | 發生未知錯誤 | JSON 解析失敗、物件解析失敗 | 檢查請求格式 |
| F981 | 400 | 資料格式錯誤 | 必填欄位缺失、資料類型格式錯誤 | 修正請求資料 |
| F982 | 404 | 資料不存在 | 代碼不存在、主檔資料不存在 | 確認資料 |
| F983 | 400 | 業務邏輯錯誤 | 已出貨不可修改、未定義的流程 | 檢查狀態邏輯 |
| F984 | 400 | 資料重複 | 請求重複 (requestId duplicated) | 客戶端檢查重複 |
| F997 | 500 | 伺服器忙碌 | 系統忙碌、等待其他資料 | 等待後重試 |
| F999 | 500 | 伺服器處理異常 | 未處理的例外 | 聯繫技術支援 |

### 加密規範

API 的請求與回應內容均需經過 AES 256 加密處理。

| 項目 | 說明 |
|------|------|
| 加密演算法 | AES 256 |
| 加密模式 | CBC (Cipher Block Chaining) |
| 填充方式 | PKCS7 |
| 金鑰 (AES Key) | 後續提供 (至少 22 bytes) |
| 初始化向量 (IV) | 從 AES Key 的第 7~22 字元提取 (16 字元) |

**加密流程**:
1. 請求加密: 原始 JSON → AES 256 加密 → Base64 編碼 → 傳送至 API
2. 回應解密: API 回應 → Base64 解碼 → AES 256 解密 → 原始 JSON

詳細加密實作請參考: [加密說明](encryption/加密說明.md)

### 認證機制

所有 API 請求（除登入接口外）均需在 HTTP Header 中傳遞 Token：

```
Authorization: Bearer {accessToken}
```

Token 透過 **A1 登入驗證** 接口取得，有效期建議為 24 小時。

詳細請參考：[認證機制 (A1)](authentication/A1-登入驗證.md)

### 通用請求參數

| 參數名 | 說明 | 類型 | 必填 |
|--------|------|------|------|
| requestId | 請求編號，格式為 GUID，每個請求唯一 | String(36) | 是 |

**重要**: 同一個請求若重複提交，必須使用相同的 requestId，方便伺服端進行重複請求防護。

### 資料型別說明

| 類型 | 說明 | 格式範例 |
|------|------|--------|
| String(n) | 字串，最大長度 n | String(15) |
| Char(n) | 固定長度字串 | Char(10) |
| GUID | 全域唯一識別碼 | "fb1a6bb8-21cd-490e-9f47-962cf99ec089" |
| DateTime | 日期時間 | "yyyy/MM/dd" 或 "yyyy/MM/dd HH:mm:ss" |
| decimal(n,m) | 小數，總位數 n，小數位數 m | decimal(19,5) |
| Array | 陣列 | [] |
| Object | 物件 | {} |

### 檢核規則

伺服端會依序執行以下檢核：

1. **安全驗證**
   - 驗證 Headers 中的 `Authorization` 參數是否合法
   - 未傳輸或 Token 失效視為非合法呼叫 (F119)

2. **資料檢驗**
   - 驗證請求數據是否可解析 (F009)
   - 驗證必要欄位是否缺漏 (F981)
   - 驗證資料類型與格式是否正確 (F981)
   - 驗證規格書中定義的代碼是否存在 (F982)
   - 驗證資料主檔是否存在 (F982)
   - 業務邏輯驗證 (F983)

3. **異常處理**
   - 資料庫逾時 (F997)
   - 系統例外狀況 (F999)

| 代號 | 接口名稱 | URL | 說明 |
|------|---------|-----|------|
|  - | 加密說明 | - | AES 256 加密詳解 (含 C# / Java / Kotlin 範例) |

### 進貨作業 (Receiving)

| 代號 | 接口名稱 | URL | 說明 |
|------|---------|-----|------|
| A1 | 登入驗證 | POST `/wmService/v1/Auth/SignInVerification` | 取得授權 Token |
| R1 | 取得預期收貨清單 | POST `/wmService/v1/PO/PoHeaderData` | 取得未完成收貨的採購單清單 |
| R2 | 取得收貨明細資料 | POST `/wmService/v1/PO/PoDetailData` | 依採購單取得收貨明細 |
| R3 | 回傳收貨資料 | POST `/wmService/v1/PO/PoReceivingItem` | 新增/更新收貨資料 |
| R4 | 取得收貨核對明細 | POST `/wmService/v1/PO/PoVerifying` | 取得收貨核對資料與儲位選單 |
| R5 | 回傳全收確認 | POST `/wmService/v1/PO/PoCfmReceipt` | 確認過帳並產生板標籤 |

### 出貨作業 (Shipping)

| 代號 | 接口名稱 | URL | 說明 |
|------|---------|-----|------|
| P1 | 取得出貨訂單清單 | POST /ORDER/OrderHeaderData | 取得未完成出貨的訂單清單 |
| P2 | 取得揀貨處理資料 | POST /ORDER/OrderPickingData | 取得揀貨明細 |
| P3 | 回傳揀貨確認資料 | POST /ORDER/OrderCfmPacking | 確認揀貨並配貨 |

> **出貨作業接口預留給未來開發使用**

## 版本紀錄

| 版本 | 日期 | 說明 |
|------|------|------|
| v1.01 | 2024-12-10 | 初始版本，定義進貨作業接口規格 |

## 聯絡資訊

如有技術問題，請聯繫：
- 技術支援：[待補充]
- 文件維護：[待補充]
