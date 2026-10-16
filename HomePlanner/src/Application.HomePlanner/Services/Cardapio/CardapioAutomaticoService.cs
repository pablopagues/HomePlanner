using System.Text.Json;
using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.CardapioIA;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Domain.HomePlanner.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.Cardapio;

public class CardapioAutomaticoService : ICardapioAutomaticoService
{
    // Erros temporários da IA: a geração é repetida em ciclos seguintes, até este limite.
    private const int MaxTentativas = 3;
    private static readonly TimeSpan IntervaloEntreTentativas = TimeSpan.FromMinutes(10);

    private static readonly HashSet<string> ErrosTemporarios =
    [
        ErrosApp.CardapioIANaoGerado.Codigo,
        ErrosApp.MuitasAnalisesIA.Codigo,
        ErrosApp.IaIndisponivel.Codigo,
    ];

    private readonly ICardapioAutomaticoRepository _repo;
    private readonly ICardapioIAService _cardapioIA;
    private readonly IReceitaService _receitas;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<CardapioAutomaticoService> _logger;

    public CardapioAutomaticoService(
        ICardapioAutomaticoRepository repo,
        ICardapioIAService cardapioIA,
        IReceitaService receitas,
        TenantContext tenantContext,
        ILogger<CardapioAutomaticoService> logger)
    {
        _repo = repo;
        _cardapioIA = cardapioIA;
        _receitas = receitas;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<AgendamentoCardapioIADTO?> ObterAgendamentoAsync(CancellationToken ct = default)
    {
        var a = await _repo.ObterAgendamentoAsync(ct);
        if (a is null) return null;

        return new AgendamentoCardapioIADTO
        {
            Ativo     = a.Ativo,
            DiaSemana = a.DiaSemana,
            Hora      = a.Hora,
            Regras    = LerRegras(a.ParametrosJson),
        };
    }

    public async Task<ResultadoOperacao> SalvarAgendamentoAsync(AgendamentoCardapioIADTO dto, CancellationToken ct = default)
    {
        if (!await _receitas.PodeGerenciarAsync())
            return ResultadoOperacao.Falha(ErrosApp.SemPermissaoCardapioIA);
        if (dto.Regras.Tipos.Count == 0)
            return ResultadoOperacao.Falha(ErrosApp.SelecioneTipoRefeicao);

        var a = await _repo.ObterAgendamentoAsync(ct);
        if (a is null)
        {
            if (_tenantContext.TenantId is not Guid tenantId)
                return ResultadoOperacao.Falha(ErrosApp.SessaoInvalida);
            a = new AgendamentoCardapioIA { TenantId = tenantId };
            _repo.AdicionarAgendamento(a);
        }

        a.Ativo          = dto.Ativo;
        a.DiaSemana      = Enum.IsDefined(dto.DiaSemana) ? dto.DiaSemana : DiaSemana.Sexta;
        a.Hora           = Math.Clamp(dto.Hora, 0, 23);
        a.ParametrosJson = JsonSerializer.Serialize(dto.Regras);

        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<RascunhoCardapioIADTO?> ObterRascunhoProntoAsync(DateOnly dataInicio, CancellationToken ct = default)
    {
        if (!await _receitas.PodeGerenciarAsync()) return null;

        var r = await _repo.ObterRascunhoAsync(dataInicio, ct);
        if (r is not { Status: StatusRascunhoCardapio.Pronto } || r.ConteudoJson is null) return null;

        var sugestao = JsonSerializer.Deserialize<CardapioSugeridoDTO>(r.ConteudoJson);
        if (sugestao is null) return null;

        return new RascunhoCardapioIADTO
        {
            Id         = r.Id,
            DataInicio = r.DataInicio,
            GeradoEm   = r.AtualizadoEm,
            Sugestao   = sugestao,
        };
    }

    public async Task<ResultadoOperacao> DescartarRascunhoAsync(int id, CancellationToken ct = default)
    {
        if (!await _receitas.PodeGerenciarAsync())
            return ResultadoOperacao.Falha(ErrosApp.SemPermissaoCardapioIA);

        var r = await _repo.ObterRascunhoPorIdAsync(id, ct);
        if (r is { Status: StatusRascunhoCardapio.Pronto })
        {
            r.Status = StatusRascunhoCardapio.Descartado;
            r.AtualizadoEm = DateTime.UtcNow;
            await _repo.SalvarAsync(ct);
        }
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> AplicarAsync(CardapioSugeridoDTO sugestao, CancellationToken ct = default)
    {
        var resultado = await _cardapioIA.AplicarAsync(sugestao, ct);
        if (!resultado.Sucesso) return resultado;

        // Qualquer cardápio por IA aceito para a semana encerra o rascunho dela.
        var r = await _repo.ObterRascunhoAsync(sugestao.DataInicio, ct);
        if (r is { Status: StatusRascunhoCardapio.Pronto })
        {
            r.Status = StatusRascunhoCardapio.Aplicado;
            r.AtualizadoEm = DateTime.UtcNow;
            await _repo.SalvarAsync(ct);
        }
        return resultado;
    }

    public async Task<ResultadoRascunhoAutomatico> GerarRascunhoAsync(
        DateOnly dataInicio, string idioma, CancellationToken ct = default)
    {
        var agendamento = await _repo.ObterAgendamentoAsync(ct);
        if (agendamento is not { Ativo: true }) return ResultadoRascunhoAutomatico.NadaAFazer;

        var agora = DateTime.UtcNow;
        var rascunho = await _repo.ObterRascunhoAsync(dataInicio, ct);
        if (rascunho is null)
        {
            // A linha é criada antes de chamar a IA: é a trava contra gerar duas vezes.
            rascunho = new RascunhoCardapioIA
            {
                TenantId     = agendamento.TenantId,
                DataInicio   = dataInicio,
                Status       = StatusRascunhoCardapio.Gerando,
                CriadoEm     = agora,
                AtualizadoEm = agora,
            };
            if (!await _repo.TentarCriarRascunhoAsync(rascunho, ct))
                return ResultadoRascunhoAutomatico.NadaAFazer;
        }
        else
        {
            if (rascunho.Status != StatusRascunhoCardapio.Gerando) return ResultadoRascunhoAutomatico.NadaAFazer;
            if (agora - rascunho.AtualizadoEm < IntervaloEntreTentativas) return ResultadoRascunhoAutomatico.NadaAFazer;
            if (rascunho.Tentativas >= MaxTentativas)
                return await FinalizarAsync(rascunho, StatusRascunhoCardapio.Falhou, ResultadoRascunhoAutomatico.Falhou, ct);
        }

        rascunho.Tentativas += 1;
        rascunho.AtualizadoEm = agora;
        await _repo.SalvarAsync(ct);

        var pedido = LerRegras(agendamento.ParametrosJson);
        pedido.DataInicio = dataInicio;

        var r = await _cardapioIA.GerarAsync(pedido, idioma, ct);
        rascunho.AtualizadoEm = DateTime.UtcNow;

        if (r.Sucesso)
        {
            rascunho.ConteudoJson = JsonSerializer.Serialize(r.Valor);
            rascunho.UltimoErro = null;
            return await FinalizarAsync(rascunho, StatusRascunhoCardapio.Pronto, ResultadoRascunhoAutomatico.Pronto, ct);
        }

        var codigo = r.Erros.FirstOrDefault()?.Codigo;
        rascunho.UltimoErro = codigo;
        _logger.LogInformation("Rascunho automático do cardápio ({Data}) não gerado: {Erro} (tentativa {Tentativa}).",
            dataInicio, codigo, rascunho.Tentativas);

        if (codigo == ErrosApp.NenhumHorarioParaPreencher.Codigo)
            return await FinalizarAsync(rascunho, StatusRascunhoCardapio.Ignorado, ResultadoRascunhoAutomatico.Ignorado, ct);

        if (codigo is not null && ErrosTemporarios.Contains(codigo) && rascunho.Tentativas < MaxTentativas)
        {
            await _repo.SalvarAsync(ct); // continua "Gerando": o próximo ciclo tenta de novo
            return ResultadoRascunhoAutomatico.TentarDepois;
        }

        return await FinalizarAsync(rascunho, StatusRascunhoCardapio.Falhou, ResultadoRascunhoAutomatico.Falhou, ct);
    }

    private async Task<ResultadoRascunhoAutomatico> FinalizarAsync(
        RascunhoCardapioIA rascunho, StatusRascunhoCardapio status, ResultadoRascunhoAutomatico resultado, CancellationToken ct)
    {
        rascunho.Status = status;
        await _repo.SalvarAsync(ct);
        return resultado;
    }

    private static PedidoCardapioIADTO LerRegras(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<PedidoCardapioIADTO>(json) ?? new PedidoCardapioIADTO();
        }
        catch (JsonException)
        {
            return new PedidoCardapioIADTO();
        }
    }
}
