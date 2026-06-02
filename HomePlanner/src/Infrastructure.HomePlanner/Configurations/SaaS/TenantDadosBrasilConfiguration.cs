using Domain.HomePlanner.Models.SaaS.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.SaaS;

public class TenantDadosBrasilConfiguration : IEntityTypeConfiguration<TenantDadosBrasil>
{
    public void Configure(EntityTypeBuilder<TenantDadosBrasil> builder)
    {
        builder.ToTable("TenantDadosBrasil");
        builder.HasKey(t => t.TenantId);
        builder.Property(t => t.Cpf).HasMaxLength(20);
        builder.HasOne(t => t.Tenant).WithOne()
            .HasForeignKey<TenantDadosBrasil>(t => t.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
