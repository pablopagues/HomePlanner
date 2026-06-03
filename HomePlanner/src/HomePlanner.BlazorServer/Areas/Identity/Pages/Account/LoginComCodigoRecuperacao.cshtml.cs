using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.HomePlanner.Middleware;
using Domain.HomePlanner.Models.SaaS.Identity;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginComCodigoRecuperacaoModel : PageModel
{
    private readonly SignInManager<Usuario> _signInManager;
    private readonly HomePlannerDbContext _db;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<LoginComCodigoRecuperacaoModel> _logger;

    public LoginComCodigoRecuperacaoModel(
        SignInManager<Usuario> signInManager,
        HomePlannerDbContext db,
        TenantContext tenantContext,
        ILogger<LoginComCodigoRecuperacaoModel> logger)
    {
        _signInManager = signInManager;
        _db = db;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "Informe um código de recuperação.")]
    public string CodigoRecuperacao { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        ReturnUrl ??= Url.Content("~/dashboard");
        var usuario = await ObterUsuario2FAAsync();
        if (usuario is null) return RedirectToPage("Login");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl ??= Url.Content("~/dashboard");

        var usuario = await ObterUsuario2FAAsync();
        if (usuario is null) return RedirectToPage("Login");

        if (!ModelState.IsValid) return Page();

        var codigoLimpo = CodigoRecuperacao.Replace(" ", string.Empty).Replace("-", string.Empty);
        var resultado = await _signInManager.TwoFactorRecoveryCodeSignInAsync(codigoLimpo);

        if (resultado.Succeeded)
        {
            usuario.UltimoLogin = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Login 2FA (código de recuperação) bem-sucedido: {Email}", usuario.Email);
            return LocalRedirect(ReturnUrl);
        }

        if (resultado.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty,
                "Conta bloqueada temporariamente após várias tentativas. Tente novamente em 15 minutos.");
            return Page();
        }

        _logger.LogWarning("Código de recuperação inválido para {Email}", usuario.Email);
        ModelState.AddModelError(string.Empty, "Código de recuperação inválido.");
        return Page();
    }

    private async Task<Usuario?> ObterUsuario2FAAsync()
    {
        var twoFactorAuth = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
        if (!twoFactorAuth.Succeeded) return null;

        var userId = twoFactorAuth.Principal?.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrEmpty(userId)) return null;

        var usuario = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (usuario is not null)
            _tenantContext.Definir(usuario.TenantId, usuario.Id, usuario.NomeCompleto);

        return usuario;
    }
}
