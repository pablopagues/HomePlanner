using Domain.HomePlanner.Models.Enums;

namespace Domain.HomePlanner.Models.SaaS.Options;

public static class LimitesPorPlano
{
    public const int DiasTrialGratis = 30;

    public static int Membros(PlanoAssinatura plano) => plano switch
    {
        // O plano Grátis é o trial inicial (30 dias): liberamos o limite do Standard (5)
        // para o usuário poder testar a gestão da família. Após o trial, o acesso é
        // bloqueado por status de assinatura — exige assinar um plano para continuar.
        PlanoAssinatura.Gratis                                          => 5,
        PlanoAssinatura.StandardMensal or PlanoAssinatura.StandardAnual => 5,
        PlanoAssinatura.ProMensal      or PlanoAssinatura.ProAnual      => 10,
        _                                                              => 1
    };

    /// <summary>
    /// Limite de importações de receita (via URL, com parsing por IA) por mês-calendário.
    /// O trial (Grátis) ganha uma cota pequena para experimentar o recurso.
    /// </summary>
    public static int ImportacoesReceitaMes(PlanoAssinatura plano) => plano switch
    {
        PlanoAssinatura.Gratis                                          => 10,
        PlanoAssinatura.StandardMensal or PlanoAssinatura.StandardAnual => 50,
        PlanoAssinatura.ProMensal      or PlanoAssinatura.ProAnual      => 200,
        _                                                              => 0
    };
}
