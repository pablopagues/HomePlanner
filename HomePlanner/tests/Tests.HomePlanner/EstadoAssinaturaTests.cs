using Application.HomePlanner.Middleware;
using Domain.HomePlanner.Models.Enums;
using Xunit;

namespace Tests.HomePlanner;

public class EstadoAssinaturaTests
{
    private static readonly DateTime Agora = new(2026, 08, 23, 12, 00, 00, DateTimeKind.Utc);

    [Fact]
    public void Trial_dentro_do_prazo_nao_bloqueia()
    {
        var e = EstadoAssinatura.Avaliar(StatusAssinatura.Trial, Agora.AddDays(1), null, Agora);
        Assert.False(e.Bloqueada);
    }

    [Fact]
    public void Trial_vencido_bloqueia_por_trial_expirado()
    {
        var e = EstadoAssinatura.Avaliar(StatusAssinatura.Trial, Agora.AddSeconds(-1), null, Agora);
        Assert.True(e.Bloqueada);
        Assert.Equal(MotivoBloqueio.TrialExpirado, e.Motivo);
    }

    [Fact]
    public void Trial_sem_data_de_fim_nao_bloqueia()
    {
        // Registro antigo/incompleto: não trancamos ninguém por falta de dado.
        var e = EstadoAssinatura.Avaliar(StatusAssinatura.Trial, null, null, Agora);
        Assert.False(e.Bloqueada);
    }

    [Fact]
    public void Suspenso_bloqueia_por_pagamento_pendente()
    {
        var e = EstadoAssinatura.Avaliar(StatusAssinatura.Suspenso, null, Agora.AddDays(10), Agora);
        Assert.Equal(MotivoBloqueio.PagamentoPendente, e.Motivo);
    }

    [Fact]
    public void Ativo_com_periodo_em_dia_nao_bloqueia()
    {
        var e = EstadoAssinatura.Avaliar(StatusAssinatura.Ativo, null, Agora.AddDays(20), Agora);
        Assert.False(e.Bloqueada);
    }

    [Fact]
    public void Ativo_vencido_dentro_da_carencia_nao_bloqueia()
    {
        // Webhook de renovação pode atrasar — a carência evita cortar quem pagou.
        var e = EstadoAssinatura.Avaliar(StatusAssinatura.Ativo, null, Agora.AddDays(-1), Agora);
        Assert.False(e.Bloqueada);
    }

    [Fact]
    public void Ativo_vencido_alem_da_carencia_bloqueia()
    {
        var vencido = Agora.AddDays(-(EstadoAssinatura.DiasCarencia + 1));
        var e = EstadoAssinatura.Avaliar(StatusAssinatura.Ativo, null, vencido, Agora);
        Assert.Equal(MotivoBloqueio.PagamentoPendente, e.Motivo);
    }

    [Fact]
    public void Cancelado_com_periodo_pago_em_aberto_nao_bloqueia()
    {
        // Cancelamento agendado no Stripe: o acesso vale até o fim do período pago.
        var e = EstadoAssinatura.Avaliar(StatusAssinatura.Cancelado, null, Agora.AddDays(5), Agora);
        Assert.False(e.Bloqueada);
    }

    [Fact]
    public void Cancelado_apos_o_periodo_pago_bloqueia()
    {
        var vencido = Agora.AddDays(-(EstadoAssinatura.DiasCarencia + 1));
        var e = EstadoAssinatura.Avaliar(StatusAssinatura.Cancelado, null, vencido, Agora);
        Assert.Equal(MotivoBloqueio.Cancelado, e.Motivo);
    }

    [Fact]
    public void Cancelado_sem_data_de_expiracao_bloqueia()
    {
        var e = EstadoAssinatura.Avaliar(StatusAssinatura.Cancelado, null, null, Agora);
        Assert.Equal(MotivoBloqueio.Cancelado, e.Motivo);
    }
}
