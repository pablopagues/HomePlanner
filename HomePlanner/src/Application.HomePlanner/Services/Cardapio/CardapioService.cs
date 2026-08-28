using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Cardapio;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.Cardapio;

public class CardapioService : ICardapioService
{
    private readonly IPlanejamentoSemanalRepository _repo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly ILogger<CardapioService> _logger;

    public CardapioService(
        IPlanejamentoSemanalRepository repo,
        TenantContextAccessor tenantAccessor,
        ILogger<CardapioService> logger)
    {
        _repo = repo;
        _tenantAccessor = tenantAccessor;
        _logger = logger;
    }

    public async Task<ResultadoOperacao<CardapioSemanaDTO>> ObterSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var dto = await _repo.ObterCardapioSemanaAsync(dataInicio, ct);
        return dto is null
            ? ResultadoOperacao<CardapioSemanaDTO>.Falha(ErrosApp.SemanaNaoEncontrada)
            : ResultadoOperacao<CardapioSemanaDTO>.Ok(dto);
    }

    public async Task<ResultadoOperacao<CardapioSemanaDTO>> ObterOuCriarSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        // DataInicio DEVE ser segunda-feira (regra de negócio crítica)
        if (dataInicio.DayOfWeek != DayOfWeek.Monday)
            return ResultadoOperacao<CardapioSemanaDTO>.Falha(ErrosApp.DataDeveSerSegunda);

        var planejamento = await _repo.ObterPorDataInicioAsync(dataInicio, ct);
        if (planejamento is null)
        {
            planejamento = new PlanejamentoSemanal { DataInicio = dataInicio };
            await _repo.AdicionarAsync(planejamento, ct);
            await _repo.SalvarAsync(ct);
            _logger.LogInformation("Novo cardápio criado para semana {Data}.", dataInicio);
        }

        var dto = await _repo.ObterCardapioSemanaAsync(dataInicio, ct);
        return ResultadoOperacao<CardapioSemanaDTO>.Ok(dto!);
    }

    public async Task<ResultadoOperacao> DefinirRefeicaoAsync(
        DefinirRefeicaoCommand cmd, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var planejamento = await _repo.ObterPorDataInicioAsync(cmd.DataInicio, ct);
        if (planejamento is null)
            return ResultadoOperacao.Falha(ErrosApp.SemanaSemCardapio);

        // Upsert idempotente do slot
        var refeicao = await _repo.ObterRefeicaoDiaAsync(
            planejamento.Id, cmd.DiaSemana, cmd.TipoRefeicao, ct);

        if (refeicao is null)
        {
            refeicao = new RefeicaoDia
            {
                PlanejamentoSemanalId = planejamento.Id,
                DiaSemana             = cmd.DiaSemana,
                TipoRefeicao          = cmd.TipoRefeicao,
            };
            await _repo.AdicionarRefeicaoDiaAsync(refeicao, ct);
        }

        refeicao.ReceitaId        = cmd.ReceitaId;
        refeicao.PorcoesDesejadas = cmd.PorcoesDesejadas;
        refeicao.Observacao       = cmd.Observacao;

        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> LimparSlotAsync(
        int planejamentoId, int refeicaoDiaId, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        // Verifica que o planejamento pertence ao tenant atual (filtro global garante)
        var planejamento = await _repo.ObterEntidadeAsync(planejamentoId, ct);
        if (planejamento is null)
            return ResultadoOperacao.Falha(ErrosApp.CardapioNaoEncontrado);

        var refeicao = await _repo.ObterRefeicaoDiaPorIdAsync(refeicaoDiaId, ct);
        if (refeicao is null || refeicao.PlanejamentoSemanalId != planejamentoId)
            return ResultadoOperacao.Falha("Slot não encontrado.");

        refeicao.ReceitaId        = null;
        refeicao.PorcoesDesejadas = null;
        refeicao.Observacao       = null;

        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao<CardapioSemanaDTO>> CopiarSemanaAsync(
        DateOnly dataInicioOrigem, DateOnly dataInicioDestino, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        if (dataInicioDestino.DayOfWeek != DayOfWeek.Monday)
            return ResultadoOperacao<CardapioSemanaDTO>.Falha(ErrosApp.DestinoDeveSerSegunda);

        var origem = await _repo.ObterCardapioSemanaAsync(dataInicioOrigem, ct);
        if (origem is null)
            return ResultadoOperacao<CardapioSemanaDTO>.Falha(ErrosApp.SemanaOrigemNaoEncontrada);

        // Cria ou obtém a semana de destino
        var planejamentoDestino = await _repo.ObterPorDataInicioAsync(dataInicioDestino, ct);
        if (planejamentoDestino is null)
        {
            planejamentoDestino = new PlanejamentoSemanal { DataInicio = dataInicioDestino };
            await _repo.AdicionarAsync(planejamentoDestino, ct);
            await _repo.SalvarAsync(ct);
        }

        // Copia cada refeição como upsert
        foreach (var rf in origem.Refeicoes)
        {
            var existente = await _repo.ObterRefeicaoDiaAsync(
                planejamentoDestino.Id, rf.DiaSemana, rf.TipoRefeicao, ct);

            if (existente is null)
            {
                existente = new RefeicaoDia
                {
                    PlanejamentoSemanalId = planejamentoDestino.Id,
                    DiaSemana             = rf.DiaSemana,
                    TipoRefeicao          = rf.TipoRefeicao,
                };
                await _repo.AdicionarRefeicaoDiaAsync(existente, ct);
            }

            existente.ReceitaId        = rf.ReceitaId;
            existente.PorcoesDesejadas = rf.PorcoesDesejadas;
            existente.Observacao       = rf.Observacao;
        }

        await _repo.SalvarAsync(ct);

        var dto = await _repo.ObterCardapioSemanaAsync(dataInicioDestino, ct);
        return ResultadoOperacao<CardapioSemanaDTO>.Ok(dto!);
    }
}
