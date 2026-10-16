using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class AgendamentoCardapioIAConfiguration : IEntityTypeConfiguration<AgendamentoCardapioIA>
{
    public void Configure(EntityTypeBuilder<AgendamentoCardapioIA> builder)
    {
        builder.ToTable("AgendamentosCardapioIA");
        builder.HasKey(a => a.TenantId);
        builder.Property(a => a.TenantId).ValueGeneratedNever(); // vem do contexto, nunca gerado
        builder.Property(a => a.ParametrosJson).IsRequired();
        builder.Property(a => a.CriadoPor).HasMaxLength(450);
        builder.Property(a => a.ModificadoPor).HasMaxLength(450);
    }
}

public class RascunhoCardapioIAConfiguration : IEntityTypeConfiguration<RascunhoCardapioIA>
{
    public void Configure(EntityTypeBuilder<RascunhoCardapioIA> builder)
    {
        builder.ToTable("RascunhosCardapioIA");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.UltimoErro).HasMaxLength(100);

        // Um rascunho por tenant/semana: trava contra gerar duas vezes.
        builder.HasIndex(r => new { r.TenantId, r.DataInicio }).IsUnique();
    }
}
