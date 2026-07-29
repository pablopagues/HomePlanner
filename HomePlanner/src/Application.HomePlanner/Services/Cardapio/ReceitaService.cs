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
        if (dto is null)
            return ResultadoOperacao<ReceitaDetalheDTO>.Falha("Receita não encontrada.");

        // Prato composto: monta a lista de ingredientes do prato inteiro (própria + componentes).
        if (dto.Componentes.Count > 0)
        {
            var proprios = dto.Ingredientes.Select(MapearParaExpandido);
            var deComponentes = await ExpandirComponentesAsync(
                dto.Componentes.Select(c => new ReceitaComponentePersistenciaDTO
                {
                    ReceitaComponenteId = c.ComponenteId,
                    PorcoesDesejadas    = c.PorcoesDesejadas,
                }).ToList(), ct);

            dto.IngredientesExpandidos = ExpansorReceita.Consolidar(proprios.Concat(deComponentes));
        }

        return ResultadoOperacao<ReceitaDetalheDTO>.Ok(dto);
    }

    public async Task<ReceitaFotoDTO?> ObterFotoAsync(int id, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        return await _repo.ObterFotoAsync(id, ct);
    }

    public async Task<IReadOnlyList<ReceitaListaDTO>> BuscarAutoCompleteAsync(
        string texto, int limite = 10, int? excluirId = null, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        // Texto vazio → retorna as primeiras receitas (mostra opções ao focar o campo).
        var normalizado = string.IsNullOrWhiteSpace(texto) ? string.Empty : TextoHelper.NormalizarNome(texto);
        return await _repo.BuscarAutoCompleteAsync(normalizado, limite, excluirId, ct);
    }

    /// <summary>Expande os componentes (cada um nas suas porções) e consolida em ingredientes.</summary>
    public async Task<IReadOnlyList<IngredienteExpandidoDTO>> ExpandirComponentesAsync(
        IReadOnlyList<ReceitaComponentePersistenciaDTO> componentes, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        if (componentes.Count == 0) return [];

        var grafo = await _repo.ObterGrafoReceitasAsync(ct);
        var linhas = componentes
            .SelectMany(c => ExpansorReceita.Expandir(grafo, c.ReceitaComponenteId, c.PorcoesDesejadas));
        return ExpansorReceita.Consolidar(linhas);
    }

    private static IngredienteExpandidoDTO MapearParaExpandido(ReceitaIngredienteDTO i) => new()
    {
        IngredienteId   = i.IngredienteId,
        NomeIngrediente = i.NomeIngrediente,
        Quantidade      = i.Quantidade,
        UnidadeMedidaId = i.UnidadeMedidaId,
        CodigoUnidade   = i.CodigoUnidade,
        NomeUnidade     = i.NomeUnidade,
    };

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

        // Valida componentes (auto-referência / ciclo) antes de persistir.
        if (dto.Componentes.Count > 0)
        {
            var grafo = await _repo.ObterGrafoReceitasAsync(ct);
            foreach (var compId in dto.Componentes.Select(c => c.ReceitaComponenteId).Distinct())
            {
                if (compId == entidade.Id && entidade.Id != 0)
                    return ResultadoOperacao<int>.Falha("Uma receita não pode ser componente de si mesma.");
                if (entidade.Id != 0 && ExpansorReceita.CriariaCiclo(grafo, entidade.Id, compId))
                    return ResultadoOperacao<int>.Falha("Esse componente criaria um ciclo (ele já usa este prato).");
            }
        }

        MapearDtoParaEntidade(dto, entidade);
        AplicarFoto(dto, entidade);
        SincronizarIngredientes(dto, entidade);
        SincronizarComponentes(dto, entidade);

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

        // Bloqueia se a receita ainda é usada como componente de outro prato.
        var pais = await _repo.ObterPaisQueUsamAsync(id, ct);
        if (pais.Count > 0)
            return ResultadoOperacao.Falha(
                $"Esta receita é usada como componente em: {string.Join(", ", pais)}. " +
                "Remova-a desses pratos antes de excluir.");

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
            Foto               = original.Foto,
            FotoContentType    = original.FotoContentType,
            FotoAtualizadaEm   = original.Foto is null ? null : DateTime.UtcNow,
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

    // Uma foto por receita: nova foto substitui a anterior; remoção limpa o blob;
    // sem alteração mantém o que já estava gravado (não recarrega/reescreve o blob).
    private static void AplicarFoto(ReceitaPersistenciaDTO dto, Receita entidade)
    {
        if (dto.RemoverFoto)
        {
            entidade.Foto = null;
            entidade.FotoContentType = null;
            entidade.FotoAtualizadaEm = null;
        }
        else if (dto.FotoConteudo is { Length: > 0 })
        {
            entidade.Foto = dto.FotoConteudo;
            entidade.FotoContentType = dto.FotoContentType;
            entidade.FotoAtualizadaEm = DateTime.UtcNow;
        }
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
        // O índice único (ReceitaId, IngredienteId) impede o mesmo ingrediente duas
        // vezes na receita. Como a consolidação faz variantes ("cebola picada") caírem
        // no mesmo ingrediente, juntamos linhas duplicadas antes de persistir.
        var linhas = ConsolidarDuplicados(dto.Ingredientes);

        // Marca como deletados os que foram removidos
        var idsNoDto = linhas.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();
        foreach (var ri in entidade.Ingredientes.Where(i => !i.IsDeleted && !idsNoDto.Contains(i.Id)))
            ri.IsDeleted = true;

        // Atualiza existentes e insere novos
        foreach (var dtoRi in linhas)
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

    private static void SincronizarComponentes(ReceitaPersistenciaDTO dto, Receita entidade)
    {
        // Dedup por componente (índice único impede o mesmo componente duas vezes).
        var linhas = dto.Componentes
            .Where(c => c.ReceitaComponenteId > 0 && c.ReceitaComponenteId != entidade.Id)
            .GroupBy(c => c.ReceitaComponenteId)
            .Select(g => g.OrderByDescending(c => c.Id > 0).ThenBy(c => c.Ordem).First())
            .ToList();

        var idsNoDto = linhas.Where(c => c.Id > 0).Select(c => c.Id).ToHashSet();
        foreach (var rc in entidade.Componentes.Where(c => !c.IsDeleted && !idsNoDto.Contains(c.Id)))
            rc.IsDeleted = true;

        foreach (var dtoRc in linhas)
        {
            var porcoes = Math.Max(1, dtoRc.PorcoesDesejadas);
            if (dtoRc.Id > 0)
            {
                var existente = entidade.Componentes.FirstOrDefault(c => c.Id == dtoRc.Id);
                if (existente is not null)
                {
                    existente.ReceitaComponenteId = dtoRc.ReceitaComponenteId;
                    existente.PorcoesDesejadas    = porcoes;
                    existente.Ordem               = dtoRc.Ordem;
                }
            }
            else
            {
                entidade.Componentes.Add(new ReceitaComponente
                {
                    ReceitaComponenteId = dtoRc.ReceitaComponenteId,
                    PorcoesDesejadas    = porcoes,
                    Ordem               = dtoRc.Ordem,
                });
            }
        }
    }

    // Junta linhas do mesmo ingrediente numa só (soma quantidades de mesma unidade,
    // concatena observações). Linhas sem ingrediente selecionado são descartadas.
    private static List<ReceitaIngredientePersistenciaDTO> ConsolidarDuplicados(
        IEnumerable<ReceitaIngredientePersistenciaDTO> linhas)
    {
        var resultado = new List<ReceitaIngredientePersistenciaDTO>();

        foreach (var grupo in linhas.Where(l => l.IngredienteId > 0)
                                     .GroupBy(l => l.IngredienteId))
        {
            // Sobrevivente: prefere uma linha já persistida (Id > 0) para reaproveitar a PK.
            var sobrevivente = grupo.OrderByDescending(l => l.Id > 0)
                                    .ThenBy(l => l.Ordem)
                                    .First();

            foreach (var outro in grupo)
            {
                if (ReferenceEquals(outro, sobrevivente)) continue;
                if (outro.UnidadeMedidaId == sobrevivente.UnidadeMedidaId)
                    sobrevivente.Quantidade += outro.Quantidade;
            }

            var obs = string.Join("; ", grupo.Select(l => l.Observacao)
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o!.Trim())
                .Distinct());
            if (!string.IsNullOrWhiteSpace(obs)) sobrevivente.Observacao = obs;

            resultado.Add(sobrevivente);
        }

        return resultado;
    }
}
