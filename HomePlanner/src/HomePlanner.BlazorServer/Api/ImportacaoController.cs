using Application.HomePlanner.Common;
using Application.HomePlanner.Services.Cardapio;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Importação de receitas: por URL, parsing de ingredientes (IA/regex) e cota do plano.</summary>
[Route("api/[controller]")]
public class ImportacaoController : ApiControllerBase
{
    private readonly IImportadorReceitaService _importador;
    private readonly ICotaImportacaoService _cota;
    private readonly IReceitaService _receitas;

    public ImportacaoController(IImportadorReceitaService importador, ICotaImportacaoService cota,
        IReceitaService receitas)
    {
        _importador = importador;
        _cota = cota;
        _receitas = receitas;
    }

    /// <summary>Uso atual da cota mensal de importação (usado/limite).</summary>
    [HttpGet("cota")]
    public async Task<IActionResult> Cota(CancellationToken ct)
        => Ok(await _cota.ObterUsoAsync(ct));

    /// <summary>Pré-check: o tenant ainda pode importar receitas este mês?</summary>
    [HttpGet("pode-importar")]
    public async Task<IActionResult> PodeImportar(CancellationToken ct)
        => Responder(await _cota.VerificarPodeImportarAsync(ct));

    /// <summary>Importa uma receita a partir de uma URL, devolvendo um preview para revisão.</summary>
    [HttpPost("url")]
    public async Task<IActionResult> ImportarUrl([FromBody] ImportarUrlRequest req, CancellationToken ct)
    {
        // Chamadas de IA têm custo: Filho não importa (nem poderia salvar o resultado).
        if (!await _receitas.PodeGerenciarAsync())
            return Responder(ResultadoOperacao.Falha(ErrosApp.SemPermissaoReceitas));
        return Responder(await _importador.ImportarDeUrlAsync(req.Url, ct));
    }

    /// <summary>Parseia um texto livre de ingredientes (IA quando habilitada, senão regex).</summary>
    [HttpPost("parsear")]
    public async Task<IActionResult> Parsear([FromBody] ParsearTextoRequest req, CancellationToken ct)
    {
        if (!await _receitas.PodeGerenciarAsync())
            return Responder(ResultadoOperacao.Falha(ErrosApp.SemPermissaoReceitas));
        return Ok(await _importador.ParsearTextoAsync(req.Texto, req.IdiomaAlvo, ct));
    }

    public record ImportarUrlRequest(string Url);
    public record ParsearTextoRequest(string? Texto, string? IdiomaAlvo);
}
