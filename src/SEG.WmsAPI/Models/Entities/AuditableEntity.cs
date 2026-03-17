namespace SEG.WmsAPI.Models.Entities;

/// <summary>
/// 基礎實體介面 (包含常用欄位)
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}

/// <summary>
/// 基礎實體類別 (包含常用欄位)
/// </summary>
public abstract class AuditableEntity : IAuditable
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}