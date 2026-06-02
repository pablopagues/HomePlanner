using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class ReceitaIngredienteConfiguration : IEntityTypeConfiguration<ReceitaIngrediente>
{
    public void Configure(EntityTypeBuilder<ReceitaIngrediente> builder)
    {
        builder.ToTable("ReceitasIngredientes");
        builder.HasKey(ri => ri.Id);
        builder.Property(ri => ri.Quantidade).HasPrecision(10, 3);
        builder.Property(ri => ri.Observacao).HasMaxLength(500);

        // Unicidade: mesmo ingrediente só uma vez por receita (ignorando deletados)
        builder.HasIndex(ri => new { ri.ReceitaId, ri.IngredienteId })
            .HasFilter("[IsDeleted] = 0")
            .IsUnique();

        builder.HasOne(ri => ri.Receita)
            .WithMany(r => r.Ingredientes)
            .HasForeignKey(ri => ri.ReceitaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ri => ri.Ingrediente)
            .WithMany(i => i.ReceitasIngredientes)
            .HasForeignKey(ri => ri.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ri => ri.UnidadeMedida)
            .WithMany()
            .HasForeignKey(ri => ri.UnidadeMedidaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
