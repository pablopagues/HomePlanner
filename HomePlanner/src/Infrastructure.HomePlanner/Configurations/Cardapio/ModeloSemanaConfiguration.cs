using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class ModeloSemanaConfiguration : IEntityTypeConfiguration<ModeloSemana>
{
    public void Configure(EntityTypeBuilder<ModeloSemana> builder)
    {
        builder.ToTable("ModelosSemana");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Nome).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Descricao).HasMaxLength(500);
        builder.Property(m => m.DeletedByUsuarioId).HasMaxLength(450);
        builder.Property(m => m.CriadoPor).HasMaxLength(450);
        builder.Property(m => m.ModificadoPor).HasMaxLength(450);
        builder.HasIndex(m => new { m.TenantId, m.Nome });
    }
}
