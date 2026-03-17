using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SEG.WmsAPI.Models.Entities;

namespace SEG.WmsAPI.Data;

/// <summary>
/// WMS 資料庫上下文
/// </summary>
public class WmsDbContext : DbContext
{
    public WmsDbContext(DbContextOptions<WmsDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// PO 主檔
    /// </summary>
    public DbSet<POHeader> POHeaders { get; set; } = null!;

    /// <summary>
    /// PO 明細
    /// </summary>
    public DbSet<PODetail> PODetails { get; set; } = null!;

    /// <summary>
    /// 倉庫
    /// </summary>
    public DbSet<Warehouse> Warehouses { get; set; } = null!;

    /// <summary>
    /// 庫存
    /// </summary>
    public DbSet<Inventory> Inventories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PO 明細：一對多關聯
        modelBuilder.Entity<PODetail>()
            .HasOne(pd => pd.POHeader)
            .WithMany(ph => ph.PODetails)
            .HasForeignKey(pd => pd.POHeaderId)
            .OnDelete(DeleteBehavior.Cascade);

        // PO 主檔：唯一索引
        modelBuilder.Entity<POHeader>()
            .HasIndex(ph => ph.PoNumber)
            .IsUnique()
            .HasFilter("[PoNumber] IS NOT NULL");

        // 庫存：複合唯一索引
        modelBuilder.Entity<Inventory>()
            .HasIndex(i => new { i.WarehouseCode, i.ItemCode })
            .IsUnique()
            .HasFilter("[WarehouseCode] IS NOT NULL AND [ItemCode] IS NOT NULL");

        // 倉庫：主鍵設定
        modelBuilder.Entity<Warehouse>()
            .HasKey(w => w.Code);

        // 全域查詢過濾器：軟刪除 (如果需要)
        // modelBuilder.Entity<Warehouse>()
        //     .HasQueryFilter(w => w.IsActive);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 自動更新時間戳記
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<IAuditable>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}