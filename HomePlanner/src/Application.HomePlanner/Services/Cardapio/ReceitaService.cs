using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;
using Application.HomePlanner.Helpers;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.Cardapio;

public class ReceitaService : IReceitaService
{
    private readonly IReceitaRepository _repo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly ILogger<ReceitaService> _logger;

    public ReceitaService(
        IReceitaRepository repo,
        TenantContextAccessor tenantAccessor,
        ILogger<ReceitaService> logger)
    {
        _repo = repo;
        _tenantAccessor = tenantAccessor;
        _logger = logger;
    }

    public async Task<ResultadoListagem<ReceitaListaDTO>> ListarAsync(
        ReceitaFiltroDTO filtro, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        filtro.Pagina = Math.Max(1, filtro.Pagina);
        filtro.TamanhoPagina = Math.Clamp(filtro.TamanhoPagina, 1, 50);

        var itens = await _repo.ListarAsync(filtro, ct);
        var total = await _repo.ContarAsync(filtro, ct);

        return new ResultadoListagem<ReceitaListaDTO>
        {
            Itens = itens, Total = total,
            Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina,
        };
    }

    public async Task<ResultadoOperacao<ReceitaDetalheDTO>> ObterAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var dto = await _repo.ObterDetalheAsync(id, ct);
        return dto is null
            ? ResultadoOperacao<ReceitaDetalheDTO>.Falha("Receita não encontrada.")
            : ResultadoOperacao<ReceitaDetalheDTO>.Ok(dto);
    }

    public async Task<ResultadoOperacao<int>> SalvarAsync(
        ReceitaPersistenciaDTO dto, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        dto.Nome = dto.Nome?.Trim() ?? string.Empty;
        if (dto.Nome.Length < 2)
            return ResultadoOperacao<int>.Falha("Nome da receita deve ter pelo menos 2 caracteres.");

        if (dto.NumeroPorcoesBase < 1)
            return ResultadoOperacao<int>.Falha("Número de porções deve ser ao menos 1.");

        Receita entidade;
        if (dto.Id == 0)
        {
            entidade = new Receita();
            await _repo.AdicionarAsync(entidade, ct);
        }
        else
        {
            entidade = await _repo.ObterEntidadeComIngredientesAsync(dto.Id, ct)
                ?? throw new InvalidOperationException($"Receita {dto.Id} não encontrada para edição.");
        }

        MapearDtoParaEntidade(dto, entidade);
        SincronizarIngredientes(dto, entidade);

        await _repo.SalvarAsync(ct);
        _logger.LogInformation("Receita {Id} '{Nome}' salva.", entidade.Id, entidade.Nome);
        return ResultadoOperacao<int>.Ok(entidade.Id);
    }

    public async Task<ResultadoOperacao> DeletarAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var entidade = await _repo.ObterEntidadeComIngredientesAsync(id, ct);
        if (entidade is null)
            return ResultadoOperacao.Falha("Receita não encontrada.");

        entidade.IsDeleted = true;
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao<int>> DuplicarAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var original = await _repo.ObterEntidadeComIngredientesAsync(id, ct);
        if (original is null)
            return ResultadoOperacao<int>.Falha("Receita não encontrada.");

        var copia = new Receita
        {
            Nome               = $"{original.Nome} (cópia)",
            NomeNormalizado    = TextoHelper.NormalizarNome($"{original.Nome} copia"),
            ModoPreparo        = original.ModoPreparo,
            NumeroPorcoesBase  = original.NumeroPorcoesBase,
            TempoPreparoMinutos = original.TempoPreparoMinutos,
            UrlOrigem          = original.UrlOrigem,
            UrlImagem          = original.UrlImagem,
            Observacoes        = original.Observacoes,
        };

        foreach (var ri in original.Ingredientes.Where(i => !i.IsDeleted))
        {
            copia.Ingredientes.Add(new ReceitaIngrediente
            {
                IngredienteId   = ri.IngredienteId,
                Quantidade      = ri.Quantidade,
                UnidadeMedidaId = ri.UnidadeMedidaId,
                Observacao      = ri.Observacao,
                Opcional        = ri.Opcional,
                Ordem           = ri.Ordem,
            });
        }

        await _repo.AdicionarAsync(copia, ct);
        await _repo.SalvarAsync(ct);
        return ResultadoOperacao<int>.Ok(copia.Id);
    }

    private static void MapearDtoParaEntidade(ReceitaPersistenciaDTO dto, Receita entidade)
    {
        entidade.Nome               = dto.Nome;
        entidade.NomeNormalizado    = TextoHelper.NormalizarNome(dto.Nome);
        entidade.ModoPreparo        = dto.ModoPreparo?.Trim();
        entidade.NumeroPorcoesBase  = dto.NumeroPorcoesBase;
        entidade.TempoPreparoMinutos = dto.TempoPreparoMinutos;
        entidade.UrlOrigem          = dto.UrlOrigem?.Trim();
        entidade.UrlImagem          = dto.UrlImagem?.Trim();
        entidade.Observacoes        = dto.Observacoes?.Trim();
    }

    private static void SincronizarIngredientes(ReceitaPersistenciaDTO dto, Receita entidade)
    {
        // Marca como deletados os que foram removidos
        var idsNoDto = dto.Ingredientes.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();
        foreach (var ri in entidade.Ingredientes.Where(i => !i.IsDeleted && !idsNoDto.Contains(i.Id)))
            ri.IsDeleted = true;

        // Atualiza existentes e insere novos
        foreach (var dtoRi in dto.Ingredientes)
        {
            if (dtoRi.Id > 0)
            {
                var existente = entidade.Ingredientes.FirstOrDefault(i => i.Id == dtoRi.Id);
                if (existente is not null)
                {
                    existente.IngredienteId   = dtoRi.IngredienteId;
                    existente.Quantidade      = dtoRi.Quantidade;
                    existente.UnidadeMedidaId = dtoRi.UnidadeMedidaId;
                    existente.Observacao      = dtoRi.Observacao;
                    existente.Opcional        = dtoRi.Opcional;
                    existente.Ordem           = dtoRi.Ordem;
                }
            }
            else
            {
                entidade.Ingredientes.Add(new ReceitaIngrediente
                {
                    IngredienteId   = dtoRi.IngredienteId,
                    Quantidade      = dtoRi.Quantidade,
                    UnidadeMedidaId = dtoRi.UnidadeMedidaId,
                    Observacao      = dtoRi.Observacao,
                    Opcional        = dtoRi.Opcional,
                    Ordem           = dtoRi.Ordem,
                });
            }
        }
    }
}
