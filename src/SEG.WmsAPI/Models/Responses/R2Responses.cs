namespace SEG.WmsAPI.Models.Responses;

public class R2ResponseData
{
    public string warehouseCode { get; set; }
    public string wmsAsnNumber { get; set; }
    public string storerCode { get; set; }
    public string externReceiptNumber { get; set; }
    public string vendorName { get; set; }
    public List<ItemColumnConfig> itemColumnConfig { get; set; }
    public List<R2Item> itemList { get; set; } = new();
}

public class R2Item
{
    public string lineNo { get; set; } = string.Empty;
    public string externLineNo { get; set; } = string.Empty;
    public string sku { get; set; } = string.Empty;
    public string descr { get; set; } = string.Empty;
    public string expiryDate { get; set; } = string.Empty;
    public string packQty { get; set; }
    public string fishingGroundName { get; set; } = string.Empty;
    public string qty { get; set; } = string.Empty;
    public string stockUom { get; set; } = string.Empty;
    public string batchNumber { get; set; } = string.Empty;
    public string mfgDate { get; set; } = string.Empty;
    public string storageStatus { get; set; } = string.Empty;
    public string stockType { get; set; } = string.Empty;
    public string other { get; set; } = string.Empty;
    public string other1 { get; set; } = string.Empty;
}

public class ItemColumnConfig
{
    public string colName { get; set; } = string.Empty;
    public bool colDisplay { get; set; }
    public bool colRequired { get; set; }
    public bool colEditable { get; set; }
}
