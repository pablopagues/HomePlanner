namespace Application.HomePlanner.Middleware;

/// <summary>
/// Lê o estado de onboarding de um tenant. Interface vive em Application para o
/// middleware usar; implementação em Infrastructure (acessa o DbContext).
/// </summary>
public interface IOnboardingStatusReader
{
    Task<bool> EstaCompletoAsync(Guid tenantId, CancellationToken ct = default);
}
