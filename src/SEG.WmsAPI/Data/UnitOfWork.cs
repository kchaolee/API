using Microsoft.EntityFrameworkCore.Storage;
using SEG.WmsAPI.Data.Repositories;
using SEG.WmsAPI.Models.Entities;

namespace SEG.WmsAPI.Data;

/// <summary>
/// Unit of Work 實作 - 管理事務
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly WmsDbContext _context;
    private Repositories.IPOHeaderRepository? _poHeaders;
    private Repository<PODetail>? _poDetails;
    private Repository<Warehouse>? _warehouses;
    private Repositories.IInventoryRepository? _inventories;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(WmsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// PO Header Repository
    /// </summary>
    public IPOHeaderRepository POHeaders =>
        _poHeaders ??= new POHeaderRepository(_context);

    /// <summary>
    /// PO Detail Repository
    /// </summary>
    public IRepository<PODetail> PODetails =>
        _poDetails ??= new Repository<PODetail>(_context);

    /// <summary>
    /// 倉庫 Repository
    /// </summary>
    public IRepository<Warehouse> Warehouses =>
        _warehouses ??= new Repository<Warehouse>(_context);

    /// <summary>
    /// 庫存 Repository
    /// </summary>
    public IInventoryRepository Inventories =>
        _inventories ??= new InventoryRepository(_context);

    /// <summary>
    /// 儲存所有變更
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 開始交易
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// 提交交易
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// 回滾交易
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// 釋放資源
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}