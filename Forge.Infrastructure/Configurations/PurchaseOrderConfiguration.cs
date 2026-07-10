using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasOne<Supplier>()
               .WithMany()
               .HasForeignKey(po => po.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(po => po.ExchangeRate)
               .HasPrecision(18, 6);

        builder.Property(po => po.Status)
               .HasConversion<string>()
               .HasMaxLength(20);
    }
}
