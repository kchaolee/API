using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NLog;
using SEG.WmsAPI.Data;
using SEG.WmsAPI.Models.Common;
using SEG.WmsAPI.Models.Requests;
using SEG.WmsAPI.Models.Responses;
using SEG.WmsAPI.Services;
using System.Diagnostics.Eventing.Reader;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace SEG.WmsAPI.Controllers;

/// <summary>
/// 收貨作業控制器 - 處理進貨作業相關接口 (R1-R5)
/// </summary>
[ApiController]
[Route("wmService/v1/PO")]
[Authorize]
public class POController : ControllerBase
{
    private NLog.ILogger _logger;
    private readonly IAesService _aesService;
    private readonly IConfiguration _configuration;
    private readonly WmsDb _wmsDb;
   
    public POController(IAesService aesService, IConfiguration configuration, WmsDb wmsDb)
    {
        _aesService = aesService;
        _configuration = configuration;
        _wmsDb = wmsDb;
    }

    /// <summary>
    /// R1 - 取得預期收貨清單
    /// POST /wmService/v1/PO/PoHeaderData
    /// </summary>
    [HttpPost("PoHeaderData")]
    public async Task<IActionResult> PoHeaderData([FromBody] JsonElement jsonElement)
    {
        string requestId = "";
        string userAccount = "";
        string strDecryptedRequest = "";
        string strErrMsg = string.Empty;
        R1Request request = null; 
        try
        {
            _logger = LogManager.GetLogger("R1-PoHeaderData");

            var tokenValidationResult = HttpContext.Items["TokenValidationResult"] as SEG.WmsAPI.Services.TokenValidationResult;
            userAccount = tokenValidationResult?.Account ?? "";

            //記錄初始請求內容
            Services.LogHelper.CreateLog<R1ResponseData>(NLog.LogLevel.Trace, _logger, requestId, jsonElement, userAccount, "R1", "PoHeaderData", "", "", null, "", "", "", "", "", "請求原始加密資料");

            #region 解密請求內容
            try
            {
                request = _aesService.DecryptRequest<R1Request>(jsonElement, ref strDecryptedRequest); 
                requestId = request.requestId;
                Services.LogHelper.CreateLog<R1ResponseData>(NLog.LogLevel.Trace, _logger, requestId, strDecryptedRequest, userAccount, "R1", "PoHeaderData", "", "", null, "", "", "", "", "", "請求解密後資料");

            }
            catch (Exception ex) 
            {
                Services.LogHelper.CreateLog<LoginResponseData>(NLog.LogLevel.Error, _logger, requestId, jsonElement, userAccount, "R1", "PoHeaderData", "", "", null, "", "", "", "", "", ex.Message);
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", ex.Message));
            }
            #endregion

            #region 請求資料檢查
            List<ErrorDetail>? errors = new List<ErrorDetail>();

            var assignStorerCode = _configuration["StorerCode"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(assignStorerCode) && request.StorerCode != assignStorerCode)
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"不存在定義-貨主 {request.StorerCode}"

                });
            }

            // DocStatus 固定帶"OPEN"
            if (request.DocStatus.ToUpper() != "OPEN")
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"不存在定義-單據狀態 {request.DocStatus}"

                });
            }

            // 以Model驗證資料的是否符合 Model 規範 (自定義檢查錯誤為空才檢查Model, 避免錯誤訊息重複)
            if (errors.Count == 0)
            {
                var strValidateMsg = "";
                strValidateMsg = Models.Common.ValidateHelper.ValidateModel<R1Request>(request);
                if (!string.IsNullOrEmpty(strValidateMsg))
                {
                    errors.Add(new ErrorDetail
                    {
                        code = "F981",
                        message = $"{strValidateMsg}"
                    });
                }
            }

            if (errors.Count > 0)
            {
                var strErrs = JsonSerializer.Serialize(errors, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = false
                });

                strErrMsg = "失敗-檢查的資料不正確";
                Services.LogHelper.CreateLog<R1ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R1", "PoHeaderData", "", "", null, "", "", "", "", "", strErrs);
                return BadRequest(ApiResponse<object>.Fail(requestId, strErrMsg, errors));
            }
            #endregion

            // Get Open PO
            var r1ResponseData = await _wmsDb.GetR1ResponseData(request.StorerCode);

            return Ok(ApiResponse<R1ResponseData>.Success(request.requestId, $"成功，共{r1ResponseData.headerList.Count}筆資料", r1ResponseData));
        }
        catch (Exception ex)
        {
            Services.LogHelper.CreateLog<R1ResponseData>(NLog.LogLevel.Error, _logger, "", jsonElement, userAccount, "R1", "PoHeaderData", "", "", null, "", "", "", "", "", ex.Message);
            return StatusCode(500, ApiResponse<object>.Fail(requestId, "伺服器處理異常", "F999", ex.Message));
        }
    }

    /// <summary>
    /// R2 - 取得收貨明細資料
    /// POST /wmService/v1/PO/PoDetailData
    /// </summary>
    [HttpPost("PoDetailData")]
    public async Task<IActionResult> PoDetailData([FromBody] JsonElement jsonElement)
    {
        string requestId = "";
        string userAccount = "";
        string strDecryptedRequest = "";
        string strErrMsg = string.Empty;
        R2Request request = null;
        
        try
        {
            _logger = LogManager.GetLogger("R2-PoDetailData");

            var tokenValidationResult = HttpContext.Items["TokenValidationResult"] as SEG.WmsAPI.Services.TokenValidationResult;
            userAccount = tokenValidationResult?.Account ?? "";

            //記錄初始請求內容
            Services.LogHelper.CreateLog<R2ResponseData>(NLog.LogLevel.Trace, _logger, requestId, jsonElement, userAccount, "R2", "PoDetailData", "", "", null, "", "", "", "", "", "請求原始加密資料");

            #region 解密請求內容
            try
            {
                request = _aesService.DecryptRequest<R2Request>(jsonElement, ref strDecryptedRequest);
                requestId = request.requestId;
                Services.LogHelper.CreateLog<R2ResponseData>(NLog.LogLevel.Trace, _logger, requestId, strDecryptedRequest, userAccount, "R2", "PoDetailData", "", "", null, "", "", "", "", "", "請求解密後資料");

            }
            catch (Exception ex)
            {
                Services.LogHelper.CreateLog<R2ResponseData>(NLog.LogLevel.Error, _logger, requestId, jsonElement, userAccount, "R2", "PoDetailData", "", "", null, "", "", "", "", "", ex.Message);
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", ex.Message));
            }
            #endregion

            #region 請求資料檢查
            List<ErrorDetail>? errors = new List<ErrorDetail>();

            if (String.IsNullOrEmpty(request.WmsAsnNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-WMS預期收貨單號"

                });
            }
            if (String.IsNullOrEmpty(request.StorerCode))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-貨主代碼"

                });
            }
            if (String.IsNullOrEmpty(request.ExternReceiptNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-外部收貨編號"

                });
            }

            // 以Model驗證資料的是否符合 Model 規範 (自定義檢查錯誤為空才檢查Model, 避免錯誤訊息重複)
            if (errors.Count == 0)
            {
                var strValidateMsg = "";
                strValidateMsg = Models.Common.ValidateHelper.ValidateModel<R2Request>(request);
                if (!string.IsNullOrEmpty(strValidateMsg))
                {
                    errors.Add(new ErrorDetail
                    {
                        code = "F981",
                        message = $"{strValidateMsg}"
                    });
                }
            }

            if (errors.Count > 0)
            {
                var strErrs = JsonSerializer.Serialize(errors, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = false
                });

                strErrMsg = "失敗-檢查的資料不正確";
                Services.LogHelper.CreateLog<R2ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R2", "PoDetailData", "", "", null, "", "", "", "", "", strErrs);
                return BadRequest(ApiResponse<object>.Fail(requestId, strErrMsg, errors));
            }
            #endregion

            // 於 WmsDb 取得收貨明細資料
            try
            {
                var responseData = await _wmsDb.GetR2ResponseData(request);
                return Ok(ApiResponse<R2ResponseData>.Success(request.requestId, "成功", responseData));
            }
            catch (Exception ex)
            {
                Services.LogHelper.CreateLog<R2ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R2", "PoDetailData", "", "", null, "", "", "", "", "", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(request.requestId, "失敗-取得收貨明細資料異常", "F983", ex.Message));
            }
           
        }
        catch (Exception ex)
        {
            Services.LogHelper.CreateLog<R2ResponseData>(NLog.LogLevel.Error, _logger, requestId, jsonElement, userAccount, "R2", "PoDetailData", "", "", null, "", "", "", "", "", ex.Message);
            return StatusCode(500, ApiResponse<object>.Fail(requestId, "伺服器處理異常", "F999", ex.Message));
        }
    }

    /// <summary>
    /// R3 - 回傳收貨資料
    /// POST /wmService/v1/PO/PoReceivingItem
    /// </summary>
    [HttpPost("PoReceivingItem")]
    public async Task<IActionResult> PoReceivingItem([FromBody] JsonElement jsonElement)
    {
        string requestId = "";
        string userAccount = "";
        string strDecryptedRequest = "";
        string strErrMsg = string.Empty;
        R3Request request = null;

        try
        {
            _logger = LogManager.GetLogger("R3-PoReceivingItem");

            var tokenValidationResult = HttpContext.Items["TokenValidationResult"] as SEG.WmsAPI.Services.TokenValidationResult;
            userAccount = tokenValidationResult?.Account ?? "";

            //記錄初始請求內容
            Services.LogHelper.CreateLog<R3ResponseData>(NLog.LogLevel.Trace, _logger, requestId, jsonElement, userAccount, "R3", "PoReceivingItem", "", "", null, "", "", "", "", "", "請求原始加密資料");

            #region 解密請求內容
            try
            {
                request = _aesService.DecryptRequest<R3Request>(jsonElement, ref strDecryptedRequest);
                requestId = request.requestId;
                Services.LogHelper.CreateLog<R3ResponseData>(NLog.LogLevel.Trace, _logger, requestId, strDecryptedRequest, userAccount, "R3", "PoReceivingItem", "", "", null, "", "", "", "", "", "請求解密後資料");

            }
            catch (Exception ex)
            {
                Services.LogHelper.CreateLog<R3ResponseData>(NLog.LogLevel.Error, _logger, requestId, jsonElement, userAccount, "R3", "PoReceivingItem", "", "", null, "", "", "", "", "", ex.Message);
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", ex.Message));
            }
            #endregion

            #region 請求資料檢查
            List<ErrorDetail>? errors = new List<ErrorDetail>();

            if (String.IsNullOrEmpty(request.RequestFnName))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-請求來源"

                });
            }
            if (String.IsNullOrEmpty(request.WmsAsnNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-WMS預期收貨單號"

                });
            }
            if (String.IsNullOrEmpty(request.StorerCode))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-貨主代碼"

                });
            }
            if (String.IsNullOrEmpty(request.ExternReceiptNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-外部收貨編號"

                });
            }
            if (String.IsNullOrEmpty(request.LineNo))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-WMS項次"

                });
            }
            if (String.IsNullOrEmpty(request.ExternLineNo))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-外部項次"

                });
            }
            if (String.IsNullOrEmpty(request.Sku))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-貨號"

                });
            }
            if (String.IsNullOrEmpty(request.ExpiryDate))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-到期日"

                });
            }
            if (!int.TryParse(request.PackQty, out int packQty))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"件數 {request.PackQty} 必須為整數"

                });
            }

            // 條件式必填：依據"2_取得收貨明細資料"的 itemColumnConfig 
            // ToDo: 需定義 itemColumnConfig 如何取得

            // 以Model驗證資料的是否符合 Model 規範 (自定義檢查錯誤為空才檢查Model, 避免錯誤訊息重複)
            if (errors.Count == 0)
            {
                var strValidateMsg = "";
                strValidateMsg = Models.Common.ValidateHelper.ValidateModel<R3Request>(request);
                if (!string.IsNullOrEmpty(strValidateMsg))
                {
                    errors.Add(new ErrorDetail
                    {
                        code = "F981",
                        message = $"{strValidateMsg}"
                    });
                }
            }

            if (errors.Count > 0)
            {
                var strErrs = JsonSerializer.Serialize(errors, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = false
                });

                strErrMsg = "失敗-檢查的資料不正確";
                Services.LogHelper.CreateLog<R2ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R3", "PoReceivingItem", "", "", null, "", "", "", "", "", strErrs);
                return BadRequest(ApiResponse<object>.Fail(requestId, strErrMsg, errors));
            }
            #endregion

            // 於 WmsDb 確認收貨明細
            try
            {
                var r3ResponseData = await _wmsDb.GetR3ResponseData(request, tokenValidationResult?.Account);  
                return Ok(ApiResponse<R3ResponseData>.Success(request.requestId, "成功", r3ResponseData));
            }
            catch (Exception ex) 
            {
                Services.LogHelper.CreateLog<R3ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R3", "PoReceivingItem", "", "", null, "", "", "", "", "", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(request.requestId, "失敗-PoReceivingItem確認異常", "F983", ex.Message));
            } 
        }
        catch (Exception ex)
        {
            Services.LogHelper.CreateLog<R3ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R3", "PoReceivingItem", "", "", null, "", "", "", "", "", ex.Message);
            return StatusCode(500, ApiResponse<object>.Fail(requestId, "伺服器處理異常", "F999", ex.Message));
        }
    }

    /// <summary>
    /// R4 - 取得收貨核對明細
    /// POST /wmService/v1/PO/PoVerifying
    /// </summary>
    [HttpPost("PoVerifying")]
    public async Task<IActionResult> PoVerifying([FromBody] JsonElement jsonElement)
    {
        string requestId = "";
        string userAccount = "";
        string strDecryptedRequest = "";
        string strErrMsg = string.Empty;
        R4Request request = null;

        try
        {
            _logger = LogManager.GetLogger("R4-PoVerifying");

            var tokenValidationResult = HttpContext.Items["TokenValidationResult"] as SEG.WmsAPI.Services.TokenValidationResult;
            userAccount = tokenValidationResult?.Account ?? "";

            //記錄初始請求內容
            Services.LogHelper.CreateLog<R4ResponseData>(NLog.LogLevel.Trace, _logger, requestId, jsonElement, userAccount, "R4", "PoVerifying", "", "", null, "", "", "", "", "", "請求原始加密資料");

            #region 解密請求內容
            try
            {
                request = _aesService.DecryptRequest<R4Request>(jsonElement, ref strDecryptedRequest);
                requestId = request.requestId;
                Services.LogHelper.CreateLog<R4ResponseData>(NLog.LogLevel.Trace, _logger, requestId, strDecryptedRequest, userAccount, "R4", "PoVerifying", "", "", null, "", "", "", "", "", "請求解密後資料");

            }
            catch (Exception ex)
            {
                Services.LogHelper.CreateLog<R4ResponseData>(NLog.LogLevel.Error, _logger, requestId, jsonElement, userAccount, "R4", "PoVerifying", "", "", null, "", "", "", "", "", ex.Message);
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", ex.Message));
            }
            #endregion

            #region 請求資料檢查
            List<ErrorDetail>? errors = new List<ErrorDetail>();

            if (String.IsNullOrEmpty(request.WmsAsnNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-WMS預期收貨單號"

                });
            }
            if (String.IsNullOrEmpty(request.StorerCode))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-貨主代碼"

                });
            }
            if (String.IsNullOrEmpty(request.ExternReceiptNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-外部收貨編號"

                });
            }

            // 以Model驗證資料的是否符合 Model 規範 (自定義檢查錯誤為空才檢查Model, 避免錯誤訊息重複)
            if (errors.Count == 0)
            {
                var strValidateMsg = "";
                strValidateMsg = Models.Common.ValidateHelper.ValidateModel<R4Request>(request);
                if (!string.IsNullOrEmpty(strValidateMsg))
                {
                    errors.Add(new ErrorDetail
                    {
                        code = "F981",
                        message = $"{strValidateMsg}"
                    });
                }
            }

            if (errors.Count > 0)
            {
                var strErrs = JsonSerializer.Serialize(errors, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = false
                });

                strErrMsg = "失敗-檢查的資料不正確";
                Services.LogHelper.CreateLog<R4ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R4", "PoVerifying", "", "", null, "", "", "", "", "", strErrs);
                return BadRequest(ApiResponse<object>.Fail(requestId, strErrMsg, errors));
            }
            #endregion

            // 於 WmsDb 讀取收貨明細
            try
            {
                var r4ResponseData = await _wmsDb.GetR4ResponseData(request);
                return Ok(ApiResponse<R4ResponseData>.Success(request.requestId, "成功", r4ResponseData));
            }
            catch (Exception ex)
            {
                Services.LogHelper.CreateLog<R4ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R4", "PoVerifying", "", "", null, "", "", "", "", "", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(request.requestId, "失敗-取得收貨核對明細異常", "F983", ex.Message));
            }
              
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "PoVerifying error");
            return StatusCode(500, ApiResponse<object>.Fail("", "伺服器處理異常", "F999", ex.Message));
        }
    }

    /// <summary>
    /// R5 - 回傳全收確認
    /// POST /wmService/v1/PO/PoCfmReceipt
    /// </summary>
    [HttpPost("PoCfmReceipt")]
    public async Task<IActionResult> PoCfmReceipt([FromBody] JsonElement jsonElement)
    {
        string requestId = "";
        string userAccount = "";
        string strDecryptedRequest = "";
        string strErrMsg = string.Empty;
        R5Request request = null;

        try
        {
            _logger = LogManager.GetLogger("R5-PoCfmReceipt");

            var tokenValidationResult = HttpContext.Items["TokenValidationResult"] as SEG.WmsAPI.Services.TokenValidationResult;
            userAccount = tokenValidationResult?.Account ?? "";

            //記錄初始請求內容
            Services.LogHelper.CreateLog<R5ResponseData>(NLog.LogLevel.Trace, _logger, requestId, jsonElement, userAccount, "R5", "PoCfmReceipt", "", "", null, "", "", "", "", "", "請求原始加密資料");

            #region 解密請求內容
            try
            {
                request = _aesService.DecryptRequest<R5Request>(jsonElement, ref strDecryptedRequest);
                requestId = request.requestId;
                Services.LogHelper.CreateLog<R5ResponseData>(NLog.LogLevel.Trace, _logger, requestId, strDecryptedRequest, userAccount, "R5", "PoCfmReceipt", "", "", null, "", "", "", "", "", "請求解密後資料");

            }
            catch (Exception ex)
            {
                Services.LogHelper.CreateLog<R5ResponseData>(NLog.LogLevel.Error, _logger, requestId, jsonElement, userAccount, "R5", "PoCfmReceipt", "", "", null, "", "", "", "", "", ex.Message);
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", ex.Message));
            }
            #endregion

            #region 請求資料檢查
            List<ErrorDetail>? errors = new List<ErrorDetail>();

            if (String.IsNullOrEmpty(request.WmsAsnNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-WMS預期收貨單號"

                });
            }

            if (String.IsNullOrEmpty(request.StorerCode))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-貨主代碼"

                });
            }

            if (String.IsNullOrEmpty(request.ExternReceiptNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-外部收貨編號"

                });
            }

            if (String.IsNullOrEmpty(request.RecLocCode))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-儲位編號"

                });
            }

            if (!int.TryParse(request.RecPalletQty, out int palletQty))
            {
                return BadRequest(ApiResponse<object>.Fail(request.requestId, "失敗-資料的格式錯誤", "F981", "板數必須為整數"));
            }

            // 以Model驗證資料的是否符合 Model 規範 (自定義檢查錯誤為空才檢查Model, 避免錯誤訊息重複)
            if (errors.Count == 0)
            {
                var strValidateMsg = "";
                strValidateMsg = Models.Common.ValidateHelper.ValidateModel<R5Request>(request);
                if (!string.IsNullOrEmpty(strValidateMsg))
                {
                    errors.Add(new ErrorDetail
                    {
                        code = "F981",
                        message = $"{strValidateMsg}"
                    });
                }
            }

            if (errors.Count > 0)
            {
                strErrMsg = "失敗-檢查的資料不正確:" + JsonSerializer.Serialize(errors);
                Services.LogHelper.CreateLog<R5ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R4", "PoVerifying", "", "", null, "", "", "", "", "", strErrMsg);
                return BadRequest(ApiResponse<object>.Fail(requestId, strErrMsg, errors));
            }
            #endregion
           
            // 於 WmsDb 全收確認
            try
            {
                var r5ResponseData = await _wmsDb.GetR5ResponseData(request, tokenValidationResult?.Account);
                return Ok(ApiResponse<R5ResponseData>.Success(request.requestId, "全收確認成功", r5ResponseData));
            }
            catch (Exception ex)
            {
                Services.LogHelper.CreateLog<R5ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R5", "PoCfmReceipt", "", "", null, "", "", "", "", "", ex.Message);
                return BadRequest(ApiResponse<object>.Fail(request.requestId, "失敗-全收確認異常", "F983", ex.Message));
            }
        }
        catch (Exception ex)
        {
            Services.LogHelper.CreateLog<R5ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "R5", "PoCfmReceipt", "", "", null, "", "", "", "", "", ex.Message);
            return StatusCode(500, ApiResponse<object>.Fail("", "伺服器處理異常", "F999", ex.Message));
        }
    }
}
