-- ============================================================
-- HomePlanner — Update 11: Notificações Web Push
-- Assinaturas (push subscriptions) por usuário/dispositivo para receber
-- notificações do navegador (lembretes de tarefa, tarefa atribuída, etc.).
-- Execute APÓS o DatabaseUpdate_10_MarcacaoCompra.sql
-- ============================================================

USE HomePlannerDb;
GO

IF OBJECT_ID('InscricoesPush', 'U') IS NULL
BEGIN
    CREATE TABLE InscricoesPush
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        UsuarioId           NVARCHAR(450) NOT NULL,
        Endpoint            NVARCHAR(2048) NOT NULL,
        P256dh              NVARCHAR(256) NOT NULL,
        Auth                NVARCHAR(256) NOT NULL,
        UserAgent           NVARCHAR(512) NULL,
        DataCriacao         DATETIME2 NOT NULL CONSTRAINT DF_InscricoesPush_DataCriacao DEFAULT (SYSUTCDATETIME()),
        UltimoEnvioEm       DATETIME2 NULL,
        IsDeleted           BIT NOT NULL CONSTRAINT DF_InscricoesPush_IsDeleted DEFAULT (0),
        DeletedAt           DATETIME2 NULL,
        DeletedByUsuarioId  NVARCHAR(450) NULL,
        CONSTRAINT PK_InscricoesPush PRIMARY KEY (Id),
        CONSTRAINT FK_InscricoesPush_Usuario FOREIGN KEY (UsuarioId)
            REFERENCES AspNetUsers (Id) ON DELETE CASCADE
    );
END
GO

-- TenantId primeiro no índice composto (padrão do projeto)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_InscricoesPush_Tenant_Usuario'
      AND object_id = OBJECT_ID('InscricoesPush'))
    CREATE INDEX IX_InscricoesPush_Tenant_Usuario
        ON InscricoesPush (TenantId, UsuarioId);
GO

-- Lookup/upsert por endpoint
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_InscricoesPush_Endpoint'
      AND object_id = OBJECT_ID('InscricoesPush'))
    CREATE INDEX IX_InscricoesPush_Endpoint
        ON InscricoesPush (Endpoint);
GO

PRINT 'Tabela InscricoesPush criada com sucesso!';
GO
