namespace SEG.WmsAPI.Data;

/// <summary>
/// Unit of Work 介面 - 管理事務
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// PO Header Repository
    /// </summary>
    Repositories.IPOHeaderRepository POHeaders { get; }

    /// <summary>
    /// PO Detail Repository
    /// </summary>
    Repositories.IRepository<Models.Entities.PODetail> PODetails { get; }

    /// <summary>
    /// 倉庫 Repository
    /// </summary>
    Repositories.IRepository<Models.Entities.Warehouse> Warehouses { get; }

    /// <summary>
    /// 庫存 Repository
    /// </summary>
    Repositories.IInventoryRepository Inventories { get; }

    /// <summary>
    /// 儲存所有變更
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// 開始交易
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// 提交交易
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// 回滾交易
    /// </summary>
    Task RollbackTransactionAsync();
}