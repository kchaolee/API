namespace SEG.WmsAPI.Models.Entities;

/// <summary>
/// 資料庫設定類別
/// </summary>
public class DatabaseSettings
{
    public const string SectionName = "Database";

    /// <summary>
    /// 連線字串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 是否啟用敏感數據日誌記錄
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// 指令執行超時時間（秒）
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// 是否啟用快取查詢
    /// </summary>
    public bool EnableQueryCache { get; set; } = true;
}