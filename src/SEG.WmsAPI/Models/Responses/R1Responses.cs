namespace SEG.WmsAPI.Models.Responses;

public class R1ResponseData
{
    public List<R1Header> HeaderList { get; set; } = new();
}

public class R1Header
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WmsAsnNumber { get; set; } = string.Empty;
    public string StorerCode { get; set; } = string.Empty;
    public string ExternReceiptNumber { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
}
