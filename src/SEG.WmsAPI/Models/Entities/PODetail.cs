using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEG.WmsAPI.Models.Entities;

/// <summary>
/// PO (Purchase Order) 明細實體
/// </summary>
[Table("PO_Detail")]
public class PODetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// PO 主檔 ID
    /// </summary>
    [Required]
    public int POHeaderId { get; set; }

    /// <summary>
    /// 行號
    /// </summary>
    [Required]
    public int LineNumber { get; set; }

    /// <summary>
    /// 產品編號
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// 產品名稱
    /// </summary>
    [MaxLength(500)]
    public string? ItemName { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    [Required]
    public decimal Quantity { get; set; }

    /// <summary>
    /// 單位
    /// </summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    /// <summary>
    /// 單價
    /// </summary>
    [Required]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 總金額
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// 已收貨數量
    /// </summary>
    public decimal ReceivedQuantity { get; set; }

    /// <summary>
    /// 狀態 (N=New, P=Partial, C=Complete)
    /// </summary>
    [MaxLength(10)]
    public string? Status { get; set; }

    /// <summary>
    /// PO 主檔
    /// </summary>
    public virtual POHeader POHeader { get; set; } = null!;
}