using Domain.HomePlanner.Models.SaaS.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.SaaS;

public class TenantDadosCanadaConfiguration : IEntityTypeConfiguration<TenantDadosCanada>
{
    public void Configure(EntityTypeBuilder<TenantDadosCanada> builder)
    {
        builder.ToTable("TenantDadosCanada");
        builder.HasKey(t => t.TenantId);
        builder.Property(t => t.Province).HasMaxLength(100);
        builder.HasOne(t => t.Tenant).WithOne()
            .HasForeignKey<TenantDadosCanada>(t => t.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
