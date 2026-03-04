namespace SEG.WmsAPI.Models.Responses;

public class R5ResponseData
{
    public string WarehouseCode { get; set; } = string.Empty;
    public string WmsAsnNumber { get; set; } = string.Empty;
    public string StorerCode { get; set; } = string.Empty;
    public string ExternReceiptNumber { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string AsnFishingGroundName { get; set; } = string.Empty;
    public string RecPalletQty { get; set; } = string.Empty;
    public string AsnTotalQty { get; set; } = string.Empty;
    public string AsnTotalUom { get; set; } = string.Empty;
    public string RecTotalPackQty { get; set; } = string.Empty;
    public List<R5PalletLabel> PalletLabelList { get; set; } = new();
}

public class R5PalletLabel
{
    public string LblExternReceiptNumber { get; set; } = string.Empty;
    public string LblPallet { get; set; } = string.Empty;
    public string LblFishingGroundName { get; set; } = string.Empty;
}
