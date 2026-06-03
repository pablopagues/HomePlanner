using Application.HomePlanner.DTOs.Auth;
using Application.HomePlanner.Services.Auth;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Assinatura;
using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Tenancy;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HomePlanner.Services.Auth;

public class RegistroTenantService : IRegistroTenantService
{
    private readonly HomePlannerDbContext _db;
    private readonly UserManager<Usuario> _userManager;
    private readonly RoleManager<Papel> _roleManager;
    private readonly ILogger<RegistroTenantService> _logger;

    public RegistroTenantService(
        HomePlannerDbContext db,
        UserManager<Usuario> userManager,
        RoleManager<Papel> roleManager,
        ILogger<RegistroTenantService> logger)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<RegistroResultadoDTO> RegistrarAsync(
        RegistroPersistenciaDTO dto, CancellationToken ct = default)
    {
        dto.NomeResponsavel = dto.NomeResponsavel?.Trim() ?? string.Empty;
        dto.Email = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (dto.NomeResponsavel.Length < 2)
            return RegistroResultadoDTO.Falha("Informe seu nome.");
        if (string.IsNullOrWhiteSpace(dto.Email))
            return RegistroResultadoDTO.Falha("Informe um e-mail válido.");

        // Email globalmente único (ignora filtro de tenant — ainda não autenticado)
        var emailExiste = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.NormalizedEmail == dto.Email.ToUpperInvariant(), ct);
        if (emailExiste)
            return RegistroResultadoDTO.Falha("Já existe uma conta com este e-mail.");

        var paisId = dto.PaisId == Paises.Canada ? Paises.Canada : Paises.Brasil;

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // 1. Tenant (OwnerUsuarioId preenchido depois)
            var tenant = new Tenant
            {
                Id              = Guid.NewGuid(),
                NomeResponsavel = dto.NomeResponsavel,
                Email           = dto.Email,
                PaisId          = paisId,
                Ativo           = true,
                OnboardingCompleto = false,
                DataCriacao     = DateTime.UtcNow,
                OwnerUsuarioId  = string.Empty,
            };
            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync(ct);

            // 2. Papéis do tenant (Owner / Membro / Filho)
            var papelOwner = await CriarPapelAsync(tenant.Id, PapelUsuario.Owner, "Owner");
            await CriarPapelAsync(tenant.Id, PapelUsuario.Membro, "Membro");
            await CriarPapelAsync(tenant.Id, PapelUsuario.Filho, "Filho");

            // 3. Usuário (owner)
            var usuario = new Usuario
            {
                Id                 = Guid.NewGuid().ToString(),
                TenantId           = tenant.Id,
                UserName           = dto.Email,
                Email              = dto.Email,
                NomeCompleto       = dto.NomeResponsavel,
                Ativo              = true,
                EmailConfirmed     = false,
                DataCriacao        = DateTime.UtcNow,
                DataAceiteTermos   = DateTime.UtcNow,
                VersaoTermosAceito = dto.VersaoTermos,
            };

            var resultado = await _userManager.CreateAsync(usuario, dto.Senha);
            if (!resultado.Succeeded)
            {
                await transaction.RollbackAsync(ct);
                return RegistroResultadoDTO.Falha(
                    resultado.Errors.Select(e => TraduzirErroIdentity(e.Code, e.Description)).ToArray());
            }

            // 4. Atribui papel Owner.
            // Inserimos a relação diretamente em vez de UserManager.AddToRoleAsync porque,
            // durante o registro, o usuário ainda não está autenticado: o filtro global de
            // tenant resolve para Guid.Empty e a busca interna do RoleManager não encontraria
            // o papel recém-criado (TenantId = tenant.Id). A tabela AspNetUserRoles não possui
            // TenantId, logo não sofre com o filtro.
            _db.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = usuario.Id,
                RoleId = papelOwner.Id,
            });

            // 5. Vincula owner ao tenant
            tenant.OwnerUsuarioId = usuario.Id;
            _db.Tenants.Update(tenant);

            // 6. Assinatura trial
            var assinatura = new ConfiguracaoAssinatura
            {
                Id              = Guid.NewGuid(),
                TenantId        = tenant.Id,
                Plano           = PlanoAssinatura.Gratis,
                Status          = StatusAssinatura.Trial,
                DataInicioTrial = DateTime.UtcNow,
                DataFimTrial    = DateTime.UtcNow.AddDays(30),
                DataCriacao     = DateTime.UtcNow,
            };
            _db.ConfiguracoesAssinatura.Add(assinatura);

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation("Novo tenant {TenantId} criado para {Email}.", tenant.Id, dto.Email);
            return RegistroResultadoDTO.Ok(tenant.Id, usuario.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "Falha ao registrar tenant para {Email}.", dto.Email);
            return RegistroResultadoDTO.Falha("Não foi possível concluir o cadastro. Tente novamente.");
        }
    }

    private async Task<Papel> CriarPapelAsync(Guid tenantId, PapelUsuario tipo, string nomeAmigavel)
    {
        // Nome técnico único globalmente: {NomeAmigavel}_{TenantId}
        var papel = new Papel
        {
            Id           = Guid.NewGuid().ToString(),
            TenantId     = tenantId,
            Tipo         = tipo,
            NomeAmigavel = nomeAmigavel,
            Name         = $"{nomeAmigavel}_{tenantId}",
        };
        await _roleManager.CreateAsync(papel);
        return papel;
    }

    private static string TraduzirErroIdentity(string code, string descricaoPadrao) => code switch
    {
        "PasswordTooShort"            => "A senha deve ter ao menos 8 caracteres.",
        "PasswordRequiresDigit"       => "A senha deve conter pelo menos um número.",
        "PasswordRequiresLower"       => "A senha deve conter pelo menos uma letra minúscula.",
        "PasswordRequiresUpper"       => "A senha deve conter pelo menos uma letra maiúscula.",
        "PasswordRequiresNonAlphanumeric" => "A senha deve conter pelo menos um caractere especial.",
        "DuplicateUserName" or "DuplicateEmail" => "Já existe uma conta com este e-mail.",
        _                             => descricaoPadrao,
    };
}
