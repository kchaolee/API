namespace SEG.WmsAPI.Models.Responses;

public class P2ResponseData
{
    public string warehouseCode { get; set; } = string.Empty;
    public string wmsOrderNumber { get; set; } = string.Empty;
    public string storerCode { get; set; } = string.Empty;
    public string externOrderNumber { get; set; } = string.Empty;
    public string customerName { get; set; } = string.Empty;
    public string assignExternReceiptNumber { get; set; } = string.Empty;
    public P2PickingData pickingData { get; set; } = new();
}

public class P2PickingData
{
    public string lineNo { get; set; } = string.Empty;
    public string externLineNo { get; set; } = string.Empty;
    public string sku { get; set; } = string.Empty;
    public string descr { get; set; } = string.Empty;
    public string packQtyRequired { get; set; } = string.Empty;
    public string packQtyPicked { get; set; } = string.Empty;
    public string pickingUom { get; set; } = string.Empty;
    public string lineRemark { get; set; } = string.Empty;
    public string totalKG { get; set; } = string.Empty;
}
