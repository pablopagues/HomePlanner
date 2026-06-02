using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class RefeicaoModeloConfiguration : IEntityTypeConfiguration<RefeicaoModelo>
{
    public void Configure(EntityTypeBuilder<RefeicaoModelo> builder)
    {
        builder.ToTable("RefeicoesModelo");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Observacao).HasMaxLength(500);

        builder.HasIndex(r => new { r.ModeloSemanaId, r.DiaSemana, r.TipoRefeicao })
            .HasFilter("[IsDeleted] = 0")
            .IsUnique();

        builder.HasOne(r => r.ModeloSemana)
            .WithMany(m => m.RefeicoesModelo)
            .HasForeignKey(r => r.ModeloSemanaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Receita)
            .WithMany(rec => rec.RefeicoesModelo)
            .HasForeignKey(r => r.ReceitaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
