using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Planner;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Planner;
using Application.HomePlanner.Services.Notificacoes;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.Planner;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.Planner;

public class TarefaService : ITarefaService
{
    private readonly ITarefaRepository _repo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly TenantContext _tenantContext;
    private readonly IPushNotificationService _push;
    private readonly ILogger<TarefaService> _logger;

    public TarefaService(
        ITarefaRepository repo,
        TenantContextAccessor tenantAccessor,
        TenantContext tenantContext,
        IPushNotificationService push,
        ILogger<TarefaService> logger)
    {
        _repo = repo;
        _tenantAccessor = tenantAccessor;
        _tenantContext = tenantContext;
        _push = push;
        _logger = logger;
    }

    public bool UsuarioRestritoAsProprias => _tenantContext.RestritoAsProprias;

    public async Task<ResultadoListagem<TarefaListaDTO>> ListarAsync(
        TarefaFiltroDTO filtro, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        filtro.Pagina = Math.Max(1, filtro.Pagina);
        filtro.TamanhoPagina = Math.Clamp(filtro.TamanhoPagina, 1, 200);

        // Regra de visibilidade (privadas só do criador) e escopo de papel (Filho só as próprias).
        filtro.UsuarioAtualId = _tenantContext.UsuarioId;
        if (_tenantContext.RestritoAsProprias)
            filtro.ResponsavelUsuarioId = _tenantContext.UsuarioId;

        var itens = await _repo.ListarAsync(filtro, ct);
        var total = await _repo.ContarAsync(filtro, ct);

        return new ResultadoListagem<TarefaListaDTO>
        {
            Itens = itens, Total = total,
            Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina,
        };
    }

    public async Task<IReadOnlyList<TarefaListaDTO>> ListarCalendarioAsync(
        DateOnly de, DateOnly ate, string? responsavelUsuarioId = null, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        if (ate < de) (de, ate) = (ate, de);

        var filtro = new TarefaFiltroDTO
        {
            Pagina = 1, TamanhoPagina = 1000,
            DataDe = de, DataAte = ate,
            UsuarioAtualId = _tenantContext.UsuarioId,
            // Papéis restritos (Filho) só enxergam as próprias — ignora o filtro escolhido na UI.
            ResponsavelUsuarioId = _tenantContext.RestritoAsProprias
                ? _tenantContext.UsuarioId
                : responsavelUsuarioId,
        };

        return await _repo.ListarAsync(filtro, ct);
    }

    public async Task<ResultadoOperacao<int>> SalvarAsync(
        TarefaPersistenciaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        dto.Titulo = dto.Titulo?.Trim() ?? string.Empty;
        if (dto.Titulo.Length < 2)
            return ResultadoOperacao<int>.Falha(ErrosApp.TituloTarefaCurto);

        if (dto.HoraInicio.HasValue && dto.HoraFim.HasValue && dto.HoraFim < dto.HoraInicio)
            return ResultadoOperacao<int>.Falha(ErrosApp.HoraFimAntesDoInicio);

        var restrito = _tenantContext.RestritoAsProprias;

        Tarefa entidade;
        if (dto.Id == 0)
        {
            entidade = new Tarefa { CriadoPorUsuarioId = _tenantContext.UsuarioId };
            await _repo.AdicionarAsync(entidade, ct);
        }
        else
        {
            entidade = await _repo.ObterEntidadeAsync(dto.Id, ct)
                ?? throw new InvalidOperationException($"Tarefa {dto.Id} não encontrada para edição.");

            // Filho (papel restrito) só pode editar as próprias tarefas.
            if (restrito && !EhDono(entidade))
                return ResultadoOperacao<int>.Falha(ErrosApp.SomenteTarefasProprias);
        }

        var responsavelAnterior = entidade.ResponsavelUsuarioId;
        var agendamentoMudou = entidade.DataPrevista != dto.DataPrevista || entidade.HoraInicio != dto.HoraInicio;

        entidade.Titulo               = dto.Titulo;
        entidade.Descricao            = dto.Descricao?.Trim();
        entidade.DataPrevista         = dto.DataPrevista;
        entidade.HoraInicio           = dto.HoraInicio;
        // Reagendou? Esquece que já avisou, para o lembrete sair no novo horário.
        if (agendamentoMudou) entidade.LembreteEnviadoEm = null;
        entidade.HoraFim              = dto.HoraFim;
        entidade.Recorrencia          = dto.Recorrencia;
        entidade.Visibilidade         = dto.Visibilidade;
        // Tarefa privada nunca notifica os pais (não vaza para fora do criador).
        entidade.NotificarResponsaveis = dto.NotificarResponsaveis && dto.Visibilidade != VisibilidadeTarefa.Privada;
        // Filho só pode atribuir tarefas a si mesmo; Owner/Membro atribuem a qualquer membro.
        entidade.ResponsavelUsuarioId = restrito
            ? _tenantContext.UsuarioId
            : (string.IsNullOrWhiteSpace(dto.ResponsavelUsuarioId) ? null : dto.ResponsavelUsuarioId);

        await _repo.SalvarAsync(ct);

        await NotificarAtribuicaoAsync(entidade, responsavelAnterior, ct);
        return ResultadoOperacao<int>.Ok(entidade.Id);
    }

