using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages.Account;

public class VerificarEmailModel : PaginaLocalizada
{
    public string Email { get; set; } = string.Empty;

    public void OnGet(string? email) => Email = email ?? string.Empty;
}
