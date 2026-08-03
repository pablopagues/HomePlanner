using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Auth;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Auth;
using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Options;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.HomePlanner.Services.Auth;

public class AuthTokenService : IAuthTokenService
{
    private readonly SignInManager<Usuario> _signInManager;
    private readonly TenantContext _tenantContext;
    private readonly HomePlannerDbContext _db;
    private readonly JwtOptions _jwt;
    private readonly ILogger<AuthTokenService> _logger;

    public AuthTokenService(
        SignInManager<Usuario> signInManager,
        TenantContext tenantContext,
        HomePlannerDbContext db,
        IOptions<JwtOptions> jwt,
        ILogger<AuthTokenService> logger)
    {
        _signInManager = signInManager;
        _tenantContext = tenantContext;
        _db = db;
        _jwt = jwt.Value;
        _logger = logger;
    }

    private const string AudienciaMfa = "mfa";

    public async Task<ResultadoOperacao<LoginRespostaDTO>> LoginAsync(LoginApiDTO dto, CancellationToken ct = default)
    {
        if (!_jwt.EstaConfigurado)
            return ResultadoOperacao<LoginRespostaDTO>.Falha("API de autenticação não está configurada.");

        var emailNormalizado = dto.Email.Trim().ToUpperInvariant();

        // Busca global (ainda não autenticado → sem filtro de tenant)
        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == emailNormalizado, ct);

        if (usuario is null || !usuario.Ativo || usuario.IsDeleted)
            return ResultadoOperacao<LoginRespostaDTO>.Falha("E-mail ou senha inválidos.");

        // Define o tenant ANTES de gerar claims, para a fábrica ler os papéis (filtro global por TenantId).
        _tenantContext.Definir(usuario.TenantId, usuario.Id, usuario.NomeCompleto);

