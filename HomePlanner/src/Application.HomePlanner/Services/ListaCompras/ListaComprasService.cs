using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Ingrediente;
using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.Services.Cardapio;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Domain.HomePlanner.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.ListaCompras;

public class ListaComprasService : IListaComprasService
{
    private readonly IPlanejamentoSemanalRepository _cardapioRepo;
    private readonly IReceitaRepository _receitaRepo;
    private readonly IUnidadeMedidaRepository _unidadeRepo;
    private readonly IIngredienteRepository _ingredienteRepo;
    private readonly IMarcacaoCompraRepository _marcacaoRepo;
    private readonly IListaCompraRepository _listaRepo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly ILogger<ListaComprasService> _logger;

    public ListaComprasService(
        IPlanejamentoSemanalRepository cardapioRepo,
        IReceitaRepository receitaRepo,
        IUnidadeMedidaRepository unidadeRepo,
        IIngredienteRepository ingredienteRepo,
        IMarcacaoCompraRepository marcacaoRepo,
        IListaCompraRepository listaRepo,
        TenantContextAccessor tenantAccessor,
        ILogger<ListaComprasService> logger)
    {
        _cardapioRepo = cardapioRepo;
        _receitaRepo  = receitaRepo;
        _unidadeRepo  = unidadeRepo;
        _ingredienteRepo = ingredienteRepo;
        _marcacaoRepo = marcacaoRepo;
        _listaRepo    = listaRepo;
        _tenantAccessor = tenantAccessor;
        _logger = logger;
    }

    public async Task<ResultadoOperacao<ListaComprasDTO>> CalcularDaSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        // Sem cardápio na semana não é erro: a lista fica vazia e os pedidos avulsos
        // (lista pessoal / de membros) continuam aparecendo normalmente.
        var cardapio = await _cardapioRepo.ObterCardapioSemanaAsync(dataInicio, ct);

        var refeicoes = cardapio?.Refeicoes.Where(r => r.ReceitaId.HasValue).ToList() ?? [];
        if (!refeicoes.Any())
            return ResultadoOperacao<ListaComprasDTO>.Ok(new ListaComprasDTO
            {
                DataInicio = dataInicio,
                TotalReceitas = 0,
            });

        // Carrega todas as unidades de medida uma única vez
        var unidades = (await _unidadeRepo.ListarAtivasAsync(ct))
            .ToDictionary(u => u.Id);

        // Grafo de receitas (carregado 1×) para expandir pratos compostos.
        var grafo = await _receitaRepo.ObterGrafoReceitasAsync(ct);

        // 1ª passada: acumula por IngredienteId (antes de resolver o produto base).
        var porIngrediente = new Dictionary<int, ItemAcumulado>();

        var receitasProcessadas = new HashSet<int>();

        foreach (var refeicao in refeicoes)
        {
            var receitaId = refeicao.ReceitaId!.Value;
            if (!grafo.PorcoesBase.TryGetValue(receitaId, out var porcoesBase)) continue;

            receitasProcessadas.Add(receitaId);

            var porcoes = refeicao.PorcoesDesejadas ?? porcoesBase;
            if (porcoes <= 0) porcoes = porcoesBase;

            // Expande a receita (própria + componentes), já escalada para as porções.
            foreach (var ing in ExpansorReceita.Expandir(grafo, receitaId, porcoes))
            {
                if (!unidades.TryGetValue(ing.UnidadeMedidaId, out var unidade))
                {
                    _logger.LogWarning("Unidade {Id} não encontrada para ingrediente {Nome}.",
                        ing.UnidadeMedidaId, ing.NomeIngrediente);
                    continue;
                }

                // Converte para unidade base (g, ml ou un)
                var qtdBase = ing.Quantidade * unidade.FatorParaBase;

                if (!porIngrediente.TryGetValue(ing.IngredienteId, out var item))
                {
                    item = new ItemAcumulado
                    {
                        IngredienteId = ing.IngredienteId,
                        Nome          = ing.NomeIngrediente,
                        Tipo          = unidade.Tipo,
                        QtdBase       = 0,
                    };
                    porIngrediente[ing.IngredienteId] = item;
                }

                // Agrega mesmo que a unidade de medida seja diferente (soma na base).
                item.QtdBase += qtdBase;
            }
        }

