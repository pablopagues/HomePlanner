namespace HomePlanner.BlazorServer.Components.Pages.Receitas;

/// <summary>
/// Resultado dos diálogos de criar/extrair componente: a receita criada e as porções
/// com que deve entrar no prato.
/// </summary>
public record ComponenteCriado(int Id, string Nome, int Porcoes);
