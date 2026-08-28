using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Onboarding;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Onboarding;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HomePlanner.Services.Onboarding;

public class OnboardingService : IOnboardingService
{
    private readonly HomePlannerDbContext _db;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(
        HomePlannerDbContext db,
        TenantContextAccessor tenantAccessor,
        TenantContext tenantContext,
        ILogger<OnboardingService> logger)
    {
        _db = db;
        _tenantAccessor = tenantAccessor;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<ConfiguracaoFamiliaDTO> ObterConfiguracaoAtualAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;

        var config = await _db.ConfiguracoesFamilia
            .FirstOrDefaultAsync(c => c.TenantId == tenantId, ct);

        if (config is null)
            return new ConfiguracaoFamiliaDTO();

        return new ConfiguracaoFamiliaDTO
        {
            TamanhoFamiliaPadrao = config.TamanhoFamiliaPadrao,
            FusoHorario          = config.FusoHorario,
            TiposRefeicaoAtivos  = config.TiposRefeicaoAtivos.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        };
    }

    public async Task<ResultadoOperacao> FinalizarAsync(ConfiguracaoFamiliaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId;
        if (tenantId is null)
            return ResultadoOperacao.Falha(ErrosApp.SessaoInvalida);

        if (dto.TamanhoFamiliaPadrao < 1)
            return ResultadoOperacao.Falha(ErrosApp.TamanhoFamiliaMinimo);
        if (dto.TiposRefeicaoAtivos.Count == 0)
            return ResultadoOperacao.Falha(ErrosApp.SelecioneTipoRefeicao);

        // 1. Cria/atualiza ConfiguracaoFamilia
        var config = await _db.ConfiguracoesFamilia
            .FirstOrDefaultAsync(c => c.TenantId == tenantId.Value, ct);

        if (config is null)
        {
            config = new ConfiguracaoFamilia { TenantId = tenantId.Value };
            _db.ConfiguracoesFamilia.Add(config);
        }
        config.TamanhoFamiliaPadrao = dto.TamanhoFamiliaPadrao;
        config.FusoHorario          = dto.FusoHorario;
        config.TiposRefeicaoAtivos  = string.Join(',', dto.TiposRefeicaoAtivos);

        // 2. Marca onboarding como completo
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId.Value, ct);
        if (tenant is null)
            return ResultadoOperacao.Falha("Tenant não encontrado.");

        tenant.OnboardingCompleto = true;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Onboarding concluído para tenant {TenantId}.", tenantId.Value);
        return ResultadoOperacao.Ok();
    }
}
