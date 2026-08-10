using Application.HomePlanner.Repositories.Cardapio;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Unidades de medida ativas (para os dropdowns de ingrediente da receita).</summary>
public class UnidadesController : ApiControllerBase
{
    private readonly IUnidadeMedidaRepository _repo;

    public UnidadesController(IUnidadeMedidaRepository repo) => _repo = repo;

    /// <summary>Lista as unidades de medida ativas.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await _repo.ListarAtivasAsync(ct));
}
