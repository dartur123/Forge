using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasOne(sm => sm.Lot)
               .WithMany()
               .HasForeignKey(sm => sm.LotId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.FromLocation)
               .WithMany()
               .HasForeignKey(sm => sm.FromLocationId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.ToLocation)
               .WithMany()
               .HasForeignKey(sm => sm.ToLocationId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.ReleasedByUser)
               .WithMany()
               .HasForeignKey(sm => sm.ReleasedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.ReceivedByUser)
               .WithMany()
               .HasForeignKey(sm => sm.ReceivedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
