using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Ingrediente;
using Application.HomePlanner.Helpers;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.Cardapio;

public class IngredienteService : IIngredienteService
{
    private readonly IIngredienteRepository _repo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly ILogger<IngredienteService> _logger;

    public IngredienteService(
        IIngredienteRepository repo,
        TenantContextAccessor tenantAccessor,
        ILogger<IngredienteService> logger)
    {
        _repo = repo;
        _tenantAccessor = tenantAccessor;
        _logger = logger;
    }

    public async Task<ResultadoListagem<IngredienteListaDTO>> ListarAsync(
        IngredienteFiltroDTO filtro, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        filtro.Pagina = Math.Max(1, filtro.Pagina);
        filtro.TamanhoPagina = Math.Clamp(filtro.TamanhoPagina, 1, 100);

        var itens = await _repo.ListarAsync(filtro, ct);
        var total = await _repo.ContarAsync(filtro, ct);

        return new ResultadoListagem<IngredienteListaDTO>
        {
            Itens = itens, Total = total,
            Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina,
        };
    }

    public async Task<ResultadoOperacao<IngredienteDetalheDTO>> ObterAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var dto = await _repo.ObterDetalheAsync(id, ct);
        return dto is null
            ? ResultadoOperacao<IngredienteDetalheDTO>.Falha("Ingrediente não encontrado.")
            : ResultadoOperacao<IngredienteDetalheDTO>.Ok(dto);
    }

    public async Task<ResultadoOperacao<int>> SalvarAsync(
        IngredientePersistenciaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        dto.Nome = dto.Nome?.Trim() ?? string.Empty;
        if (dto.Nome.Length < 2)
            return ResultadoOperacao<int>.Falha("Nome do ingrediente deve ter pelo menos 2 caracteres.");

        var nomeNormalizado = TextoHelper.NormalizarNome(dto.Nome);

        if (await _repo.ExisteNomeDuplicadoAsync(nomeNormalizado, dto.Id == 0 ? null : dto.Id, ct))
            return ResultadoOperacao<int>.Falha($"Já existe um ingrediente com o nome '{dto.Nome}'.");

        Ingrediente entidade;
        if (dto.Id == 0)
        {
            entidade = new Ingrediente
            {
                Nome                 = dto.Nome,
                NomeNormalizado      = nomeNormalizado,
                Categoria            = dto.Categoria?.Trim(),
                UnidadeMedidaPadraoId = dto.UnidadeMedidaPadraoId,
            };
            await _repo.AdicionarAsync(entidade, ct);
        }
        else
        {
            entidade = await _repo.ObterEntidadeAsync(dto.Id, ct)
                ?? throw new InvalidOperationException($"Ingrediente {dto.Id} não encontrado para edição.");

            entidade.Nome                 = dto.Nome;
            entidade.NomeNormalizado      = nomeNormalizado;
            entidade.Categoria            = dto.Categoria?.Trim();
            entidade.UnidadeMedidaPadraoId = dto.UnidadeMedidaPadraoId;
        }

        await _repo.SalvarAsync(ct);
        _logger.LogInformation("Ingrediente {Id} '{Nome}' salvo.", entidade.Id, entidade.Nome);
        return ResultadoOperacao<int>.Ok(entidade.Id);
    }

    public async Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var entidade = await _repo.ObterEntidadeAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha("Ingrediente não encontrado.");

        entidade.IsDeleted = true;
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<IReadOnlyList<IngredienteListaDTO>> BuscarAutoCompleteAsync(
        string texto, int limite = 10, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (string.IsNullOrWhiteSpace(texto)) return [];
        return await _repo.BuscarAutoCompleteAsync(TextoHelper.NormalizarNome(texto), limite, ct);
    }

    public async Task<IReadOnlyList<IngredienteListaDTO>> DetectarSimilaresAsync(
        string nome, int? excluirId = null, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (string.IsNullOrWhiteSpace(nome)) return [];
        return await _repo.DetectarSimilaresAsync(TextoHelper.NormalizarNome(nome), excluirId, ct);
    }

    public async Task<ResultadoOperacao> DefinirProdutoBaseAsync(
        int ingredienteId, int? baseId, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var ingrediente = await _repo.ObterEntidadeAsync(ingredienteId, ct);
        if (ingrediente is null)
            return ResultadoOperacao.Falha("Ingrediente não encontrado.");

        // Desagrupar.
        if (baseId is null)
        {
            ingrediente.IngredienteBaseId = null;
            await _repo.SalvarAsync(ct);
            return ResultadoOperacao.Ok();
        }

        if (baseId == ingredienteId)
            return ResultadoOperacao.Falha("Um ingrediente não pode ser produto base de si mesmo.");

        var alvo = await _repo.ObterEntidadeAsync(baseId.Value, ct);
        if (alvo is null)
            return ResultadoOperacao.Falha("Produto base não encontrado.");

        // Mantém a invariante "base sempre aponta para a raiz": achata a cadeia do alvo.
        var raizId = alvo.IngredienteBaseId ?? alvo.Id;
        if (raizId == ingredienteId)
            return ResultadoOperacao.Falha("Este agrupamento criaria um ciclo.");

        ingrediente.IngredienteBaseId = raizId;

        // Variantes que apontavam para este ingrediente passam a apontar para a raiz.
        foreach (var filho in await _repo.ObterFilhosDiretosAsync(ingredienteId, ct))
            filho.IngredienteBaseId = raizId;

        await _repo.SalvarAsync(ct);
        _logger.LogInformation("Ingrediente {Id} agrupado sob produto base {BaseId}.",
            ingredienteId, raizId);
        return ResultadoOperacao.Ok();
    }
}
