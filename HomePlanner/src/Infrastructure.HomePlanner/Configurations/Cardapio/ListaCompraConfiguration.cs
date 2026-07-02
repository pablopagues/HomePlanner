using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class ListaCompraConfiguration : IEntityTypeConfiguration<ListaCompra>
{
    public void Configure(EntityTypeBuilder<ListaCompra> builder)
    {
        builder.ToTable("ListasCompra");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Nome).HasMaxLength(100).IsRequired();
        builder.Property(l => l.Icone).HasMaxLength(60);
        builder.Property(l => l.Cor).HasMaxLength(20);
        builder.Property(l => l.DeletedByUsuarioId).HasMaxLength(450);
        builder.Property(l => l.CriadoPor).HasMaxLength(450);
        builder.Property(l => l.ModificadoPor).HasMaxLength(450);

        // TenantId primeiro no índice composto
        builder.HasIndex(l => new { l.TenantId, l.Ordem });
    }
}
