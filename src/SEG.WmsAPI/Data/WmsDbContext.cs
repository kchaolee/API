using Microsoft.EntityFrameworkCore;
using SEG.WmsAPI.Models.Entities;

namespace SEG.WmsAPI.Data;

/// <summary>
/// WMS 資料庫上下文
/// </summary>
public class WmsDbContext : DbContext
{
    private readonly string _connectionString;
    private readonly bool _enableSensitiveDataLogging;
    private readonly int _commandTimeout;

    public WmsDbContext(string connectionString, bool enableSensitiveDataLogging, int commandTimeout)
    {
        _connectionString = connectionString;
        _enableSensitiveDataLogging = enableSensitiveDataLogging;
        _commandTimeout = commandTimeout;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString, options =>
        {
            options.CommandTimeout(_commandTimeout);
            options.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), null);
        });

        if (_enableSensitiveDataLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    public DbSet<ASN> ASNs { get; set; }
    public DbSet<ASNDetail> ASNDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ASN>(entity =>
        {
            entity.HasKey(e => e.ASNKey);
        });

        modelBuilder.Entity<ASNDetail>(entity =>
        {
            entity.HasKey(e => new { e.WmsAsnNumber, e.LineNo }); 
        });
    }
}
