using System.ComponentModel.DataAnnotations;
using Application.HomePlanner.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages.Account;

/// <summary>
/// Só coleta o e-mail — as regras (throttle, resposta idêntica, quem recebe o link)
/// vivem no <see cref="ISenhaService"/>, compartilhado com a API do app. Duplicar
/// aqui faria a web escapar do throttle.
/// </summary>
public class EsqueciSenhaModel : PageModel
{
    private readonly ISenhaService _senha;

    public EsqueciSenhaModel(ISenhaService senha) => _senha = senha;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Enviado { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        await _senha.SolicitarResetAsync(Input.Email, $"{Request.Scheme}://{Request.Host}");

        // Sempre mostra sucesso — não revela se o e-mail existe.
        Enviado = true;
        return Page();
    }
}
