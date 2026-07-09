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
    public DbSet<ApprovalInstance> ApprovalInstances { get; set; }
    public DbSet<ApprovalDecision> ApprovalDecisions { get; set; }

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

        modelBuilder.Entity<Location>()
                    .HasQueryFilter(l => l.IsActive);

        modelBuilder.Entity<Location>()
                    .Property(l => l.IsActive)
                    .HasDefaultValue(true);

        modelBuilder.Entity<LocationType>()
                    .HasQueryFilter(lt => lt.IsActive);

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

        modelBuilder.Entity<Lot>()
                    .HasQueryFilter(l => l.IsActive);

        modelBuilder.Entity<Lot>()
                    .Property(l => l.IsActive)
                    .HasDefaultValue(true);

        modelBuilder.Entity<Material>()
                    .HasQueryFilter(m => m.IsActive);

        modelBuilder.Entity<Material>()
                    .Property(m => m.IsActive)
                    .HasDefaultValue(true);

        modelBuilder.Entity<Supplier>()
                    .HasQueryFilter(s => s.IsActive);

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
                    .HasQueryFilter(ar => ar.IsActive);

        modelBuilder.Entity<ApprovalRule>()
                    .Property(ar => ar.IsActive)
                    .HasDefaultValue(true);

        modelBuilder.Entity<ApprovalDecision>()
                    .HasOne(ad => ad.ApprovalInstance)
                    .WithMany(ai => ai.Decisions)
                    .HasForeignKey(ad => ad.ApprovalInstanceId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApprovalDecision>()
                    .HasOne(ad => ad.DecidedByUser)
                    .WithMany()
                    .HasForeignKey(ad => ad.DecidedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Subcontractor>()
                    .Property(subcon => subcon.IsActive)
                    .HasDefaultValue(true);

        modelBuilder.Entity<Subcontractor>()
                    .HasQueryFilter(subcon => subcon.IsActive);

        modelBuilder.Entity<PurchaseOrder>()
                    .HasOne<Supplier>()
                    .WithMany()
                    .HasForeignKey(po => po.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
                    .Property(po => po.ExchangeRate)
                    .HasPrecision(18, 6);

        modelBuilder.Entity<PurchaseOrder>()
                    .Property(po => po.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

        modelBuilder.Entity<PurchaseOrderLine>()
                    .HasOne(pol=>pol.PurchaseOrder)
                    .WithMany(po => po.Lines)
                    .HasForeignKey(pol => pol.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseOrderLine>()
                    .HasOne<Material>()
                    .WithMany()
                    .HasForeignKey(pol => pol.MaterialId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrderLine>()
                    .Property(pol => pol.Quantity)
                    .HasPrecision(18, 4);

        modelBuilder.Entity<PurchaseOrderLine>()
                    .Property(pol => pol.UnitCostForeign)
                    .HasPrecision(18, 4);

        modelBuilder.Entity<SubconOrder>()
            .HasOne(so => so.Subcontractor)
            .WithMany()
            .HasForeignKey(so => so.SubcontractorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SubconOrder>()
                    .HasOne(so => so.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(so => so.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SubconOrder>()
                    .Property(so => so.ExchangeRate)
                    .HasPrecision(18, 6);

        modelBuilder.Entity<SubconOrder>()
                    .Property(so => so.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

        modelBuilder.Entity<SubconOrderLine>()
                    .HasOne(sol => sol.SubconOrder)
                    .WithMany(so => so.Lines)
                    .HasForeignKey(sol => sol.SubconOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SubconOrderLine>()
                    .HasOne<Material>()
                    .WithMany()
                    .HasForeignKey(sol => sol.MaterialId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SubconOrderLine>()
                    .HasOne<Material>()
                    .WithMany()
                    .HasForeignKey(sol => sol.ExpectedOutputMaterialId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SubconOrderLine>()
                    .Property(sol => sol.QuantitySent)
                    .HasPrecision(18, 4);

        modelBuilder.Entity<SubconOrderLine>()
                    .Property(sol => sol.ExpectedOutputQuantity)
                    .HasPrecision(18, 4);

        modelBuilder.Entity<SubconOrderLine>()
                    .Property(sol => sol.ProcessingCostForeign)
                    .HasPrecision(18, 4);
    }
}