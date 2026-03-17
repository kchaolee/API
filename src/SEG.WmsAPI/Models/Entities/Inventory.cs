using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEG.WmsAPI.Models.Entities;

/// <summary>
/// 庫存實體
/// </summary>
[Table("Inventory")]
public class Inventory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 倉庫代號
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string WarehouseCode { get; set; } = string.Empty;

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
    /// 庫存數量
    /// </summary>
    [Required]
    public decimal Quantity { get; set; }

    /// <summary>
    /// 已分配數量
    /// </summary>
    public decimal AllocatedQuantity { get; set; }

    /// <summary>
    /// 單位
    /// </summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    /// <summary>
    /// 最後更新日期
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [MaxLength(1000)]
    public string? Remarks { get; set; }
}