using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class ReceitaConfiguration : IEntityTypeConfiguration<Receita>
{
    public void Configure(EntityTypeBuilder<Receita> builder)
    {
        builder.ToTable("Receitas");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nome).HasMaxLength(300).IsRequired();
        builder.Property(r => r.NomeNormalizado).HasMaxLength(300).IsRequired();
        builder.Property(r => r.UrlOrigem).HasMaxLength(2000);
        builder.Property(r => r.UrlImagem).HasMaxLength(2000);
        builder.Property(r => r.DeletedByUsuarioId).HasMaxLength(450);
        builder.Property(r => r.CriadoPor).HasMaxLength(450);
        builder.Property(r => r.ModificadoPor).HasMaxLength(450);

        builder.HasIndex(r => new { r.TenantId, r.NomeNormalizado });
    }
}
