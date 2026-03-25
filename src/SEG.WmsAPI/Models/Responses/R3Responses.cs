namespace SEG.WmsAPI.Models.Responses;

public class R3ResponseData
{
    public string warehouseCode { get; set; } = string.Empty;
    public string wmsAsnNumber { get; set; } = string.Empty;
    public string storerCode { get; set; } = string.Empty;
    public string externReceiptNumber { get; set; } = string.Empty;
    public string vendorName { get; set; } = string.Empty;
    public string lineNo { get; set; } = string.Empty;
    public string externLineNo { get; set; } = string.Empty;
    public string sku { get; set; } = string.Empty;
    public string descr { get; set; } = string.Empty;
    public string expiryDate { get; set; } = string.Empty;
    public string packQty { get; set; } = string.Empty;
    public string fishingGroundName { get; set; } = string.Empty;
    public string qty { get; set; } = string.Empty;
    public string stockUom { get; set; } = string.Empty;
    public string? batchNumber { get; set; } = string.Empty;
    public string? mfgDate { get; set; } = string.Empty;
    public string? storageStatus { get; set; } = string.Empty;
    public string? stockType { get; set; } = string.Empty;
    public string? other { get; set; } = string.Empty;
    public string? other1 { get; set; } = string.Empty;
}

