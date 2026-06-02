using System.Globalization;
using System.Text;

namespace Application.HomePlanner.Helpers;

public static class TextoHelper
{
    /// <summary>
    /// Remove acentos e converte para minúsculas — usado para campos NomeNormalizado.
    /// </summary>
    public static string NormalizarNome(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var normalizado = texto.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);
        foreach (char c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
