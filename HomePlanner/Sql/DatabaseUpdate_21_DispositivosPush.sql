-- ============================================================
-- HomePlanner — Update 21: Dispositivos Push (FCM / apps MAUI)
-- Tokens de registro FCM por usuário/aparelho, para push nativo (Android + iOS
-- via APNs). Independente de InscricoesPush (Web Push do site).
-- Execute APÓS o DatabaseUpdate_20_RefreshTokens.sql
-- ============================================================

USE HomePlannerDb;
GO

IF OBJECT_ID('DispositivosPush', 'U') IS NULL
BEGIN
    CREATE TABLE DispositivosPush
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        UsuarioId           NVARCHAR(450) NOT NULL,
        Token               NVARCHAR(512) NOT NULL,
        Plataforma          NVARCHAR(20) NULL,
        DispositivoInfo     NVARCHAR(512) NULL,
        DataCriacao         DATETIME2 NOT NULL CONSTRAINT DF_DispositivosPush_DataCriacao DEFAULT (SYSUTCDATETIME()),
        UltimoEnvioEm       DATETIME2 NULL,
        IsDeleted           BIT NOT NULL CONSTRAINT DF_DispositivosPush_IsDeleted DEFAULT (0),
        DeletedAt           DATETIME2 NULL,
        DeletedByUsuarioId  NVARCHAR(450) NULL,
        CONSTRAINT PK_DispositivosPush PRIMARY KEY (Id),
        CONSTRAINT FK_DispositivosPush_Usuario FOREIGN KEY (UsuarioId)
            REFERENCES AspNetUsers (Id) ON DELETE CASCADE
    );
END
GO

-- TenantId primeiro no índice composto (padrão do projeto)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_DispositivosPush_Tenant_Usuario'
      AND object_id = OBJECT_ID('DispositivosPush'))
    CREATE INDEX IX_DispositivosPush_Tenant_Usuario
        ON DispositivosPush (TenantId, UsuarioId);
GO

-- Lookup/upsert por token
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_DispositivosPush_Token'
      AND object_id = OBJECT_ID('DispositivosPush'))
    CREATE INDEX IX_DispositivosPush_Token
        ON DispositivosPush (Token);
GO

PRINT 'Tabela DispositivosPush criada com sucesso!';
GO
