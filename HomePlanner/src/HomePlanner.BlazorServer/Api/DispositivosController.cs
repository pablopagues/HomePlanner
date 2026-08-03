using Application.HomePlanner.DTOs.Notificacoes;
using Application.HomePlanner.Services.Notificacoes;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Registro de aparelhos para push nativo (FCM) dos apps MAUI.</summary>
public class DispositivosController : ApiControllerBase
{
    private readonly IDispositivoPushService _dispositivos;

    public DispositivosController(IDispositivoPushService dispositivos) => _dispositivos = dispositivos;

    /// <summary>Indica se o push nativo (FCM) está habilitado no servidor.</summary>
    [HttpGet("status")]
    public IActionResult Status() => Ok(new { habilitado = _dispositivos.Habilitado });

    /// <summary>Registra (ou reativa) o token FCM do aparelho para o usuário atual.</summary>
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarDispositivoDTO dto, CancellationToken ct)
        => Responder(await _dispositivos.RegistrarAsync(dto, ct));

    /// <summary>Remove o token do aparelho (logout/desativação do push neste aparelho).</summary>
    [HttpDelete]
    public async Task<IActionResult> Remover([FromBody] RegistrarDispositivoDTO dto, CancellationToken ct)
    {
        await _dispositivos.RemoverAsync(dto.Token, ct);
        return NoContent();
    }
}
