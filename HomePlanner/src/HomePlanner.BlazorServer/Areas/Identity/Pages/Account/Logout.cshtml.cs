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

    public async Task<IActionResult> OnGetAsync() => await SairAsync();
    public async Task<IActionResult> OnPostAsync() => await SairAsync();

    private async Task<IActionResult> SairAsync()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/");
    }
}
