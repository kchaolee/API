namespace SEG.WmsAPI.Models.Responses;

public class P3ResponseData
{
    public string warehouseCode { get; set; } = string.Empty;
    public string wmsOrderNumber { get; set; } = string.Empty;
    public string storerCode { get; set; } = string.Empty;
    public string externOrderNumber { get; set; } = string.Empty;
    public string customerName { get; set; } = string.Empty;
    public string assignExternReceiptNumber { get; set; } = string.Empty;
    public P3ShippingLabel shippingLabel { get; set; } = new();
}

public class P3ShippingLabel
{
    public string lblExternOrderNumber { get; set; } = string.Empty;
    public string lblCustomerName { get; set; } = string.Empty;
    public string lblCaseID { get; set; } = string.Empty;
}
