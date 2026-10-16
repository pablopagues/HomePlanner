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

    /// <summary>
    /// Limite de pedidos de sugestão de receitas à IA por mês-calendário. Cada pedido
    /// gera 3 sugestões e é contado ao gerar (é ali que está o custo).
    /// </summary>
    public static int SugestoesReceitaMes(PlanoAssinatura plano) => plano switch
    {
        PlanoAssinatura.Gratis                                          => 10,
        PlanoAssinatura.StandardMensal or PlanoAssinatura.StandardAnual => 30,
        PlanoAssinatura.ProMensal      or PlanoAssinatura.ProAnual      => 100,
        _                                                              => 0
    };

    /// <summary>
    /// Limite de cardápios da semana gerados pela IA por mês-calendário. Contado ao
    /// gerar (cada geração é uma chamada ao modelo maior, com todo o catálogo da casa).
    /// </summary>
    public static int CardapiosIAMes(PlanoAssinatura plano) => plano switch
    {
        PlanoAssinatura.Gratis                                          => 4,
        PlanoAssinatura.StandardMensal or PlanoAssinatura.StandardAnual => 15,
        PlanoAssinatura.ProMensal      or PlanoAssinatura.ProAnual      => 50,
        _                                                              => 0
    };
}
