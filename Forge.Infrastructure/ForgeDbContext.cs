using Forge.Domain;
using Microsoft.EntityFrameworkCore;

namespace Forge.Infrastructure;

public class ForgeDbContext : DbContext
{
    public ForgeDbContext(DbContextOptions<ForgeDbContext> options) : base(options)
    {
    }

    public DbSet<Material> Materials { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Subcontractor> Subcontractors { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<LocationType> LocationTypes { get; set; }
    public DbSet<Lot> Lots { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
    public DbSet<SubconOrder> SubconOrders { get; set; }
    public DbSet<SubconOrderLine> SubconOrderLines { get; set; }
    public DbSet<BillOfMaterials> BillOfMaterials { get; set; }
    public DbSet<BillOfMaterialsLine> BillOfMaterialsLines { get; set; }
    public DbSet<ApprovalRule> ApprovalRules { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<CompanySettings> CompanySettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillOfMaterialsLine>()
                    .HasOne<BillOfMaterialsLine>()
                    .WithMany(line => line.Children)
                    .HasForeignKey(line => line.ParentLineId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Location>()
                    .HasOne(l => l.LocationType)
                    .WithMany(lt => lt.Locations)
                    .HasForeignKey(l => l.LocationTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LocationType>()
                    .Property(lt => lt.IsActive)
                    .HasDefaultValue(true);

        modelBuilder.Entity<Lot>()
                    .HasOne(l => l.Material)
                    .WithMany(m => m.Lots)
                    .HasForeignKey(l => l.MaterialId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lot>()
                    .HasOne(l => l.Supplier)
                    .WithMany(s => s.Lots)
                    .HasForeignKey(l => l.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lot>()
                    .HasOne(l => l.CurrentLocation)
                    .WithMany()
                    .HasForeignKey(l => l.CurrentLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Material>()
                    .Property(m => m.IsActive)
                    .HasDefaultValue(true);

        modelBuilder.Entity<Lot>()
                    .Property(l => l.IsActive)
                    .HasDefaultValue(true);

        modelBuilder.Entity<Supplier>()
                    .Property(s => s.IsActive)
                    .HasDefaultValue(true);

        modelBuilder.Entity<StockMovement>()
                    .HasOne(sm => sm.Lot)
                    .WithMany()
                    .HasForeignKey(sm => sm.LotId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockMovement>()
                    .HasOne(sm => sm.FromLocation)
                    .WithMany()
                    .HasForeignKey(sm => sm.FromLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockMovement>()
                    .HasOne(sm => sm.ToLocation)
                    .WithMany()
                    .HasForeignKey(sm => sm.ToLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockMovement>()
                    .HasOne(sm => sm.ReleasedByUser)
                    .WithMany()
                    .HasForeignKey(sm => sm.ReleasedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockMovement>()
                    .HasOne(sm => sm.ReceivedByUser)
                    .WithMany()
                    .HasForeignKey(sm => sm.ReceivedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApprovalRule>()
                    .HasOne(ar => ar.RequiredRole)
                    .WithMany()
                    .HasForeignKey(ar => ar.RequiredRoleId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApprovalRule>()
                    .Property(ar => ar.IsActive)
                    .HasDefaultValue(true);
    }
}