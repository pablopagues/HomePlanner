using System.ComponentModel.DataAnnotations;
using System.Text;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Email;
using Domain.HomePlanner.Models.SaaS.Identity;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages.Account;

public class EsqueciSenhaModel : PageModel
{
    private readonly UserManager<Usuario> _userManager;
    private readonly IEmailService _emailService;
    private readonly TenantContext _tenantContext;
    private readonly HomePlannerDbContext _db;

    public EsqueciSenhaModel(
        UserManager<Usuario> userManager,
        IEmailService emailService,
        TenantContext tenantContext,
        HomePlannerDbContext db)
    {
        _userManager = userManager;
        _emailService = emailService;
        _tenantContext = tenantContext;
        _db = db;
    }

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

        var emailNormalizado = Input.Email.Trim().ToUpperInvariant();
        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == emailNormalizado);

        // Sempre mostra sucesso (não revela se o e-mail existe)
        Enviado = true;

        if (usuario is not null && usuario.EmailConfirmed)
        {
            _tenantContext.Definir(usuario.TenantId, usuario.Id, usuario.NomeCompleto);
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = Url.Page("/Account/RedefinirSenha",
                pageHandler: null,
                values: new { area = "Identity", userId = usuario.Id, token = tokenCodificado },
                protocol: Request.Scheme)!;

            await _emailService.EnviarResetSenhaAsync(usuario.Email!, usuario.NomeCompleto, link);
        }

        return Page();
    }
}
