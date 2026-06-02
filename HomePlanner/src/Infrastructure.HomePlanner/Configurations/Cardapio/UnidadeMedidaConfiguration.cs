using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class UnidadeMedidaConfiguration : IEntityTypeConfiguration<UnidadeMedida>
{
    public void Configure(EntityTypeBuilder<UnidadeMedida> builder)
    {
        builder.ToTable("UnidadesMedida");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Codigo).HasMaxLength(10).IsRequired();
        builder.Property(u => u.Nome).HasMaxLength(50).IsRequired();
        builder.Property(u => u.ChaveTraducao).HasMaxLength(100).IsRequired();
        builder.Property(u => u.FatorParaBase).HasPrecision(18, 6);
        builder.HasIndex(u => u.Codigo).IsUnique();
    }
}
