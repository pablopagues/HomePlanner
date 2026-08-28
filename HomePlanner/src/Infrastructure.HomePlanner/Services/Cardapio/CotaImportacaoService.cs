using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Assinatura;
using Application.HomePlanner.Services.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Options;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.HomePlanner.Services.Cardapio;

public class CotaImportacaoService : ICotaImportacaoService
{
    private const int LimiteChamadasIAHora = 40; // teto anti-abuso por tenant/hora

    private readonly HomePlannerDbContext _db;
    private readonly IAssinaturaRepository _assinaturaRepo;
    private readonly TenantContext _tenantContext;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly IMemoryCache _cache;

    public CotaImportacaoService(
        HomePlannerDbContext db,
        IAssinaturaRepository assinaturaRepo,
        TenantContext tenantContext,
        TenantContextAccessor tenantAccessor,
        IMemoryCache cache)
    {
        _db = db;
        _assinaturaRepo = assinaturaRepo;
        _tenantContext = tenantContext;
        _tenantAccessor = tenantAccessor;
        _cache = cache;
    }

    public async Task<ResultadoOperacao> VerificarPodeImportarAsync(CancellationToken ct = default)
    {
        var uso = await ObterUsoAsync(ct);
        if (uso.AtingiuLimite)
            return ResultadoOperacao.Falha(ErrosApp.LimiteImportacoes(uso.Limite));
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> VerificarThrottleIAAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;

        var bucket = DateTime.UtcNow.ToString("yyyyMMddHH");
        var key = $"ia-throttle:{tenantId}:{bucket}";
        var atual = _cache.TryGetValue(key, out int c) ? c : 0;

        if (atual >= LimiteChamadasIAHora)
            return ResultadoOperacao.Falha(ErrosApp.MuitasAnalisesIA);

        _cache.Set(key, atual + 1, TimeSpan.FromHours(1));
        return ResultadoOperacao.Ok();
    }

    public async Task RegistrarImportacaoAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var anoMes = AnoMesAtual();

        var uso = await _db.UsosImportacaoReceita
            .FirstOrDefaultAsync(u => u.AnoMes == anoMes, ct);

        if (uso is null)
        {
            uso = new UsoImportacaoReceita { AnoMes = anoMes, Quantidade = 0 };
            _db.UsosImportacaoReceita.Add(uso);
        }

        uso.Quantidade += 1;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<UsoCotaImportacaoDTO> ObterUsoAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var assinatura = await _assinaturaRepo.ObterMinhaAssinaturaAsync(ct);
        var plano = assinatura?.Plano ?? PlanoAssinatura.Gratis;
        var limite = LimitesPorPlano.ImportacoesReceitaMes(plano);

        var anoMes = AnoMesAtual();
        var uso = await _db.UsosImportacaoReceita
            .FirstOrDefaultAsync(u => u.AnoMes == anoMes, ct);

        return new UsoCotaImportacaoDTO
        {
            Plano  = plano,
            Usado  = uso?.Quantidade ?? 0,
            Limite = limite,
        };
    }

    private static int AnoMesAtual()
    {
        var hoje = DateTime.UtcNow;
        return hoje.Year * 100 + hoje.Month;
    }
}
