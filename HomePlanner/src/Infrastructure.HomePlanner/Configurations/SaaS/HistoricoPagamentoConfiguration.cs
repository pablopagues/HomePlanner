using Domain.HomePlanner.Models.SaaS.Assinatura;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.SaaS;

public class HistoricoPagamentoConfiguration : IEntityTypeConfiguration<HistoricoPagamento>
{
    public void Configure(EntityTypeBuilder<HistoricoPagamento> builder)
    {
        builder.ToTable("HistoricosPagamento");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Valor).HasPrecision(10, 2);
        builder.Property(h => h.Moeda).HasMaxLength(3).IsRequired();
        builder.Property(h => h.StripeInvoiceId).HasMaxLength(100);
        builder.Property(h => h.StripePaymentIntentId).HasMaxLength(100);
        builder.Property(h => h.MotivoFalha).HasMaxLength(500);
        builder.HasIndex(h => new { h.TenantId, h.DataPagamento });
        builder.HasOne(h => h.Tenant).WithMany()
            .HasForeignKey(h => h.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
