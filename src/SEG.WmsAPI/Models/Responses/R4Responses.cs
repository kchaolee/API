namespace SEG.WmsAPI.Models.Responses;

public class R4ResponseData
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WmsAsnNumber { get; set; } = string.Empty;
    public string StorerCode { get; set; } = string.Empty;
    public string ExternReceiptNumber { get; set; } = string.Empty;
    public string PoTotalQty { get; set; } = string.Empty;
    public string RecTotalPackQty { get; set; } = string.Empty;
    public List<R4VerifyDetail> VerifyList { get; set; } = new();
    public List<R4Location> LocList { get; set; } = new();
}

public class R4VerifyDetail
{
    public string LineNo { get; set; } = string.Empty;
    public string ExternLineNo { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Descr { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string RecPackQty { get; set; } = string.Empty;
}

public class R4Location
{
    public string LocCode { get; set; } = string.Empty;
}
