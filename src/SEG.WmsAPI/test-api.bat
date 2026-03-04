@echo off
REM SEG WMS API 測試腳本
REM 測試登入和進貨作業 API

setlocal

REM API 基礎 URL
set BASE_URL=http://localhost:5000

REM 加密金鑰（從 appsettings.json 的 Encryption:AesKey 取得）
set AES_KEY=ThisIsASecretKeyForAESEncryptionThatIs32Chars!

echo ========================================
echo SEG WMS API 測試
echo ========================================
echo.

echo [1] 測試登入...
curl -X POST "%BASE_URL%/wmService/v1/Auth/SignInVerification" ^
  -H "Content-Type: application/json" ^
  -d "{\"RequestData\":\"注意：這裡需要先加密登入請求\"}" ^
  -w "\nHTTP Status: %%{http_code}\n"
echo.

echo [2] 測試取得預期收貨清單（R1）...
REM 需要先登入取得 Token
curl -X POST "%BASE_URL%/wmService/v1/PO/PoHeaderData" ^
  -H "Authorization: Bearer {YOUR_TOKEN_HERE}" ^
  -H "Content-Type: application/json" ^
  -d "{\"RequestData\":\"注意：這裡需要先加密R1請求\"}" ^
  -w "\nHTTP Status: %%{http_code}\n"
echo.

echo [3] 測試取得收貨明細資料（R2）...
curl -X POST "%BASE_URL%/wmService/v1/PO/PoDetailData" ^
  -H "Authorization: Bearer {YOUR_TOKEN_HERE}" ^
  -H "Content-Type: application/json" ^
  -d "{\"RequestData\":\"注意：這裡需要先加密R2請求\"}" ^
  -w "\nHTTP Status: %%{http_code}\n"
echo.

echo [4] 測試回傳收貨資料（R3）...
curl -X POST "%BASE_URL%/wmService/v1/PO/PoReceivingItem" ^
  -H "Authorization: Bearer {YOUR_TOKEN_HERE}" ^
  -H "Content-Type: application/json" ^
  -d "{\"RequestData\":\"注意：這裡需要先加密R3請求\"}" ^
  -w "\nHTTP Status: %%{http_code}\n"
echo.

echo [5] 測試取得收貨核對明細（R4）...
curl -X POST "%BASE_URL%/wmService/v1/PO/PoVerifying" ^
  -H "Authorization: Bearer {YOUR_TOKEN_HERE}" ^
  -H "Content-Type: application/json" ^
  -d "{\"RequestData\":\"注意：這裡需要先加密R4請求\"}" ^
  -w "\nHTTP Status: %%{http_code}\n"
echo.

echo [6] 測試回傳全收確認（R5）...
curl -X POST "%BASE_URL%/wmService/v1/PO/PoCfmReceipt" ^
  -H "Authorization: Bearer {YOUR_TOKEN_HERE}" ^
  -H "Content-Type: application/json" ^
  -d "{\"RequestData\":\"注意：這裡需要先加密R5請求\"}" ^
  -w "\nHTTP Status: %%{http_code}\n"
echo.

echo ========================================
echo 測試完成
echo ========================================
echo.
echo 重要提示：
echo 1. 確保 API 已已啟動（執行 dotnet run）
echo 2. 所有請求都需要先進行 AES 256 加密
echo 3. IV 從 AES Key 的第 7-22 字元提取
echo 4. 除登入外，所有 API 都需要 JWT Token
echo.

pause
