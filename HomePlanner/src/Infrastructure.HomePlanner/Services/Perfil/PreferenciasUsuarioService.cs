using Application.HomePlanner.Common;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Services.Perfil;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Services.Perfil;

/// <inheritdoc cref="IPreferenciasUsuarioService"/>
public class PreferenciasUsuarioService : IPreferenciasUsuarioService
{
    /// <summary>Códigos curtos aceitos — os mesmos que a web grava pelo /set-lang.</summary>
    private static readonly string[] _aceitos = ["pt", "en", "es", "fr"];

    private readonly HomePlannerDbContext _db;
    private readonly TenantContext _tenantContext;
    private readonly TenantContextAccessor _tenantAccessor;

    public PreferenciasUsuarioService(
        HomePlannerDbContext db,
        TenantContext tenantContext,
        TenantContextAccessor tenantAccessor)
    {
        _db = db;
        _tenantContext = tenantContext;
        _tenantAccessor = tenantAccessor;
    }

    public async Task<ResultadoOperacao> DefinirIdiomaAsync(string idioma, CancellationToken ct = default)
    {
        await _tenantAccessor.GarantirHidratadoAsync();

        var codigo = (idioma ?? string.Empty).Trim().ToLowerInvariant();
        if (!_aceitos.Contains(codigo))
            return ResultadoOperacao.Falha(ErrosApp.IdiomaNaoSuportado);

        var usuarioId = _tenantContext.UsuarioId;
        if (string.IsNullOrEmpty(usuarioId))
            return ResultadoOperacao.Falha(ErrosApp.SessaoExpirada);

        var usuario = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == usuarioId, ct);

        if (usuario is null)
            return ResultadoOperacao.Falha(ErrosApp.SessaoExpirada);

        if (usuario.Idioma != codigo)
        {
            usuario.Idioma = codigo;
            await _db.SaveChangesAsync(ct);
        }

        return ResultadoOperacao.Ok();
    }
}
