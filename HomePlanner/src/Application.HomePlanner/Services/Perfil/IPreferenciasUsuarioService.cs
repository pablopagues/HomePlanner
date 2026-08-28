using Application.HomePlanner.Common;

namespace Application.HomePlanner.Services.Perfil;

/// <summary>Preferências pessoais do usuário autenticado (não da família).</summary>
public interface IPreferenciasUsuarioService
{
    /// <summary>
    /// Grava o idioma preferido em código curto (pt, en, es, fr). É o que as notificações
    /// consultam: elas são montadas por um background service, fora de qualquer requisição,
    /// então não têm Accept-Language nem cookie para consultar.
    /// </summary>
    Task<ResultadoOperacao> DefinirIdiomaAsync(string idioma, CancellationToken ct = default);
}
