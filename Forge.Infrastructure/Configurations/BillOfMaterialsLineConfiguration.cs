using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class BillOfMaterialsLineConfiguration : IEntityTypeConfiguration<BillOfMaterialsLine>
{
    public void Configure(EntityTypeBuilder<BillOfMaterialsLine> builder)
    {
        builder.HasOne(boml => boml.BillOfMaterials)
               .WithMany(bom => bom.Lines)
               .HasForeignKey(boml => boml.BillOfMaterialsId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(boml => boml.Material)
               .WithMany()
               .HasForeignKey(boml => boml.MaterialId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
