using Microsoft.EntityFrameworkCore;
using FixPoint.Api.Models;

namespace FixPoint.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    // ДОБАВИТЬ ЭТО:
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<RepairRequest> RepairRequests => Set<RepairRequest>();
    public DbSet<RepairEstimate> RepairEstimates => Set<RepairEstimate>();
    public DbSet<RepairService> RepairServices => Set<RepairService>();
    public DbSet<EstimateFeedback> EstimateFeedbacks => Set<EstimateFeedback>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ... (настройка User, как было ранее) ...
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(20).IsRequired();
        });

        // Связи для заявок
        modelBuilder.Entity<RepairRequest>()
            .HasOne(r => r.Client)
            .WithMany()
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RepairRequest>()
            .HasOne(r => r.Master)
            .WithMany()
            .HasForeignKey(r => r.MasterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}