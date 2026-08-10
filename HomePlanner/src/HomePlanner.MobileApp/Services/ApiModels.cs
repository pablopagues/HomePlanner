namespace HomePlanner.MobileApp.Services;

// Espelham (parcialmente) os DTOs da API. JSON em camelCase (case-insensitive na leitura).
// Enums vêm como STRING (a API usa JsonStringEnumConverter).

// ── Auth ──────────────────────────────────────────────────────────────────
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string? DispositivoInfo { get; set; }
}

public class LoginRespostaDTO
{
    public bool Requer2FA { get; set; }
    public string? MfaToken { get; set; }
    public TokensDTO? Tokens { get; set; }
}

public class Confirmar2FARequest
{
    public string MfaToken { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public bool CodigoRecuperacao { get; set; }
    public string? DispositivoInfo { get; set; }
}

public class TokensDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiraEm { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public string UsuarioId { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public bool EhOwner { get; set; }
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
    public string? DispositivoInfo { get; set; }
}

public class RegistroRequest
{
    public string NomeResponsavel { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string PaisId { get; set; } = "BR";
}

// ── Comum ─────────────────────────────────────────────────────────────────
public class ResultadoListagem<T>
{
    public List<T> Itens { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalPaginas { get; set; }
}

// ── Cardápio ──────────────────────────────────────────────────────────────
public class CardapioSemanaDTO
{
    public int Id { get; set; }
    public DateOnly DataInicio { get; set; }
    public string? Nome { get; set; }
    public List<RefeicaoDiaDTO> Refeicoes { get; set; } = new();
}

public class RefeicaoDiaDTO
{
    public int Id { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public string TipoRefeicao { get; set; } = string.Empty;
    public int? ReceitaId { get; set; }
    public string? ReceitaNome { get; set; }
    public int? PorcoesDesejadas { get; set; }
}

public class DefinirRefeicaoRequest
{
    public DateOnly DataInicio { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public string TipoRefeicao { get; set; } = string.Empty;
    public int? ReceitaId { get; set; }
    public int? PorcoesDesejadas { get; set; }
}

// ── Receitas ──────────────────────────────────────────────────────────────
public class ReceitaListaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int NumeroPorcoesBase { get; set; }
    public int? TempoPreparoMinutos { get; set; }
    public int TotalIngredientes { get; set; }
    public int TotalComponentes { get; set; }
}

public class ReceitaDetalheDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? ModoPreparo { get; set; }
    public int NumeroPorcoesBase { get; set; }
    public int? TempoPreparoMinutos { get; set; }
    public string? Observacoes { get; set; }
    public List<ReceitaIngredienteDTO> Ingredientes { get; set; } = new();
    public List<ReceitaComponenteDetalheDTO> Componentes { get; set; } = new();
}

public class ReceitaIngredienteDTO
{
    public int Id { get; set; }
    public int IngredienteId { get; set; }
    public string NomeIngrediente { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public int UnidadeMedidaId { get; set; }
    public string CodigoUnidade { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public bool Opcional { get; set; }
    public int Ordem { get; set; }
}

public class ReceitaComponenteDetalheDTO
{
    public int Id { get; set; }
    public int ComponenteId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int PorcoesDesejadas { get; set; }
    public int Ordem { get; set; }
}

public class ReceitaPersistenciaRequest
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? ModoPreparo { get; set; }
    public int NumeroPorcoesBase { get; set; } = 4;
    public int? TempoPreparoMinutos { get; set; }
    public string? Observacoes { get; set; }
    public List<ReceitaIngredientePersistReq> Ingredientes { get; set; } = new();
    public List<ReceitaComponentePersistReq> Componentes { get; set; } = new();
}

public class ReceitaIngredientePersistReq
{
    public int Id { get; set; }
    public int IngredienteId { get; set; }
    public decimal Quantidade { get; set; }
    public int UnidadeMedidaId { get; set; }
    public string? Observacao { get; set; }
    public bool Opcional { get; set; }
    public int Ordem { get; set; }
}

public class ReceitaComponentePersistReq
{
    public int Id { get; set; }
    public int ReceitaComponenteId { get; set; }
    public string NomeComponente { get; set; } = string.Empty;
    public int PorcoesDesejadas { get; set; }
    public int Ordem { get; set; }
}

// ── Ingredientes ──────────────────────────────────────────────────────────
public class IngredienteListaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Categoria { get; set; }
}

public class IngredientePersistenciaRequest
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public int? UnidadeMedidaPadraoId { get; set; }
}

// ── Modelos de semana ─────────────────────────────────────────────────────
public class ModeloSemanaListaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int TotalRefeicoes { get; set; }
}

// ── Segurança (2FA) ───────────────────────────────────────────────────────
public class ChaveAutenticadorDTO
{
    public string ChaveRaw { get; set; } = string.Empty;
    public string ChaveFormatada { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class StatusDoisFatores
{
    public bool Ativo { get; set; }
}

// ── Conta ─────────────────────────────────────────────────────────────────
public class AtualizarEmpresaRequest
{
    public string NomeResponsavel { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Province { get; set; }
}

// ── Planner ───────────────────────────────────────────────────────────────
public class TarefaListaDTO
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateOnly? DataPrevista { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFim { get; set; }
    public bool Concluida { get; set; }
    public string Recorrencia { get; set; } = "Nenhuma";
    public string Visibilidade { get; set; } = "Familia";
    public string? ResponsavelUsuarioId { get; set; }
    public string? ResponsavelNome { get; set; }
    public bool NotificarResponsaveis { get; set; }
}

public class TarefaPersistenciaRequest
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateOnly? DataPrevista { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFim { get; set; }
    public string Recorrencia { get; set; } = "Nenhuma";
    public string Visibilidade { get; set; } = "Familia";
    public string? ResponsavelUsuarioId { get; set; }
    public bool NotificarResponsaveis { get; set; }
}

public class MembroSimplesDTO
{
    public string UsuarioId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
}

// ── Compras ───────────────────────────────────────────────────────────────
public class ListaComprasDTO
{
    public DateOnly DataInicio { get; set; }
    public int TotalReceitas { get; set; }
    public List<ItemComprasDTO> Itens { get; set; } = new();
}

public class ItemComprasDTO
{
    public int IngredienteId { get; set; }
    public string NomeIngrediente { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string QuantidadeFormatada { get; set; } = string.Empty;
    public int? ListaId { get; set; }
}

public class PedidoMembroGrupoDTO
{
    public string SolicitanteNome { get; set; } = string.Empty;
    public List<PedidoCompraDTO> Itens { get; set; } = new();
}

public class PedidoCompraDTO
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Quantidade { get; set; }
    public bool Comprado { get; set; }
    public bool PodeEditar { get; set; }
}

public class PedidoCompraPersistenciaRequest
{
    public DateOnly DataInicioSemana { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Quantidade { get; set; }
    public string? SolicitanteUsuarioId { get; set; }
}

public class FeedbackRequest
{
    public string Tipo { get; set; } = "Sugestao";
    public string Mensagem { get; set; } = string.Empty;
    public string? PaginaAtual { get; set; }
}

// ── Família ───────────────────────────────────────────────────────────────
public class MembroFamiliaDetalheDTO
{
    public string UsuarioId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Papel { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public bool SenhaDefinida { get; set; }
}

public class ResumoFamiliaDTO
{
    public int MembrosAtivos { get; set; }
    public int LimiteMembros { get; set; }
}

public class NovoMembroRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Papel { get; set; } = "Membro";
}

// ── Conta / Configuração / Assinatura ─────────────────────────────────────
public class EmpresaDetalheDTO
{
    public string NomeResponsavel { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmado { get; set; }
    public string PaisId { get; set; } = string.Empty;
}

public class ConfiguracaoFamiliaDTO
{
    public int TamanhoFamiliaPadrao { get; set; } = 4;
    public string FusoHorario { get; set; } = "America/Toronto";
    public int MinutosAntecedenciaLembrete { get; set; } = 15;
    public List<string> TiposRefeicaoAtivos { get; set; } = new() { "Almoco" };
}

public class AssinaturaAtualDTO
{
    public string Plano { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int LimiteMembros { get; set; }
    public int LimiteImportacoesReceita { get; set; }
    public int? DiasRestantesTrial { get; set; }
    public bool TemAssinaturaStripe { get; set; }
}
