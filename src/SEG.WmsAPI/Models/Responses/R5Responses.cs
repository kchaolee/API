namespace SEG.WmsAPI.Models.Responses;

public class R5ResponseData
{
    public string warehouseCode { get; set; } = string.Empty;
    public string wmsAsnNumber { get; set; } = string.Empty;
    public string storerCode { get; set; } = string.Empty;
    public string externReceiptNumber { get; set; } = string.Empty;
    public string vendorName { get; set; } = string.Empty;
    public string asnFishingGroundName { get; set; } = string.Empty;
    public string recPalletQty { get; set; } = string.Empty;
    public string asnTotalQty { get; set; } = string.Empty;
    public string asnTotalUom { get; set; } = string.Empty;
    public string recTotalPackQty { get; set; } = string.Empty;
    public List<R5PalletLabel> PalletLabelList { get; set; } = new();
}

public class R5PalletLabel
{
    public string lblExternReceiptNumber { get; set; } = string.Empty;
    public string lblPallet { get; set; } = string.Empty;
    public string lblFishingGroundName { get; set; } = string.Empty;
}
