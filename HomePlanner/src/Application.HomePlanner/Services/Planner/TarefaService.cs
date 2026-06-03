using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Planner;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Planner;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.Planner;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.Planner;

public class TarefaService : ITarefaService
{
    private readonly ITarefaRepository _repo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<TarefaService> _logger;

    public TarefaService(
        ITarefaRepository repo,
        TenantContextAccessor tenantAccessor,
        TenantContext tenantContext,
        ILogger<TarefaService> logger)
    {
        _repo = repo;
        _tenantAccessor = tenantAccessor;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public bool UsuarioRestritoAsProprias => _tenantContext.RestritoAsProprias;

    public async Task<ResultadoListagem<TarefaListaDTO>> ListarAsync(
        TarefaFiltroDTO filtro, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        filtro.Pagina = Math.Max(1, filtro.Pagina);
        filtro.TamanhoPagina = Math.Clamp(filtro.TamanhoPagina, 1, 200);

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
            return ResultadoOperacao<int>.Falha("O título da tarefa deve ter pelo menos 2 caracteres.");

        if (dto.HoraInicio.HasValue && dto.HoraFim.HasValue && dto.HoraFim < dto.HoraInicio)
            return ResultadoOperacao<int>.Falha("A hora de fim deve ser igual ou posterior à hora de início.");

        Tarefa entidade;
        if (dto.Id == 0)
        {
            entidade = new Tarefa();
            await _repo.AdicionarAsync(entidade, ct);
        }
        else
        {
            entidade = await _repo.ObterEntidadeAsync(dto.Id, ct)
                ?? throw new InvalidOperationException($"Tarefa {dto.Id} não encontrada para edição.");
        }

        entidade.Titulo               = dto.Titulo;
        entidade.Descricao            = dto.Descricao?.Trim();
        entidade.DataPrevista         = dto.DataPrevista;
        entidade.HoraInicio           = dto.HoraInicio;
        entidade.HoraFim              = dto.HoraFim;
        entidade.Recorrencia          = dto.Recorrencia;
        entidade.Visibilidade         = dto.Visibilidade;
        entidade.ResponsavelUsuarioId = string.IsNullOrWhiteSpace(dto.ResponsavelUsuarioId)
            ? null : dto.ResponsavelUsuarioId;

        await _repo.SalvarAsync(ct);
        return ResultadoOperacao<int>.Ok(entidade.Id);
    }

    public async Task<ResultadoOperacao> ConcluirAsync(int id, bool concluida, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var entidade = await _repo.ObterEntidadeAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha("Tarefa não encontrada.");

        entidade.Concluida     = concluida;
        entidade.DataConclusao = concluida ? DateTime.UtcNow : null;

        // Tarefas recorrentes: ao concluir, reagenda a próxima ocorrência
        if (concluida && entidade.Recorrencia != Recorrencia.Nenhuma && entidade.DataPrevista.HasValue)
        {
            var proxima = ProximaData(entidade.DataPrevista.Value, entidade.Recorrencia);
            entidade.DataPrevista  = proxima;
            entidade.Concluida     = false;
            entidade.DataConclusao = null;
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
            return ResultadoOperacao.Falha("Tarefa não encontrada.");

        entidade.IsDeleted = true;
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<IReadOnlyList<MembroFamiliaDTO>> ListarMembrosFamiliaAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        return await _repo.ListarMembrosFamiliaAsync(ct);
    }

    private static DateOnly ProximaData(DateOnly atual, Recorrencia recorrencia) => recorrencia switch
    {
        Recorrencia.Diaria  => atual.AddDays(1),
        Recorrencia.Semanal => atual.AddDays(7),
        Recorrencia.Mensal  => atual.AddMonths(1),
        _                   => atual,
    };
}
