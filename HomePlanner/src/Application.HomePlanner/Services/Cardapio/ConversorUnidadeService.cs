using Application.HomePlanner.Common;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Services.Cardapio;

public class ConversorUnidadeService : IConversorUnidadeService
{
    private readonly IUnidadeMedidaRepository _repo;

    public ConversorUnidadeService(IUnidadeMedidaRepository repo) => _repo = repo;

    public bool SaoCompativeis(UnidadeMedida origem, UnidadeMedida destino)
        => origem.Tipo == destino.Tipo;

    public decimal Converter(decimal quantidade, UnidadeMedida origem, UnidadeMedida destino)
    {
        if (!SaoCompativeis(origem, destino))
            throw new InvalidOperationException(
                $"Não é possível converter de {origem.Codigo} ({origem.Tipo}) para {destino.Codigo} ({destino.Tipo}).");

        // Converte para unidade base, depois para destino
        var emBase = quantidade * origem.FatorParaBase;
        return emBase / destino.FatorParaBase;
    }

    public async Task<ResultadoOperacao<decimal>> ConverterAsync(
        decimal quantidade, int unidadeOrigemId, int unidadeDestinoId, CancellationToken ct = default)
    {
        var origem = await _repo.ObterPorIdAsync(unidadeOrigemId, ct);
        if (origem is null)
            return ResultadoOperacao<decimal>.Falha(ErrosApp.UnidadeNaoEncontrada(unidadeOrigemId));

        var destino = await _repo.ObterPorIdAsync(unidadeDestinoId, ct);
        if (destino is null)
            return ResultadoOperacao<decimal>.Falha(ErrosApp.UnidadeNaoEncontrada(unidadeDestinoId));

        if (!SaoCompativeis(origem, destino))
            return ResultadoOperacao<decimal>.Falha(ErrosApp.UnidadesIncompativeis(origem.Codigo, destino.Codigo));

        return ResultadoOperacao<decimal>.Ok(Converter(quantidade, origem, destino));
    }
}
