using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class SubconOrderConfiguration : IEntityTypeConfiguration<SubconOrder>
{
    public void Configure(EntityTypeBuilder<SubconOrder> builder)
    {
        builder.HasOne(so => so.Subcontractor)
               .WithMany()
               .HasForeignKey(so => so.SubcontractorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(so => so.CreatedByUser)
               .WithMany()
               .HasForeignKey(so => so.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(so => so.ExchangeRate)
               .HasPrecision(18, 6);

        builder.Property(so => so.Status)
               .HasConversion<string>()
               .HasMaxLength(20);
    }
}
