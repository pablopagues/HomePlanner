-- ============================================================
-- HomePlanner — Update 26: Rascunho automático do cardápio por IA
-- AgendamentosCardapioIA: em que dia/hora a IA prepara a semana seguinte e com
--   quais regras (uma linha por tenant).
-- RascunhosCardapioIA: o cardápio preparado, aguardando a família revisar
--   (uma linha por tenant + semana; também evita gerar duas vezes).
-- Execute APÓS o DatabaseUpdate_25_CotaCardapioIA.sql
-- ============================================================

USE HomePlannerDb;
GO

IF OBJECT_ID('AgendamentosCardapioIA', 'U') IS NULL
BEGIN
    CREATE TABLE AgendamentosCardapioIA (
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        Ativo            BIT              NOT NULL DEFAULT 0,
        DiaSemana        INT              NOT NULL DEFAULT 5,   -- 1=segunda … 7=domingo (5 = sexta)
        Hora             INT              NOT NULL DEFAULT 9,   -- hora local da família (0–23)
        ParametrosJson   NVARCHAR(MAX)    NOT NULL,
        DataCriacao      DATETIME2        NOT NULL,
        DataModificacao  DATETIME2        NULL,
        CriadoPor        NVARCHAR(450)    NULL,
        ModificadoPor    NVARCHAR(450)    NULL,
        CONSTRAINT PK_AgendamentosCardapioIA PRIMARY KEY (TenantId)
    );
    PRINT 'Tabela AgendamentosCardapioIA criada.';
END
GO

IF OBJECT_ID('RascunhosCardapioIA', 'U') IS NULL
BEGIN
    CREATE TABLE RascunhosCardapioIA (
        Id            INT              NOT NULL IDENTITY(1,1),
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        DataInicio    DATE             NOT NULL,  -- segunda-feira da semana
        Status        INT              NOT NULL,  -- 0 Gerando, 1 Pronto, 2 Aplicado, 3 Descartado, 4 Ignorado, 5 Falhou
        ConteudoJson  NVARCHAR(MAX)    NULL,
        Tentativas    INT              NOT NULL DEFAULT 0,
        UltimoErro    NVARCHAR(100)    NULL,
        CriadoEm      DATETIME2        NOT NULL,
        AtualizadoEm  DATETIME2        NOT NULL,
        CONSTRAINT PK_RascunhosCardapioIA PRIMARY KEY (Id)
    );

    CREATE UNIQUE INDEX IX_RascunhosCardapioIA_Tenant_DataInicio
        ON RascunhosCardapioIA (TenantId, DataInicio);
    PRINT 'Tabela RascunhosCardapioIA criada.';
END
GO

PRINT 'Update 26 (cardápio automático por IA) aplicado com sucesso!';
GO
