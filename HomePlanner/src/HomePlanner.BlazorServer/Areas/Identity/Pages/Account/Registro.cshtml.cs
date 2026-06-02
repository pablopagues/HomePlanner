using System.ComponentModel.DataAnnotations;
using System.Text;
using Application.HomePlanner.DTOs.Auth;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Auth;
using Application.HomePlanner.Services.Email;
using Domain.HomePlanner.Models.SaaS.Identity;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace HomePlanner.BlazorServer.Areas.Identity.Pages.Account;

public class RegistroModel : PageModel
{
    private readonly IRegistroTenantService _registroService;
    private readonly UserManager<Usuario> _userManager;
    private readonly IEmailService _emailService;
    private readonly TenantContext _tenantContext;
    private readonly HomePlannerDbContext _db;
    private readonly ILogger<RegistroModel> _logger;

    public RegistroModel(
        IRegistroTenantService registroService,
        UserManager<Usuario> userManager,
        IEmailService emailService,
        TenantContext tenantContext,
        HomePlannerDbContext db,
        ILogger<RegistroModel> logger)
    {
        _registroService = registroService;
        _userManager = userManager;
        _emailService = emailService;
        _tenantContext = tenantContext;
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Informe seu nome.")]
        [Display(Name = "Seu nome")]
        public string NomeResponsavel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe um e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PaisId { get; set; } = "BR";

        [Required(ErrorMessage = "Informe uma senha.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter ao menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(Senha), ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmacaoSenha { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "Você precisa aceitar os termos.")]
        public bool AceitaTermos { get; set; }
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var resultado = await _registroService.RegistrarAsync(new RegistroPersistenciaDTO
        {
            NomeResponsavel = Input.NomeResponsavel,
            Email           = Input.Email,
            Senha           = Input.Senha,
            PaisId          = Input.PaisId,
        });

        if (!resultado.Sucesso)
        {
            foreach (var erro in resultado.Erros)
                ModelState.AddModelError(string.Empty, erro);
            return Page();
        }

        // Hidrata contexto para que FindByIdAsync respeite o tenant recém-criado
        _tenantContext.Definir(resultado.TenantId, resultado.UsuarioId, Input.NomeResponsavel);

        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == resultado.UsuarioId);
        if (usuario is null)
            return RedirectToPage("./Login");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);

        if (_emailService.Habilitado)
        {
            var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = Url.Page("/Account/ConfirmarEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = usuario.Id, token = tokenCodificado },
                protocol: Request.Scheme)!;

            await _emailService.EnviarConfirmacaoEmailAsync(usuario.Email!, usuario.NomeCompleto, link);
            return RedirectToPage("./VerificarEmail", new { email = usuario.Email });
        }

        // Modo dev (sem SMTP): confirma direto para facilitar testes
        await _userManager.ConfirmEmailAsync(usuario, token);
        _logger.LogWarning("SMTP desabilitado — e-mail de {Email} confirmado automaticamente.", usuario.Email);
        return RedirectToPage("./Login", new { confirmado = true });
    }
}
