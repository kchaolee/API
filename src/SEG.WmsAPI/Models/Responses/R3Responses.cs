namespace SEG.WmsAPI.Models.Responses;

public class R3ResponseData
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WmsAsnNumber { get; set; } = string.Empty;
    public string StorerCode { get; set; } = string.Empty;
    public string ExternReceiptNumber { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string AsnTotalQty { get; set; } = string.Empty;
    public string AsnTotalUom { get; set; } = string.Empty;
    public string RecTotalPackQty { get; set; } = string.Empty;
    public string RecPalletQty { get; set; } = string.Empty;
    public List<RecLoc> RecLocList { get; set; } = new();
    public List<R3Item> ItemList { get; set; } = new();
}

public class RecLoc
{
    public string RecLocCode { get; set; } = string.Empty;
    public string RecLocName { get; set; } = string.Empty;
}

public class R3Item
{
    public string LineNo { get; set; } = string.Empty;
    public string ExternLineNo { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Descr { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string PackQty { get; set; } = string.Empty;
    public string Qty { get; set; } = string.Empty;
    public string StockUom { get; set; } = string.Empty;
    public string FishingGroundName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public string? MfgDate { get; set; }
    public string? StorageStatus { get; set; }
    public string? StockType { get; set; }
    public string? Other { get; set; }
    public string? Other1 { get; set; }
}
