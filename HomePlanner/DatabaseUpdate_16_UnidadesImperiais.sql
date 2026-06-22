-- ============================================================
-- HomePlanner — Update 16: Unidades de medida imperiais
-- Adiciona oz, lb, fl oz, cup, pint, quart para suportar receitas em inglês.
-- Permite o switch métrico/imperial (conversão dentro da mesma dimensão).
-- Tipo: 1=Massa, 2=Volume. FatorParaBase: g para massa, ml para volume.
-- Idempotente (MERGE). Execute APÓS o DatabaseUpdate_15_CotaImportacaoReceita.sql
-- ============================================================

USE HomePlannerDb;
GO

SET IDENTITY_INSERT UnidadesMedida ON;

MERGE INTO UnidadesMedida AS target
USING (VALUES
    (13, 'oz',    'Onça',          'UnidadeMedida_Onca',         1, 28.349523),
    (14, 'lb',    'Libra',         'UnidadeMedida_Libra',        1, 453.592370),
    (15, 'floz',  'Onça líquida',  'UnidadeMedida_OncaLiquida',  2, 29.573530),
    (16, 'cup',   'Cup (US 237ml)','UnidadeMedida_Cup',          2, 236.588236),
    (17, 'pint',  'Pint',          'UnidadeMedida_Pint',         2, 473.176473),
    (18, 'quart', 'Quart',         'UnidadeMedida_Quart',        2, 946.352946)
) AS source (Id, Codigo, Nome, ChaveTraducao, Tipo, FatorParaBase)
ON target.Id = source.Id
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Codigo, Nome, ChaveTraducao, Tipo, FatorParaBase, IsAtivo)
    VALUES (source.Id, source.Codigo, source.Nome, source.ChaveTraducao, source.Tipo, source.FatorParaBase, 1);

SET IDENTITY_INSERT UnidadesMedida OFF;
GO

PRINT 'Update 16 (unidades imperiais) aplicado com sucesso!';
GO
