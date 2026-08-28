-- ============================================================
-- HomePlanner — Update 02: Tabela Tarefas (módulo Planner)
-- Execute APÓS o DatabaseCreate.sql
-- ============================================================

USE HomePlannerDb;
GO

IF OBJECT_ID('Tarefas', 'U') IS NULL
CREATE TABLE Tarefas (
    Id                   INT              IDENTITY(1,1) NOT NULL,
    TenantId             UNIQUEIDENTIFIER NOT NULL,
    Titulo               NVARCHAR(300)    NOT NULL,
    Descricao            NVARCHAR(2000)   NULL,
    DataPrevista         DATE             NULL,
    Concluida            BIT              NOT NULL DEFAULT 0,
    DataConclusao        DATETIME2        NULL,
    Recorrencia          INT              NOT NULL DEFAULT 0,
    Visibilidade         INT              NOT NULL DEFAULT 0,
    ResponsavelUsuarioId NVARCHAR(450)    NULL,
    IsDeleted            BIT              NOT NULL DEFAULT 0,
    DeletedAt            DATETIME2        NULL,
    DeletedByUsuarioId   NVARCHAR(450)    NULL,
    DataCriacao          DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DataModificacao      DATETIME2        NULL,
    CriadoPor            NVARCHAR(450)    NULL,
    ModificadoPor        NVARCHAR(450)    NULL,
    CONSTRAINT PK_Tarefas PRIMARY KEY (Id),
    CONSTRAINT FK_Tarefas_Responsavel FOREIGN KEY (ResponsavelUsuarioId)
        REFERENCES AspNetUsers(Id) ON DELETE SET NULL
);
GO

-- TenantId primeiro nos índices compostos
CREATE INDEX IX_Tarefas_TenantId_DataPrevista ON Tarefas (TenantId, DataPrevista);
CREATE INDEX IX_Tarefas_TenantId_Concluida    ON Tarefas (TenantId, Concluida);
GO

PRINT 'Tabela Tarefas criada com sucesso!';
GO
