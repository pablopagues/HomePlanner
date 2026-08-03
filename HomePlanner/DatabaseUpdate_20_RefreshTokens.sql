-- ============================================================
-- HomePlanner — Update 20: Refresh Tokens (API mobile / JWT)
-- Guarda o HASH (nunca o valor em claro) dos refresh tokens emitidos para os
-- apps. Rotacionados a cada uso; NÃO tem filtro global de tenant (o refresh
-- acontece antes de o tenant estar resolvido).
-- Execute APÓS o DatabaseUpdate_19_FotoReceita.sql
-- ============================================================

USE HomePlannerDb;
GO

IF OBJECT_ID('RefreshTokens', 'U') IS NULL
BEGIN
    CREATE TABLE RefreshTokens
    (
        Id               BIGINT IDENTITY(1,1) NOT NULL,
        TokenHash        NVARCHAR(128) NOT NULL,
        UsuarioId        NVARCHAR(450) NOT NULL,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        CriadoEm         DATETIME2 NOT NULL CONSTRAINT DF_RefreshTokens_CriadoEm DEFAULT (SYSUTCDATETIME()),
        ExpiraEm         DATETIME2 NOT NULL,
        RevogadoEm       DATETIME2 NULL,
        DispositivoInfo  NVARCHAR(512) NULL,
        CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
        CONSTRAINT FK_RefreshTokens_Usuario FOREIGN KEY (UsuarioId)
            REFERENCES AspNetUsers (Id) ON DELETE CASCADE
    );
END
GO

-- Lookup por hash na validação do refresh (único)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_RefreshTokens_TokenHash'
      AND object_id = OBJECT_ID('RefreshTokens'))
    CREATE UNIQUE INDEX IX_RefreshTokens_TokenHash
        ON RefreshTokens (TokenHash);
GO

-- Listagem/limpeza de tokens ativos por usuário
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_RefreshTokens_Usuario_Revogado'
      AND object_id = OBJECT_ID('RefreshTokens'))
    CREATE INDEX IX_RefreshTokens_Usuario_Revogado
        ON RefreshTokens (UsuarioId, RevogadoEm);
GO

PRINT 'Tabela RefreshTokens criada com sucesso!';
GO