    public async Task<ResultadoOperacao> ConcluirAsync(int id, bool concluida, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var entidade = await _repo.ObterEntidadeAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha(ErrosApp.TarefaNaoEncontrada);

        if (_tenantContext.RestritoAsProprias && !EhDono(entidade))
            return ResultadoOperacao.Falha(ErrosApp.SomenteTarefasProprias);

        entidade.Concluida     = concluida;
        entidade.DataConclusao = concluida ? DateTime.UtcNow : null;

        // Tarefas recorrentes: ao concluir, reagenda a próxima ocorrência
        if (concluida && entidade.Recorrencia != Recorrencia.Nenhuma && entidade.DataPrevista.HasValue)
        {
            var proxima = ProximaData(entidade.DataPrevista.Value, entidade.Recorrencia);
            entidade.DataPrevista      = proxima;
            entidade.Concluida         = false;
            entidade.DataConclusao     = null;
            entidade.LembreteEnviadoEm = null; // nova ocorrência → avisar de novo
            _logger.LogInformation("Tarefa recorrente {Id} reagendada para {Data}.", id, proxima);
        }

        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var entidade = await _repo.ObterEntidadeAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha(ErrosApp.TarefaNaoEncontrada);

        if (_tenantContext.RestritoAsProprias && !EhDono(entidade))
            return ResultadoOperacao.Falha(ErrosApp.SomenteTarefasProprias);

        entidade.IsDeleted = true;
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<IReadOnlyList<MembroFamiliaDTO>> ListarMembrosFamiliaAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        return await _repo.ListarMembrosFamiliaAsync(ct);
    }

    /// <summary>
    /// Avisa o responsável por push quando a tarefa é atribuída a outra pessoa (não a si mesmo)
    /// e a atribuição mudou. Falha de envio nunca quebra o salvamento.
    /// </summary>
    private async Task NotificarAtribuicaoAsync(Tarefa tarefa, string? responsavelAnterior, CancellationToken ct)
    {
        var novoResponsavel = tarefa.ResponsavelUsuarioId;
        var tenantId = _tenantContext.TenantId;

        // Só notifica nova atribuição a outra pessoa.
        if (novoResponsavel is null
            || tenantId is null
            || novoResponsavel == _tenantContext.UsuarioId
            || novoResponsavel == responsavelAnterior)
            return;

        try
        {
            // Texto resolvido no idioma do destinatário dentro do serviço de push.
            await _push.EnviarTarefaAtribuidaAsync(tenantId.Value, novoResponsavel, tarefa.Titulo, tarefa.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao notificar atribuição da tarefa {Id}.", tarefa.Id);
        }
    }

    /// <summary>Tarefa pertence ao usuário atual (é responsável OU criador). Usado nas guardas de papel restrito.</summary>
    private bool EhDono(Tarefa t) =>
        t.ResponsavelUsuarioId == _tenantContext.UsuarioId
        || t.CriadoPorUsuarioId == _tenantContext.UsuarioId;

    private static DateOnly ProximaData(DateOnly atual, Recorrencia recorrencia) => recorrencia switch
    {
        Recorrencia.Diaria  => atual.AddDays(1),
        Recorrencia.Semanal => atual.AddDays(7),
        Recorrencia.Mensal  => atual.AddMonths(1),
        _                   => atual,
    };
}
