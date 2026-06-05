using Domain.HomePlanner.Models.SaaS.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.SaaS;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.Property(u => u.NomeCompleto).HasMaxLength(200).IsRequired();
        builder.Property(u => u.VersaoTermosAceito).HasMaxLength(20);
        builder.Property(u => u.Foto).HasColumnType("varbinary(max)");
        builder.Property(u => u.FotoContentType).HasMaxLength(100);
        builder.HasIndex(u => u.TenantId);
    }
}
