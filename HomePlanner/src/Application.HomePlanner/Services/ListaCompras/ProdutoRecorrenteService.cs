using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.ListaCompras;

public class ProdutoRecorrenteService : IProdutoRecorrenteService
{
    private readonly IProdutoRecorrenteRepository _repo;
    private readonly IPedidoCompraRepository _pedidoRepo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<ProdutoRecorrenteService> _logger;

    public ProdutoRecorrenteService(
        IProdutoRecorrenteRepository repo,
        IPedidoCompraRepository pedidoRepo,
        TenantContextAccessor tenantAccessor,
        TenantContext tenantContext,
        ILogger<ProdutoRecorrenteService> logger)
    {
        _repo = repo;
        _pedidoRepo = pedidoRepo;
        _tenantAccessor = tenantAccessor;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public bool PodeGerenciar => !_tenantContext.RestritoAsProprias;

    public async Task<IReadOnlyList<ProdutoRecorrenteDTO>> ListarAsync(
        bool apenasAtivos = false, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var produtos = await _repo.ListarAsync(ct);
        return produtos
            .Where(p => !apenasAtivos || p.Ativo)
            .Select(ParaDTO)
            .ToList();
    }

    public async Task<ResultadoOperacao<int>> CriarAsync(
        ProdutoRecorrentePersistenciaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (!PodeGerenciar)
            return ResultadoOperacao<int>.Falha("Você não tem permissão para gerenciar produtos recorrentes.");

        var descricao = dto.Descricao?.Trim() ?? string.Empty;
        if (descricao.Length < 2)
            return ResultadoOperacao<int>.Falha("A descrição do produto deve ter pelo menos 2 caracteres.");

        var entidade = new ProdutoRecorrente
        {
            Descricao  = descricao,
            Quantidade = string.IsNullOrWhiteSpace(dto.Quantidade) ? null : dto.Quantidade.Trim(),
            ListaId    = dto.ListaId,
            Ativo      = dto.Ativo,
            Ordem      = await _repo.ProximaOrdemAsync(ct),
        };

        await _repo.AdicionarAsync(entidade, ct);
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao<int>.Ok(entidade.Id);
    }

    public async Task<ResultadoOperacao> AtualizarAsync(
        int id, ProdutoRecorrentePersistenciaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (!PodeGerenciar)
            return ResultadoOperacao.Falha("Você não tem permissão para gerenciar produtos recorrentes.");

        var entidade = await _repo.ObterEntidadeAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha("Produto não encontrado.");

        var descricao = dto.Descricao?.Trim() ?? string.Empty;
        if (descricao.Length < 2)
            return ResultadoOperacao.Falha("A descrição do produto deve ter pelo menos 2 caracteres.");

        entidade.Descricao  = descricao;
        entidade.Quantidade = string.IsNullOrWhiteSpace(dto.Quantidade) ? null : dto.Quantidade.Trim();
        entidade.ListaId    = dto.ListaId;
        entidade.Ativo      = dto.Ativo;

        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> ExcluirAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (!PodeGerenciar)
            return ResultadoOperacao.Falha("Você não tem permissão para gerenciar produtos recorrentes.");

        var entidade = await _repo.ObterEntidadeAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha("Produto não encontrado.");

        entidade.IsDeleted = true;
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<IReadOnlyList<string>> ListarSugestoesHistoricoAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        return await _repo.ListarDescricoesHistoricoAsync(ct);
    }

    public async Task<ResultadoOperacao<int>> ImportarDoHistoricoAsync(
        IReadOnlyCollection<string> descricoes, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (!PodeGerenciar)
            return ResultadoOperacao<int>.Falha("Você não tem permissão para gerenciar produtos recorrentes.");

        // Descrições já no catálogo (para não duplicar), comparadas sem diferenciar caixa.
        var existentes = (await _repo.ListarAsync(ct))
            .Select(p => p.Descricao)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var vistas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordem = await _repo.ProximaOrdemAsync(ct);
        var criados = 0;

        foreach (var bruta in descricoes)
        {
            var descricao = bruta?.Trim() ?? string.Empty;
            if (descricao.Length < 2) continue;
            if (existentes.Contains(descricao) || !vistas.Add(descricao)) continue;

            await _repo.AdicionarAsync(new ProdutoRecorrente
            {
                Descricao = descricao,
                Ativo     = true,
                Ordem     = ordem++,
            }, ct);
            criados++;
        }

        if (criados > 0)
            await _repo.SalvarAsync(ct);

        return ResultadoOperacao<int>.Ok(criados);
    }

    public async Task<ResultadoOperacao<AdicaoRecorrentesResultadoDTO>> AdicionarASemanaAsync(
        IReadOnlyCollection<int> produtoIds, DateOnly dataInicio, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (!PodeGerenciar)
            return ResultadoOperacao<AdicaoRecorrentesResultadoDTO>.Falha(
                "Você não tem permissão para gerenciar produtos recorrentes.");

        if (produtoIds.Count == 0)
            return ResultadoOperacao<AdicaoRecorrentesResultadoDTO>.Ok(new AdicaoRecorrentesResultadoDTO());

        var produtos = await _repo.ObterVariasAsync(produtoIds, ct);

        // Já na lista da semana? Compara pela descrição, sem diferenciar caixa.
        var jaNaSemana = (await _pedidoRepo.ListarDaSemanaAsync(dataInicio, ct))
            .Select(p => p.Descricao)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var adicionados = 0;
        var pulados = 0;

        foreach (var produto in produtos)
        {
            // jaNaSemana.Add devolve false se já existia (na semana ou num item anterior do lote).
            if (!jaNaSemana.Add(produto.Descricao))
            {
                pulados++;
                continue;
            }

            await _pedidoRepo.AdicionarAsync(new PedidoCompra
            {
                DataInicioSemana     = dataInicio,
                Descricao            = produto.Descricao,
                Quantidade           = produto.Quantidade,
                ListaId              = produto.ListaId,
                SolicitanteUsuarioId = _tenantContext.UsuarioId,
            }, ct);
            adicionados++;
        }

        if (adicionados > 0)
            await _pedidoRepo.SalvarAsync(ct);

        return ResultadoOperacao<AdicaoRecorrentesResultadoDTO>.Ok(
            new AdicaoRecorrentesResultadoDTO { Adicionados = adicionados, Pulados = pulados });
    }

    private static ProdutoRecorrenteDTO ParaDTO(ProdutoRecorrente p) => new()
    {
        Id         = p.Id,
        Descricao  = p.Descricao,
        Quantidade = p.Quantidade,
        ListaId    = p.ListaId,
        Ativo      = p.Ativo,
        Ordem      = p.Ordem,
    };
}
