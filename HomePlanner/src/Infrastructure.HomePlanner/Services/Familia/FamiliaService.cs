using System.Text;
using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Familia;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Assinatura;
using Application.HomePlanner.Services.Email;
using Application.HomePlanner.Services.Familia;
using Domain.HomePlanner.Models.Enums;
using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Options;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.HomePlanner.Services.Familia;

public class FamiliaService : IFamiliaService
{
    private readonly HomePlannerDbContext _db;
    private readonly UserManager<Usuario> _userManager;
    private readonly TenantContextAccessor _tenantAccessor;
    private readonly TenantContext _tenantContext;
    private readonly IEmailService _emailService;
    private readonly IAssinaturaRepository _assinaturaRepo;
    private readonly ILogger<FamiliaService> _logger;

    public FamiliaService(
        HomePlannerDbContext db,
        UserManager<Usuario> userManager,
        TenantContextAccessor tenantAccessor,
        TenantContext tenantContext,
        IEmailService emailService,
        IAssinaturaRepository assinaturaRepo,
        ILogger<FamiliaService> logger)
    {
        _db = db;
        _userManager = userManager;
        _tenantAccessor = tenantAccessor;
        _tenantContext = tenantContext;
        _emailService = emailService;
        _assinaturaRepo = assinaturaRepo;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MembroFamiliaDetalheDTO>> ListarMembrosAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        // Filtros globais (tenant + soft-delete) aplicados a Users e Roles.
        var membros = await (
            from u in _db.Users
            join ur in _db.UserRoles on u.Id equals ur.UserId into urs
            from ur in urs.DefaultIfEmpty()
            join r in _db.Roles on ur.RoleId equals r.Id into rs
            from r in rs.DefaultIfEmpty()
            select new MembroFamiliaDetalheDTO
            {
                UsuarioId     = u.Id,
                Nome          = u.NomeCompleto,
                Email         = u.Email!,
                Papel         = r != null ? r.Tipo : PapelUsuario.Filho,
                Ativo         = u.Ativo,
                SenhaDefinida = u.PasswordHash != null,
                UltimoLogin   = u.UltimoLogin,
            })
            .ToListAsync(ct);

        // Owner primeiro, depois pais, depois filhos; ordem alfabética dentro de cada grupo.
        return membros
            .OrderBy(m => m.Papel)
            .ThenBy(m => m.Nome)
            .ToList();
    }

    public async Task<ResultadoOperacao<ConviteMembroResultadoDTO>> AdicionarMembroAsync(
        NovoMembroDTO dto, string baseUrl, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var tenantId = _tenantContext.TenantId;
        if (tenantId is null)
            return ResultadoOperacao<ConviteMembroResultadoDTO>.Falha("Sessão inválida. Faça login novamente.");

        var nome = dto.Nome?.Trim() ?? string.Empty;
        var email = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (nome.Length < 2)
            return ResultadoOperacao<ConviteMembroResultadoDTO>.Falha("Informe o nome do membro.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return ResultadoOperacao<ConviteMembroResultadoDTO>.Falha("Informe um e-mail válido.");
        if (dto.Papel != PapelUsuario.Membro && dto.Papel != PapelUsuario.Filho)
            return ResultadoOperacao<ConviteMembroResultadoDTO>.Falha("Papel inválido para um membro.");

        var emailExiste = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => !u.IsDeleted && u.NormalizedEmail == email.ToUpperInvariant(), ct);
        if (emailExiste)
            return ResultadoOperacao<ConviteMembroResultadoDTO>.Falha("Já existe uma conta com este e-mail.");

        // Limite de membros do plano (conta apenas membros ativos como assentos ocupados).
        var (totalAtual, limite) = await ObterUsoMembrosAsync(ct);
        if (totalAtual >= limite)
            return ResultadoOperacao<ConviteMembroResultadoDTO>.Falha(
                $"Seu plano atual permite até {limite} membro(s) ativo(s). " +
                "Faça upgrade do plano para adicionar mais.");

        var papel = await _db.Roles.FirstOrDefaultAsync(r => r.Tipo == dto.Papel, ct);
        if (papel is null)
            return ResultadoOperacao<ConviteMembroResultadoDTO>.Falha("Papel não encontrado para esta família.");

        var usuario = new Usuario
        {
            Id             = Guid.NewGuid().ToString(),
            TenantId       = tenantId.Value,
            UserName       = email,
            Email          = email,
            NomeCompleto   = nome,
            Ativo          = true,
            EmailConfirmed = false,
            DataCriacao    = DateTime.UtcNow,
        };

        var criado = await _userManager.CreateAsync(usuario);
        if (!criado.Succeeded)
            return ResultadoOperacao<ConviteMembroResultadoDTO>.Falha(
                criado.Errors.Select(e => e.Description).ToArray());

        // Atribui o papel inserindo diretamente (mesmo padrão do registro do tenant).
        _db.UserRoles.Add(new IdentityUserRole<string> { UserId = usuario.Id, RoleId = papel.Id });
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Membro {Email} ({Papel}) adicionado ao tenant {TenantId}.",
            email, dto.Papel, tenantId.Value);

