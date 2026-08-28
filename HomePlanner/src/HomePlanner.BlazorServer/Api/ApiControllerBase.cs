using Application.HomePlanner.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>
/// Base dos controllers da API mobile. Exige token JWT (esquema Bearer) e
/// centraliza o mapeamento de <see cref="ResultadoOperacao"/> para respostas HTTP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Resultado sem payload → 204 no sucesso, 400 com a lista de erros na falha.</summary>
    protected IActionResult Responder(ResultadoOperacao r)
        => r.Sucesso ? NoContent() : BadRequest(CorpoErro(r.Erros));

    /// <summary>Resultado com payload → 200 no sucesso, 400 com a lista de erros na falha.</summary>
    protected IActionResult Responder<T>(ResultadoOperacao<T> r)
        => r.Sucesso ? Ok(r.Valor) : BadRequest(CorpoErro(r.Erros));

    /// <summary>
    /// Corpo padrão de erro da API.
    ///
    /// <c>erros</c> leva o código e os argumentos — é o contrato: o cliente traduz.
    /// <c>mensagens</c> repete o texto padrão em português, para clientes antigos e
    /// para códigos que o cliente ainda não conheça. Sem ele, um app desatualizado
    /// mostraria o código cru.
    /// </summary>
    private static object CorpoErro(IReadOnlyList<ErroOperacao> erros) => new
    {
        erros = erros.Select(e => new
        {
            codigo = e.Externo ? null : e.Codigo,
            args = e.Args,
            texto = e.TextoPadrao,
        }),
        mensagens = erros.Select(e => e.TextoPadrao),
    };
}
