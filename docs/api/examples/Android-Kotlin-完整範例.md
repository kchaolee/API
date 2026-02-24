# Android API 整合範例

## 完整進貨作業流程示範

本文提供 Android Kotlin 完整的進貨作業 API 整合範例，從登入到完成收貨確認。

## 1. 專案結構

```
com.example.wmsapp/
├── data/
│   ├── api/
│   │   ├── ApiService.kt              # API 服務介面
│   │   └── TokenManager.kt            # Token 管理
│   ├── model/
│   │   ├── ReceivingHeader.kt         # R1 資料模型
│   │   ├── ReceivingDetail.kt         # R2 資料模型
│   │   ├── ReceivingInput.kt          # R3 輸入模型
│   │   ├── VerifyData.kt              # R4 資料模型
│   │   └── ConfirmReceiptResult.kt    # R5 資料模型
│   └── repository/
│       └── ReceivingRepository.kt     # 收貨作業 Repository
├── ui/
│   ├── login/
│   │   └── LoginViewModel.kt          # 登入
│   └── receiving/
│       ├── ReceiveListFragment.kt     # R1 採購單清單
│       ├── ReceiveDetailFragment.kt   # R2 明細輸入
│       └── ReceiveVerifyFragment.kt   # R4 核對確認
```

## 2. API 服務介面 (ApiService.kt)

```kotlin
import okhttp3.*
import org.json.JSONObject
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.util.*

data class ApiResponse<T>(
    val requestId: String,
    val status: String,
    val message: String,
    val data: T?,
    val errors: List<ApiError>?
)

data class ApiError(
    val code: String,
    val message: String
)

class ApiService {
    companion object {
        private const val BASE_URL = "http://192.168.x.x/wmService/v1"
        private val JSON_MEDIA_TYPE = "application/json; charset=utf-8".toMediaType()
    }

    private val client = OkHttpClient()

    private fun createRequest(
        endpoint: String,
        token: String?,
        body: String?
    ): Request {
        val builder = Request.Builder().url("$BASE_URL$endpoint")

        token?.let { builder.addHeader("Authorization", "Bearer $token") }

        body?.let {
            builder.post(it.toRequestBody(JSON_MEDIA_TYPE))
        }

        return builder.build()
    }

    suspend fun <T> post(
        endpoint: String,
        token: String?,
        requestBody: Map<String, Any>,
        dataParser: (JSONObject) -> T
    ): Result<ApiResponse<T>> = withContext(Dispatchers.IO) {
        try {
            val jsonBody = JSONObject(requestBody)
            val request = createRequest(endpoint, token, jsonBody.toString())

            val response = client.newCall(request).execute()
            val responseBody = response.body?.string() ?: throw Exception("無回應")

            val jsonResponse = JSONObject(responseBody)

            val apiResponse = ApiResponse(
                requestId = jsonResponse.getString("requestId"),
                status = jsonResponse.getString("status"),
                message = jsonResponse.getString("message"),
                data = if (jsonResponse.has("data") && !jsonResponse.isNull("data")) {
                    dataParser(jsonResponse.getJSONObject("data"))
                } else null,
                errors = if (jsonResponse.has("errors") && !jsonResponse.isNull("errors")) {
                    val errorsArray = jsonResponse.getJSONArray("errors")
                    (0 until errorsArray.length()).map { i ->
                        val error = errorsArray.getJSONObject(i)
                        ApiError(
                            code = error.getString("code"),
                            message = error.getString("message")
                        )
                    }
                } else null
            )

            Result.success(apiResponse)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
```

## 3. Token 管理器 (TokenManager.kt)

```kotlin
import android.content.Context
import android.content.SharedPreferences

class TokenManager(context: Context) {
    private val prefs: SharedPreferences = context.getSharedPreferences("WMSApp", Context.MODE_PRIVATE)

    fun saveToken(token: String, expires: String) {
        prefs.edit().apply {
            putString("accessToken", token)
            putString("tokenExpires", expires)
            apply()
        }
    }

    fun getToken(): String? {
        return prefs.getString("accessToken", null)
    }

    fun clearToken() {
        prefs.edit().apply {
            remove("accessToken")
            remove("tokenExpires")
            apply()
        }
    }

    fun isTokenValid(): Boolean {
        val token = getToken() ?: return false
        val expires = prefs.getString("tokenExpires", null) ?: return false

        // 檢查是否已過期（簡單的時間比較）
        // 實際應使用 SimpleDateFormat 解析日期並比較
        return true // 簡化示範
    }
}
```

