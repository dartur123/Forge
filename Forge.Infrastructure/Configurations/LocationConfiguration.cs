using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasOne(l => l.LocationType)
               .WithMany(lt => lt.Locations)
               .HasForeignKey(l => l.LocationTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(l => l.IsActive);

        builder.Property(l => l.IsActive)
               .HasDefaultValue(true);
    }
}
