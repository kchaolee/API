using Azure.Core;
using Microsoft.EntityFrameworkCore;
using SEG.WmsAPI.Models.Common;
using SEG.WmsAPI.Models.Entities;
using SEG.WmsAPI.Models.Requests;
using SEG.WmsAPI.Models.Responses;
using System.Data.SqlTypes;

namespace SEG.WmsAPI.Data;

/// <summary>
/// WMS 資料庫方法
/// </summary>
public class WmsDb
{
    private readonly WmsDbContext _context;

    public WmsDb(WmsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// R1 - 取得預期收貨清單的資料庫存取
    /// </summary>
    public async Task<R1ResponseData> GetR1ResponseData(string strStorerCode)
    {
        // 是否使用測試資料回傳
        bool blnUseFakeData = true;
        if (blnUseFakeData)
        {
            var returnFakeData = new R1ResponseData
            {
                headerList = new List<R1Header>
                {
                    new R1Header
                    {
                        warehouseCode = "1020",
                        wmsAsnNumber = "0000000001",
                        storerCode = "97286918",
                        externReceiptNumber = "API21052600601",
                        vendorName = "供應商名稱"
                    },
                    new R1Header
                    {
                        warehouseCode = "1020",
                        wmsAsnNumber = "0000000002",
                        storerCode = "97286918",
                        externReceiptNumber = "API21052600602",
                        vendorName = "供應商名稱1"
                    },
                    new R1Header
                    {
                        warehouseCode = "1050",
                        wmsAsnNumber = "0000000003",
                        storerCode = "97286918",
                        externReceiptNumber = "API21052600603",
                        vendorName = "供應商名稱2"
                    }
                }
            };

            return returnFakeData;
        }

        // 由 DB 讀取預期收貨清單
        var asnList = await _context.ASNs
        .Where(asn => asn.Closed == "0" && asn.StorerKey == strStorerCode)
        .OrderBy(asn => asn.ExternASNKey)
        .ToListAsync();

        var responseData = new R1ResponseData
        {
            headerList = asnList.Select(asn => new R1Header
            {
                warehouseCode = asn.Facility.Trim(),
                wmsAsnNumber = asn.ASNKey.Trim(),
                storerCode = asn.StorerKey.Trim(),
                externReceiptNumber = asn.ExternASNKey.Trim(),
                vendorName = asn.SellerName.Trim()
            }).ToList()
        };

        return responseData;
    }

    /// <summary>
    /// R2 - 取得收貨明細資料 的資料庫存取
    /// </summary>
    public async Task<R2ResponseData> GetR2ResponseData(R2Request request)
    {
        // 是否使用測試資料回傳
        bool blnUseFakeData = true;
        if (blnUseFakeData)
        {
            var returnFakeData = new R2ResponseData
            {
                warehouseCode = "1020",
                wmsAsnNumber = "0000000001",
                storerCode = "97286918",
                externReceiptNumber = "API21052600601",
                vendorName = "供應商名稱",
                itemColumnConfig = new List<ItemColumnConfig>
                {
                    new ItemColumnConfig
                    {
                        colName = "lineNo",
                        colDisplay = false,
                        colRequired = true,
                        colEditable = false
                    },
                    new ItemColumnConfig
                    {
                        colName = "externLineNo",
                        colDisplay = true,
                        colRequired = true,
                        colEditable = false
                    },
                    new ItemColumnConfig
                    {
                        colName = "sku",
                        colDisplay = false,
                        colRequired = true,
                        colEditable = false
                    },
                    new ItemColumnConfig
                    {
                        colName = "descr",
                        colDisplay = true,
                        colRequired = true,
                        colEditable = false
                    },
                    new ItemColumnConfig
                    {
                        colName = "expiryDate",
                        colDisplay = true,
                        colRequired = true,
                        colEditable = true
                    },
                    new ItemColumnConfig
                    {
                        colName = "packQty",
                        colDisplay = true,
                        colRequired = true,
                        colEditable = true
                    },
                    new ItemColumnConfig
                    {
                        colName = "fishingGroundName",
                        colDisplay = true,
                        colRequired = false,
                        colEditable = false
                    },
                    new ItemColumnConfig
                    {
                        colName = "qty",
                        colDisplay = false,
                        colRequired = false,
                        colEditable = false
                    },
                    new ItemColumnConfig
                    {
                        colName = "stockUom",
                        colDisplay = false,
                        colRequired = false,
                        colEditable = false
                    },
                    new ItemColumnConfig
                    {
                        colName = "batchNumber",
                        colDisplay = false,
                        colRequired = false,
                        colEditable = false
                    }
                },
                itemList = new List<R2Item>
                {
                    new R2Item
                    {
                        lineNo = "00001",
                        externLineNo = "100",
                        sku = "100048",
                        descr = "挪威鮭6-7kg",
                        expiryDate = "2026/02/17",
                        packQty = "0",
                        fishingGroundName = "M394",
                        qty = "30.5",
                        stockUom = "KG",
                        batchNumber = "",
                        mfgDate = "",
                        storageStatus = "",
                        stockType = "",
                        other = "",
                        other1 = ""
                    },
                    new R2Item
                    {
                        lineNo = "00002",
                        externLineNo = "200",
                        sku = "100048",
                        descr = "挪威鮭6-7kg",
                        expiryDate = "2026/02/17",
                        packQty =  "0",
                        fishingGroundName = "M395",
                        qty = "31.7",
                        stockUom = "KG",
                        batchNumber = "",
                        mfgDate = "",
                        storageStatus = "",
                        stockType = "",
                        other = "",
                        other1 = ""
                    },
                    new R2Item
                    {
                        lineNo = "00003",
                        externLineNo = "300",
                        sku = "100047",
                        descr = "挪威鮭5-6kg",
                        expiryDate = "2026/02/17",
                        packQty =  "0",
                        fishingGroundName = "M394",
                        qty = "20.7",
                        stockUom = "KG",
                        batchNumber = "",
                        mfgDate = "",
                        storageStatus = "",
                        stockType = "",
                        other = "",
                        other1 = ""
                    }
                }
            };
            return returnFakeData;
        }
        
        // 由 DB 讀取收貨明細
        var asn = await _context.ASNs
            .FirstOrDefaultAsync(asn => asn.ASNKey == request.WmsAsnNumber && asn.StorerKey == request.StorerCode);

        if (asn == null)
        {
            return new R2ResponseData();
        }

        var asnDetailList = await _context.ASNDetails
            .Where(detail => detail.WmsAsnNumber == request.WmsAsnNumber)
            .OrderBy(detail => detail.LineNo)
            .ToListAsync();

        var responseData = new R2ResponseData
        {
            warehouseCode = asn.Facility.Trim(),
            wmsAsnNumber = asn.ASNKey.Trim(),
            storerCode = asn.StorerKey.Trim(),
            externReceiptNumber = asn.ExternASNKey.Trim(),
            vendorName = asn.SellerName.Trim(),
            itemColumnConfig = new List<ItemColumnConfig>
            {
                new ItemColumnConfig
                {
                    colName = "lineNo",
                    colDisplay = false,
                    colRequired = true,
                    colEditable = false
                },
                new ItemColumnConfig
                {
                    colName = "externLineNo",
                    colDisplay = true,
                    colRequired = true,
                    colEditable = false
                },
                new ItemColumnConfig
                {
                    colName = "sku",
                    colDisplay = false,
                    colRequired = true,
                    colEditable = false
                },
                new ItemColumnConfig
                {
                    colName = "descr",
                    colDisplay = true,
                    colRequired = true,
                    colEditable = false
                },
                new ItemColumnConfig
                {
                    colName = "expiryDate",
                    colDisplay = true,
                    colRequired = true,
                    colEditable = true
                },
                new ItemColumnConfig
                {
                    colName = "packQty",
                    colDisplay = true,
                    colRequired = true,
                    colEditable = true
                },
                new ItemColumnConfig
                {
                    colName = "fishingGroundName",
                    colDisplay = true,
                    colRequired = false,
                    colEditable = false
                },
                new ItemColumnConfig
                {
                    colName = "qty",
                    colDisplay = false,
                    colRequired = false,
                    colEditable = false
                },
                new ItemColumnConfig
                {
                    colName = "stockUom",
                    colDisplay = false,
                    colRequired = false,
                    colEditable = false
                },
                new ItemColumnConfig
                {
                    colName = "batchNumber",
                    colDisplay = false,
                    colRequired = false,
                    colEditable = false
                }
            },
            itemList = asnDetailList.Select(detail => new R2Item
            {
                lineNo = detail.LineNo.Trim(),
                externLineNo = detail.ExternLineNo.Trim(),
                sku = detail.Sku.Trim(),
                descr = detail.Descr.Trim(),
                expiryDate = detail.ExpiryDate?.ToString("yyyyMMDD") ?? "",
                packQty = detail.PackQty.ToString(),
                fishingGroundName = detail.FishingGroundName.Trim(),
                qty = detail.Qty.ToString(),
                stockUom = detail.StockUom.Trim(),
                batchNumber = "",
                mfgDate = detail.MfgDate?.ToString("yyyyMMDD") ?? "",
                storageStatus = detail.StorageStatus?.Trim() ?? "",
                stockType = "",
                other = "",
                other1 = ""
            }).ToList()
        };

        return responseData;
    }

    /// <summary>
    /// R3 - 回傳收貨資料的資料庫存取
    /// </summary>
    public async Task<R3ResponseData> GetR3ResponseData(R3Request request, string userAccount)
    {
        R3ResponseData responseData = new R3ResponseData();

        // ToDo: WMSDB 確認收貨
        bool asnDetailConfirmSuccess = true;
        if (!asnDetailConfirmSuccess)
        {
            throw new Exception("確認收貨失敗");
        }

        responseData.warehouseCode = "1020";
        responseData.wmsAsnNumber = "0000000001";
        responseData.storerCode = "97286918";
        responseData.externReceiptNumber = "API21052600601";
        responseData.vendorName = "供應商名稱";
        responseData.lineNo = "00002";
        responseData.externLineNo = "200";
        responseData.sku = "100048";
        responseData.descr = "挪威鮭6-7kg";
        responseData.expiryDate = "2026/02/18";
        responseData.packQty = "5";
        responseData.fishingGroundName = "M395";
        responseData.qty = "31.7";
        responseData.stockUom = "KG";
        responseData.batchNumber = "";
        responseData.mfgDate = "";
        responseData.storageStatus = "";
        responseData.stockType = "";
        responseData.other = "";
        responseData.other1 = "";

        return responseData;
    }


    /// <summary>
    /// R4 - 取得收貨核對明細的資料庫存取
    /// </summary>
    public async Task<R4ResponseData> GetR4ResponseData(R4Request request)
    {
        R4ResponseData responseData = new R4ResponseData();

        //  (ToDo: WMSDB 讀取收貨資料)
        bool getAsnDetailVerifyingSuccess = true;
        if (!getAsnDetailVerifyingSuccess)
        {
            throw new Exception("讀取收貨資料失敗");
        }

        responseData.warehouseCode = "";
        responseData.wmsAsnNumber = request.WmsAsnNumber.Trim();
        responseData.storerCode = request.StorerCode.Trim();
        responseData.externReceiptNumber = request.ExternReceiptNumber.Trim();

        responseData = new R4ResponseData()
        {
            warehouseCode = "1020",
            wmsAsnNumber = "0000000001",
            storerCode = "69512619",
            externReceiptNumber = "API21052600601",
            vendorName = "供應商名稱",
            asnTotalQty = "82.9",
            asnTotalUom = "KG",
            recTotalPackQty = "8",
            recPalletQty = "0",
            recLocList = new List<RecLoc>
            {
                new RecLoc
                {
                    recLocCode = "0000000090",
                    recLocName  = "Normal"
                },
                new RecLoc
                {
                    recLocCode = "0000000091",
                    recLocName = "QC"
                }
            },
            itemList = new List<R4Item>
            {
                new R4Item
                {
                    lineNo = "00001",
                    externLineNo = "100",
                    sku = "100048",
                    descr = "挪威鮭6-7kg",
                    expiryDate = "2026/02/17",
                    packQty = "3",
                    fishingGroundName = "M394",
                    qty = "30.5",
                    stockUom = "KG",
                    batchNumber = "",
                    mfgDate = "",
                    storageStatus = "",
                    stockType = "",
                    other = "",
                    other1 = ""
                },
                new R4Item
                {
                    lineNo = "00002",
                    externLineNo = "200",
                    sku = "100048",
                    descr = "挪威鮭6-7kg",
                    expiryDate = "2026/02/18",
                    packQty = "5",
                    fishingGroundName = "M394",
                    qty = "31.7",
                    stockUom = "KG",
                    batchNumber = "",
                    mfgDate = "",
                    storageStatus = "",
                    stockType = "",
                    other = "",
                    other1 = ""

                },
                new R4Item
                {
                    lineNo = "00003",
                    externLineNo = "300",
                    sku = "100048",
                    descr = "挪威鮭6-7kg",
                    expiryDate = "2026/02/18",
                    packQty = "5",
                    fishingGroundName = "M394",
                    qty = "31.7",
                    stockUom = "KG",
                    batchNumber = "",
                    mfgDate = "",
                    storageStatus = "",
                    stockType = "",
                    other = "",
                    other1 = ""
                }
            },
        };

        return responseData;
    }

    /// <summary>
    /// R5 - 回傳全收確認的資料庫存取
    /// </summary>
    public async Task<R5ResponseData> GetR5ResponseData(R5Request request, string userAccount)
    {
        R5ResponseData responseData;

        // ToDo: WMSDB 全收確認
        bool asnConfirmSuccess = true;
        if (!asnConfirmSuccess)
        {
            throw new Exception("全收確認失敗");
        }

        responseData = new R5ResponseData
        {
            warehouseCode = "1020",
            wmsAsnNumber = "0000000001",
            storerCode = "97286918",
            externReceiptNumber = "API21052600601",
            vendorName = "供應商名稱",
            asnFishingGroundName = "M394,H107",
            recPalletQty = "5",
            asnTotalQty = "82.9",
            asnTotalUom = "KG",
            recTotalPackQty = "8",
            PalletLabelList = new List<R5PalletLabel>
                {
                    new R5PalletLabel
                    {
                        lblExternReceiptNumber = "採購單號 API21052600601",
                        lblPallet = "板數 5-1",
                        lblFishingGroundName = "漁廠 M394,H107"
                    },
                    new R5PalletLabel
                    {
                        lblExternReceiptNumber = "採購單號 API21052600601",
                        lblPallet = "板數 5-2",
                        lblFishingGroundName = "漁廠 M394,H107"
                    },
                    new R5PalletLabel
                    {
                        lblExternReceiptNumber = "採購單號 API21052600601",
                        lblPallet = "板數 5-3",
                        lblFishingGroundName = "漁廠 M394,H107"
                    },
                    new R5PalletLabel
                    {
                        lblExternReceiptNumber = "採購單號 API21052600601",
                        lblPallet = "板數 5-4",
                        lblFishingGroundName = "漁廠 M394,H107"
                    },
                    new R5PalletLabel
                    {
                        lblExternReceiptNumber = "採購單號 API21052600601",
                        lblPallet = "板數 5-5",
                        lblFishingGroundName = "漁廠 M394,H107"
                    }
                }
        };

        return responseData;
    }

}
