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
[Route("wmService/v1/ORDER")]
[Authorize]
public class OrderController : ControllerBase
{
    private NLog.ILogger _logger;
    private readonly IAesService _aesService;
    private readonly IConfiguration _configuration;
    private readonly WmsDb _wmsDb;
   
    public OrderController(IAesService aesService, IConfiguration configuration, WmsDb wmsDb)
    {
        _aesService = aesService;
        _configuration = configuration;
        _wmsDb = wmsDb;
    }

    /// <summary>
    /// P1 - 取得出貨訂單清單
    /// POST /wmService/v1/ORDER/OrderHeaderData
    /// </summary>
    [HttpPost("OrderHeaderData")]
    public async Task<IActionResult> OrderHeaderData([FromBody] JsonElement jsonElement)
    {
        string requestId = "";
        string userAccount = "";
        string strDecryptedRequest = "";
        string strErrMsg = string.Empty;
        P1Request request = null; 
        try
        {
            _logger = LogManager.GetLogger("P1-OrderHeaderData");

            var tokenValidationResult = HttpContext.Items["TokenValidationResult"] as SEG.WmsAPI.Services.TokenValidationResult;
            userAccount = tokenValidationResult?.Account ?? "";

            //記錄初始請求內容
            Services.LogHelper.CreateLog<P1ResponseData>(NLog.LogLevel.Trace, _logger, requestId, jsonElement, userAccount, "P1", "OrderHeaderData", "", "", null, "", "", "", "", "", "請求原始加密資料");

            #region 解密請求內容
            try
            {
                request = _aesService.DecryptRequest<P1Request>(jsonElement, ref strDecryptedRequest); 
                requestId = request.requestId;
                Services.LogHelper.CreateLog<P1ResponseData>(NLog.LogLevel.Trace, _logger, requestId, strDecryptedRequest, userAccount, "P1", "OrderHeaderData", "", "", null, "", "", "", "", "", "請求解密後資料");

            }
            catch (Exception ex) 
            {
                Services.LogHelper.CreateLog<P1ResponseData>(NLog.LogLevel.Error, _logger, requestId, jsonElement, userAccount, "P1", "OrderHeaderData", "", "", null, "", "", "", "", "", ex.Message);
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

            if (String.IsNullOrEmpty(request.AssignExternReceiptNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-指定採購單號"
                });
            } 

            // 以Model驗證資料的是否符合 Model 規範 (自定義檢查錯誤為空才檢查Model, 避免錯誤訊息重複)
            if (errors.Count == 0)
            {
                var strValidateMsg = "";
                strValidateMsg = Models.Common.ValidateHelper.ValidateModel<P1Request>(request);
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
                Services.LogHelper.CreateLog<P1ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "P1", "OrderHeaderData", "", "", null, "", "", "", "", "", strErrs);
                return BadRequest(ApiResponse<object>.Fail(requestId, strErrMsg, errors));
            }
            #endregion

            // Get Open Order
            var p1ResponseData = await _wmsDb.GetP1ResponseData(request.StorerCode);

            return Ok(ApiResponse<P1ResponseData>.Success(request.requestId, $"成功，共{p1ResponseData.headerList.Count} 筆資料", p1ResponseData));
        }
        catch (Exception ex)
        {
            Services.LogHelper.CreateLog<P1ResponseData>(NLog.LogLevel.Error, _logger, "", jsonElement, userAccount, "P1", "OrderHeaderData", "", "", null, "", "", "", "", "", ex.Message);
            return StatusCode(500, ApiResponse<object>.Fail(requestId, "伺服器處理異常", "F999", ex.Message));
        }
    }

    /// <summary>
    /// P2 - 取得揀貨處理資料
    /// POST /wmService/v1/ORDER/OrderPickingData
    /// </summary>
    [HttpPost("OrderPickingData")]
    public async Task<IActionResult> OrderPickingData([FromBody] JsonElement jsonElement)
    {
        string requestId = "";
        string userAccount = "";
        string strDecryptedRequest = "";
        string strErrMsg = string.Empty;
        P2Request request = null;
        try
        {
            _logger = LogManager.GetLogger("P2-OrderPickingData");

            var tokenValidationResult = HttpContext.Items["TokenValidationResult"] as SEG.WmsAPI.Services.TokenValidationResult;
            userAccount = tokenValidationResult?.Account ?? "";

            //記錄初始請求內容
            Services.LogHelper.CreateLog<P2ResponseData>(NLog.LogLevel.Trace, _logger, requestId, jsonElement, userAccount, "P2", "OrderPickingData", "", "", null, "", "", "", "", "", "請求原始加密資料");

            #region 解密請求內容
            try
            {
                request = _aesService.DecryptRequest<P2Request>(jsonElement, ref strDecryptedRequest);
                requestId = request.requestId;
                Services.LogHelper.CreateLog<P2ResponseData>(NLog.LogLevel.Trace, _logger, requestId, strDecryptedRequest, userAccount, "P2", "OrderPickingData", "", "", null, "", "", "", "", "", "請求解密後資料");

            }
            catch (Exception ex)
            {
                Services.LogHelper.CreateLog<P2ResponseData>(NLog.LogLevel.Error, _logger, requestId, jsonElement, userAccount, "P2", "OrderPickingData", "", "", null, "", "", "", "", "", ex.Message);
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
            if (String.IsNullOrEmpty(request.RequestAction))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-請求操作"

                });
            }
            if (String.IsNullOrEmpty(request.WmsOrderNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-WMS出貨單號"

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
            if (String.IsNullOrEmpty(request.ExternOrderNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-外部出貨編號"

                });
            }
            if (String.IsNullOrEmpty(request.AssignExternReceiptNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-指定採購單號"

                });
            }
            if (String.IsNullOrEmpty(request.CurrentLineNo))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-目前訂單項次"

                });
            } 

            // 以Model驗證資料的是否符合 Model 規範 (自定義檢查錯誤為空才檢查Model, 避免錯誤訊息重複)
            if (errors.Count == 0)
            {
                var strValidateMsg = "";
                strValidateMsg = Models.Common.ValidateHelper.ValidateModel<P2Request>(request);
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
                Services.LogHelper.CreateLog<P2ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "P2", "OrderPickingData", "", "", null, "", "", "", "", "", strErrs);
                return BadRequest(ApiResponse<object>.Fail(requestId, strErrMsg, errors));
            }
            #endregion

            var p2ResponseData = await _wmsDb.GetP2ResponseData(request.StorerCode);

            return Ok(ApiResponse<P2ResponseData>.Success(request.requestId, $"取得揀貨處理資料成功", p2ResponseData));
        }
        catch (Exception ex)
        {
            Services.LogHelper.CreateLog<P2ResponseData>(NLog.LogLevel.Error, _logger, "", jsonElement, userAccount, "P2", "OrderPickingData", "", "", null, "", "", "", "", "", ex.Message);
            return StatusCode(500, ApiResponse<object>.Fail(requestId, "伺服器處理異常", "F999", ex.Message));
        }
    }

    /// <summary>
    /// P3 - 回傳揀貨確認資料
    /// POST /wmService/v1/ORDER/OrderCfmPacking
    /// </summary>
    [HttpPost("OrderCfmPacking")]
    public async Task<IActionResult> OrderCfmPacking([FromBody] JsonElement jsonElement)
    {
        string requestId = "";
        string userAccount = "";
        string strDecryptedRequest = "";
        string strErrMsg = string.Empty;
        P3Request request = null;
        try
        {
            _logger = LogManager.GetLogger("P3-OrderCfmPacking");

            var tokenValidationResult = HttpContext.Items["TokenValidationResult"] as SEG.WmsAPI.Services.TokenValidationResult;
            userAccount = tokenValidationResult?.Account ?? "";

            //記錄初始請求內容
            Services.LogHelper.CreateLog<P3ResponseData>(NLog.LogLevel.Trace, _logger, requestId, jsonElement, userAccount, "P3", "OrderCfmPacking", "", "", null, "", "", "", "", "", "請求原始加密資料");

            #region 解密請求內容
            try
            {
                request = _aesService.DecryptRequest<P3Request>(jsonElement, ref strDecryptedRequest);
                requestId = request.requestId;
                Services.LogHelper.CreateLog<P3ResponseData>(NLog.LogLevel.Trace, _logger, requestId, strDecryptedRequest, userAccount, "P3", "OrderCfmPacking", "", "", null, "", "", "", "", "", "請求解密後資料");

            }
            catch (Exception ex)
            {
                Services.LogHelper.CreateLog<P3ResponseData>(NLog.LogLevel.Error, _logger, requestId, jsonElement, userAccount, "P3", "OrderCfmPacking", "", "", null, "", "", "", "", "", ex.Message);
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
            if (String.IsNullOrEmpty(request.RequestAction))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-請求操作"

                });
            }
            if (String.IsNullOrEmpty(request.WmsOrderNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-WMS出貨單號"

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
            if (String.IsNullOrEmpty(request.ExternOrderNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-外部出貨編號"

                });
            }
            if (String.IsNullOrEmpty(request.AssignExternReceiptNumber))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-指定採購單號"

                });
            }
            if (String.IsNullOrEmpty(request.CurrentLineNo))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-目前訂單項次"

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
            if (String.IsNullOrEmpty(request.CurrentKG))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-本次公斤數"

                });
            }
            if (!decimal.TryParse(request.CurrentKG, out decimal decCurrentKG))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"件數 {request.PackQtyPicked} 必須為 Decimal"

                });
            }

            if (String.IsNullOrEmpty(request.AssignExpiryDate))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"必要資訊不可為空-到期日"

                });
            }
            if (!int.TryParse(request.PackQtyPicked, out int packQtyPicked))
            {
                errors.Add(new ErrorDetail
                {
                    code = "F981",
                    message = $"件數 {request.PackQtyPicked} 必須為整數"

                });
            } 

            // 以Model驗證資料的是否符合 Model 規範 (自定義檢查錯誤為空才檢查Model, 避免錯誤訊息重複)
            if (errors.Count == 0)
            {
                var strValidateMsg = "";
                strValidateMsg = Models.Common.ValidateHelper.ValidateModel<P3Request>(request);
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
                Services.LogHelper.CreateLog<P2ResponseData>(NLog.LogLevel.Error, _logger, requestId, strDecryptedRequest, userAccount, "P3", "OrderCfmPacking", "", "", null, "", "", "", "", "", strErrs);
                return BadRequest(ApiResponse<object>.Fail(requestId, strErrMsg, errors));
            }
            #endregion

            var p3ResponseData = await _wmsDb.GetP3ResponseData(request.StorerCode);

            return Ok(ApiResponse<P3ResponseData>.Success(request.requestId, $"取得揀貨處理資料成功", p3ResponseData));
        }
        catch (Exception ex)
        {
            Services.LogHelper.CreateLog<P3ResponseData>(NLog.LogLevel.Error, _logger, "", jsonElement, userAccount, "P3", "OrderCfmPacking", "", "", null, "", "", "", "", "", ex.Message);
            return StatusCode(500, ApiResponse<object>.Fail(requestId, "伺服器處理異常", "F999", ex.Message));
        }
    }

}