## 4. 資料模型

### ReceivingHeader.kt (R1)

```kotlin
data class ReceivingHeader(
    val warehouseCode: String,
    val wmsAsnNumber: String,
    val storerCode: String,
    val externReceiptNumber: String,
    val vendorName: String
)
```

### ReceivingDetail.kt (R2)

```kotlin
data class ReceivingDetail(
    val lineNo: String,
    val externLineNo: String,
    val sku: String,
    val itemName: String,
    val expiryDate: String?,
    val fishingGroundCode: String,
    val itemColumnConfig: List<ColumnConfig>
)

data class ColumnConfig(
    val colName: String,
    val colRequired: Boolean
)
```

### ReceivingInput.kt (R3)

```kotlin
data class ReceivingInput(
    val lineNo: String,
    val externLineNo: String,
    val sku: String,
    val expiryDate: String,
    val packQty: Int,
    val qty: Double? = null,
    val batchNumber: String? = null,
    val mfgDate: String? = null,
    val storageStatus: String? = null,
    val stockType: String? = null,
    val other: String? = null,
    val other1: String? = null
)

enum class RequestFnName(val value: String) {
    CONFIRM("3.2確認收貨"),
    MODIFY("3.3修改")
}
```

### VerifyData.kt (R4)

```kotlin
data class VerifyData(
    val warehouseCode: String,
    val wmsAsnNumber: String,
    val storerCode: String,
    val externReceiptNumber: String,
    val poTotalQty: String,
    val recTotalPackQty: Int,
    val verifyList: List<VerifyDetail>,
    val locList: List<Location>
)

data class VerifyDetail(
    val lineNo: String,
    val externLineNo: String,
    val sku: String,
    val itemName: String,
    val expiryDate: String,
    val recPackQty: Int
)

data class Location(
    val locCode: String
)
```

### ConfirmReceiptResult.kt (R5)

```kotlin
data class ConfirmReceiptResult(
    val warehouseCode: String,
    val wmsAsnNumber: String,
    val storerCode: String,
    val externReceiptNumber: String,
    val vendorName: String,
    val asnFishingGroundName: String,
    val recPalletQty: Int,
    val asnTotalQty: String,
    val asnTotalUom: String,
    val recTotalPackQty: Int,
    val palletLabelList: List<PalletLabel>
)

data class PalletLabel(
    val lblExternReceiptNumber: String,
    val lblVendorName: String,
    val lblPalletQty: Int,
    val lblLocCode: String,
    val lblSku: String,
    val lblItemName: String
)
```

## 5. 收貨作業 Repository (ReceivingRepository.kt)

