using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class LotConfiguration : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> builder)
    {
        builder.HasOne(l => l.Material)
               .WithMany(m => m.Lots)
               .HasForeignKey(l => l.MaterialId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Supplier)
               .WithMany(s => s.Lots)
               .HasForeignKey(l => l.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.CurrentLocation)
               .WithMany()
               .HasForeignKey(l => l.CurrentLocationId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(l => l.IsActive);

        builder.Property(l => l.IsActive)
               .HasDefaultValue(true);
    }
}
