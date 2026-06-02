using Domain.HomePlanner.Models.SaaS.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.SaaS;

public class TenantDelecaoSolicitadaConfiguration : IEntityTypeConfiguration<TenantDelecaoSolicitada>
{
    public void Configure(EntityTypeBuilder<TenantDelecaoSolicitada> builder)
    {
        builder.ToTable("TenantDelecoesSolicitadas");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.SolicitadoPorUsuarioId).HasMaxLength(450).IsRequired();
        builder.HasOne(t => t.Tenant).WithMany()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
