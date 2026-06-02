using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Application.HomePlanner.Services.ListaCompras;

public class ListaComprasService : IListaComprasService
{
    private readonly IPlanejamentoSemanalRepository _cardapioRepo;
    private readonly IReceitaRepository _receitaRepo;
    private readonly IUnidadeMedidaRepository _unidadeRepo;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly ILogger<ListaComprasService> _logger;

    public ListaComprasService(
        IPlanejamentoSemanalRepository cardapioRepo,
        IReceitaRepository receitaRepo,
        IUnidadeMedidaRepository unidadeRepo,
        TenantContextAccessor tenantAccessor,
        ILogger<ListaComprasService> logger)
    {
        _cardapioRepo = cardapioRepo;
        _receitaRepo  = receitaRepo;
        _unidadeRepo  = unidadeRepo;
        _tenantAccessor = tenantAccessor;
        _logger = logger;
    }

    public async Task<ResultadoOperacao<ListaComprasDTO>> CalcularDaSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var cardapio = await _cardapioRepo.ObterCardapioSemanaAsync(dataInicio, ct);
        if (cardapio is null)
            return ResultadoOperacao<ListaComprasDTO>.Falha("Semana não encontrada. Abra o cardápio e crie a semana primeiro.");

        var refeicoes = cardapio.Refeicoes.Where(r => r.ReceitaId.HasValue).ToList();
        if (!refeicoes.Any())
            return ResultadoOperacao<ListaComprasDTO>.Ok(new ListaComprasDTO
            {
                DataInicio = dataInicio,
                TotalReceitas = 0,
            });

        // Carrega todas as unidades de medida uma única vez
        var unidades = (await _unidadeRepo.ListarAtivasAsync(ct))
            .ToDictionary(u => u.Id);

        // Acumulador: chave = IngredienteId
        var acumulador = new Dictionary<int, ItemAcumulado>();

        var receitasProcessadas = new HashSet<int>();

        foreach (var refeicao in refeicoes)
        {
            var receitaDetalhe = await _receitaRepo.ObterDetalheAsync(refeicao.ReceitaId!.Value, ct);
            if (receitaDetalhe is null) continue;

            receitasProcessadas.Add(receitaDetalhe.Id);

            var porcoes = refeicao.PorcoesDesejadas ?? receitaDetalhe.NumeroPorcoesBase;
            if (porcoes <= 0) porcoes = receitaDetalhe.NumeroPorcoesBase;
            var escala = (decimal)porcoes / Math.Max(1, receitaDetalhe.NumeroPorcoesBase);

            foreach (var ing in receitaDetalhe.Ingredientes)
            {
                if (!unidades.TryGetValue(ing.UnidadeMedidaId, out var unidade))
                {
                    _logger.LogWarning("Unidade {Id} não encontrada para ingrediente {Nome}.",
                        ing.UnidadeMedidaId, ing.NomeIngrediente);
                    continue;
                }

                // Converte para unidade base (g, ml ou un)
                var qtdBase = ing.Quantidade * escala * unidade.FatorParaBase;

                if (!acumulador.TryGetValue(ing.IngredienteId, out var item))
                {
                    item = new ItemAcumulado
                    {
                        IngredienteId = ing.IngredienteId,
                        Nome          = ing.NomeIngrediente,
                        Tipo          = unidade.Tipo,
                        QtdBase       = 0,
                    };
                    acumulador[ing.IngredienteId] = item;
                }

                // Agrega mesmo que a unidade de medida seja diferente (mesmo tipo)
                if (item.Tipo == unidade.Tipo)
                    item.QtdBase += qtdBase;
                else
                    // Tipo incompatível (ex: g e un para mesmo ingrediente) — soma como unidade
                    item.QtdBase += qtdBase;
            }
        }

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
