using Domain.HomePlanner.Models.SaaS.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages.Account;

[IgnoreAntiforgeryToken]
public class LogoutModel : PageModel
{
    private readonly SignInManager<Usuario> _signInManager;

    public LogoutModel(SignInManager<Usuario> signInManager)
        => _signInManager = signInManager;

    public async Task<IActionResult> OnGetAsync(string? paraLogin = null) => await SairAsync(paraLogin);
    public async Task<IActionResult> OnPostAsync(string? paraLogin = null) => await SairAsync(paraLogin);

    private async Task<IActionResult> SairAsync(string? paraLogin)
    {
        await _signInManager.SignOutAsync();
        // Saída do Modo Mural por recarga do tablet: vai direto para o login.
        return Redirect(paraLogin == "1" ? "/Identity/Account/Login" : "/");
    }
}
