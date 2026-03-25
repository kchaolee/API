using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEG.WmsAPI.Models.Entities;

/// <summary>
/// ASN (Advance Shipping Notice) 實體
/// </summary>
[Table("ASN")]
public class ASN
{
    [Key]
    [Column("ASNKey")]
    public string ASNKey { get; set; } = string.Empty;

    [Column("Facility")]
    public string Facility { get; set; } = string.Empty;

    [Column("StorerKey")]
    public string StorerKey { get; set; } = string.Empty;

    [Column("ExternASNKey")]
    public string ExternASNKey { get; set; } = string.Empty;

    [Column("SellerName")]
    public string SellerName { get; set; } = string.Empty;

    [Column("Closed")]
    public string Closed { get; set; }

    [Column("AddDate")]
    public DateTime AddDate { get; set; }

    [Column("EditDate")]
    public DateTime? EditDate { get; set; }
}
