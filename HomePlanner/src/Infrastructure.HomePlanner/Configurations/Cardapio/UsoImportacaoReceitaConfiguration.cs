using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class UsoImportacaoReceitaConfiguration : IEntityTypeConfiguration<UsoImportacaoReceita>
{
    public void Configure(EntityTypeBuilder<UsoImportacaoReceita> builder)
    {
        builder.ToTable("UsosImportacaoReceita");
        builder.HasKey(u => u.Id);

        // Uma linha por tenant/mês (TenantId primeiro, como nos demais).
        builder.HasIndex(u => new { u.TenantId, u.AnoMes }).IsUnique();
    }
}
