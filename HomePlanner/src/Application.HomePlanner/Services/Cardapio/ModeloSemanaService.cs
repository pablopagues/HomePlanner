using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Cardapio;
using Application.HomePlanner.DTOs.Cardapio.ModeloSemana;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.Cardapio;

public class ModeloSemanaService : IModeloSemanaService
{
    private readonly IModeloSemanaRepository _repo;
    private readonly IPlanejamentoSemanalRepository _planejamentoRepo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly ILogger<ModeloSemanaService> _logger;

    public ModeloSemanaService(
        IModeloSemanaRepository repo,
        IPlanejamentoSemanalRepository planejamentoRepo,
        TenantContextAccessor tenantAccessor,
        ILogger<ModeloSemanaService> logger)
    {
        _repo = repo;
        _planejamentoRepo = planejamentoRepo;
        _tenantAccessor = tenantAccessor;
        _logger = logger;
    }

    public async Task<ResultadoListagem<ModeloSemanaListaDTO>> ListarAsync(
        ModeloSemanaFiltroDTO filtro, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        filtro.Pagina = Math.Max(1, filtro.Pagina);
        filtro.TamanhoPagina = Math.Clamp(filtro.TamanhoPagina, 1, 50);

        var itens = await _repo.ListarAsync(filtro, ct);
        var total = await _repo.ContarAsync(filtro, ct);

        return new ResultadoListagem<ModeloSemanaListaDTO>
        {
            Itens = itens, Total = total,
            Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina,
        };
    }

    public async Task<ResultadoOperacao<ModeloSemanaDetalheDTO>> ObterAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var dto = await _repo.ObterDetalheAsync(id, ct);
        return dto is null
            ? ResultadoOperacao<ModeloSemanaDetalheDTO>.Falha("Modelo de semana não encontrado.")
            : ResultadoOperacao<ModeloSemanaDetalheDTO>.Ok(dto);
    }

    public async Task<ResultadoOperacao<int>> SalvarAsync(
        ModeloSemanaPersistenciaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        dto.Nome = dto.Nome?.Trim() ?? string.Empty;
        if (dto.Nome.Length < 2)
            return ResultadoOperacao<int>.Falha("Nome do modelo deve ter pelo menos 2 caracteres.");

        ModeloSemana entidade;
        if (dto.Id == 0)
        {
            entidade = new ModeloSemana();
            await _repo.AdicionarAsync(entidade, ct);
        }
        else
        {
            entidade = await _repo.ObterEntidadeComRefeicoesAsync(dto.Id, ct)
                ?? throw new InvalidOperationException($"Modelo {dto.Id} não encontrado para edição.");
        }

        entidade.Nome     = dto.Nome;
        entidade.Descricao = dto.Descricao?.Trim();

        await _repo.SalvarAsync(ct);
        return ResultadoOperacao<int>.Ok(entidade.Id);
    }

    public async Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var entidade = await _repo.ObterEntidadeComRefeicoesAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha("Modelo de semana não encontrado.");

        entidade.IsDeleted = true;
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao<CardapioSemanaDTO>> AplicarModeloAsync(
        int modeloId, DateOnly dataInicio, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        if (dataInicio.DayOfWeek != DayOfWeek.Monday)
            return ResultadoOperacao<CardapioSemanaDTO>.Falha("A data de início deve ser uma segunda-feira.");

        var modelo = await _repo.ObterEntidadeComRefeicoesAsync(modeloId, ct);
        if (modelo is null)
            return ResultadoOperacao<CardapioSemanaDTO>.Falha("Modelo não encontrado.");

        var planejamento = await _planejamentoRepo.ObterPorDataInicioAsync(dataInicio, ct);
        if (planejamento is null)
        {
            planejamento = new PlanejamentoSemanal
            {
                DataInicio = dataInicio,
                ModeloSemanaOrigemId = modeloId,
            };
            await _planejamentoRepo.AdicionarAsync(planejamento, ct);
            await _planejamentoRepo.SalvarAsync(ct);
        }

        foreach (var rm in modelo.RefeicoesModelo.Where(r => !r.IsDeleted))
        {
            var existente = await _planejamentoRepo.ObterRefeicaoDiaAsync(
                planejamento.Id, rm.DiaSemana, rm.TipoRefeicao, ct);

            if (existente is null)
            {
                existente = new RefeicaoDia
                {
                    PlanejamentoSemanalId = planejamento.Id,
                    DiaSemana             = rm.DiaSemana,
                    TipoRefeicao          = rm.TipoRefeicao,
                };
                await _planejamentoRepo.AdicionarRefeicaoDiaAsync(existente, ct);
            }

            existente.ReceitaId        = rm.ReceitaId;
            existente.PorcoesDesejadas = rm.PorcoesDesejadas;
            existente.Observacao       = rm.Observacao;
        }

        await _planejamentoRepo.SalvarAsync(ct);

        var dto = await _planejamentoRepo.ObterCardapioSemanaAsync(dataInicio, ct);
        return ResultadoOperacao<CardapioSemanaDTO>.Ok(dto!);
    }

    public async Task<ResultadoOperacao<int>> SalvarPlanejamentoComoModeloAsync(
        DateOnly dataInicio, string nomeModelo, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        nomeModelo = nomeModelo?.Trim() ?? string.Empty;
        if (nomeModelo.Length < 2)
            return ResultadoOperacao<int>.Falha("Nome do modelo deve ter pelo menos 2 caracteres.");

        var cardapio = await _planejamentoRepo.ObterCardapioSemanaAsync(dataInicio, ct);
        if (cardapio is null)
            return ResultadoOperacao<int>.Falha("Cardápio da semana não encontrado.");

        var modelo = new ModeloSemana { Nome = nomeModelo };
        foreach (var rf in cardapio.Refeicoes)
        {
            modelo.RefeicoesModelo.Add(new RefeicaoModelo
            {
                DiaSemana        = rf.DiaSemana,
                TipoRefeicao     = rf.TipoRefeicao,
                ReceitaId        = rf.ReceitaId,
                PorcoesDesejadas = rf.PorcoesDesejadas,
                Observacao       = rf.Observacao,
            });
        }

        await _repo.AdicionarAsync(modelo, ct);
        await _repo.SalvarAsync(ct);
        _logger.LogInformation("Modelo '{Nome}' criado a partir do cardápio {Data}.", nomeModelo, dataInicio);
        return ResultadoOperacao<int>.Ok(modelo.Id);
    }
}
