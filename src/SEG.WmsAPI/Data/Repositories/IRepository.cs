namespace SEG.WmsAPI.Data.Repositories;

/// <summary>
/// 泛型 Repository 介面
/// </summary>
/// <typeparam name="T">實體類型</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// 根據 ID 取得實體
    /// </summary>
    Task<T?> GetByIdAsync(object id);

    /// <summary>
    /// 取得所有實體
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// 根據條件查詢實體
    /// </summary>
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

    /// <summary>
    /// 檢查是否存在符合條件的實體
    /// </summary>
    Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

    /// <summary>
    /// 新增實體
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// 新增多個實體
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// 更新實體
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// 更新多個實體
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// 刪除實體
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// 刪除多個實體
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// 計數
    /// </summary>
    Task<int> CountAsync();

    /// <summary>
    /// 根據條件計數
    /// </summary>
    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
}