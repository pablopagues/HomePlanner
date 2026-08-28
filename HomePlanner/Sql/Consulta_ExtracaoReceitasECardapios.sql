/* =============================================================================
   Extração de Receitas e Cardápios de um Tenant  (SOMENTE LEITURA)
   SQL Server 2017+ (usa STRING_AGG)

   Como usar: preencha @Email OU @TenantId abaixo e execute tudo.
   Retorna 6 result sets:
     1) Identificação do tenant
     2) Receitas (com ingredientes e componentes agregados em uma linha)
     3) Receitas x Ingredientes (detalhado, uma linha por ingrediente)
     4) Receitas compostas x Componentes
     5) Cardápios semanais usados (PlanejamentosSemanais x RefeicoesDia)
     6) Modelos de semana (templates) x RefeicoesModelo
   ============================================================================= */

SET NOCOUNT ON;

DECLARE @Email    nvarchar(256) = N'pablo.pagues@gmail.com';  -- e-mail do tenant
DECLARE @TenantId uniqueidentifier = NULL;                    -- ou informe o Guid direto

-- Incluir registros logicamente excluídos (IsDeleted = 1)? 0 = não, 1 = sim
DECLARE @IncluirExcluidos bit = 0;

IF @TenantId IS NULL
    SELECT @TenantId = Id FROM dbo.Tenants WHERE Email = @Email;

IF @TenantId IS NULL
BEGIN
    RAISERROR('Tenant nao encontrado. Verifique @Email ou informe @TenantId.', 16, 1);
    RETURN;
END

/* ---------- 1) Tenant --------------------------------------------------- */
SELECT
    t.Id                AS TenantId,
    t.NomeResponsavel,
    t.Email,
    t.PaisId,
    t.Ativo,
    t.OnboardingCompleto,
    t.DataCriacao
FROM dbo.Tenants t
WHERE t.Id = @TenantId;

/* ---------- 2) Receitas (resumo com ingredientes agregados) -------------- */
SELECT
    r.Id                    AS ReceitaId,
    r.Nome,
    r.NumeroPorcoesBase     AS PorcoesBase,
    r.TempoPreparoMinutos,
    r.UrlOrigem,
    r.UrlImagem,
    CASE WHEN r.Foto IS NOT NULL THEN 'Sim' ELSE 'Nao' END AS TemFotoNoBanco,
    r.Observacoes,
    r.ModoPreparo,
    ing.Ingredientes,
    comp.Componentes,
    r.IsDeleted,
    r.DataCriacao,
    r.DataModificacao
FROM dbo.Receitas r
OUTER APPLY (
    SELECT Ingredientes = STRING_AGG(
               CONVERT(nvarchar(30), CAST(ri.Quantidade AS float))
               + N' ' + um.Codigo
               + N' ' + i.Nome
               + CASE WHEN ri.Opcional = 1 THEN N' (opcional)' ELSE N'' END
               + CASE WHEN NULLIF(ri.Observacao, N'') IS NOT NULL
                      THEN N' [' + ri.Observacao + N']' ELSE N'' END,
               N' | ') WITHIN GROUP (ORDER BY ri.Ordem, i.Nome)
    FROM dbo.ReceitasIngredientes ri
    JOIN dbo.Ingredientes    i  ON i.Id  = ri.IngredienteId
    JOIN dbo.UnidadesMedida  um ON um.Id = ri.UnidadeMedidaId
    WHERE ri.ReceitaId = r.Id
      AND (@IncluirExcluidos = 1 OR ri.IsDeleted = 0)
) ing
OUTER APPLY (
    SELECT Componentes = STRING_AGG(
               rc2.Nome + N' (' + CONVERT(nvarchar(10), rc.PorcoesDesejadas) + N' porcoes)',
               N' | ') WITHIN GROUP (ORDER BY rc.Ordem)
    FROM dbo.ReceitasComponentes rc
    JOIN dbo.Receitas rc2 ON rc2.Id = rc.ReceitaComponenteId
    WHERE rc.ReceitaPaiId = r.Id
      AND (@IncluirExcluidos = 1 OR rc.IsDeleted = 0)
) comp
WHERE r.TenantId = @TenantId
  AND (@IncluirExcluidos = 1 OR r.IsDeleted = 0)
ORDER BY r.Nome;

/* ---------- 3) Receitas x Ingredientes (detalhado) ----------------------- */
SELECT
    r.Id            AS ReceitaId,
    r.Nome          AS Receita,
    ri.Ordem,
    i.Nome          AS Ingrediente,
    i.Categoria,
    ib.Nome         AS IngredienteBase,
    ri.Quantidade,
    um.Codigo       AS Unidade,
    um.Nome         AS UnidadeNome,
    ri.Opcional,
    ri.Observacao,
    ri.IsDeleted
