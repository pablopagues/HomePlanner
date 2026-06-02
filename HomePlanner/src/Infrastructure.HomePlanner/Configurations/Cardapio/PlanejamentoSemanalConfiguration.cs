using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class PlanejamentoSemanalConfiguration : IEntityTypeConfiguration<PlanejamentoSemanal>
{
    public void Configure(EntityTypeBuilder<PlanejamentoSemanal> builder)
    {
        builder.ToTable("PlanejamentosSemanais");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).HasMaxLength(200);
        builder.Property(p => p.DeletedByUsuarioId).HasMaxLength(450);
        builder.Property(p => p.CriadoPor).HasMaxLength(450);
        builder.Property(p => p.ModificadoPor).HasMaxLength(450);

        // Um planejamento por semana por tenant (ignorando deletados)
        builder.HasIndex(p => new { p.TenantId, p.DataInicio })
            .HasFilter("[IsDeleted] = 0")
            .IsUnique();

        builder.HasOne(p => p.ModeloSemanaOrigem)
            .WithMany()
            .HasForeignKey(p => p.ModeloSemanaOrigemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
