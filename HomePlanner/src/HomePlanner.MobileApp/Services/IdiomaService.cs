using System.Globalization;

namespace HomePlanner.MobileApp.Services;

/// <summary>
/// Idioma do app. Guarda a escolha entre execuções e aplica a cultura no processo, para
/// o IStringLocalizer resolver as .resx compartilhadas com a web.
///
/// O mesmo código de idioma vai no Accept-Language de toda chamada (ver
/// <see cref="IdiomaMessageHandler"/>) e é gravado em Usuario.Idioma no servidor, que é
/// o que as notificações leem quando não há requisição em curso.
/// </summary>
public class IdiomaService
{
    private const string Chave = "hp_idioma";

    /// <summary>Códigos curtos aceitos, iguais aos da web.</summary>
    public static readonly string[] Suportados = ["pt", "en", "es", "fr"];

    /// <summary>Disparado quando o idioma muda, para as telas re-renderizarem.</summary>
    public event Action? Alterou;

    public string Atual { get; private set; } = "pt";

    public IdiomaService()
    {
        var salvo = Preferences.Default.Get<string?>(Chave, null);
        Aplicar(EhSuportado(salvo) ? salvo! : DoAparelho(), persistir: false);
    }

    /// <summary>Idioma do aparelho, quando o usuário ainda não escolheu — cai em pt se não suportarmos.</summary>
    private static string DoAparelho()
    {
        var duasLetras = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return EhSuportado(duasLetras) ? duasLetras : "pt";
    }

    public static bool EhSuportado(string? lang)
        => !string.IsNullOrEmpty(lang) && Suportados.Contains(lang);

    public void Definir(string lang)
    {
        if (!EhSuportado(lang) || lang == Atual) return;
        Aplicar(lang, persistir: true);
        Alterou?.Invoke();
    }

    private void Aplicar(string lang, bool persistir)
    {
        Atual = lang;

        var cultura = ParaCultura(lang);
        CultureInfo.CurrentCulture = cultura;
        CultureInfo.CurrentUICulture = cultura;
        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.DefaultThreadCurrentUICulture = cultura;

        if (persistir) Preferences.Default.Set(Chave, lang);
    }

    /// <summary>Cultura completa — o pt do app é pt-BR, que é a .resx neutra.</summary>
    public static CultureInfo ParaCultura(string lang) => lang switch
    {
        "en" => new CultureInfo("en-US"),
        "es" => new CultureInfo("es-ES"),
        "fr" => new CultureInfo("fr-FR"),
        _    => new CultureInfo("pt-BR"),
    };

    public string NomeDoIdioma(string lang) => lang switch
    {
        "en" => "English",
        "es" => "Español",
        "fr" => "Français",
        _    => "Português",
    };
}
