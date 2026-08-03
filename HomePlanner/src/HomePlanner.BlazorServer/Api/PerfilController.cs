using Application.HomePlanner.Services.Perfil;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Perfil do usuário atual: foto (upload, remover, servir) autorizada por JWT.</summary>
public class PerfilController : ApiControllerBase
{
    private readonly IFotoUsuarioService _foto;

    public PerfilController(IFotoUsuarioService foto) => _foto = foto;

    /// <summary>Token de versão da foto atual (null se não houver) — útil para cache-busting.</summary>
    [HttpGet("foto/versao")]
    public async Task<IActionResult> Versao(CancellationToken ct)
        => Ok(new { versao = await _foto.ObterVersaoFotoAsync(ct) });

    /// <summary>Conteúdo binário da foto do usuário atual.</summary>
    [HttpGet("foto")]
    public async Task<IActionResult> MinhaFoto(CancellationToken ct)
    {
        var foto = await _foto.ObterConteudoFotoAsync(ct);
        if (foto is null) return NotFound();
        return File(foto.Conteudo, foto.ContentType, lastModified: null,
            entityTag: new EntityTagHeaderValue($"\"{foto.Versao}\""));
    }

    /// <summary>Conteúdo binário da foto de um membro da mesma família.</summary>
    [HttpGet("foto/{usuarioId}")]
    public async Task<IActionResult> FotoMembro(string usuarioId, CancellationToken ct)
    {
        var foto = await _foto.ObterConteudoFotoAsync(usuarioId, ct);
        if (foto is null) return NotFound();
        return File(foto.Conteudo, foto.ContentType, lastModified: null,
            entityTag: new EntityTagHeaderValue($"\"{foto.Versao}\""));
    }

    /// <summary>Envia (ou substitui) a foto de perfil. multipart/form-data, campo "arquivo". Devolve o novo token de versão.</summary>
    [HttpPost("foto")]
    public async Task<IActionResult> Upload(IFormFile arquivo, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(new { erros = new[] { "Nenhum arquivo enviado." } });

        using var ms = new MemoryStream();
        await arquivo.CopyToAsync(ms, ct);

        var r = await _foto.AtualizarFotoAsync(ms.ToArray(), arquivo.ContentType, ct);
        return r.Sucesso ? Ok(new { versao = r.Valor }) : BadRequest(new { erros = r.Erros });
    }

    /// <summary>Remove a foto de perfil do usuário atual.</summary>
    [HttpDelete("foto")]
    public async Task<IActionResult> Remover(CancellationToken ct)
        => Responder(await _foto.RemoverFotoAsync(ct));
}
