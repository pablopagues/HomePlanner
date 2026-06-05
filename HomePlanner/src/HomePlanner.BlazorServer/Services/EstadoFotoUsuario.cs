namespace HomePlanner.BlazorServer.Services;

/// <summary>
/// Estado compartilhado (por circuito) do avatar do usuário. Permite que a
/// página de Perfil avise o AppBar para recarregar a foto após upload/remoção.
/// </summary>
public class EstadoFotoUsuario
{
    /// <summary>Token de versão da foto atual, ou null se não houver foto.</summary>
    public string? Versao { get; private set; }

    /// <summary>Disparado sempre que a foto muda.</summary>
    public event Action? OnChange;

    public void Definir(string? versao)
    {
        Versao = versao;
        OnChange?.Invoke();
    }
}
