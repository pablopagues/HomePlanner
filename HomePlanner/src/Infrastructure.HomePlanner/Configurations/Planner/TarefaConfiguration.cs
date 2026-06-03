using Domain.HomePlanner.Models.Planner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.Planner;

public class TarefaConfiguration : IEntityTypeConfiguration<Tarefa>
{
    public void Configure(EntityTypeBuilder<Tarefa> builder)
    {
        builder.ToTable("Tarefas");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Titulo).HasMaxLength(300).IsRequired();
        builder.Property(t => t.Descricao).HasMaxLength(2000);
        builder.Property(t => t.HoraInicio).HasColumnType("time");
        builder.Property(t => t.HoraFim).HasColumnType("time");
        builder.Property(t => t.ResponsavelUsuarioId).HasMaxLength(450);
        builder.Property(t => t.DeletedByUsuarioId).HasMaxLength(450);
        builder.Property(t => t.CriadoPor).HasMaxLength(450);
        builder.Property(t => t.ModificadoPor).HasMaxLength(450);

        // TenantId primeiro no índice composto
        builder.HasIndex(t => new { t.TenantId, t.DataPrevista });
        builder.HasIndex(t => new { t.TenantId, t.Concluida });

        builder.HasOne(t => t.Responsavel)
            .WithMany()
            .HasForeignKey(t => t.ResponsavelUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
