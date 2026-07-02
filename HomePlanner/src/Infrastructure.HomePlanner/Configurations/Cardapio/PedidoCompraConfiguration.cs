using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class PedidoCompraConfiguration : IEntityTypeConfiguration<PedidoCompra>
{
    public void Configure(EntityTypeBuilder<PedidoCompra> builder)
    {
        builder.ToTable("PedidosCompra");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Descricao).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Quantidade).HasMaxLength(100);
        builder.Property(p => p.SolicitanteUsuarioId).HasMaxLength(450);
        builder.Property(p => p.DeletedByUsuarioId).HasMaxLength(450);
        builder.Property(p => p.CriadoPor).HasMaxLength(450);
        builder.Property(p => p.ModificadoPor).HasMaxLength(450);

        // TenantId primeiro no índice composto
        builder.HasIndex(p => new { p.TenantId, p.DataInicioSemana });

        builder.HasOne(p => p.Solicitante)
            .WithMany()
            .HasForeignKey(p => p.SolicitanteUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Lista)
            .WithMany()
            .HasForeignKey(p => p.ListaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
