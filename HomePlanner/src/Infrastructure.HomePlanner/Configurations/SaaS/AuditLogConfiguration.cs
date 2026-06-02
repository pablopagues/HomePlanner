using Domain.HomePlanner.Models.SaaS.Auditoria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.SaaS;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityColumn();
        builder.Property(a => a.Acao).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Entidade).HasMaxLength(100);
        builder.Property(a => a.EntidadeId).HasMaxLength(100);
        builder.Property(a => a.UsuarioId).HasMaxLength(450);
        builder.Property(a => a.IpAddress).HasMaxLength(50);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.HasIndex(a => new { a.TenantId, a.DataHora });
    }
}
