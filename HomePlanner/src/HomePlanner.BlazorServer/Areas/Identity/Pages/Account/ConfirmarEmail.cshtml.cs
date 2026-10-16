using System.Text;
using Application.HomePlanner.Middleware;
using Domain.HomePlanner.Models.SaaS.Identity;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages.Account;

public class ConfirmarEmailModel : PaginaLocalizada
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
            Mensagem = L["Conta_LinkInvalido"];
            return;
        }

        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (usuario is null)
        {
            Mensagem = L["Conta_UsuarioNaoEncontrado"];
            return;
        }

        if (usuario.EmailConfirmed)
        {
            Sucesso = true;
            Mensagem = L["Conta_EmailJaConfirmado"];
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
                Mensagem = L["Conta_EmailConfirmadoOk"];
            }
            else
            {
                Mensagem = L["Conta_EmailNaoConfirmado"];
            }
        }
        catch
        {
            Mensagem = L["Conta_LinkInvalidoExpirado"];
        }
    }
}
