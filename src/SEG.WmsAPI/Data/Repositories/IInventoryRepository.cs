using SEG.WmsAPI.Models.Entities;

namespace SEG.WmsAPI.Data.Repositories;

/// <summary>
///庫存 Repository 介面
/// </summary>
public interface IInventoryRepository : IRepository<Inventory>
{
    /// <summary>
    /// 根據倉庫和產品取得庫存
    /// </summary>
    Task<Inventory?> GetByWarehouseAndItemAsync(string warehouseCode, string itemCode);

    /// <summary>
    /// 取得指定倉庫的所有庫存
    /// </summary>
    Task<IEnumerable<Inventory>> GetByWarehouseAsync(string warehouseCode);

    /// <summary>
    /// 取得指定產品在所有倉庫的庫存
    /// </summary>
    Task<IEnumerable<Inventory>> GetByItemAsync(string itemCode);

    /// <summary>
    /// 更新庫存數量
    /// </summary>
    Task<int> UpdateQuantityAsync(string warehouseCode, string itemCode, decimal quantityDelta);

    /// <summary>
    /// 取得低於安全庫存的產品列表
    /// </summary>
    Task<IEnumerable<Inventory>> GetLowStockAsync(string warehouseCode, decimal threshold);
}