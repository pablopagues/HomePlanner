using Domain.HomePlanner.Models.SaaS.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.SaaS;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.NomeResponsavel).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Email).HasMaxLength(256).IsRequired();
        builder.Property(t => t.PaisId).HasMaxLength(2);
        builder.Property(t => t.OwnerUsuarioId).HasMaxLength(450).IsRequired();
        builder.HasIndex(t => t.Email).IsUnique();
    }
}
