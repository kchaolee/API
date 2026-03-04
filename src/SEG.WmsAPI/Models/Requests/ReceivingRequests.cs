namespace SEG.WmsAPI.Models.Requests;

public class R1Request : Models.Common.RequestBase
{
    public string StorerCode { get; set; } = string.Empty;
    public string DocStatus { get; set; } = string.Empty;
}

public class R2Request : Models.Common.RequestBase
{
    public string WmsAsnNumber { get; set; } = string.Empty;
    public string StorerCode { get; set; } = string.Empty;
    public string ExternReceiptNumber { get; set; } = string.Empty;
}

public class R3Request : Models.Common.RequestBase
{
    public string RequestFnName { get; set; } = string.Empty;
    public string WmsAsnNumber { get; set; } = string.Empty;
    public string StorerCode { get; set; } = string.Empty;
    public string ExternReceiptNumber { get; set; } = string.Empty;
    public string LineNo { get; set; } = string.Empty;
    public string ExternLineNo { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string PackQty { get; set; } = string.Empty;
    public string? Qty { get; set; }
    public string? BatchNumber { get; set; }
    public string? MfgDate { get; set; }
    public string? StorageStatus { get; set; }
    public string? StockType { get; set; }
    public string? Other { get; set; }
    public string? Other1 { get; set; }
}

public class R4Request : Models.Common.RequestBase
{
    public string WmsAsnNumber { get; set; } = string.Empty;
    public string StorerCode { get; set; } = string.Empty;
    public string ExternReceiptNumber { get; set; } = string.Empty;
}

public class R5Request : Models.Common.RequestBase
{
    public string WmsAsnNumber { get; set; } = string.Empty;
    public string StorerCode { get; set; } = string.Empty;
    public string ExternReceiptNumber { get; set; } = string.Empty;
    public string RecPalletQty { get; set; } = string.Empty;
    public string RecLocCode { get; set; } = string.Empty;
}
