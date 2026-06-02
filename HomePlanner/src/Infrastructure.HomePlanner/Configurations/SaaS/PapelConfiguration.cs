using Domain.HomePlanner.Models.SaaS.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.SaaS;

public class PapelConfiguration : IEntityTypeConfiguration<Papel>
{
    public void Configure(EntityTypeBuilder<Papel> builder)
    {
        builder.Property(p => p.NomeAmigavel).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.TenantId);
    }
}
