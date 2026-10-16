using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Resources.HomePlanner;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages;

/// <summary>
/// Base das páginas de conta (login, registro...): expõe o localizador do resx compartilhado
/// para as mensagens montadas no código (ModelState, avisos), no idioma da requisição.
/// </summary>
public abstract class PaginaLocalizada : PageModel
{
    protected IStringLocalizer L =>
        HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
}
