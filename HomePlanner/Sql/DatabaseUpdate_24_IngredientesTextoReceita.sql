-- ============================================================
-- HomePlanner — Update 24: Campo "Ingredientes (texto)" na receita
-- Separa o texto livre de ingredientes (colado/importado, que depois é
-- "lançado" em ingredientes estruturados) do campo Observações, que volta
-- a ser só observações.
--
-- Migração dos dados: receitas importadas gravavam os ingredientes em
-- Observacoes com um cabeçalho ("Ingredientes originais:" na web,
-- "Ingredientes (importados):" no app). Essas são movidas para a coluna
-- nova, sem o cabeçalho. As demais ficam como estão — se alguma tiver
-- ingredientes digitados à mão em Observações, mova pela tela de edição.
-- Execute APÓS o DatabaseUpdate_23_CotaSugestoesReceita.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('Receitas', 'IngredientesTexto') IS NULL
BEGIN
    ALTER TABLE Receitas ADD IngredientesTexto NVARCHAR(MAX) NULL;
END
GO

-- Move os ingredientes importados (remove a 1ª linha = cabeçalho).
UPDATE Receitas
SET IngredientesTexto = LTRIM(RTRIM(
        CASE WHEN CHARINDEX(CHAR(10), Observacoes) > 0
             THEN STUFF(Observacoes, 1, CHARINDEX(CHAR(10), Observacoes), '')
             ELSE '' END)),
    Observacoes = NULL
WHERE IngredientesTexto IS NULL
  AND (Observacoes LIKE 'Ingredientes originais:%'
       OR Observacoes LIKE 'Ingredientes (importados):%');
GO

PRINT 'Update 24 (ingredientes em texto na receita) aplicado com sucesso!';
GO
