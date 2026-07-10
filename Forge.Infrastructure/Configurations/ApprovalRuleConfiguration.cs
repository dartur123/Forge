using Forge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Forge.Infrastructure.Configurations;

public class ApprovalRuleConfiguration : IEntityTypeConfiguration<ApprovalRule>
{
    public void Configure(EntityTypeBuilder<ApprovalRule> builder)
    {
        builder.HasOne(ar => ar.RequiredRole)
               .WithMany()
               .HasForeignKey(ar => ar.RequiredRoleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(ar => ar.IsActive);

        builder.Property(ar => ar.IsActive)
               .HasDefaultValue(true);
    }
}
