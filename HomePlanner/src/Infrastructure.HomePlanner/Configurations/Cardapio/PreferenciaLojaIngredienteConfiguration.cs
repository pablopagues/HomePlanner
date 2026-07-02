using Domain.HomePlanner.Models.Cardapio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Cardapio;

public class PreferenciaLojaIngredienteConfiguration : IEntityTypeConfiguration<PreferenciaLojaIngrediente>
{
    public void Configure(EntityTypeBuilder<PreferenciaLojaIngrediente> builder)
    {
        builder.ToTable("PreferenciasLojaIngrediente");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.CriadoPor).HasMaxLength(450);
        builder.Property(p => p.ModificadoPor).HasMaxLength(450);

        // Uma preferência por ingrediente dentro do tenant (TenantId primeiro)
        builder.HasIndex(p => new { p.TenantId, p.IngredienteId }).IsUnique();

        // Listas são soft-deletadas; a limpeza das preferências é feita no serviço.
        builder.HasOne(p => p.Lista)
            .WithMany()
            .HasForeignKey(p => p.ListaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
