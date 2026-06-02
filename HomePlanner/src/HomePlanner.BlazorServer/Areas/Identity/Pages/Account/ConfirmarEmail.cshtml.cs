using System.Text;
using Application.HomePlanner.Middleware;
using Domain.HomePlanner.Models.SaaS.Identity;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages.Account;

public class ConfirmarEmailModel : PageModel
{
    private readonly UserManager<Usuario> _userManager;
    private readonly TenantContext _tenantContext;
    private readonly HomePlannerDbContext _db;

    public ConfirmarEmailModel(
        UserManager<Usuario> userManager,
        TenantContext tenantContext,
        HomePlannerDbContext db)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
        _db = db;
    }

    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;

    public async Task OnGetAsync(string? userId, string? token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            Mensagem = "Link de confirmação inválido.";
            return;
        }

        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (usuario is null)
        {
            Mensagem = "Usuário não encontrado.";
            return;
        }

        if (usuario.EmailConfirmed)
        {
            Sucesso = true;
            Mensagem = "Seu e-mail já estava confirmado. Você pode entrar normalmente.";
            return;
        }

        _tenantContext.Definir(usuario.TenantId, usuario.Id, usuario.NomeCompleto);

        try
        {
            var tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var resultado = await _userManager.ConfirmEmailAsync(usuario, tokenDecodificado);
            if (resultado.Succeeded)
            {
                Sucesso = true;
                Mensagem = "E-mail confirmado com sucesso! Bem-vindo(a) ao HomePlanner.";
            }
            else
            {
                Mensagem = "Não foi possível confirmar o e-mail. O link pode ter expirado.";
            }
        }
        catch
        {
            Mensagem = "Link de confirmação inválido ou expirado.";
        }
    }
}