FROM dbo.Receitas r
JOIN dbo.ReceitasIngredientes ri ON ri.ReceitaId = r.Id
JOIN dbo.Ingredientes    i  ON i.Id  = ri.IngredienteId
JOIN dbo.UnidadesMedida  um ON um.Id = ri.UnidadeMedidaId
LEFT JOIN dbo.Ingredientes ib ON ib.Id = i.IngredienteBaseId
WHERE r.TenantId = @TenantId
  AND (@IncluirExcluidos = 1 OR (r.IsDeleted = 0 AND ri.IsDeleted = 0))
ORDER BY r.Nome, ri.Ordem, i.Nome;

/* ---------- 4) Receitas compostas x Componentes -------------------------- */
SELECT
    pai.Id      AS ReceitaPaiId,
    pai.Nome    AS ReceitaPai,
    rc.Ordem,
    filho.Id    AS ComponenteId,
    filho.Nome  AS Componente,
    rc.PorcoesDesejadas,
    rc.IsDeleted
FROM dbo.ReceitasComponentes rc
JOIN dbo.Receitas pai   ON pai.Id   = rc.ReceitaPaiId
JOIN dbo.Receitas filho ON filho.Id = rc.ReceitaComponenteId
WHERE pai.TenantId = @TenantId
  AND (@IncluirExcluidos = 1 OR (rc.IsDeleted = 0 AND pai.IsDeleted = 0))
ORDER BY pai.Nome, rc.Ordem;

/* ---------- 5) Cardápios semanais já usados ------------------------------ */
SELECT
    ps.Id           AS PlanejamentoId,
    ps.DataInicio,
    ps.Nome         AS NomePlanejamento,
    ms.Nome         AS ModeloOrigem,
    rd.DiaSemana    AS DiaSemanaNum,
    CASE rd.DiaSemana
        WHEN 1 THEN 'Segunda' WHEN 2 THEN 'Terca'  WHEN 3 THEN 'Quarta'
        WHEN 4 THEN 'Quinta'  WHEN 5 THEN 'Sexta'  WHEN 6 THEN 'Sabado'
        WHEN 7 THEN 'Domingo' END                       AS DiaSemana,
    rd.TipoRefeicao AS TipoRefeicaoNum,
    CASE rd.TipoRefeicao
        WHEN 1 THEN 'Cafe da Manha' WHEN 2 THEN 'Almoco' WHEN 3 THEN 'Lanche'
        WHEN 4 THEN 'Jantar'        WHEN 5 THEN 'Ceia'  END AS TipoRefeicao,
    rd.ReceitaId,
    rec.Nome        AS Receita,
    rd.PorcoesDesejadas,
    rd.Observacao,
    ps.IsDeleted    AS PlanejamentoExcluido,
    rd.IsDeleted    AS RefeicaoExcluida,
    ps.DataCriacao
FROM dbo.PlanejamentosSemanais ps
LEFT JOIN dbo.RefeicoesDia rd ON rd.PlanejamentoSemanalId = ps.Id
                             AND (@IncluirExcluidos = 1 OR rd.IsDeleted = 0)
LEFT JOIN dbo.Receitas      rec ON rec.Id = rd.ReceitaId
LEFT JOIN dbo.ModelosSemana ms  ON ms.Id  = ps.ModeloSemanaOrigemId
WHERE ps.TenantId = @TenantId
  AND (@IncluirExcluidos = 1 OR ps.IsDeleted = 0)
ORDER BY ps.DataInicio DESC, rd.DiaSemana, rd.TipoRefeicao;

/* ---------- 6) Modelos de semana (templates) ----------------------------- */
SELECT
    ms.Id           AS ModeloSemanaId,
    ms.Nome         AS Modelo,
    ms.Descricao,
    rm.DiaSemana    AS DiaSemanaNum,
    CASE rm.DiaSemana
        WHEN 1 THEN 'Segunda' WHEN 2 THEN 'Terca'  WHEN 3 THEN 'Quarta'
        WHEN 4 THEN 'Quinta'  WHEN 5 THEN 'Sexta'  WHEN 6 THEN 'Sabado'
        WHEN 7 THEN 'Domingo' END                       AS DiaSemana,
    rm.TipoRefeicao AS TipoRefeicaoNum,
    CASE rm.TipoRefeicao
        WHEN 1 THEN 'Cafe da Manha' WHEN 2 THEN 'Almoco' WHEN 3 THEN 'Lanche'
        WHEN 4 THEN 'Jantar'        WHEN 5 THEN 'Ceia'  END AS TipoRefeicao,
    rm.ReceitaId,
    rec.Nome        AS Receita,
    rm.PorcoesDesejadas,
    rm.Observacao,
    ms.IsDeleted    AS ModeloExcluido,
    rm.IsDeleted    AS RefeicaoExcluida
FROM dbo.ModelosSemana ms
LEFT JOIN dbo.RefeicoesModelo rm ON rm.ModeloSemanaId = ms.Id
                                AND (@IncluirExcluidos = 1 OR rm.IsDeleted = 0)
LEFT JOIN dbo.Receitas rec ON rec.Id = rm.ReceitaId
WHERE ms.TenantId = @TenantId
  AND (@IncluirExcluidos = 1 OR ms.IsDeleted = 0)
ORDER BY ms.Nome, rm.DiaSemana, rm.TipoRefeicao;
