using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEG.WmsAPI.Models.Entities;

/// <summary>
/// PO (Purchase Order) 主檔實體
/// </summary>
[Table("PO_Header")]
public class POHeader
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// PO 號
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PoNumber { get; set; } = string.Empty;

    /// <summary>
    /// 供應商代號
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string VendorCode { get; set; } = string.Empty;

    /// <summary>
    /// 供應商名稱
    /// </summary>
    [MaxLength(200)]
    public string? VendorName { get; set; }

    /// <summary>
    /// 倉庫代號
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string WarehouseCode { get; set; } = string.Empty;

    /// <summary>
    /// PO 日期
    /// </summary>
    [Required]
    public DateTime PoDate { get; set; }

    /// <summary>
    /// 預計到貨日期
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// 狀態 (N=New, C=Confirmed, S=Shipped, R=Received, X=Cancelled)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Status { get; set; } = "N";

    /// <summary>
    /// 幣別
    /// </summary>
    [MaxLength(10)]
    public string? Currency { get; set; }

    /// <summary>
    /// 總金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [MaxLength(1000)]
    public string? Remarks { get; set; }

    /// <summary>
    /// 建立日期
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 建立者
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 更新日期
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// PO 明細
    /// </summary>
    public virtual ICollection<PODetail> PODetails { get; set; } = new List<PODetail>();
}