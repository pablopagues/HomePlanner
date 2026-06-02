using Domain.HomePlanner.Models.Enums;

namespace Domain.HomePlanner.Models.SaaS.Options;

public static class LimitesPorPlano
{
    public const int DiasTrialGratis = 30;

    public static int Membros(PlanoAssinatura plano) => plano switch
    {
        PlanoAssinatura.Gratis                                        => 1,
        PlanoAssinatura.StandardMensal or PlanoAssinatura.StandardAnual => 5,
        PlanoAssinatura.ProMensal      or PlanoAssinatura.ProAnual      => 10,
        _                                                              => 1
    };
}
