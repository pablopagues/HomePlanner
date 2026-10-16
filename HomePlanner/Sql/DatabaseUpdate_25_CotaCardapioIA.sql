-- ============================================================
-- HomePlanner — Update 25: Cota mensal de cardápios da semana gerados por IA
-- Novo contador na tabela de uso mensal (por tenant + yyyyMM), usado para
-- aplicar o limite de cardápios IA por plano (Gratis=4, Standard=15, Pro=50).
-- Execute APÓS o DatabaseUpdate_24_IngredientesTextoReceita.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('UsosImportacaoReceita', 'QuantidadeCardapiosIA') IS NULL
BEGIN
    ALTER TABLE UsosImportacaoReceita
        ADD QuantidadeCardapiosIA INT NOT NULL
            CONSTRAINT DF_UsosImportacaoReceita_QuantidadeCardapiosIA DEFAULT 0;
END
GO

PRINT 'Update 25 (cota de cardápios por IA) aplicado com sucesso!';
GO
