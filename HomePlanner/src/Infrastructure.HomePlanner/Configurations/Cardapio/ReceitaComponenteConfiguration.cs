using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class ReceitaComponenteConfiguration : IEntityTypeConfiguration<ReceitaComponente>
{
    public void Configure(EntityTypeBuilder<ReceitaComponente> builder)
    {
        builder.ToTable("ReceitasComponentes");
        builder.HasKey(rc => rc.Id);

        // Unicidade: mesmo componente só uma vez por prato (ignorando deletados)
        builder.HasIndex(rc => new { rc.ReceitaPaiId, rc.ReceitaComponenteId })
            .HasFilter("[IsDeleted] = 0")
            .IsUnique();

        builder.HasOne(rc => rc.ReceitaPai)
            .WithMany(r => r.Componentes)
            .HasForeignKey(rc => rc.ReceitaPaiId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict no componente: evita cascata cíclica e impede apagar uma receita
        // ainda usada como componente sem tratamento explícito.
        builder.HasOne(rc => rc.Componente)
            .WithMany()
            .HasForeignKey(rc => rc.ReceitaComponenteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
