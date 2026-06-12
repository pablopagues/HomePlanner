using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class MarcacaoCompraConfiguration : IEntityTypeConfiguration<MarcacaoCompra>
{
    public void Configure(EntityTypeBuilder<MarcacaoCompra> builder)
    {
        builder.ToTable("MarcacoesCompra");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.CriadoPor).HasMaxLength(450);
        builder.Property(m => m.ModificadoPor).HasMaxLength(450);

        // Uma marcação por ingrediente/semana (TenantId primeiro, como nos demais)
        builder.HasIndex(m => new { m.TenantId, m.DataInicioSemana, m.IngredienteId })
            .IsUnique();
    }
}
