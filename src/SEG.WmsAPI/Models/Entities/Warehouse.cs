using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEG.WmsAPI.Models.Entities;

/// <summary>
/// 倉庫實體
/// </summary>
[Table("Warehouse")]
public class Warehouse
{
    [Key]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 倉庫名稱
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 地址
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// 聯絡人
    /// </summary>
    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    /// <summary>
    /// 聯絡電話
    /// </summary>
    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

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
}