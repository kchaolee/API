using System.ComponentModel.DataAnnotations;

namespace SEG.WmsAPI.Models.Requests;

public class P1Request : Models.Common.RequestBase
{
    [MaxLength(15)]
    public string StorerCode { get; set; } = string.Empty;

    [MaxLength(10)]
    public string DocStatus { get; set; } = string.Empty;

    [MaxLength(30)]
    public string AssignExternReceiptNumber { get; set; } = string.Empty;
}

public class P2Request : Models.Common.RequestBase
{
    [MaxLength(30)]
    public string RequestFnName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string RequestAction { get; set; } = string.Empty;

    [MaxLength(10)]
    public string WmsOrderNumber { get; set; } = string.Empty;

    [MaxLength(15)]
    public string StorerCode { get; set; } = string.Empty;

    [MaxLength(30)]
    public string ExternOrderNumber { get; set; } = string.Empty;

    [MaxLength(30)]
    public string AssignExternReceiptNumber { get; set; } = string.Empty;

    [MaxLength(5)]
    public string CurrentLineNo { get; set; } = string.Empty;
}

public class P3Request : Models.Common.RequestBase
{
    [MaxLength(30)]
    public string RequestFnName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string RequestAction { get; set; } = string.Empty;

    [MaxLength(10)]
    public string WmsOrderNumber { get; set; } = string.Empty;

    [MaxLength(15)]
    public string StorerCode { get; set; } = string.Empty;

    [MaxLength(30)]
    public string ExternOrderNumber { get; set; } = string.Empty;

    [MaxLength(30)]
    public string AssignExternReceiptNumber { get; set; } = string.Empty;

    [MaxLength(5)]
    public string CurrentLineNo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [MaxLength(10)]
    public string PackQtyPicked { get; set; } = string.Empty;

    [MaxLength(14)]
    public string CurrentKG { get; set; } = string.Empty;

    [MaxLength(10)]
    public string AssignExpiryDate { get; set; } = string.Empty;
}
