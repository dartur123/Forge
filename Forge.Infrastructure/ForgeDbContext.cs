using Forge.Domain;
using Forge.Domain.Enums;
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
    }
}