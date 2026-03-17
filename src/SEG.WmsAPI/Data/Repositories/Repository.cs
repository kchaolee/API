using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SEG.WmsAPI.Data.Repositories;

/// <summary>
/// 泛型 Repository 實作
/// </summary>
/// <typeparam name="T">實體類型</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly WmsDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(WmsDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// 根據 ID 取得實體
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// 取得所有實體
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// 根據條件查詢實體
    /// </summary>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// 檢查是否存在符合條件的實體
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    /// <summary>
    /// 新增實體
    /// </summary>
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// 新增多個實體
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    /// <summary>
    /// 更新實體
    /// </summary>
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// 更新多個實體
    /// </summary>
    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    /// <summary>
    /// 刪除實體
    /// </summary>
    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// 刪除多個實體
    /// </summary>
    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// 計數
    /// </summary>
    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    /// <summary>
    /// 根據條件計數
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }
}