        var convite = await GerarEnviarConviteAsync(usuario, baseUrl, ct);
        return ResultadoOperacao<ConviteMembroResultadoDTO>.Ok(convite);
    }

    public async Task<ResultadoOperacao<ConviteMembroResultadoDTO>> ReenviarConviteAsync(
        string usuarioId, string baseUrl, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var usuario = await _db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
        if (usuario is null)
            return ResultadoOperacao<ConviteMembroResultadoDTO>.Falha("Membro não encontrado.");

        var convite = await GerarEnviarConviteAsync(usuario, baseUrl, ct);
        return ResultadoOperacao<ConviteMembroResultadoDTO>.Ok(convite);
    }

    public async Task<ResultadoOperacao> EditarMembroAsync(
        string usuarioId, string nome, string email, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var nomeLimpo = nome?.Trim() ?? string.Empty;
        var emailLimpo = email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (nomeLimpo.Length < 2)
            return ResultadoOperacao.Falha("Informe o nome do membro.");
        if (string.IsNullOrWhiteSpace(emailLimpo) || !emailLimpo.Contains('@'))
            return ResultadoOperacao.Falha("Informe um e-mail válido.");

        var usuario = await _db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
        if (usuario is null)
            return ResultadoOperacao.Falha("Membro não encontrado.");

        var papel = await ObterPapelDoUsuarioAsync(usuarioId, ct);
        if (papel?.Tipo == PapelUsuario.Owner)
            return ResultadoOperacao.Falha("Os dados do administrador (Owner) devem ser alterados no perfil.");

        // Só é editável enquanto o convite está pendente (senha ainda não definida).
        // Depois que o membro define a senha, ele já pode ter dados — o e-mail vira imutável.
        if (usuario.PasswordHash != null)
            return ResultadoOperacao.Falha(
                "Este membro já ativou a conta. O e-mail não pode mais ser alterado; use Desativar se necessário.");

        var emailNormalizado = emailLimpo.ToUpperInvariant();
        var emailEmUso = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => !u.IsDeleted && u.Id != usuarioId && u.NormalizedEmail == emailNormalizado, ct);
        if (emailEmUso)
            return ResultadoOperacao.Falha("Já existe uma conta com este e-mail.");

        var emailMudou = usuario.NormalizedEmail != emailNormalizado;

        usuario.NomeCompleto = nomeLimpo;
        // Mantém UserName == Email (padrão do registro) e os campos normalizados em sincronia.
        usuario.Email = emailLimpo;
        usuario.NormalizedEmail = emailNormalizado;
        usuario.UserName = emailLimpo;
        usuario.NormalizedUserName = emailNormalizado;
        // Trocou o e-mail: o anterior nunca foi confirmado de fato — exige novo convite/confirmação.
        if (emailMudou)
            usuario.EmailConfirmed = false;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Dados do membro {UsuarioId} atualizados.", usuarioId);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> AlterarPapelAsync(
        string usuarioId, PapelUsuario novoPapel, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        if (novoPapel != PapelUsuario.Membro && novoPapel != PapelUsuario.Filho)
            return ResultadoOperacao.Falha("Só é possível alternar entre Pai e Filho.");

        var usuario = await _db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
        if (usuario is null)
            return ResultadoOperacao.Falha("Membro não encontrado.");

        var papelAtual = await ObterPapelDoUsuarioAsync(usuarioId, ct);
        if (papelAtual?.Tipo == PapelUsuario.Owner)
            return ResultadoOperacao.Falha("O papel do administrador (Owner) não pode ser alterado.");

        var novoPapelEntidade = await _db.Roles.FirstOrDefaultAsync(r => r.Tipo == novoPapel, ct);
        if (novoPapelEntidade is null)
            return ResultadoOperacao.Falha("Papel não encontrado para esta família.");

        // Remove vínculos atuais e adiciona o novo.
        var vinculos = await _db.UserRoles.Where(ur => ur.UserId == usuarioId).ToListAsync(ct);
        _db.UserRoles.RemoveRange(vinculos);
        _db.UserRoles.Add(new IdentityUserRole<string> { UserId = usuarioId, RoleId = novoPapelEntidade.Id });
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Papel do membro {UsuarioId} alterado para {Papel}.", usuarioId, novoPapel);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> DefinirAtivoAsync(
        string usuarioId, bool ativo, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var usuario = await _db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
        if (usuario is null)
            return ResultadoOperacao.Falha("Membro não encontrado.");

        var papel = await ObterPapelDoUsuarioAsync(usuarioId, ct);
        if (papel?.Tipo == PapelUsuario.Owner)
            return ResultadoOperacao.Falha("O administrador (Owner) não pode ser desativado.");

        // Reativar consome um assento — respeita o limite do plano.
        if (ativo && !usuario.Ativo)
        {
            var (totalAtual, limite) = await ObterUsoMembrosAsync(ct);
            if (totalAtual >= limite)
                return ResultadoOperacao.Falha(
                    $"Seu plano atual permite até {limite} membro(s) ativo(s). " +
                    "Faça upgrade do plano para reativar este membro.");
        }

        usuario.Ativo = ativo;
        // Bloqueia/desbloqueia o login via lockout do Identity (respeitado no PasswordSignInAsync).
        usuario.LockoutEnabled = true;
        usuario.LockoutEnd = ativo ? null : DateTimeOffset.MaxValue;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Membro {UsuarioId} {Estado}.", usuarioId, ativo ? "ativado" : "desativado");
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao> RemoverMembroAsync(string usuarioId, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var usuario = await _db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
        if (usuario is null)
            return ResultadoOperacao.Falha("Membro não encontrado.");

        var papel = await ObterPapelDoUsuarioAsync(usuarioId, ct);
        if (papel?.Tipo == PapelUsuario.Owner)
            return ResultadoOperacao.Falha("O administrador (Owner) não pode ser removido.");

        // Exclusão FÍSICA só é permitida na fase de criação (convite pendente, senha não definida):
        // sem senha o membro nunca logou, logo não tem dados no sistema. Depois disso, só Desativar.
        if (usuario.PasswordHash != null)
            return ResultadoOperacao.Falha(
                "Este membro já ativou a conta e pode ter dados no sistema. Use Desativar em vez de remover.");

        // DELETE físico (libera totalmente o e-mail no Identity). ExecuteDelete ignora o soft-delete
        // do SaveChanges; respeita os filtros globais (tenant + não-removido), então é escopado ao tenant.
        await _db.UserRoles.Where(ur => ur.UserId == usuarioId).ExecuteDeleteAsync(ct);
        var removidos = await _db.Users.Where(u => u.Id == usuarioId).ExecuteDeleteAsync(ct);
        if (removidos == 0)
            return ResultadoOperacao.Falha("Membro não encontrado.");

        _logger.LogInformation("Membro {UsuarioId} removido fisicamente (convite pendente).", usuarioId);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResumoFamiliaDTO> ObterResumoAsync(CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();
        var (total, limite) = await ObterUsoMembrosAsync(ct);
        return new ResumoFamiliaDTO { MembrosAtivos = total, LimiteMembros = limite };
    }

    /// <summary>Retorna (membros ativos, limite do plano). Assume contexto de tenant hidratado.</summary>
    private async Task<(int total, int limite)> ObterUsoMembrosAsync(CancellationToken ct)
    {
        var total = await _db.Users.CountAsync(u => u.Ativo, ct);
        var assinatura = await _assinaturaRepo.ObterMinhaAssinaturaAsync(ct);
        var plano = assinatura?.Plano ?? PlanoAssinatura.Gratis;
        return (total, LimitesPorPlano.Membros(plano));
    }

    private async Task<Papel?> ObterPapelDoUsuarioAsync(string usuarioId, CancellationToken ct)
    {
        return await (
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            where ur.UserId == usuarioId
            select r).FirstOrDefaultAsync(ct);
    }

    private async Task<ConviteMembroResultadoDTO> GerarEnviarConviteAsync(
        Usuario usuario, string baseUrl, CancellationToken ct)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = $"{baseUrl.TrimEnd('/')}/Identity/Account/RedefinirSenha" +
                   $"?userId={Uri.EscapeDataString(usuario.Id)}&token={tokenCodificado}";

        if (_emailService.Habilitado)
        {
            await _emailService.EnviarResetSenhaAsync(usuario.Email!, usuario.NomeCompleto, link, ct);
            return new ConviteMembroResultadoDTO { UsuarioId = usuario.Id, EmailEnviado = true };
        }

        // Sem SMTP (dev): devolve o link para o Owner compartilhar manualmente.
        _logger.LogWarning("SMTP desabilitado — link de convite de {Email} retornado para compartilhamento manual.",
            usuario.Email);
        return new ConviteMembroResultadoDTO { UsuarioId = usuario.Id, EmailEnviado = false, LinkConvite = link };
    }
}
