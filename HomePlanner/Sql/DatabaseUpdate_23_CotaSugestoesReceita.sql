-- ============================================================
-- HomePlanner — Update 23: Cota mensal de sugestões de receitas por IA
-- Novo contador na tabela de uso mensal (por tenant + yyyyMM), usado para
-- aplicar o limite de sugestões por plano (Gratis=10, Standard=30, Pro=100).
-- Execute APÓS o DatabaseUpdate_22_ConviteMembro.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('UsosImportacaoReceita', 'QuantidadeSugestoes') IS NULL
BEGIN
    ALTER TABLE UsosImportacaoReceita
        ADD QuantidadeSugestoes INT NOT NULL
            CONSTRAINT DF_UsosImportacaoReceita_QuantidadeSugestoes DEFAULT 0;
END
GO

PRINT 'Update 23 (cota de sugestões de receita) aplicado com sucesso!';
GO
