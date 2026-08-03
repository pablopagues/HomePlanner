using Domain.HomePlanner.Models.Notificacoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Notificacoes;

public class DispositivoPushConfiguration : IEntityTypeConfiguration<DispositivoPush>
{
    public void Configure(EntityTypeBuilder<DispositivoPush> builder)
    {
        builder.ToTable("DispositivosPush");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.UsuarioId).HasMaxLength(450).IsRequired();
        builder.Property(d => d.Token).HasMaxLength(512).IsRequired();
        builder.Property(d => d.Plataforma).HasMaxLength(20);
        builder.Property(d => d.DispositivoInfo).HasMaxLength(512);
        builder.Property(d => d.DeletedByUsuarioId).HasMaxLength(450);

        // TenantId primeiro no índice composto (padrão do projeto)
        builder.HasIndex(d => new { d.TenantId, d.UsuarioId });
        // Lookup/upsert por token
        builder.HasIndex(d => d.Token);

        builder.HasOne(d => d.Usuario)
            .WithMany()
            .HasForeignKey(d => d.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
