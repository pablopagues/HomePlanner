using Application.HomePlanner.Middleware;
using Domain.HomePlanner.Models.Cardapio;
using Domain.HomePlanner.Models.Planner;
using Domain.HomePlanner.Models.SaaS.Assinatura;
using Domain.HomePlanner.Models.SaaS.Auditoria;
using Domain.HomePlanner.Models.SaaS.Configuracao;
using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Interfaces;
using Domain.HomePlanner.Models.SaaS.Tenancy;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Infrastructure.HomePlanner.Contexts;

public class HomePlannerDbContext : IdentityDbContext<Usuario, Papel, string>
{
    private readonly IServiceProvider _serviceProvider;

    public HomePlannerDbContext(
        DbContextOptions<HomePlannerDbContext> options,
        IServiceProvider serviceProvider)
        : base(options)
    {
        _serviceProvider = serviceProvider;
    }

    // SaaS
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantDadosBrasil> TenantDadosBrasil => Set<TenantDadosBrasil>();
    public DbSet<TenantDadosCanada> TenantDadosCanada => Set<TenantDadosCanada>();
    public DbSet<TenantDelecaoSolicitada> TenantDelecoesSolicitadas => Set<TenantDelecaoSolicitada>();
    public DbSet<ConfiguracaoAssinatura> ConfiguracoesAssinatura => Set<ConfiguracaoAssinatura>();
    public DbSet<HistoricoPagamento> HistoricosPagamento => Set<HistoricoPagamento>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Parametro> Parametros => Set<Parametro>();

    // Cardápio
    public DbSet<UnidadeMedida> UnidadesMedida => Set<UnidadeMedida>();
    public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();
    public DbSet<Receita> Receitas => Set<Receita>();
    public DbSet<ReceitaIngrediente> ReceitasIngredientes => Set<ReceitaIngrediente>();
    public DbSet<PlanejamentoSemanal> PlanejamentosSemanais => Set<PlanejamentoSemanal>();
    public DbSet<RefeicaoDia> RefeicoesDia => Set<RefeicaoDia>();
    public DbSet<ModeloSemana> ModelosSemana => Set<ModeloSemana>();
    public DbSet<RefeicaoModelo> RefeicoesModelo => Set<RefeicaoModelo>();
    public DbSet<ConfiguracaoFamilia> ConfiguracoesFamilia => Set<ConfiguracaoFamilia>();
    public DbSet<PedidoCompra> PedidosCompra => Set<PedidoCompra>();

    // Planner
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        AplicarFiltrosGlobais(modelBuilder);
    }

    private void AplicarFiltrosGlobais(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var isTenant = typeof(ITenantEntity).IsAssignableFrom(clrType);
            var isDeletable = typeof(IDeletableEntity).IsAssignableFrom(clrType);

            if (!isTenant && !isDeletable) continue;

            var parameter = Expression.Parameter(clrType, "e");
            Expression? combined = null;

            if (isTenant)
            {
                var tenantProp = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                var getMethod = typeof(HomePlannerDbContext)
                    .GetMethod(nameof(ObterTenantIdAtual), BindingFlags.NonPublic | BindingFlags.Instance)!;
                var call = Expression.Call(Expression.Constant(this), getMethod);
                combined = Expression.Equal(tenantProp, call);
            }

            if (isDeletable)
            {
                var isDeletedProp = Expression.Property(parameter, nameof(IDeletableEntity.IsDeleted));
                var notDeleted = Expression.Equal(isDeletedProp, Expression.Constant(false));
                combined = combined == null ? notDeleted : Expression.AndAlso(combined, notDeleted);
            }

            modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(combined!, parameter));
        }
    }

    private Guid ObterTenantIdAtual()
    {
        var tenantCtx = _serviceProvider.GetService(typeof(TenantContext)) as TenantContext;
        return tenantCtx?.TenantId ?? Guid.Empty;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ProcessarSoftDelete();
        ProcessarAuditoria();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ProcessarSoftDelete()
    {
        var tenantCtx = _serviceProvider.GetService(typeof(TenantContext)) as TenantContext;
        var agora = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IDeletableEntity>()
            .Where(e => e.State == EntityState.Deleted).ToList())
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = agora;
            entry.Entity.DeletedByUsuarioId = tenantCtx?.UsuarioId;
        }
    }

    private void ProcessarAuditoria()
    {
        var tenantCtx = _serviceProvider.GetService(typeof(TenantContext)) as TenantContext;
        var agora = DateTime.UtcNow;
        var usuarioId = tenantCtx?.UsuarioId ?? "system";

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.DataCriacao = agora;
                    entry.Entity.CriadoPor = usuarioId;
                    break;
                case EntityState.Modified:
                    entry.Entity.DataModificacao = agora;
                    entry.Entity.ModificadoPor = usuarioId;
                    break;
            }
        }

        // Preenche TenantId automaticamente em inserções
        if (tenantCtx?.TenantId != null)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>()
                .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty))
            {
                entry.Entity.TenantId = tenantCtx.TenantId.Value;
            }
        }
    }
}
