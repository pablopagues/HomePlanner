using Domain.HomePlanner.Models.SaaS.Assinatura;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.HomePlanner.Configurations.SaaS;

public class ConfiguracaoAssinaturaConfiguration : IEntityTypeConfiguration<ConfiguracaoAssinatura>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoAssinatura> builder)
    {
        builder.ToTable("ConfiguracoesAssinatura");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.StripeCustomerId).HasMaxLength(100);
        builder.Property(c => c.StripeSubscriptionId).HasMaxLength(100);
        builder.Property(c => c.StripePriceId).HasMaxLength(100);
        builder.Property(c => c.UltimoCartaoFinal).HasMaxLength(4);
        builder.HasIndex(c => c.TenantId).IsUnique();
        builder.HasIndex(c => c.StripeCustomerId);
        builder.HasIndex(c => c.StripeSubscriptionId);
        builder.HasOne(c => c.Tenant).WithOne()
            .HasForeignKey<ConfiguracaoAssinatura>(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
