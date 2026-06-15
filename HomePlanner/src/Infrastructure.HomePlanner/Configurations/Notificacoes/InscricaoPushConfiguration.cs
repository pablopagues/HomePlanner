using Domain.HomePlanner.Models.Notificacoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Notificacoes;

public class InscricaoPushConfiguration : IEntityTypeConfiguration<InscricaoPush>
{
    public void Configure(EntityTypeBuilder<InscricaoPush> builder)
    {
        builder.ToTable("InscricoesPush");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.UsuarioId).HasMaxLength(450).IsRequired();
        builder.Property(i => i.Endpoint).HasMaxLength(2048).IsRequired();
        builder.Property(i => i.P256dh).HasMaxLength(256).IsRequired();
        builder.Property(i => i.Auth).HasMaxLength(256).IsRequired();
        builder.Property(i => i.UserAgent).HasMaxLength(512);
        builder.Property(i => i.DeletedByUsuarioId).HasMaxLength(450);

        // TenantId primeiro no índice composto (padrão do projeto)
        builder.HasIndex(i => new { i.TenantId, i.UsuarioId });
        // Lookup/upsert por endpoint
        builder.HasIndex(i => i.Endpoint);

        builder.HasOne(i => i.Usuario)
            .WithMany()
            .HasForeignKey(i => i.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
