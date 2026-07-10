using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class BillOfMaterialsConfiguration : IEntityTypeConfiguration<BillOfMaterials>
{
    public void Configure(EntityTypeBuilder<BillOfMaterials> builder)
    {
        builder.HasOne(bom => bom.OutputMaterial)
               .WithMany()
               .HasForeignKey(bom => bom.OutputMaterialId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(bom => bom.BillOfMaterialStatus)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.HasQueryFilter(m => m.IsActive);

        builder.Property(m => m.IsActive)
               .HasDefaultValue(true);
    }
}
