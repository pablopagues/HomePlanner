-- ============================================================
-- HomePlanner — Update 04: Colunas Stripe na ConfiguracaoAssinatura
-- Execute APÓS o DatabaseCreate.sql
-- ============================================================

USE HomePlannerDb;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'DataExpiracao' AND Object_ID = Object_ID(N'ConfiguracoesAssinatura'))
BEGIN
    ALTER TABLE ConfiguracoesAssinatura ADD DataExpiracao DATETIME2 NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'CanceladoAoFimDoPeriodo' AND Object_ID = Object_ID(N'ConfiguracoesAssinatura'))
BEGIN
    ALTER TABLE ConfiguracoesAssinatura ADD CanceladoAoFimDoPeriodo BIT NOT NULL DEFAULT 0;
END
GO

PRINT 'Colunas Stripe adicionadas com sucesso!';
GO
