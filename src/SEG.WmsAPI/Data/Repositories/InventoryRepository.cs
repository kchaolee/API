using Microsoft.EntityFrameworkCore;
using SEG.WmsAPI.Models.Entities;

namespace SEG.WmsAPI.Data.Repositories;

/// <summary>
/// 庫存 Repository 實作
/// </summary>
public class InventoryRepository : Repository<Inventory>, IInventoryRepository
{
    public InventoryRepository(WmsDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 根據倉庫和產品取得庫存
    /// </summary>
    public async Task<Inventory?> GetByWarehouseAndItemAsync(string warehouseCode, string itemCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.WarehouseCode == warehouseCode && i.ItemCode == itemCode);
    }

    /// <summary>
    /// 取得指定倉庫的所有庫存
    /// </summary>
    public async Task<IEnumerable<Inventory>> GetByWarehouseAsync(string warehouseCode)
    {
        return await _dbSet
            .Where(i => i.WarehouseCode == warehouseCode)
            .ToListAsync();
    }

    /// <summary>
    /// 取得指定產品在所有倉庫的庫存
    /// </summary>
    public async Task<IEnumerable<Inventory>> GetByItemAsync(string itemCode)
    {
        return await _dbSet
            .Where(i => i.ItemCode == itemCode)
            .ToListAsync();
    }

    /// <summary>
    /// 更新庫存數量
    /// </summary>
    public async Task<int> UpdateQuantityAsync(string warehouseCode, string itemCode, decimal quantityDelta)
    {
        var inventory = await GetByWarehouseAndItemAsync(warehouseCode, itemCode);

        if (inventory == null)
        {
            return 0;
        }

        inventory.Quantity += quantityDelta;
        inventory.LastUpdated = DateTime.UtcNow;

        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 取得低於安全庫存的產品列表
    /// </summary>
    public async Task<IEnumerable<Inventory>> GetLowStockAsync(string warehouseCode, decimal threshold)
    {
        return await _dbSet
            .Where(i => i.WarehouseCode == warehouseCode && i.Quantity < threshold)
            .OrderBy(i => i.Quantity)
            .ToListAsync();
    }
}