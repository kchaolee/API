namespace SEG.WmsAPI.Models.Responses;

public class R1ResponseData
{
    public List<R1Header> headerList { get; set; } = new();
}

public class R1Header
{
    public string warehouseCode { get; set; } = string.Empty;
    public string wmsAsnNumber { get; set; } = string.Empty;
    public string storerCode { get; set; } = string.Empty;
    public string externReceiptNumber { get; set; } = string.Empty;
    public string vendorName { get; set; } = string.Empty;
}
