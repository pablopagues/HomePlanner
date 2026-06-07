using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class IngredienteConfiguration : IEntityTypeConfiguration<Ingrediente>
{
    public void Configure(EntityTypeBuilder<Ingrediente> builder)
    {
        builder.ToTable("Ingredientes");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Nome).HasMaxLength(200).IsRequired();
        builder.Property(i => i.NomeNormalizado).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Categoria).HasMaxLength(100);
        builder.Property(i => i.DeletedByUsuarioId).HasMaxLength(450);
        builder.Property(i => i.CriadoPor).HasMaxLength(450);
        builder.Property(i => i.ModificadoPor).HasMaxLength(450);

        // TenantId primeiro no índice composto
        builder.HasIndex(i => new { i.TenantId, i.NomeNormalizado });

        builder.HasOne(i => i.UnidadeMedidaPadrao)
            .WithMany()
            .HasForeignKey(i => i.UnidadeMedidaPadraoId)
            .OnDelete(DeleteBehavior.SetNull);

        // Auto-referência para o produto base de compra.
        builder.HasOne(i => i.IngredienteBase)
            .WithMany()
            .HasForeignKey(i => i.IngredienteBaseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(i => i.IngredienteBaseId);
    }
}
