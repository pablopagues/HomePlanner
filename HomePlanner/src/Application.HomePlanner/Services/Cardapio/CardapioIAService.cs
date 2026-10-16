using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Cardapio;
using Application.HomePlanner.DTOs.Cardapio.CardapioIA;
using Application.HomePlanner.Repositories.Cardapio;
using Application.HomePlanner.Services.Configuracao;
using Domain.HomePlanner.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.Cardapio;

public class CardapioIAService : ICardapioIAService
{
    // Teto de receitas enviadas à IA (custo do prompt). As mais usadas pela família vêm primeiro.
    private const int MaxCandidatas = 150;

    // Janela do histórico usada para "gosto da casa" (uso por tipo de refeição).
    private const int DiasHistoricoUso = 182;

    // Semanas anteriores mostradas à IA para variar em relação ao que foi comido.
    private const int DiasRecentes = 28;

    private const int MaxIngredientesPorReceita = 6;

    private readonly IGeradorCardapioIA _ia;
    private readonly ICotaImportacaoService _cota;
    private readonly IReceitaService _receitas;
    private readonly IReceitaRepository _receitaRepo;
    private readonly IPlanejamentoSemanalRepository _planejamentoRepo;
    private readonly ICardapioService _cardapio;
    private readonly IConfiguracaoFamiliaService _config;
    private readonly ILogger<CardapioIAService> _logger;

    public CardapioIAService(
        IGeradorCardapioIA ia,
        ICotaImportacaoService cota,
        IReceitaService receitas,
        IReceitaRepository receitaRepo,
        IPlanejamentoSemanalRepository planejamentoRepo,
        ICardapioService cardapio,
        IConfiguracaoFamiliaService config,
        ILogger<CardapioIAService> logger)
    {
        _ia = ia;
        _cota = cota;
        _receitas = receitas;
        _receitaRepo = receitaRepo;
        _planejamentoRepo = planejamentoRepo;
        _cardapio = cardapio;
        _config = config;
        _logger = logger;
    }

    // Adulto = quem pode gerenciar receitas (Owner e Pai/Mãe; Filho não).
    public async Task<bool> PodeGerarAsync()
        => _ia.Habilitado && await _receitas.PodeGerenciarAsync();

    public async Task<ResultadoOperacao<PrevisaoCardapioIADTO>> PreverAsync(
        PedidoCardapioIADTO pedido, CancellationToken ct = default)
    {
        var plano = await MontarPlanoAsync(pedido, ct);
        if (!plano.Sucesso)
            return ResultadoOperacao<PrevisaoCardapioIADTO>.Falha(plano.Erros.ToArray());

        var p = plano.Valor!;
        return ResultadoOperacao<PrevisaoCardapioIADTO>.Ok(new PrevisaoCardapioIADTO
        {
            Horarios              = p.Contexto.Horarios.Count,
            ReceitasDisponiveis   = p.Contexto.Candidatas.Count,
            ReceitasInsuficientes = !pedido.PermitirRepetirNaSemana
                                    && p.Contexto.Candidatas.Count < p.Contexto.Horarios.Count,
        });
    }

