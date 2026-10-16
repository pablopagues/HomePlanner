using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

public class SugestaoReceitaService : ISugestaoReceitaService
{
    // Teto de nomes de receitas da casa enviados como contexto (custo do prompt).
    private const int MaxReceitasContexto = 150;

    private readonly IAssistenteReceitaIA _assistenteIA;
    private readonly ICotaImportacaoService _cota;
    private readonly IReceitaService _receitas;

    public SugestaoReceitaService(
        IAssistenteReceitaIA assistenteIA,
        ICotaImportacaoService cota,
        IReceitaService receitas)
    {
        _assistenteIA = assistenteIA;
        _cota = cota;
        _receitas = receitas;
    }

    public async Task<ResultadoOperacao<IReadOnlyList<ReceitaImportadaPreviewDTO>>> SugerirAsync(
        PedidoSugestaoReceitaDTO pedido, string idioma, CancellationToken ct = default)
    {
        // Gera custo de IA e consome a cota da família: só quem pode criar receitas.
        if (!await _receitas.PodeGerenciarAsync())
            return ResultadoOperacao<IReadOnlyList<ReceitaImportadaPreviewDTO>>.Falha(ErrosApp.SemPermissaoReceitas);

        if (!_assistenteIA.Habilitado)
            return ResultadoOperacao<IReadOnlyList<ReceitaImportadaPreviewDTO>>.Falha(ErrosApp.IaIndisponivel);

        var cota = await _cota.VerificarPodeSugerirAsync(ct);
        if (!cota.Sucesso)
            return ResultadoOperacao<IReadOnlyList<ReceitaImportadaPreviewDTO>>.Falha(cota.Erros.ToArray());

        var throttle = await _cota.VerificarThrottleIAAsync(ct);
        if (!throttle.Sucesso)
            return ResultadoOperacao<IReadOnlyList<ReceitaImportadaPreviewDTO>>.Falha(throttle.Erros.ToArray());

        IReadOnlyList<string> daCasa = [];
        if (pedido.ConsiderarReceitasDaCasa)
        {
            var lista = await _receitas.ListarAsync(
                new ReceitaFiltroDTO { Pagina = 1, TamanhoPagina = MaxReceitasContexto }, ct);
            daCasa = lista.Itens.Select(r => r.Nome).ToList();
        }

        var sugestoes = await _assistenteIA.SugerirAsync(pedido, daCasa, idioma, ct);
        if (sugestoes is null || sugestoes.Count == 0)
            return ResultadoOperacao<IReadOnlyList<ReceitaImportadaPreviewDTO>>.Falha(ErrosApp.SugestoesNaoGeradas);

        // O custo está na geração: conta 1 pedido na cota mensal só quando deu certo.
        await _cota.RegistrarSugestaoAsync(ct);
        return ResultadoOperacao<IReadOnlyList<ReceitaImportadaPreviewDTO>>.Ok(sugestoes);
    }
}
