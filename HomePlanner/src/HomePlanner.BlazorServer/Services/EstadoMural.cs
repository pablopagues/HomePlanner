namespace HomePlanner.BlazorServer.Services;

/// <summary>
/// Estado do "Modo Mural" (visão de família somente-leitura para deixar aberta
/// num tablet). É <c>Scoped</c> — vive por circuito Blazor. Numa recarga de página
/// o circuito é recriado e o estado some; o <c>MainLayout</c> trata esse caso
/// caindo para a tela de login (ver marcador em localStorage <c>hp_mural</c>).
/// </summary>
public class EstadoMural
{
    /// <summary>Verdadeiro quando o circuito está no Modo Mural.</summary>
    public bool Ativo { get; private set; }

    /// <summary>Disparado quando o modo é ligado/desligado, para a UI re-renderizar.</summary>
    public event Action? OnChange;

    public void Ativar()
    {
        if (Ativo) return;
        Ativo = true;
        OnChange?.Invoke();
    }

    public void Desativar()
    {
        if (!Ativo) return;
        Ativo = false;
        OnChange?.Invoke();
    }
}
