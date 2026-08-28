using System.ComponentModel.DataAnnotations;
using System.Text;
using Application.HomePlanner.Middleware;
using Domain.HomePlanner.Models.SaaS.Identity;
using Infrastructure.HomePlanner.Contexts;
using Infrastructure.HomePlanner.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages.Account;

public class RedefinirSenhaModel : PageModel
{
    private readonly UserManager<Usuario> _userManager;
    private readonly TenantContext _tenantContext;
    private readonly HomePlannerDbContext _db;

    public RedefinirSenhaModel(
        UserManager<Usuario> userManager,
        TenantContext tenantContext,
        HomePlannerDbContext db)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Concluido { get; set; }

    public class InputModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        /// <summary>True quando o link veio de um convite de membro (token do provider de convite,
        /// validade longa) e não do fluxo de "esqueci minha senha".</summary>
        public bool Convite { get; set; }

        [Required(ErrorMessage = "Informe uma senha.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter ao menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(Senha), ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmacaoSenha { get; set; } = string.Empty;
    }

    public IActionResult OnGet(string? userId, string? token, bool convite = false)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return RedirectToPage("./Login");

        Input.UserId = userId;
        Input.Token = token;
        Input.Convite = convite;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == Input.UserId);
        if (usuario is null)
        {
            // Não revela existência — mostra concluído mesmo assim
            Concluido = true;
            return Page();
        }

        _tenantContext.Definir(usuario.TenantId, usuario.Id, usuario.NomeCompleto);

        var tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Token));

        var resultado = Input.Convite
            ? await AceitarConviteAsync(usuario, tokenDecodificado)
            : await _userManager.ResetPasswordAsync(usuario, tokenDecodificado, Input.Senha);

        if (resultado.Succeeded)
        {
            Concluido = true;
            return Page();
        }

        foreach (var erro in resultado.Errors)
            ModelState.AddModelError(string.Empty, erro.Description);
        return Page();
    }

    /// <summary>
    /// Primeiro acesso de um membro convidado: valida o token do provider de convite (que o
    /// ResetPasswordAsync não conhece) e define a senha inicial. Abrir o link comprova o controle
    /// da caixa de e-mail, então o endereço já entra confirmado.
    /// </summary>
    private async Task<IdentityResult> AceitarConviteAsync(Usuario usuario, string token)
    {
        var valido = await _userManager.VerifyUserTokenAsync(
            usuario, ConviteToken.Provider, ConviteToken.Proposito, token);

        if (!valido)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "ConviteInvalido",
                Description = "Este convite expirou ou já foi usado. Peça um novo convite a quem administra a família.",
            });

        if (usuario.PasswordHash is not null)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "ContaJaAtivada",
                Description = "Esta conta já tem senha definida. Use \"Esqueci minha senha\" na tela de login.",
            });

        // AddPasswordAsync renova o SecurityStamp, o que invalida qualquer outro convite pendente.
        var resultado = await _userManager.AddPasswordAsync(usuario, Input.Senha);
        if (!resultado.Succeeded) return resultado;

        usuario.EmailConfirmed = true;
        await _userManager.UpdateAsync(usuario);
        return IdentityResult.Success;
    }
}
