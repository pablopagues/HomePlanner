using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class ConfiguracaoFamiliaConfiguration : IEntityTypeConfiguration<ConfiguracaoFamilia>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoFamilia> builder)
    {
        builder.ToTable("ConfiguracoesFamilia");
        builder.HasKey(c => c.TenantId);
        builder.Property(c => c.TiposRefeicaoAtivos).HasMaxLength(200).IsRequired();
        builder.Property(c => c.FusoHorario).HasMaxLength(100).IsRequired();
        builder.Property(c => c.CriadoPor).HasMaxLength(450);
        builder.Property(c => c.ModificadoPor).HasMaxLength(450);
    }
}
