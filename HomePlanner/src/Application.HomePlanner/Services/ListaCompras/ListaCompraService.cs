using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.ListaCompras;

public class ListaCompraService : IListaCompraService
{
    private readonly IListaCompraRepository _repo;
    private readonly IPedidoCompraRepository _pedidoRepo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<ListaCompraService> _logger;

    public ListaCompraService(
        IListaCompraRepository repo,
        IPedidoCompraRepository pedidoRepo,
        TenantContextAccessor tenantAccessor,
        TenantContext tenantContext,
        ILogger<ListaCompraService> logger)
    {
        _repo = repo;
        _pedidoRepo = pedidoRepo;
        _tenantAccessor = tenantAccessor;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public bool PodeGerenciar => !_tenantContext.RestritoAsProprias;

    public async Task<IReadOnlyList<ListaCompraDTO>> ListarAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var listas = await _repo.ListarAsync(ct);
        return listas.Select(l => new ListaCompraDTO
        {
            Id    = l.Id,
            Nome  = l.Nome,
            Icone = l.Icone,
            Cor   = l.Cor,
            Ordem = l.Ordem,
        }).ToList();
    }

    public async Task<ResultadoOperacao<int>> CriarAsync(ListaCompraPersistenciaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (!PodeGerenciar)
            return ResultadoOperacao<int>.Falha("Você não tem permissão para gerenciar listas.");

        var nome = dto.Nome?.Trim() ?? string.Empty;
        if (nome.Length < 2)
            return ResultadoOperacao<int>.Falha("O nome da lista deve ter pelo menos 2 caracteres.");

        var entidade = new ListaCompra
        {
            Nome  = nome,
            Icone = string.IsNullOrWhiteSpace(dto.Icone) ? null : dto.Icone.Trim(),
            Cor   = string.IsNullOrWhiteSpace(dto.Cor) ? null : dto.Cor.Trim(),
            Ordem = await _repo.ProximaOrdemAsync(ct),
        };

        await _repo.AdicionarAsync(entidade, ct);
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao<int>.Ok(entidade.Id);
    }

    public async Task<ResultadoOperacao> AtualizarAsync(int id, ListaCompraPersistenciaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (!PodeGerenciar)
            return ResultadoOperacao.Falha("Você não tem permissão para gerenciar listas.");

        var entidade = await _repo.ObterEntidadeAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha("Lista não encontrada.");

        var nome = dto.Nome?.Trim() ?? string.Empty;
        if (nome.Length < 2)
            return ResultadoOperacao.Falha("O nome da lista deve ter pelo menos 2 caracteres.");

        entidade.Nome  = nome;
        entidade.Icone = string.IsNullOrWhiteSpace(dto.Icone) ? null : dto.Icone.Trim();
        entidade.Cor   = string.IsNullOrWhiteSpace(dto.Cor) ? null : dto.Cor.Trim();

        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> ExcluirAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (!PodeGerenciar)
            return ResultadoOperacao.Falha("Você não tem permissão para gerenciar listas.");

        var entidade = await _repo.ObterEntidadeAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha("Lista não encontrada.");

        // Remove as preferências que apontam para esta lista (senão os itens ficariam órfãos).
        await _repo.RemoverPreferenciasDaListaAsync(id, ct);
        entidade.IsDeleted = true;
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> AtribuirItemCardapioAsync(int ingredienteId, int? listaId, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        if (listaId is not null && await _repo.ObterEntidadeAsync(listaId.Value, ct) is null)
            return ResultadoOperacao.Falha("Lista não encontrada.");

        var pref = await _repo.ObterPreferenciaAsync(ingredienteId, ct);

        if (listaId is null)
        {
            // "Geral" → limpa a preferência (item volta ao balde padrão).
            if (pref is not null) _repo.RemoverPreferencia(pref);
        }
        else if (pref is null)
        {
            await _repo.AdicionarPreferenciaAsync(new PreferenciaLojaIngrediente
            {
                IngredienteId = ingredienteId,
                ListaId       = listaId.Value,
            }, ct);
        }
        else
        {
            pref.ListaId = listaId.Value;
        }

        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> AtribuirPedidoAsync(int pedidoId, int? listaId, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var pedido = await _pedidoRepo.ObterEntidadeAsync(pedidoId, ct);
        if (pedido is null)
            return ResultadoOperacao.Falha("Pedido não encontrado.");

        // Filho só pode remanejar os próprios pedidos.
        if (_tenantContext.RestritoAsProprias && pedido.SolicitanteUsuarioId != _tenantContext.UsuarioId)
            return ResultadoOperacao.Falha("Você só pode remanejar os seus próprios pedidos.");

        if (listaId is not null && await _repo.ObterEntidadeAsync(listaId.Value, ct) is null)
            return ResultadoOperacao.Falha("Lista não encontrada.");

        pedido.ListaId = listaId;
        await _pedidoRepo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }
}
