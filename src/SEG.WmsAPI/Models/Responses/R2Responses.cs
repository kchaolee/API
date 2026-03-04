namespace SEG.WmsAPI.Models.Responses;

public class R2ResponseData
{
    public List<R2Item> ItemList { get; set; } = new();
}

public class R2Item
{
    public string LineNo { get; set; } = string.Empty;
    public string ExternLineNo { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Descr { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string PackQty { get; set; } = string.Empty;
    public string FishingGroundName { get; set; } = string.Empty;
    public List<ColumnConfig> ItemColumnConfig { get; set; } = new();
}

public class ColumnConfig
{
    public string ColName { get; set; } = string.Empty;
    public bool ColRequired { get; set; }
}
