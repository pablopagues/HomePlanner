using Microsoft.AspNetCore.Authorization;

namespace Application.HomePlanner.Middleware;

public static class PoliciesAutorizacao
{
    public const string OwnerOnly = "OwnerOnly";
    public const string MembroOuSuperior = "MembroOuSuperior";
    public const string TodosFamilia = "TodosFamilia";

    public static void Registrar(AuthorizationOptions opts)
    {
        // Owner: gerencia assinatura, usuários, configurações
        opts.AddPolicy(OwnerOnly, p => p.RequireRole("Owner"));
        // Membro: Owner ou Membro adulto (leitura + escrita)
        opts.AddPolicy(MembroOuSuperior, p => p.RequireRole("Owner", "Membro"));
        // TodosFamilia: qualquer membro autenticado (inclusive Filho - somente leitura via UI)
        opts.AddPolicy(TodosFamilia, p => p.RequireRole("Owner", "Membro", "Filho"));
    }
}
