using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class ApprovalDecisionConfiguration : IEntityTypeConfiguration<ApprovalDecision>
{
    public void Configure(EntityTypeBuilder<ApprovalDecision> builder)
    {
        builder.HasOne(ad => ad.ApprovalInstance)
               .WithMany(ai => ai.Decisions)
               .HasForeignKey(ad => ad.ApprovalInstanceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ad => ad.DecidedByUser)
               .WithMany()
               .HasForeignKey(ad => ad.DecidedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
