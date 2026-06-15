using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Onboarding;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Configuracao;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HomePlanner.Services.Configuracao;

public class ConfiguracaoFamiliaService : IConfiguracaoFamiliaService
{
    private readonly HomePlannerDbContext _db;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<ConfiguracaoFamiliaService> _logger;

    public ConfiguracaoFamiliaService(
        HomePlannerDbContext db,
        TenantContextAccessor tenantAccessor,
        TenantContext tenantContext,
        ILogger<ConfiguracaoFamiliaService> logger)
    {
        _db = db;
        _tenantAccessor = tenantAccessor;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<ConfiguracaoFamiliaDTO> ObterAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;

        var config = await _db.ConfiguracoesFamilia
            .FirstOrDefaultAsync(c => c.TenantId == tenantId, ct);

        if (config is null)
            return new ConfiguracaoFamiliaDTO();

        return new ConfiguracaoFamiliaDTO
        {
            TamanhoFamiliaPadrao         = config.TamanhoFamiliaPadrao,
            FusoHorario                  = config.FusoHorario,
            MinutosAntecedenciaLembrete  = config.MinutosAntecedenciaLembrete,
            TiposRefeicaoAtivos          = config.TiposRefeicaoAtivos.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        };
    }

    public async Task<ResultadoOperacao> SalvarAsync(ConfiguracaoFamiliaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId;
        if (tenantId is null)
            return ResultadoOperacao.Falha("Sessão inválida. Faça login novamente.");

        if (dto.TamanhoFamiliaPadrao < 1)
            return ResultadoOperacao.Falha("O tamanho da família deve ser ao menos 1.");
        if (dto.TiposRefeicaoAtivos.Count == 0)
            return ResultadoOperacao.Falha("Selecione ao menos um tipo de refeição.");
        if (dto.MinutosAntecedenciaLembrete is < 0 or > 1440)
            return ResultadoOperacao.Falha("A antecedência do lembrete deve estar entre 0 e 1440 minutos.");

        var config = await _db.ConfiguracoesFamilia
            .FirstOrDefaultAsync(c => c.TenantId == tenantId.Value, ct);

        if (config is null)
        {
            config = new ConfiguracaoFamilia { TenantId = tenantId.Value };
            _db.ConfiguracoesFamilia.Add(config);
        }
        config.TamanhoFamiliaPadrao        = dto.TamanhoFamiliaPadrao;
        config.FusoHorario                 = dto.FusoHorario;
        config.MinutosAntecedenciaLembrete = dto.MinutosAntecedenciaLembrete;
        config.TiposRefeicaoAtivos         = string.Join(',', dto.TiposRefeicaoAtivos);

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Configuração da família atualizada para tenant {TenantId}.", tenantId.Value);
        return ResultadoOperacao.Ok();
    }
}
