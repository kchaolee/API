using SEG.WmsAPI.Models.Entities;

namespace SEG.WmsAPI.Data.Repositories;

/// <summary>
/// PO Header Repository 介面
/// </summary>
public interface IPOHeaderRepository : IRepository<POHeader>
{
    /// <summary>
    /// 根據 PO 號取得 PO
    /// </summary>
    Task<POHeader?> GetByPoNumberAsync(string poNumber);

    /// <summary>
    /// 取得指定狀態的 PO 列表
    /// </summary>
    Task<IEnumerable<POHeader>> GetByStatusAsync(string status);

    /// <summary>
    /// 根據供應商代號取得 PO 列表
    /// </summary>
    Task<IEnumerable<POHeader>> GetByVendorCodeAsync(string vendorCode);

    /// <summary>
    /// 取得 PO 列表（含明細）
    /// </summary>
    Task<POHeader?> GetWithDetailsAsync(string poNumber);

    /// <summary>
    /// 分頁查詢 PO 列表
    /// </summary>
    Task<(IEnumerable<POHeader> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        string? status = null,
        string? vendorCode = null);
}