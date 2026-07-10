using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class LocationTypeConfiguration : IEntityTypeConfiguration<LocationType>
{
    public void Configure(EntityTypeBuilder<LocationType> builder)
    {
        builder.HasQueryFilter(lt => lt.IsActive);

        builder.Property(lt => lt.IsActive)
               .HasDefaultValue(true);
    }
}