        var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, dto.Senha, lockoutOnFailure: true);

        if (resultado.IsLockedOut)
            return ResultadoOperacao<LoginRespostaDTO>.Falha("Conta temporariamente bloqueada após várias tentativas. Tente novamente em 15 minutos.");

        if (!resultado.Succeeded)
            return ResultadoOperacao<LoginRespostaDTO>.Falha("E-mail ou senha inválidos.");

        // 2FA ativo → devolve um token curto; os tokens finais saem no passo /2fa.
        if (usuario.TwoFactorEnabled)
        {
            var mfaToken = GerarMfaToken(usuario);
            return ResultadoOperacao<LoginRespostaDTO>.Ok(new LoginRespostaDTO { Requer2FA = true, MfaToken = mfaToken });
        }

        var tokens = await EmitirTokensAsync(usuario, dto.DispositivoInfo, ct);

        usuario.UltimoLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Login API bem-sucedido: {Email}", usuario.Email);
        return ResultadoOperacao<LoginRespostaDTO>.Ok(new LoginRespostaDTO { Requer2FA = false, Tokens = tokens });
    }

    public async Task<ResultadoOperacao<TokensDTO>> Confirmar2FAAsync(Confirmar2FADTO dto, CancellationToken ct = default)
    {
        if (!_jwt.EstaConfigurado)
            return ResultadoOperacao<TokensDTO>.Falha("API de autenticação não está configurada.");

        var usuarioId = ValidarMfaToken(dto.MfaToken);
        if (usuarioId is null)
            return ResultadoOperacao<TokensDTO>.Falha("Sessão de verificação expirada. Faça login novamente.");

        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == usuarioId, ct);

        if (usuario is null || !usuario.Ativo || usuario.IsDeleted)
            return ResultadoOperacao<TokensDTO>.Falha("Sessão de verificação expirada. Faça login novamente.");

        _tenantContext.Definir(usuario.TenantId, usuario.Id, usuario.NomeCompleto);

        var userMgr = _signInManager.UserManager;
        var codigoLimpo = dto.Codigo.Replace(" ", string.Empty).Replace("-", string.Empty);

        bool ok;
        if (dto.CodigoRecuperacao)
        {
            var r = await userMgr.RedeemTwoFactorRecoveryCodeAsync(usuario, codigoLimpo);
            ok = r.Succeeded;
        }
        else
        {
            ok = await userMgr.VerifyTwoFactorTokenAsync(
                usuario, userMgr.Options.Tokens.AuthenticatorTokenProvider, codigoLimpo);
        }

        if (!ok)
            return ResultadoOperacao<TokensDTO>.Falha("Código inválido. Verifique o app autenticador e tente novamente.");

        var tokens = await EmitirTokensAsync(usuario, dto.DispositivoInfo, ct);

        usuario.UltimoLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Login 2FA API bem-sucedido: {Email}", usuario.Email);
        return ResultadoOperacao<TokensDTO>.Ok(tokens);
    }

    public async Task<ResultadoOperacao<TokensDTO>> RenovarAsync(RefreshApiDTO dto, CancellationToken ct = default)
    {
        if (!_jwt.EstaConfigurado)
            return ResultadoOperacao<TokensDTO>.Falha("API de autenticação não está configurada.");

        var hash = HashToken(dto.RefreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || !token.EstaAtivo)
            return ResultadoOperacao<TokensDTO>.Falha("Sessão expirada. Faça login novamente.");

        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == token.UsuarioId, ct);

        if (usuario is null || !usuario.Ativo || usuario.IsDeleted)
            return ResultadoOperacao<TokensDTO>.Falha("Sessão expirada. Faça login novamente.");

        _tenantContext.Definir(usuario.TenantId, usuario.Id, usuario.NomeCompleto);

        // Rotação: revoga o token usado e emite um novo par.
        token.RevogadoEm = DateTime.UtcNow;
        var tokens = await EmitirTokensAsync(usuario, dto.DispositivoInfo, ct);
        await _db.SaveChangesAsync(ct);

        return ResultadoOperacao<TokensDTO>.Ok(tokens);
    }

    public async Task<ResultadoOperacao> RevogarAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = HashToken(refreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is not null && token.RevogadoEm is null)
        {
            token.RevogadoEm = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return ResultadoOperacao.Ok();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<TokensDTO> EmitirTokensAsync(Usuario usuario, string? dispositivoInfo, CancellationToken ct)
    {
        // Reusa a fábrica de claims registrada (TenantUserClaimsPrincipalFactory):
        // o app recebe exatamente os mesmos claims do login por cookie.
        var principal = await _signInManager.CreateUserPrincipalAsync(usuario);
        var (accessToken, expiraEm) = GerarAccessToken(principal);

        var refreshBruto = GerarRefreshTokenBruto();
        _db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = HashToken(refreshBruto),
            UsuarioId = usuario.Id,
            TenantId = usuario.TenantId,
            CriadoEm = DateTime.UtcNow,
            ExpiraEm = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDias),
            DispositivoInfo = dispositivoInfo,
        });

        return new TokensDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshBruto,
            AccessTokenExpiraEm = expiraEm,
            UsuarioId = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            EhOwner = principal.IsInRole("Owner"),
        };
    }

    private (string token, DateTime expiraEm) GerarAccessToken(ClaimsPrincipal principal)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var expiraEm = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutos);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: principal.Claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEm,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }

    /// <summary>Token curto que prova "a senha passou"; só serve para o passo /2fa (audience "mfa").</summary>
    private string GerarMfaToken(Usuario usuario)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: AudienciaMfa,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, usuario.Id)],
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Valida o MFA token e devolve o UsuarioId, ou null se inválido/expirado.</summary>
    private string? ValidarMfaToken(string mfaToken)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var parametros = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = AudienciaMfa,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = chave,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        try
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var principal = handler.ValidateToken(mfaToken, parametros, out _);
            return principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
        catch
        {
            return null;
        }
    }

    private static string GerarRefreshTokenBruto()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Base64UrlEncoder.Encode(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
