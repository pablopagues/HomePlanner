using System.Text;
using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Empresa;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Email;
using Application.HomePlanner.Services.Empresa;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Tenancy;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HomePlanner.Services.Empresa;

public class EmpresaService : IEmpresaService
{
    private readonly HomePlannerDbContext _db;
    private readonly UserManager<Usuario> _userManager;
    private readonly TenantContext _tenantContext;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmpresaService> _logger;

    public EmpresaService(
        HomePlannerDbContext db,
        UserManager<Usuario> userManager,
        TenantContext tenantContext,
        TenantContextAccessor tenantAccessor,
        IEmailService emailService,
        ILogger<EmpresaService> logger)
    {
        _db = db;
        _userManager = userManager;
        _tenantContext = tenantContext;
        _tenantAccessor = tenantAccessor;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<EmpresaDetalheDTO> ObterAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;
        var usuarioId = _tenantContext.UsuarioId;

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {tenantId} não encontrado.");

        var usuario = await _db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);

        var dadosBr = await _db.TenantDadosBrasil.FirstOrDefaultAsync(d => d.TenantId == tenantId, ct);
        var dadosCa = await _db.TenantDadosCanada.FirstOrDefaultAsync(d => d.TenantId == tenantId, ct);

        return new EmpresaDetalheDTO
        {
            NomeResponsavel = tenant.NomeResponsavel,
            Email           = usuario?.Email ?? tenant.Email,
            EmailConfirmado = usuario?.EmailConfirmed ?? false,
            PaisId          = tenant.PaisId ?? Paises.Brasil,
            Cpf             = dadosBr?.Cpf,
            Province        = dadosCa?.Province,
        };
    }

    public async Task<List<string>> SalvarDadosAsync(AtualizarEmpresaDTO dto, CancellationToken ct = default)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.NomeResponsavel))
            erros.Add("Informe o nome do responsável.");

        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        if (tenant is null) { erros.Add("Conta não encontrada."); return erros; }

        var paisId = tenant.PaisId ?? Paises.Brasil;

        if (paisId == Paises.Brasil)
        {
            var cpf = SomenteDigitos(dto.Cpf);
            if (!string.IsNullOrEmpty(cpf) && cpf.Length != 11)
                erros.Add("CPF deve ter 11 dígitos.");

            if (erros.Count > 0) return erros;

            await UpsertDadosBrasilAsync(tenantId, string.IsNullOrEmpty(cpf) ? null : cpf, ct);
        }
        else if (paisId == Paises.Canada)
        {
            if (erros.Count > 0) return erros;
            await UpsertDadosCanadaAsync(tenantId, dto.Province?.Trim(), ct);
        }

        tenant.NomeResponsavel = dto.NomeResponsavel.Trim();
        _db.Tenants.Update(tenant);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Dados da conta atualizados (tenant={TenantId})", tenantId);
        return erros;
    }

    public async Task<ResultadoOperacao> ReenviarConfirmacaoEmailAsync(string baseUrl, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var usuarioId = _tenantContext.UsuarioId;

        var usuario = usuarioId is null ? null : await _userManager.FindByIdAsync(usuarioId);
        if (usuario is null)
            return ResultadoOperacao.Falha("Usuário não encontrado.");

        if (usuario.EmailConfirmed)
            return ResultadoOperacao.Falha("Seu e-mail já está confirmado.");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);
        var tokenCodificado = Base64UrlEncode(token);
        var link = $"{baseUrl.TrimEnd('/')}/Identity/Account/ConfirmarEmail"
                 + $"?userId={usuario.Id}&token={tokenCodificado}";

        if (_emailService.Habilitado)
        {
            await _emailService.EnviarConfirmacaoEmailAsync(usuario.Email!, usuario.NomeCompleto, link, ct);
            _logger.LogInformation("E-mail de confirmação reenviado para {Email}.", usuario.Email);
        }
        else
        {
            // Modo dev (sem SMTP): registra o link no log para teste manual
            _logger.LogWarning("SMTP desabilitado — link de confirmação para {Email}: {Link}", usuario.Email, link);
        }

        return ResultadoOperacao.Ok();
    }

    private async Task UpsertDadosBrasilAsync(Guid tenantId, string? cpf, CancellationToken ct)
    {
        var dados = await _db.TenantDadosBrasil.FirstOrDefaultAsync(d => d.TenantId == tenantId, ct);
        if (dados is null)
        {
            _db.TenantDadosBrasil.Add(new TenantDadosBrasil { TenantId = tenantId, Cpf = cpf });
        }
        else
        {
            dados.Cpf = cpf;
            _db.TenantDadosBrasil.Update(dados);
        }
    }

    private async Task UpsertDadosCanadaAsync(Guid tenantId, string? province, CancellationToken ct)
    {
        var dados = await _db.TenantDadosCanada.FirstOrDefaultAsync(d => d.TenantId == tenantId, ct);
        if (dados is null)
        {
            _db.TenantDadosCanada.Add(new TenantDadosCanada { TenantId = tenantId, Province = province });
        }
        else
        {
            dados.Province = province;
            _db.TenantDadosCanada.Update(dados);
        }
    }

    private static string SomenteDigitos(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
            if (char.IsDigit(c)) sb.Append(c);
        return sb.ToString();
    }

    // Base64Url sem padding — compatível com WebEncoders.Base64UrlDecode usado em ConfirmarEmail.
    private static string Base64UrlEncode(string valor)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(valor))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
