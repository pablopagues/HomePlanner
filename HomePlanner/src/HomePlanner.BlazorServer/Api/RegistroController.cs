using Application.HomePlanner.DTOs.Auth;
using Application.HomePlanner.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Cadastro de uma nova conta/família pelo app (cria Tenant + Owner + trial atomicamente).</summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[Produces("application/json")]
public class RegistroController : ControllerBase
{
    private readonly IRegistroTenantService _registro;

    public RegistroController(IRegistroTenantService registro) => _registro = registro;

    /// <summary>Registra uma nova conta. Em caso de sucesso, o app deve seguir para o login.</summary>
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistroPersistenciaDTO dto, CancellationToken ct)
    {
        var r = await _registro.RegistrarAsync(dto, ct);
        return r.Sucesso
            ? Ok(new { tenantId = r.TenantId, usuarioId = r.UsuarioId })
            : BadRequest(new { erros = r.Erros });
    }
}