    public async Task<ResultadoOperacao<CardapioSugeridoDTO>> GerarAsync(
        PedidoCardapioIADTO pedido, string idioma, CancellationToken ct = default)
    {
        if (!await _receitas.PodeGerenciarAsync())
            return ResultadoOperacao<CardapioSugeridoDTO>.Falha(ErrosApp.SemPermissaoCardapioIA);

        if (!_ia.Habilitado)
            return ResultadoOperacao<CardapioSugeridoDTO>.Falha(ErrosApp.IaIndisponivel);

        var montagem = await MontarPlanoAsync(pedido, ct);
        if (!montagem.Sucesso)
            return ResultadoOperacao<CardapioSugeridoDTO>.Falha(montagem.Erros.ToArray());
        var plano = montagem.Valor!;

        if (plano.Contexto.Horarios.Count == 0)
            return ResultadoOperacao<CardapioSugeridoDTO>.Falha(ErrosApp.NenhumHorarioParaPreencher);
        if (plano.Contexto.Candidatas.Count == 0)
            return ResultadoOperacao<CardapioSugeridoDTO>.Falha(ErrosApp.SemReceitasParaCardapio);

        var cota = await _cota.VerificarPodeGerarCardapioAsync(ct);
        if (!cota.Sucesso)
            return ResultadoOperacao<CardapioSugeridoDTO>.Falha(cota.Erros.ToArray());

        // Até 2 tentativas: a segunda só quando a primeira veio com menos da metade
        // dos horários válidos (a IA errou ids/horários). Fica a melhor das duas.
        Dictionary<(DiaSemana, TipoRefeicao), EscolhaCardapioIA>? melhor = null;
        string? observacoes = null;
        for (var tentativa = 1; tentativa <= 2; tentativa++)
        {
            var throttle = await _cota.VerificarThrottleIAAsync(ct);
            if (!throttle.Sucesso)
            {
                if (melhor is not null) break;
                return ResultadoOperacao<CardapioSugeridoDTO>.Falha(throttle.Erros.ToArray());
            }

            var resposta = await _ia.GerarAsync(plano.Contexto, idioma, ct);
            if (resposta is null) continue;

            var validas = Validar(resposta, plano, pedido.PermitirRepetirNaSemana);
            _logger.LogInformation(
                "Cardápio IA (tentativa {Tentativa}): {Validas}/{Horarios} horários válidos de {Escolhas} escolhas.",
                tentativa, validas.Count, plano.Contexto.Horarios.Count, resposta.Escolhas.Count);

            if (melhor is null || validas.Count > melhor.Count)
            {
                melhor = validas;
                observacoes = resposta.Observacoes;
            }
            if (melhor.Count * 2 >= plano.Contexto.Horarios.Count) break;
        }

        if (melhor is null || melhor.Count == 0)
            return ResultadoOperacao<CardapioSugeridoDTO>.Falha(ErrosApp.CardapioIANaoGerado);

        // O custo está na geração: conta 1 na cota mensal só quando deu certo.
        await _cota.RegistrarCardapioIAAsync(ct);

        var candidatas = plano.Contexto.Candidatas.ToDictionary(c => c.Id);
        var sugestao = new CardapioSugeridoDTO
        {
            DataInicio  = pedido.DataInicio,
            Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim(),
            Refeicoes   = plano.Contexto.Horarios.Select(h =>
            {
                melhor.TryGetValue((h.Dia, h.Tipo), out var e);
                var c = e is null ? null : candidatas[e.ReceitaId];
                return new RefeicaoSugeridaDTO
                {
                    DiaSemana           = h.Dia,
                    TipoRefeicao        = h.Tipo,
                    ReceitaId           = c?.Id,
                    ReceitaNome         = c?.Nome,
                    TempoPreparoMinutos = c?.TempoPreparoMinutos,
                    PorcoesDesejadas    = plano.Contexto.Porcoes,
                    Motivo              = e?.Motivo,
                };
            }).ToList(),
        };
        return ResultadoOperacao<CardapioSugeridoDTO>.Ok(sugestao);
    }

    public async Task<ResultadoOperacao> AplicarAsync(CardapioSugeridoDTO sugestao, CancellationToken ct = default)
    {
        if (!await _receitas.PodeGerenciarAsync())
            return ResultadoOperacao.Falha(ErrosApp.SemPermissaoCardapioIA);

        var comandos = sugestao.Refeicoes
            .Where(r => r.ReceitaId.HasValue)
            .Select(r => new DefinirRefeicaoCommand
            {
                DataInicio       = sugestao.DataInicio,
                DiaSemana        = r.DiaSemana,
                TipoRefeicao     = r.TipoRefeicao,
                ReceitaId        = r.ReceitaId,
                PorcoesDesejadas = r.PorcoesDesejadas,
            })
            .ToList();

        if (comandos.Count == 0) return ResultadoOperacao.Ok();
        return await _cardapio.DefinirRefeicoesEmLoteAsync(sugestao.DataInicio, comandos, ct);
    }

    // ─── Montagem do contexto (regras rígidas) ────────────────────────────────

    private sealed record PlanoGeracao(
        ContextoCardapioIA Contexto,
        IReadOnlySet<int> UsadasNaSemana);

