using System.ComponentModel.DataAnnotations;
using Application.HomePlanner.Middleware;
using Domain.HomePlanner.Models.SaaS.Identity;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<Usuario> _signInManager;
    private readonly TenantContext _tenantContext;
    private readonly HomePlannerDbContext _db;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<Usuario> signInManager,
        TenantContext tenantContext,
        HomePlannerDbContext db,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _tenantContext = tenantContext;
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Confirmado { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe sua senha.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        public bool Lembrar { get; set; }
    }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return Page();

        var emailNormalizado = Input.Email.Trim().ToUpperInvariant();

        // Busca global (sem filtro de tenant — ainda não autenticado)
        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == emailNormalizado);

        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return Page();
        }

        // Define o contexto do tenant ANTES do login, para que a fábrica de claims
        // consiga ler os papéis (que têm filtro global por TenantId).
        _tenantContext.Definir(usuario.TenantId, usuario.Id, usuario.NomeCompleto);

        var resultado = await _signInManager.PasswordSignInAsync(
            usuario.UserName!, Input.Senha, Input.Lembrar, lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            usuario.UltimoLogin = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Login bem-sucedido: {Email}", usuario.Email);
            return LocalRedirect(returnUrl ?? "~/dashboard");
        }

        if (resultado.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty,
                "Seu e-mail ainda não foi confirmado. Verifique sua caixa de entrada.");
            return Page();
        }

        if (resultado.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty,
                "Conta temporariamente bloqueada após várias tentativas. Tente novamente em 15 minutos.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
        return Page();
    }
}
