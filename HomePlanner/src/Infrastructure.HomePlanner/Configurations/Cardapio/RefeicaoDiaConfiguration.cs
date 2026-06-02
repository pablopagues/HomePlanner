using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class RefeicaoDiaConfiguration : IEntityTypeConfiguration<RefeicaoDia>
{
    public void Configure(EntityTypeBuilder<RefeicaoDia> builder)
    {
        builder.ToTable("RefeicoesDia");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Observacao).HasMaxLength(500);

        // Um slot único por (planejamento, dia, tipo) - ignorando deletados
        builder.HasIndex(r => new { r.PlanejamentoSemanalId, r.DiaSemana, r.TipoRefeicao })
            .HasFilter("[IsDeleted] = 0")
            .IsUnique();

        builder.HasOne(r => r.PlanejamentoSemanal)
            .WithMany(p => p.RefeicoesDia)
            .HasForeignKey(r => r.PlanejamentoSemanalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Receita)
            .WithMany(rec => rec.RefeicoesDia)
            .HasForeignKey(r => r.ReceitaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
