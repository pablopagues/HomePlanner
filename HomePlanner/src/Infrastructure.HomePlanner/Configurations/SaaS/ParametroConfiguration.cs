using Domain.HomePlanner.Models.SaaS.Configuracao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.SaaS;

public class ParametroConfiguration : IEntityTypeConfiguration<Parametro>
{
    public void Configure(EntityTypeBuilder<Parametro> builder)
    {
        builder.ToTable("Parametros");
        builder.HasKey(p => p.Chave);
        builder.Property(p => p.Chave).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Valor).HasMaxLength(2000).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(500);
    }
}
