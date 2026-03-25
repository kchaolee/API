using System.ComponentModel.DataAnnotations.Schema;

namespace SEG.WmsAPI.Models.Entities;

/// <summary>
/// ASN 詳細實體
/// </summary>
[Table("ASNDetail")]
public class ASNDetail
{
    [Column("ASNKey")]
    public string WmsAsnNumber { get; set; } = string.Empty;

    [Column("ASNLineNumber")]
    public string LineNo { get; set; } = string.Empty;

    [Column("ExternLineNo")]
    public string ExternLineNo { get; set; } = string.Empty;

    [Column("SKU")]
    public string Sku { get; set; } = string.Empty;

    [Column("SKUDescription")]
    public string Descr { get; set; } = string.Empty;

    [Column("Lottable05")]
    public DateTime? ExpiryDate { get; set; }

    [Column("ExternQty")]
    public decimal PackQty { get; set; }

    [Column("QtyOrdered")]
    public decimal Qty { get; set; }

    [Column("ExternUOM")]
    public string StockUom { get; set; } = string.Empty;

    [Column("FishingGroundName")]
    public string FishingGroundName { get; set; } = string.Empty;

    //[Column("BatchNumber")]
    //public string? BatchNumber { get; set; }

    [Column("Lottable04")]
    public DateTime? MfgDate { get; set; }

    [Column("Lottable06")]
    public string? StorageStatus { get; set; }

    //[Column("StockType")]
    //public string? StockType { get; set; }

    //[Column("Other")]
    //public string? Other { get; set; }

    //[Column("Other1")]
    //public string? Other1 { get; set; }
}
