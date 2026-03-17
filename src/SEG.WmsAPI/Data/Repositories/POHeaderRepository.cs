using Microsoft.EntityFrameworkCore;
using SEG.WmsAPI.Models.Entities;

namespace SEG.WmsAPI.Data.Repositories;

/// <summary>
/// PO Header Repository 實作
/// </summary>
public class POHeaderRepository : Repository<POHeader>, IPOHeaderRepository
{
    public POHeaderRepository(WmsDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 根據 PO 號取得 PO
    /// </summary>
    public async Task<POHeader?> GetByPoNumberAsync(string poNumber)
    {
        return await _dbSet
            .Include(ph => ph.PODetails)
            .FirstOrDefaultAsync(ph => ph.PoNumber == poNumber);
    }

    /// <summary>
    /// 取得指定狀態的 PO 列表
    /// </summary>
    public async Task<IEnumerable<POHeader>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Include(ph => ph.PODetails)
            .Where(ph => ph.Status == status)
            .ToListAsync();
    }

    /// <summary>
    /// 根據供應商代號取得 PO 列表
    /// </summary>
    public async Task<IEnumerable<POHeader>> GetByVendorCodeAsync(string vendorCode)
    {
        return await _dbSet
            .Include(ph => ph.PODetails)
            .Where(ph => ph.VendorCode == vendorCode)
            .ToListAsync();
    }

    /// <summary>
    /// 取得 PO 列表（含明細）
    /// </summary>
    public async Task<POHeader?> GetWithDetailsAsync(string poNumber)
    {
        return await _dbSet
            .Include(ph => ph.PODetails)
            .FirstOrDefaultAsync(ph => ph.PoNumber == poNumber);
    }

    /// <summary>
    /// 分頁查詢 PO 列表
    /// </summary>
    public async Task<(IEnumerable<POHeader> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        string? status = null,
        string? vendorCode = null)
    {
        var query = _dbSet
            .Include(ph => ph.PODetails)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(ph => ph.Status == status);
        }

        if (!string.IsNullOrEmpty(vendorCode))
        {
            query = query.Where(ph => ph.VendorCode == vendorCode);
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(ph => ph.PoDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}