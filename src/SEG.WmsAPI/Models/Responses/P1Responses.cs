namespace SEG.WmsAPI.Models.Responses;

public class P1ResponseData
{
    public List<P1Header> headerList { get; set; } = new();
}

public class P1Header
{
    public string assignExternReceiptNumber { get; set; } = string.Empty;
    public string warehouseCode { get; set; } = string.Empty;
    public string wmsOrderNumber { get; set; } = string.Empty;
    public string storerCode { get; set; } = string.Empty;
    public string externOrderNumber { get; set; } = string.Empty;
    public string customerName { get; set; } = string.Empty;
}