```kotlin
class ReceivingRepository(
    private val apiService: ApiService,
    private val tokenManager: TokenManager
) {
    // R1: 取得預期收貨清單
    suspend fun getReceivingHeaders(): Result<List<ReceivingHeader>> {
        val token = tokenManager.getToken() ?: return Result.failure(Exception("未登入"))

        return apiService.post(
            endpoint = "/PO/PoHeaderData",
            token = token,
            requestBody = mapOf(
                "requestId" to UUID.randomUUID().toString(),
                "storerCode" to "97286918",
                "docStatus" to "OPEN"
            )
        ) { data ->
            val headerList = data.getJSONArray("headerList")
            (0 until headerList.length()).map { i ->
                val header = headerList.getJSONObject(i)
                ReceivingHeader(
                    warehouseCode = header.getString("warehouseCode"),
                    wmsAsnNumber = header.getString("wmsAsnNumber"),
                    storerCode = header.getString("storerCode"),
                    externReceiptNumber = header.getString("externReceiptNumber"),
                    vendorName = header.getString("vendorName")
                )
            }
        }.map { response ->
            if (response.status == "S") {
                Result.success(response.data ?: emptyList())
            } else {
                val errorMsg = response.errors?.joinToString { it.message } ?: response.message
                Result.failure(Exception(errorMsg))
            }
        }
    }

    // R2: 取得收貨明細資料
    suspend fun getReceivingDetails(
        wmsAsnNumber: String,
        storerCode: String,
        externReceiptNumber: String
    ): Result<List<ReceivingDetail>> {
        val token = tokenManager.getToken() ?: return Result.failure(Exception("未登入"))

        return apiService.post(
            endpoint = "/PO/PoDetailData",
            token = token,
            requestBody = mapOf(
                "requestId" to UUID.randomUUID().toString(),
                "wmsAsnNumber" to wmsAsnNumber,
                "storerCode" to storerCode,
                "externReceiptNumber" to externReceiptNumber
            )
        ) { data ->
            val detailList = data.getJSONArray("detailList")
            (0 until detailList.length()).map { i ->
                val detail = detailList.getJSONObject(i)
                val columnConfigs = detail.getJSONArray("itemColumnConfig")
                val configs = (0 until columnConfigs.length()).map { j ->
                    val config = columnConfigs.getJSONObject(j)
                    ColumnConfig(
                        colName = config.getString("colName"),
                        colRequired = config.getBoolean("colRequired")
                    )
                }
                ReceivingDetail(
                    lineNo = detail.getString("lineNo"),
                    externLineNo = detail.getString("externLineNo"),
                    sku = detail.getString("sku"),
                    itemName = detail.getString("itemName"),
                    expiryDate = if (detail.has("expiryDate") && !detail.isNull("expiryDate")) {
                        detail.getString("expiryDate")
                    } else null,
                    fishingGroundCode = detail.getString("fishingGroundCode"),
                    itemColumnConfig = configs
                )
            }
        }.map { response ->
            if (response.status == "S") {
                Result.success(response.data ?: emptyList())
            } else {
                val errorMsg = response.errors?.joinToString { it.message } ?: response.message
                Result.failure(Exception(errorMsg))
            }
        }
    }

    // R3: 回傳收貨資料
    suspend fun saveReceivingItem(
        wmsAsnNumber: String,
        storerCode: String,
        externReceiptNumber: String,
        input: ReceivingInput,
        requestFnName: RequestFnName
    ): Result<Unit> {
        val token = tokenManager.getToken() ?: return Result.failure(Exception("未登入"))

        val body = mutableMapOf(
            "requestId" to UUID.randomUUID().toString(),
            "requestFnName" to requestFnName.value,
            "wmsAsnNumber" to wmsAsnNumber,
            "storerCode" to storerCode,
            "externReceiptNumber" to externReceiptNumber,
            "lineNo" to input.lineNo,
            "externLineNo" to input.externLineNo,
            "sku" to input.sku,
            "expiryDate" to input.expiryDate,
            "packQty" to input.packQty.toString()
        )

        input.qty?.let { body["qty"] = it.toString() }
        input.batchNumber?.let { body["batchNumber"] = it }
        input.mfgDate?.let { body["mfgDate"] = it }
        input.storageStatus?.let { body["storageStatus"] = it }
        input.stockType?.let { body["stockType"] = it }
        input.other?.let { body["other"] = it }
        input.other1?.let { body["other1"] = it }

        return apiService.post<Unit>(
            endpoint = "/PO/PoReceivingItem",
            token = token,
            requestBody = body
        ) { Unit }.map { response ->
            if (response.status == "S") {
                Result.success(Unit)
            } else {
                val errorMsg = response.errors?.joinToString { it.message } ?: response.message
                Result.failure(Exception(errorMsg))
            }
        }
    }

    // R4: 取得收貨核對明細
    suspend fun getReceivingVerifyData(
        wmsAsnNumber: String,
        storerCode: String,
        externReceiptNumber: String
    ): Result<VerifyData> {
        val token = tokenManager.getToken() ?: return Result.failure(Exception("未登入"))

        return apiService.post(
            endpoint = "/PO/PoVerifying",
            token = token,
            requestBody = mapOf(
                "requestId" to UUID.randomUUID().toString(),
                "wmsAsnNumber" to wmsAsnNumber,
                "storerCode" to storerCode,
                "externReceiptNumber" to externReceiptNumber
            )
        ) { data ->
            val verifyList = data.getJSONArray("verifyList")
            val details = (0 until verifyList.length()).map { i ->
                val detail = verifyList.getJSONObject(i)
                VerifyDetail(
                    lineNo = detail.getString("lineNo"),
                    externLineNo = detail.getString("externLineNo"),
                    sku = detail.getString("sku"),
                    itemName = detail.getString("itemName"),
                    expiryDate = detail.getString("expiryDate"),
                    recPackQty = detail.getString("recPackQty").toInt()
                )
            }

            val locList = data.getJSONArray("locList")
            val locations = (0 until locList.length()).map { i ->
                val loc = locList.getJSONObject(i)
                Location(locCode = loc.getString("locCode"))
            }

            VerifyData(
                warehouseCode = data.getString("warehouseCode"),
                wmsAsnNumber = data.getString("wmsAsnNumber"),
                storerCode = data.getString("storerCode"),
                externReceiptNumber = data.getString("externReceiptNumber"),
                poTotalQty = data.getString("poTotalQty"),
                recTotalPackQty = data.getString("recTotalPackQty").toInt(),
                verifyList = details,
                locList = locations
            )
        }.map { response ->
            if (response.status == "S") {
                Result.success(response.data!!)
            } else {
                val errorMsg = response.errors?.joinToString { it.message } ?: response.message
                Result.failure(Exception(errorMsg))
            }
        }
    }

    // R5: 回傳全收確認
    suspend fun confirmReceipt(
        wmsAsnNumber: String,
        storerCode: String,
        externReceiptNumber: String,
        recPalletQty: Int,
        recLocCode: String
    ): Result<ConfirmReceiptResult> {
        val token = tokenManager.getToken() ?: return Result.failure(Exception("未登入"))

        return apiService.post(
            endpoint = "/PO/PoCfmReceipt",
            token = token,
            requestBody = mapOf(
                "requestId" to UUID.randomUUID().toString(),
                "wmsAsnNumber" to wmsAsnNumber,
                "storerCode" to storerCode,
                "externReceiptNumber" to externReceiptNumber,
                "recPalletQty" to recPalletQty.toString(),
                "recLocCode" to recLocCode
            )
        ) { data ->
            val labelList = data.getJSONArray("palletLabelList")
            val labels = (0 until labelList.length()).map { i ->
                val label = labelList.getJSONObject(i)
                PalletLabel(
                    lblExternReceiptNumber = label.getString("lblExternReceiptNumber"),
                    lblVendorName = label.getString("lblVendorName"),
                    lblPalletQty = label.getString("lblPalletQty").toInt(),
                    lblLocCode = label.getString("lblLocCode"),
                    lblSku = label.getString("lblSku"),
                    lblItemName = label.getString("lblItemName")
                )
            }

            ConfirmReceiptResult(
                warehouseCode = data.getString("warehouseCode"),
                wmsAsnNumber = data.getString("wmsAsnNumber"),
                storerCode = data.getString("storerCode"),
                externReceiptNumber = data.getString("externReceiptNumber"),
                vendorName = data.getString("vendorName"),
                asnFishingGroundName = data.getString("asnFishingGroundName"),
                recPalletQty = data.getString("recPalletQty").toInt(),
                asnTotalQty = data.getString("asnTotalQty"),
                asnTotalUom = data.getString("asnTotalUom"),
                recTotalPackQty = data.getString("recTotalPackQty").toInt(),
                palletLabelList = labels
            )
        }.map { response ->
            if (response.status == "S") {
                Result.success(response.data!!)
            } else {
                val errorMsg = response.errors?.joinToString { it.message } ?: response.message
                Result.failure(Exception(errorMsg))
            }
        }
    }
}
```

