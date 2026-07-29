using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class ProdutoRecorrenteConfiguration : IEntityTypeConfiguration<ProdutoRecorrente>
{
    public void Configure(EntityTypeBuilder<ProdutoRecorrente> builder)
    {
        builder.ToTable("ProdutosRecorrentes");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Descricao).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Quantidade).HasMaxLength(100);
        builder.Property(p => p.DeletedByUsuarioId).HasMaxLength(450);
        builder.Property(p => p.CriadoPor).HasMaxLength(450);
        builder.Property(p => p.ModificadoPor).HasMaxLength(450);

        // Loja opcional; se a loja for excluída, o produto volta ao balde "Geral".
        builder.HasOne(p => p.Lista)
            .WithMany()
            .HasForeignKey(p => p.ListaId)
            .OnDelete(DeleteBehavior.SetNull);

        // TenantId primeiro no índice composto
        builder.HasIndex(p => new { p.TenantId, p.Ordem });
    }
}
