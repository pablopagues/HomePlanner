using System.Globalization;
using Domain.HomePlanner.Models.Enums;

namespace HomePlanner.BlazorServer.Services;

/// <summary>
/// Nomes dos dias da semana na cultura atual (já traduzidos pelo .NET em pt/en/es/fr),
/// em vez de manter chaves de tradução para cada dia.
/// </summary>
public static class DiaHelper
{
    // DiaSemana: Segunda=1 … Domingo=7  →  DayOfWeek: Sunday=0 … Saturday=6
    private static DayOfWeek ParaDayOfWeek(DiaSemana d) => (DayOfWeek)((int)d % 7);

    /// <summary>"Seg" / "Mon" / "lun." / "lun." (primeira letra maiúscula).</summary>
    public static string NomeCurto(DiaSemana d)
        => Capitalizar(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(ParaDayOfWeek(d)).TrimEnd('.'));

    /// <summary>"Segunda-feira" / "Monday" / "Lunes" / "Lundi".</summary>
    public static string NomeLongo(DiaSemana d)
        => Capitalizar(CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(ParaDayOfWeek(d)));

    /// <summary>Dia/mês no formato da cultura: "15/10" (pt/es/fr) ou "10/15" (en-US).</summary>
    public static string DiaMes(DateOnly d)
    {
        var padrao = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
        var sep = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
        // Tira o ano e o separador que sobra ("dd/MM/yyyy" → "dd/MM"; "M/d/yyyy" → "M/d").
        var semAno = padrao.Replace("yyyy", "").Replace("yy", "").Trim();
        semAno = semAno.Trim(sep.ToCharArray()).Trim('.', '-', '/', ' ');
        return d.ToString(semAno, CultureInfo.CurrentCulture);
    }

    /// <summary>Data curta completa no formato da cultura ("15/10/2026" / "10/15/2026").</summary>
    public static string Data(DateOnly d) => d.ToString("d", CultureInfo.CurrentCulture);

    /// <inheritdoc cref="Data(DateOnly)"/>
    public static string Data(DateTime d) => d.ToString("d", CultureInfo.CurrentCulture);

    private static string Capitalizar(string s)
        => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0], CultureInfo.CurrentCulture) + s[1..];
}
