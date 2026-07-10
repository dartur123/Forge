using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class SubconOrderLineConfiguration : IEntityTypeConfiguration<SubconOrderLine>
{
    public void Configure(EntityTypeBuilder<SubconOrderLine> builder)
    {
        builder.HasOne(sol => sol.SubconOrder)
               .WithMany(so => so.Lines)
               .HasForeignKey(sol => sol.SubconOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Material>()
               .WithMany()
               .HasForeignKey(sol => sol.MaterialId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Material>()
               .WithMany()
               .HasForeignKey(sol => sol.ExpectedOutputMaterialId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(sol => sol.QuantitySent)
               .HasPrecision(18, 4);

        builder.Property(sol => sol.ExpectedOutputQuantity)
               .HasPrecision(18, 4);

        builder.Property(sol => sol.ProcessingCostForeign)
               .HasPrecision(18, 4);
    }
}