        // 2ª passada: consolida variantes sob o produto base de compra.
        var mapaBase = await _ingredienteRepo.ObterMapaBaseAsync(porIngrediente.Keys.ToList(), ct);
        var acumulador = new Dictionary<int, ItemAcumulado>();
        foreach (var (id, item) in porIngrediente)
        {
            var info = mapaBase.TryGetValue(id, out var b) ? b : new(id, item.Nome);
            if (!acumulador.TryGetValue(info.BaseId, out var alvo))
            {
                alvo = new ItemAcumulado
                {
                    IngredienteId = info.BaseId,
                    Nome          = info.BaseNome,
                    Tipo          = item.Tipo,
                    QtdBase       = 0,
                };
                acumulador[info.BaseId] = alvo;
            }
            alvo.QtdBase += item.QtdBase;
        }

        // Loja padrão (preferência) de cada ingrediente para já posicionar o item na loja certa.
        var preferencias = await _listaRepo.ObterPreferenciasAsync(ct);

        var itens = acumulador.Values
            .OrderBy(i => i.Nome)
            .Select(a =>
            {
                var (qtd, codigo, nome) = MelhorUnidade(a.QtdBase, a.Tipo);
                return new ItemComprasDTO
                {
                    IngredienteId   = a.IngredienteId,
                    NomeIngrediente = a.Nome,
                    Quantidade      = qtd,
                    CodigoUnidade   = codigo,
                    NomeUnidade     = nome,
                    Tipo            = a.Tipo,
                    ListaId         = preferencias.TryGetValue(a.IngredienteId, out var lid) ? lid : null,
                };
            })
            .ToList();

        return ResultadoOperacao<ListaComprasDTO>.Ok(new ListaComprasDTO
        {
            DataInicio    = dataInicio,
            TotalReceitas = receitasProcessadas.Count,
            Itens         = itens,
        });
    }

    public async Task<IReadOnlyDictionary<int, bool>> ObterMarcacoesSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        try
        {
            return await _marcacaoRepo.ObterDaSemanaAsync(dataInicio, ct);
        }
        catch (Exception ex)
        {
            // Não derruba a lista de compras se a tabela/migração ainda não existir no ambiente.
            _logger.LogError(ex, "Falha ao obter marcações da semana {Semana} — seguindo sem marcações.", dataInicio);
            return new Dictionary<int, bool>();
        }
    }

    public async Task<ResultadoOperacao> MarcarItemAsync(
        DateOnly dataInicio, int ingredienteId, bool comprado, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        try
        {
            var existente = await _marcacaoRepo.ObterAsync(dataInicio, ingredienteId, ct);
            if (existente is null)
            {
                await _marcacaoRepo.AdicionarAsync(new MarcacaoCompra
                {
                    DataInicioSemana = dataInicio,
                    IngredienteId    = ingredienteId,
                    Comprado         = comprado,
                    DataCompra       = comprado ? DateTime.UtcNow : null,
                }, ct);
            }
            else
            {
                existente.Comprado   = comprado;
                existente.DataCompra = comprado ? DateTime.UtcNow : null;
            }

            await _marcacaoRepo.SalvarAsync(ct);
            return ResultadoOperacao.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao marcar ingrediente {Id} na semana {Semana}.", ingredienteId, dataInicio);
            return ResultadoOperacao.Falha(ErrosApp.FalhaSalvarMarcacao);
        }
    }

    public async Task<ResultadoOperacao> LimparMarcacoesSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        try
        {
            var marcacoes = await _marcacaoRepo.ListarRastreadasDaSemanaAsync(dataInicio, ct);
            foreach (var m in marcacoes.Where(m => m.Comprado))
            {
                m.Comprado   = false;
                m.DataCompra = null;
            }
            await _marcacaoRepo.SalvarAsync(ct);
            return ResultadoOperacao.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao limpar marcações da semana {Semana}.", dataInicio);
            return ResultadoOperacao.Falha(ErrosApp.FalhaLimparMarcacoes);
        }
    }

    // Converte qtdBase para a unidade mais legível
    private static (decimal qtd, string codigo, string nome) MelhorUnidade(
        decimal qtdBase, TipoUnidadeMedida tipo)
    {
        return tipo switch
        {
            TipoUnidadeMedida.Massa when qtdBase >= 1000
                => (Math.Round(qtdBase / 1000, 3), "kg", "kg"),
            TipoUnidadeMedida.Massa
                => (Math.Round(qtdBase, 0), "g", "g"),
            TipoUnidadeMedida.Volume when qtdBase >= 1000
                => (Math.Round(qtdBase / 1000, 3), "l", "l"),
            TipoUnidadeMedida.Volume
                => (Math.Round(qtdBase, 0), "ml", "ml"),
            _   => (Math.Round(qtdBase, 2), "un", "un"),
        };
    }

    private class ItemAcumulado
    {
        public int IngredienteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public TipoUnidadeMedida Tipo { get; set; }
        public decimal QtdBase { get; set; }
    }
}
