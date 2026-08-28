using Application.HomePlanner.Middleware;
using Xunit;

namespace Tests.HomePlanner;

public class AssinaturaRotasTests
{
    [Theory]
    // A saída do bloqueio
    [InlineData("/assinatura")]
    [InlineData("/subscription")]
    [InlineData("/suscripcion")]
    [InlineData("/abonnement")]
    [InlineData("/assinatura-expirada")]
    // Conta do usuário: sair, exportar dados, encerrar a conta
    [InlineData("/perfil")]
    [InlineData("/perfil/foto")]
    [InlineData("/profile")]
    [InlineData("/configuracoes")]
    [InlineData("/settings")]
    [InlineData("/empresa")]
    // Infraestrutura — bloquear aqui derrubaria o circuito ou o login
    [InlineData("/_blazor")]
    [InlineData("/_framework/blazor.web.js")]
    [InlineData("/Identity/Account/Logout")]
    [InlineData("/set-lang")]
    [InlineData("/")]
    [InlineData("")]
    // Público
    [InlineData("/termos")]
    [InlineData("/privacy")]
    // API mobile liberada
    [InlineData("/api/auth/login")]
    [InlineData("/api/assinatura")]
    [InlineData("/api/webhook/stripe")]
    [InlineData("/api/perfil")]
    public void Rotas_liberadas(string path)
        => Assert.True(AssinaturaRotas.Liberada(path), path);

    [Theory]
    // Módulos do produto
    [InlineData("/dashboard")]
    [InlineData("/cardapio")]
    [InlineData("/menu/2026-01-01")]
    [InlineData("/receitas/editar/7")]
    [InlineData("/compras")]
    [InlineData("/planner")]
    [InlineData("/calendario")]
    [InlineData("/familia")]
    [InlineData("/modelos")]
    // API mobile bloqueada
    [InlineData("/api/cardapio")]
    [InlineData("/api/receitas/12")]
    [InlineData("/api/planner/membros")]
    public void Rotas_bloqueadas(string path)
        => Assert.False(AssinaturaRotas.Liberada(path), path);
}
