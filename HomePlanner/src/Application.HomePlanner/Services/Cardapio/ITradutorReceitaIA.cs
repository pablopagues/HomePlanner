using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Traduz o texto livre de uma receita (nome, modo de preparo, observações) para o
/// idioma do usuário, via IA (Claude). Retorna <c>null</c> quando a IA está
/// desabilitada ou falha — nesse caso os textos originais são preservados.
/// </summary>
public interface ITradutorReceitaIA
{
    bool Habilitado { get; }

    /// <param name="idiomaAlvo">"pt" / "en" / "es".</param>
    Task<TraducaoReceitaDTO?> TraduzirAsync(
        string? nome, string? modoPreparo, string? observacoes,
        string idiomaAlvo, CancellationToken ct = default);
}
