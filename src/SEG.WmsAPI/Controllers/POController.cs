using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEG.WmsAPI.Models.Common;
using SEG.WmsAPI.Models.Requests;
using SEG.WmsAPI.Models.Responses;
using SEG.WmsAPI.Services;

namespace SEG.WmsAPI.Controllers;

/// <summary>
/// 收貨作業控制器 - 處理進貨作業相關接口 (R1-R5)
/// </summary>
[ApiController]
[Route("wmService/v1/PO")]
[Authorize]
public class POController : ControllerBase
{
    private readonly ILogger<POController> _logger;
    private readonly IAesService _aesService;
    private readonly IConfiguration _configuration;

    public POController(ILogger<POController> logger, IAesService aesService, IConfiguration configuration)
    {
        _logger = logger;
        _aesService = aesService;
        _configuration = configuration;
    }

    /// <summary>
    /// R1 - 取得預期收貨清單
    /// POST /wmService/v1/PO/PoHeaderData
    /// </summary>
    [HttpPost("PoHeaderData")]
    public async Task<IActionResult> PoHeaderData([FromBody] JsonElement jsonElement)
    {
        try
        {
            var encryptedRequest = JsonSerializer.Deserialize<EncryptedRequest>(jsonElement.ToString(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            string decryptedRequest = _aesService.Decrypt(encryptedRequest.RequestData);

            var request = JsonSerializer.Deserialize<R1Request>(decryptedRequest, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", "無法解析請求資料"));
            }

            var data = new R1ResponseData
            {
                HeaderList = new List<R1Header>
                {
                    new R1Header
                    {
                        WarehouseCode = "1020",
                        WmsAsnNumber = "0000000001",
                        StorerCode = "69512619",
                        ExternReceiptNumber = "API21052600601",
                        VendorName = "供應商名稱"
                    },
                    new R1Header
                    {
                        WarehouseCode = "1020",
                        WmsAsnNumber = "0000002",
                        StorerCode = "69512619",
                        ExternReceiptNumber = "API21052600602",
                        VendorName = "供應商名稱1"
                    },
                    new R1Header
                    {
                        WarehouseCode = "1050",
                        WmsAsnNumber = "0000003",
                        StorerCode = "69512619",
                        ExternReceiptNumber = "API21052600603",
                        VendorName = "供應商名稱2"
                    }
                }
            };

            return Ok(ApiResponse<R1ResponseData>.Success(request.requestId, $"成功，共{data.HeaderList.Count}筆資料", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PoHeaderData error");
            return StatusCode(500, ApiResponse<object>.Fail("", "伺服器處理異常", "F999", ex.Message));
        }
    }

    /// <summary>
    /// R2 - 取得收貨明細資料
    /// POST /wmService/v1/PO/PoDetailData
    /// </summary>
    [HttpPost("PoDetailData")]
    public async Task<IActionResult> PoDetailData([FromBody] JsonElement jsonElement)
    {
        try
        {
            var encryptedRequest = JsonSerializer.Deserialize<EncryptedRequest>(jsonElement.ToString(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            string decryptedRequest = _aesService.Decrypt(encryptedRequest.RequestData);

            var request = JsonSerializer.Deserialize<R2Request>(decryptedRequest, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", "無法解析請求資料"));
            }

            var data = new R2ResponseData
            {
                ItemList = new List<R2Item>
                {
                    new R2Item
                    {
                        LineNo = "00001",
                        ExternLineNo = "100",
                        Sku = "100048",
                        Descr = "挪威鮭 6-7kg",
                        ExpiryDate = "2026/02/17",
                        PackQty = "0",
                        FishingGroundName = "M394",
                        ItemColumnConfig = new List<ColumnConfig>
                        {
                            new ColumnConfig { ColName = "qty", ColRequired = false },
                            new ColumnConfig { ColName = "batchNumber", ColRequired = false }
                        }
                    },
                    new R2Item
                    {
                        LineNo = "00002",
                        ExternLineNo = "200",
                        Sku = "100048",
                        Descr = "挪威鮭 6-7kg",
                        ExpiryDate = "2026/02/17",
                        PackQty = "0",
                        FishingGroundName = "M395",
                        ItemColumnConfig = new List<ColumnConfig>
                        {
                            new ColumnConfig { ColName = "qty", ColRequired = true },
                            new ColumnConfig { ColName = "batchNumber", ColRequired = true }
                        }
                    }
                }
            };

            return Ok(ApiResponse<R2ResponseData>.Success(request.requestId, "成功", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PoDetailData error");
            return StatusCode(500, ApiResponse<object>.Fail("", "伺服器處理異常", "F999", ex.Message));
        }
    }

    /// <summary>
    /// R3 - 回傳收貨資料
    /// POST /wmService/v1/PO/PoReceivingItem
    /// </summary>
    [HttpPost("PoReceivingItem")]
    public async Task<IActionResult> PoReceivingItem([FromBody] JsonElement jsonElement)
    {
        try
        {
            var encryptedRequest = JsonSerializer.Deserialize<EncryptedRequest>(jsonElement.ToString(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            string decryptedRequest = _aesService.Decrypt(encryptedRequest.RequestData);

            var request = JsonSerializer.Deserialize<R3Request>(decryptedRequest, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", "無法解析請求資料"));
            }

            if (!int.TryParse(request.PackQty, out int packQty))
            {
                return BadRequest(ApiResponse<object>.Fail(request.requestId, "失敗-資料的格式錯誤", "F981", "件數必須為整數"));
            }

            var data = new R3ResponseData
            {
                WarehouseCode = "1020",
                WmsAsnNumber = "0000001",
                StorerCode = "69512619",
                ExternReceiptNumber = "API21052600601",
                VendorName = "供應商名稱",
                AsnTotalQty = "100.0",
                AsnTotalUom = "KG",
                RecTotalPackQty = "8",
                RecPalletQty = "0",
                RecLocList = new List<RecLoc>
                {
                    new RecLoc { RecLocCode = "0000000090", RecLocName = "Normal" },
                    new RecLoc { RecLocCode = "0000000091", RecLocName = "QC" }
                },
                ItemList = new List<R3Item>
                {
                    new R3Item
                    {
                        LineNo = "00001",
                        ExternLineNo = "100",
                        Sku = "100048",
                        Descr = "挪威鮭 6-7kg",
                        ExpiryDate = "2026/02/17",
                        PackQty = request.PackQty,
                        Qty = "30.5",
                        StockUom = "KG",
                        FishingGroundName = "M394",
                        BatchNumber = request.BatchNumber ?? "",
                        MfgDate = request.MfgDate ?? "",
                        StorageStatus = request.StorageStatus ?? "",
                        StockType = request.StockType ?? "",
                        Other = request.Other ?? "",
                        Other1 = request.Other1 ?? ""
                    }
                }
            };

            return Ok(ApiResponse<R3ResponseData>.Success(request.requestId, "成功", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PoReceivingItem error");
            return StatusCode(500, ApiResponse<object>.Fail("", "伺服器處理異常", "F999", ex.Message));
        }
    }

    /// <summary>
    /// R4 - 取得收貨核對明細
    /// POST /wmService/v1/PO/PoVerifying
    /// </summary>
    [HttpPost("PoVerifying")]
    public async Task<IActionResult> PoVerifying([FromBody] JsonElement jsonElement)
    {
        try
        {
            var encryptedRequest = JsonSerializer.Deserialize<EncryptedRequest>(jsonElement.ToString(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            string decryptedRequest = _aesService.Decrypt(encryptedRequest.RequestData);

            var request = JsonSerializer.Deserialize<R4Request>(decryptedRequest, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", "無法解析請求資料"));
            }

            var data = new R4ResponseData
            {
                WarehouseCode = "1020",
                WmsAsnNumber = "0000001",
                StorerCode = "69512619",
                ExternReceiptNumber = "API21052600601",
                PoTotalQty = "500.000",
                RecTotalPackQty = "300",
                VerifyList = new List<R4VerifyDetail>
                {
                    new R4VerifyDetail
                    {
                        LineNo = "00001",
                        ExternLineNo = "000010",
                        Sku = "SKU001",
                        Descr = "鮭魚切片",
                        ExpiryDate = "2025/12/31",
                        RecPackQty = "100"
                    },
                    new R4VerifyDetail
                    {
                        LineNo = "00002",
                        ExternLineNo = "000020",
                        Sku = "SKU002",
                        Descr = "鱸魚",
                        ExpiryDate = "2025/12/25",
                        RecPackQty = "200"
                    }
                },
                LocList = new List<R4Location>
                {
                    new R4Location { LocCode = "A01" },
                    new R4Location { LocCode = "A02" },
                    new R4Location { LocCode = "B01" }
                }
            };

            return Ok(ApiResponse<R4ResponseData>.Success(request.requestId, "成功", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PoVerifying error");
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
        try
        {
            var encryptedRequest = JsonSerializer.Deserialize<EncryptedRequest>(jsonElement.ToString(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            string decryptedRequest = _aesService.Decrypt(encryptedRequest.RequestData);

            var request = JsonSerializer.Deserialize<R5Request>(decryptedRequest, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Fail("", "請求格式錯誤", "F981", "無法解析請求資料"));
            }

            if (!int.TryParse(request.RecPalletQty, out int palletQty))
            {
                return BadRequest(ApiResponse<object>.Fail(request.requestId, "失敗-資料的格式錯誤", "F981", "板數必須為整數"));
            }

            var data = new R5ResponseData
            {
                WarehouseCode = "1020",
                WmsAsnNumber = "0000001",
                StorerCode = "69512619",
                ExternReceiptNumber = "API21052600601",
                VendorName = "供應商名稱",
                AsnFishingGroundName = "漁廠A、漁廠B",
                RecPalletQty = request.RecPalletQty,
                AsnTotalQty = "500.000",
                AsnTotalUom = "KG",
                RecTotalPackQty = "300",
                PalletLabelList = new List<R5PalletLabel>
                {
                    new R5PalletLabel
                    {
                        LblExternReceiptNumber = "採購單號 API21052600601",
                        LblPallet = "5-1",
                        LblFishingGroundName = "M394,H107"
                    },
                    new R5PalletLabel
                    {
                        LblExternReceiptNumber = "採購單號 API21052600601",
                        LblPallet = "5-2",
                        LblFishingGroundName = "M394,H107"
                    }
                }
            };

            return Ok(ApiResponse<R5ResponseData>.Success(request.requestId, "收貨確認成功", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PoCfmReceipt error");
            return StatusCode(500, ApiResponse<object>.Fail("", "伺服器處理異常", "F999", ex.Message));
        }
    }
}
