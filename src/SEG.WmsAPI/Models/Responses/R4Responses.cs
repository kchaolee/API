namespace SEG.WmsAPI.Models.Responses;

public class R4ResponseData
{
    public string warehouseCode { get; set; } = string.Empty;
    public string wmsAsnNumber { get; set; } = string.Empty;
    public string storerCode { get; set; } = string.Empty;
    public string externReceiptNumber { get; set; } = string.Empty;
    public string vendorName { get; set; } = string.Empty;
    public string asnTotalQty { get; set; } = string.Empty;
    public string asnTotalUom { get; set; } = string.Empty;
    public string recTotalPackQty { get; set; } = string.Empty;
    public string recPalletQty { get; set; } = string.Empty;
    public List<RecLoc> recLocList { get; set; } = new();
    public List<R4Item> itemList { get; set; } = new();
}

public class RecLoc
{
    public string recLocCode { get; set; } = string.Empty;
    public string recLocName { get; set; } = string.Empty;
}

public class R4Item
{
    public string lineNo { get; set; } = string.Empty;
    public string externLineNo { get; set; } = string.Empty;
    public string sku { get; set; } = string.Empty;
    public string descr { get; set; } = string.Empty;
    public string expiryDate { get; set; } = string.Empty;
    public string? packQty { get; set; }
    public string fishingGroundName { get; set; } = string.Empty;
    public string qty { get; set; }
    public string stockUom { get; set; } = string.Empty;
    public string? batchNumber { get; set; }
    public string? mfgDate { get; set; }
    public string? storageStatus { get; set; }
    public string? stockType { get; set; }
    public string? other { get; set; }
    public string? other1 { get; set; }
}