    private async Task<ResultadoOperacao<PlanoGeracao>> MontarPlanoAsync(PedidoCardapioIADTO pedido, CancellationToken ct)
    {
        if (pedido.Tipos.Count == 0)
            return ResultadoOperacao<PlanoGeracao>.Falha(ErrosApp.SelecioneTipoRefeicao);

        // Garante a semana (e valida que começa numa segunda).
        var semanaR = await _cardapio.ObterOuCriarSemanaAsync(pedido.DataInicio, ct);
        if (!semanaR.Sucesso)
            return ResultadoOperacao<PlanoGeracao>.Falha(semanaR.Erros.ToArray());
        var semana = semanaR.Valor!;

        var dataInicio = pedido.DataInicio;
        var dataFim = dataInicio.AddDays(6);
        DateOnly DataDe(DiaSemana d) => dataInicio.AddDays((int)d - 1);

        // Horários a preencher: dias × tipos pedidos, pulando os que já têm prato
        // (a não ser que a família peça para substituir).
        var horarios = new List<HorarioCardapioIA>();
        foreach (var dia in pedido.Dias.Distinct().Order())
        foreach (var tipo in pedido.Tipos.Distinct().Order())
        {
            var atual = semana.Refeicoes.FirstOrDefault(r => r.DiaSemana == dia && r.TipoRefeicao == tipo);
            if (atual?.ReceitaId is not null && !pedido.SubstituirExistentes) continue;

            var fimDeSemana = dia is DiaSemana.Sabado or DiaSemana.Domingo;
            horarios.Add(new HorarioCardapioIA
            {
                Dia             = dia,
                Data            = DataDe(dia),
                Tipo            = tipo,
                TempoMaxMinutos = PositivoOuNulo(fimDeSemana ? pedido.TempoMaxFimDeSemana : pedido.TempoMaxDiaUtil),
            });
        }
        var slotsAPreencher = horarios.Select(h => (h.Dia, h.Tipo)).ToHashSet();

        // Pratos que ficam na semana (qualquer tipo de refeição, não só os pedidos).
        var jaNaSemana = semana.Refeicoes
            .Where(r => r.ReceitaId.HasValue && !slotsAPreencher.Contains((r.DiaSemana, r.TipoRefeicao)))
            .Select(r => new RefeicaoHistoricoDTO(DataDe(r.DiaSemana), r.TipoRefeicao, r.ReceitaId!.Value, r.ReceitaNome))
            .ToList();

        // Janela de não repetição.
        (DateOnly De, DateOnly Ate)? janela = pedido.NaoRepetir switch
        {
            PeriodoSemRepeticao.Nenhum      => null,
            PeriodoSemRepeticao.MesCorrente => (new DateOnly(dataInicio.Year, dataInicio.Month, 1),
                                                new DateOnly(dataFim.Year, dataFim.Month, 1).AddMonths(1).AddDays(-1)),
            _                               => (dataInicio.AddDays(-7 * (int)pedido.NaoRepetir), dataFim),
        };

        var inicioHist = dataInicio.AddDays(-DiasHistoricoUso);
        if (janela is { } j1 && j1.De < inicioHist) inicioHist = j1.De;
        var fimHist = janela is { } j2 && j2.Ate > dataFim ? j2.Ate : dataFim;

        // Histórico sem os horários que vão ser trocados agora (o prato antigo deles sai).
        var historico = (await _planejamentoRepo.ListarRefeicoesPeriodoAsync(inicioHist, fimHist, ct))
            .Where(r => !(r.Data >= dataInicio && r.Data <= dataFim
                          && slotsAPreencher.Contains(((DiaSemana)((r.Data.DayNumber - dataInicio.DayNumber) + 1), r.Tipo))))
            .ToList();

        var bloqueadas = new HashSet<int>();
        if (janela is { } j)
            bloqueadas.UnionWith(historico.Where(r => r.Data >= j.De && r.Data <= j.Ate).Select(r => r.ReceitaId));

        var usadasNaSemana = jaNaSemana.Select(r => r.ReceitaId).ToHashSet();
        if (!pedido.PermitirRepetirNaSemana)
            bloqueadas.UnionWith(usadasNaSemana);

        // Gosto da casa: uso por tipo de refeição no passado (só datas anteriores à semana).
        var passado = historico.Where(r => r.Data < dataInicio && r.Data >= dataInicio.AddDays(-DiasHistoricoUso)).ToList();
        var usosPorReceita = passado
            .GroupBy(r => r.ReceitaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var todas = await _receitaRepo.ListarCandidatasCardapioAsync(ct);
        var candidatas = todas
            .Where(r => !bloqueadas.Contains(r.Id))
            // Componente (ex.: "Arroz branco") que nunca foi servido sozinho não é prato principal.
            .Where(r => !r.EhComponente || usosPorReceita.ContainsKey(r.Id))
            .OrderByDescending(r => usosPorReceita.TryGetValue(r.Id, out var u) ? u.Count : 0)
            .ThenBy(r => r.Nome)
            .Take(MaxCandidatas)
            .Select(r =>
            {
                usosPorReceita.TryGetValue(r.Id, out var usos);
                return new CandidataCardapioIA
                {
                    Id                  = r.Id,
                    Nome                = r.Nome,
                    TempoPreparoMinutos = r.TempoPreparoMinutos,
                    Ingredientes        = r.Ingredientes.Count > 0 ? r.Ingredientes : LinhasIngredientes(r.IngredientesTexto),
                    UsosPorTipo         = (usos ?? []).GroupBy(u => u.Tipo).ToDictionary(g => g.Key, g => g.Count()),
                    UltimoUso           = usos?.Max(u => u.Data),
                };
            })
            .ToList();

        var porcoes = pedido.Porcoes is > 0
            ? pedido.Porcoes.Value
            : (await _config.ObterAsync(ct)).TamanhoFamiliaPadrao;

        var contexto = new ContextoCardapioIA
        {
            Candidatas              = candidatas,
            Horarios                = horarios,
            JaNaSemana              = jaNaSemana,
            Recentes                = passado.Where(r => r.Data >= dataInicio.AddDays(-DiasRecentes)).ToList(),
            Pedido                  = string.IsNullOrWhiteSpace(pedido.Texto) ? null : pedido.Texto.Trim(),
            Porcoes                 = porcoes,
            PermitirRepetirNaSemana = pedido.PermitirRepetirNaSemana,
        };
        return ResultadoOperacao<PlanoGeracao>.Ok(new PlanoGeracao(contexto, usadasNaSemana));
    }

    // ─── Validação da resposta (regras rígidas de novo) ───────────────────────

    private static Dictionary<(DiaSemana, TipoRefeicao), EscolhaCardapioIA> Validar(
        RespostaCardapioIA resposta, PlanoGeracao plano, bool permitirRepetir)
    {
        var horarios = plano.Contexto.Horarios.ToDictionary(h => (h.Dia, h.Tipo));
        var candidatas = plano.Contexto.Candidatas.ToDictionary(c => c.Id);
        var usadas = new HashSet<int>(plano.UsadasNaSemana);
        var validas = new Dictionary<(DiaSemana, TipoRefeicao), EscolhaCardapioIA>();

        foreach (var e in resposta.Escolhas)
        {
            var slot = (e.Dia, e.Tipo);
            if (!horarios.TryGetValue(slot, out var horario)) continue;     // horário não pedido
            if (validas.ContainsKey(slot)) continue;                        // horário repetido
            if (!candidatas.TryGetValue(e.ReceitaId, out var c)) continue;  // id inventado ou bloqueado
            if (!permitirRepetir && usadas.Contains(e.ReceitaId)) continue; // repetiu na semana
            if (horario.TempoMaxMinutos is int max && c.TempoPreparoMinutos is int t && t > max) continue;

            validas[slot] = e;
            usadas.Add(e.ReceitaId);
        }
        return validas;
    }

    private static int? PositivoOuNulo(int? v) => v is > 0 ? v : null;

    // Receita ainda sem ingredientes estruturados: usa as primeiras linhas do texto livre.
    private static IReadOnlyList<string> LinhasIngredientes(string? texto)
        => string.IsNullOrWhiteSpace(texto)
            ? []
            : texto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(l => l.Length > 50 ? l[..50] : l)
                .Take(MaxIngredientesPorReceita)
                .ToList();
}
