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
        => r.Sucesso ? NoContent() : BadRequest(new { erros = r.Erros });

    /// <summary>Resultado com payload → 200 no sucesso, 400 com a lista de erros na falha.</summary>
    protected IActionResult Responder<T>(ResultadoOperacao<T> r)
        => r.Sucesso ? Ok(r.Valor) : BadRequest(new { erros = r.Erros });
}
