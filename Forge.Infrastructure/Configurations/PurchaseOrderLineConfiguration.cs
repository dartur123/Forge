using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.HasOne(pol => pol.PurchaseOrder)
               .WithMany(po => po.Lines)
               .HasForeignKey(pol => pol.PurchaseOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Material>()
               .WithMany()
               .HasForeignKey(pol => pol.MaterialId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(pol => pol.Quantity)
               .HasPrecision(18, 4);

        builder.Property(pol => pol.UnitCostForeign)
               .HasPrecision(18, 4);
    }
}
