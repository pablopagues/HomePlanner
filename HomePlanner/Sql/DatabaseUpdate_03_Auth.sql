-- ============================================================
-- HomePlanner — Update 03: Coluna OnboardingCompleto no Tenant
-- Execute APÓS o DatabaseCreate.sql
-- ============================================================

USE HomePlannerDb;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'OnboardingCompleto' AND Object_ID = Object_ID(N'Tenants'))
BEGIN
    ALTER TABLE Tenants ADD OnboardingCompleto BIT NOT NULL DEFAULT 0;
END
GO

PRINT 'Coluna OnboardingCompleto adicionada com sucesso!';
GO
