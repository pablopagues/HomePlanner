using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.DTOs.Planner;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.ListaCompras;

public class PedidoCompraService : IPedidoCompraService
{
    private readonly IPedidoCompraRepository _repo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<PedidoCompraService> _logger;

    public PedidoCompraService(
        IPedidoCompraRepository repo,
        TenantContextAccessor tenantAccessor,
        TenantContext tenantContext,
        ILogger<PedidoCompraService> logger)
    {
        _repo = repo;
        _tenantAccessor = tenantAccessor;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public bool UsuarioRestritoAsProprias => _tenantContext.RestritoAsProprias;

    public async Task<IReadOnlyList<PedidoMembroGrupoDTO>> ListarDaSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var pedidos = await _repo.ListarDaSemanaAsync(dataInicio, ct);

        // Todos veem tudo; só o Owner/Membro (não restrito) ou o próprio solicitante editam.
        foreach (var p in pedidos)
            p.PodeEditar = !_tenantContext.RestritoAsProprias
                        || p.SolicitanteUsuarioId == _tenantContext.UsuarioId;

        return pedidos
            .GroupBy(p => p.SolicitanteUsuarioId)
            .Select(g => new PedidoMembroGrupoDTO
            {
                SolicitanteUsuarioId        = g.Key,
                SolicitanteNome             = g.First().SolicitanteNome,
                SolicitanteFotoAtualizadaEm = g.First().SolicitanteFotoAtualizadaEm,
                Itens                       = g.ToList(),
            })
            .OrderBy(g => g.SolicitanteNome)
            .ToList();
    }

    public async Task<ResultadoOperacao<int>> AdicionarAsync(
        PedidoCompraPersistenciaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        dto.Descricao = dto.Descricao?.Trim() ?? string.Empty;
        if (dto.Descricao.Length < 2)
            return ResultadoOperacao<int>.Falha(ErrosApp.DescricaoPedidoCurta);

        // Filho só pode pedir para si; Owner/Membro escolhem (null = si mesmo).
        var solicitanteId = _tenantContext.RestritoAsProprias
            ? _tenantContext.UsuarioId
            : (string.IsNullOrWhiteSpace(dto.SolicitanteUsuarioId)
                ? _tenantContext.UsuarioId
                : dto.SolicitanteUsuarioId);

        var entidade = new PedidoCompra
        {
            DataInicioSemana     = dto.DataInicioSemana,
            Descricao            = dto.Descricao,
            Quantidade           = string.IsNullOrWhiteSpace(dto.Quantidade) ? null : dto.Quantidade.Trim(),
            SolicitanteUsuarioId = solicitanteId,
        };

        await _repo.AdicionarAsync(entidade, ct);
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao<int>.Ok(entidade.Id);
    }

    public async Task<ResultadoOperacao> MarcarCompradoAsync(int id, bool comprado, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var entidade = await _repo.ObterEntidadeAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha(ErrosApp.PedidoNaoEncontrado);

        if (_tenantContext.RestritoAsProprias && !EhDono(entidade))
            return ResultadoOperacao.Falha(ErrosApp.SomentePedidosProprios);

        entidade.Comprado   = comprado;
        entidade.DataCompra = comprado ? DateTime.UtcNow : null;

        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var entidade = await _repo.ObterEntidadeAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha(ErrosApp.PedidoNaoEncontrado);

        if (_tenantContext.RestritoAsProprias && !EhDono(entidade))
            return ResultadoOperacao.Falha(ErrosApp.SomentePedidosProprios);

        entidade.IsDeleted = true;
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<IReadOnlyList<MembroFamiliaDTO>> ListarMembrosFamiliaAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        return await _repo.ListarMembrosFamiliaAsync(ct);
    }

    private bool EhDono(PedidoCompra p) => p.SolicitanteUsuarioId == _tenantContext.UsuarioId;
}