## 6. 完整使用流程範例

```kotlin
class ReceivingViewModel(
    private val repository: ReceivingRepository
) : ViewModel() {

    // 進貨作業完整流程
    suspend fun executeFullReceivingWorkflow() {
        // 步驟 1: 取得待收貨採購單清單 (R1)
        val headersResult = repository.getReceivingHeaders()
        if (headersResult.isFailure) {
            handleApiError(headersResult.exceptionOrNull())
            return
        }

        val headers = headersResult.getOrNull() ?: return
        if (headers.isEmpty()) {
            showMessage("目前無待收貨採購單")
            return
        }

        // 選擇第一個採購單（實際應讓使用者選擇）
        val selectedHeader = headers[0]

        // 步驟 2: 取得收貨明細 (R2)
        val detailsResult = repository.getReceivingDetails(
            wmsAsnNumber = selectedHeader.wmsAsnNumber,
            storerCode = selectedHeader.storerCode,
            externReceiptNumber = selectedHeader.externReceiptNumber
        )
        if (detailsResult.isFailure) {
            handleApiError(detailsResult.exceptionOrNull())
            return
        }

        val details = detailsResult.getOrNull() ?: return

        // 步驟 3: 逐項輸入收貨資料 (R3)
        details.forEach { detail ->
            // 模擬使用者輸入
            val packQty = getUserInputPackQty(detail) // 從 UI 取得
            val expiryDate = detail.expiryDate ?: ""

            val input = ReceivingInput(
                lineNo = detail.lineNo,
                externLineNo = detail.externLineNo,
                sku = detail.sku,
                expiryDate = expiryDate,
                packQty = packQty
            )

            val saveResult = repository.saveReceivingItem(
                wmsAsnNumber = selectedHeader.wmsAsnNumber,
                storerCode = selectedHeader.storerCode,
                externReceiptNumber = selectedHeader.externReceiptNumber,
                input = input,
                requestFnName = RequestFnName.CONFIRM
            )
            if (saveResult.isFailure) {
                handleApiError(saveResult.exceptionOrNull())
                return
            }

            showMessage("收貨項目 ${detail.externLineNo} 已儲存")
        }

        // 步驟 4: 取得核對明細 (R4)
        val verifyResult = repository.getReceivingVerifyData(
            wmsAsnNumber = selectedHeader.wmsAsnNumber,
            storerCode = selectedHeader.storerCode,
            externReceiptNumber = selectedHeader.externReceiptNumber
        )
        if (verifyResult.isFailure) {
            handleApiError(verifyResult.exceptionOrNull())
            return
        }

        val verifyData = verifyResult.getOrNull() ?: return

        // 檢查是否所有項次皆有件數
        val hasUnreceivedItems = verifyData.verifyList.any { it.recPackQty == 0 }
        if (hasUnreceivedItems) {
            showMessage("警告：有項次尚未收貨")
        }

        // 步驟 5: 使用者選擇儲位並輸入板數
        val selectedLoc = verifyData.locList.firstOrNull() ?: run {
            showMessage("無可用儲位")
            return
        }

        val recPalletQty = getUserInputPalletQty() // 從 UI 取得

        // 步驟 6: 全收確認 (R5)
        val confirmResult = repository.confirmReceipt(
            wmsAsnNumber = selectedHeader.wmsAsnNumber,
            storerCode = selectedHeader.storerCode,
            externReceiptNumber = selectedHeader.externReceiptNumber,
            recPalletQty = recPalletQty,
            recLocCode = selectedLoc.locCode
        )
        if (confirmResult.isFailure) {
            handleApiError(confirmResult.exceptionOrNull())
            return
        }

        val confirmData = confirmResult.getOrNull() ?: return

        // 步驟 7: 列印板標籤
        printPalletLabels(confirmData.palletLabelList)

        showMessage("收貨完成！")
    }

    private fun handleApiError(exception: Throwable?) {
        when (exception?.message) {
            null -> showMessage("未知錯誤")
            else -> showMessage("錯誤：${exception.message}")
        }
    }

    private fun showMessage(message: String) {
        // 顯示訊息給使用者
    }

    private fun getUserInputPackQty(detail: ReceivingDetail): Int {
        // 實際應從 UI 取得使用者輸入
        return 100 // 示範值
    }

    private fun getUserInputPalletQty(): Int {
        // 實際應從 UI 取得使用者輸入
        return 5 // 示範值
    }

    private fun printPalletLabels(labels: List<PalletLabel>) {
        // 列印板標籤
        labels.forEach { label ->
            println("列印標籤：${label.lblExternReceiptNumber}")
        }
    }
}
```

## 錯誤處理最佳實務

```kotlin
suspend fun safeApiCall(
    block: suspend () -> Result<*>
): Result<*> {
    return try {
        block()
    } catch (e: Exception) {
        when (e.message) {
            // Token 失效
            "未登入" -> {
                // 重新登入
                Result.failure(Exception("請重新登入"))
            }
            // 網路錯誤
            "無回應" -> {
                Result.failure(Exception("網路連線失敗，請檢查網路"))
            }
            // 其他錯誤
            else -> Result.failure(e)
        }
    }
}
```

## 注意事項

1. **RequestId 管理**: 每個請求都需要唯一的 GUID，重試請求使用相同的 requestId
2. **Token 驗證**: 每個 API 請求都需要在 Header 中傳遞 Token
3. **錯誤處理**: 需要處理 F119 (Token 失效)、F981 (資料格式錯誤)、F983 (業務邏輯錯誤) 等
4. **日期格式**: 統一使用 "yyyy/MM/dd" 格式
5. **件數格式**: `packQty` 需為整數，傳遞時使用字串格式
