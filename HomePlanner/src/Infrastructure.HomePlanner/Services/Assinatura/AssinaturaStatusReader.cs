using Application.HomePlanner.Middleware;
using Domain.HomePlanner.Models.Enums;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.HomePlanner.Services.Assinatura;

/// <summary>
/// Lê a linha de ConfiguracoesAssinatura e aplica <see cref="EstadoAssinatura.Avaliar"/>.
/// O status gravado sozinho não basta: o trial vence pela data, sem ninguém marcar nada,
/// e o webhook do Stripe pode atrasar ou se perder.
/// </summary>
public class AssinaturaStatusReader : IAssinaturaStatusReader
{
    private static readonly TimeSpan _ttlCache = TimeSpan.FromSeconds(60);

    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;
    private readonly IMemoryCache _cache;

    public AssinaturaStatusReader(
        IDbContextFactory<HomePlannerDbContext> dbFactory,
        IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache = cache;
    }

    public async Task<EstadoAssinatura> ObterAsync(Guid tenantId, CancellationToken ct = default)
    {
        var chave = Chave(tenantId);
        if (_cache.TryGetValue<EstadoAssinatura>(chave, out var emCache) && emCache is not null)
            return emCache;

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var dados = await db.ConfiguracoesAssinatura
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == tenantId)
            .Select(a => new { a.Status, a.DataFimTrial, a.DataExpiracao })
            .FirstOrDefaultAsync(ct);

        // Tenant sem linha de assinatura é estado anômalo (bug de dados, não inadimplência):
        // não trancamos o usuário para fora por causa disso.
        var estado = dados is null
            ? EstadoAssinatura.Liberada
            : EstadoAssinatura.Avaliar(dados.Status, dados.DataFimTrial, dados.DataExpiracao, DateTime.UtcNow);

        _cache.Set(chave, estado, _ttlCache);
        return estado;
    }

    public void Invalidar(Guid tenantId) => _cache.Remove(Chave(tenantId));

    private static string Chave(Guid tenantId) => $"assinatura_estado_{tenantId}";
}
