-- ============================================================
-- HomePlanner — Update 15: Cota mensal de importação de receitas
-- Contador por tenant + mês-calendário (yyyyMM), usado para aplicar o
-- limite de importações de receita por plano (Gratis=10, Standard=50, Pro=200).
-- Execute APÓS o DatabaseUpdate_14_NotificarPais.sql
-- ============================================================

USE HomePlannerDb;
GO

IF OBJECT_ID('UsosImportacaoReceita', 'U') IS NULL
BEGIN
    CREATE TABLE UsosImportacaoReceita (
        Id          INT              NOT NULL IDENTITY(1,1),
        TenantId    UNIQUEIDENTIFIER NOT NULL,
        AnoMes      INT              NOT NULL,  -- formato yyyyMM (ex.: 202606)
        Quantidade  INT              NOT NULL DEFAULT 0,
        CONSTRAINT PK_UsosImportacaoReceita PRIMARY KEY (Id)
    );

    CREATE UNIQUE INDEX IX_UsosImportacaoReceita_Tenant_AnoMes
        ON UsosImportacaoReceita (TenantId, AnoMes);
END
GO

PRINT 'Update 15 (cota de importação de receita) aplicado com sucesso!';
GO
