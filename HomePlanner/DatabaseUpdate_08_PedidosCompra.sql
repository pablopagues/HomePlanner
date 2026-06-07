-- ============================================================
-- HomePlanner — Update 07: Tabela PedidosCompra (lista de compras)
-- Pedidos avulsos de membros da família, complementando os itens do cardápio.
-- Execute APÓS o DatabaseCreate.sql
-- ============================================================

USE HomePlannerDb;
GO

IF OBJECT_ID('PedidosCompra', 'U') IS NULL
CREATE TABLE PedidosCompra (
    Id                   INT              IDENTITY(1,1) NOT NULL,
    TenantId             UNIQUEIDENTIFIER NOT NULL,
    DataInicioSemana     DATE             NOT NULL,
    Descricao            NVARCHAR(300)    NOT NULL,
    Quantidade           NVARCHAR(100)    NULL,
    SolicitanteUsuarioId NVARCHAR(450)    NULL,
    Comprado             BIT              NOT NULL DEFAULT 0,
    DataCompra           DATETIME2        NULL,
    IsDeleted            BIT              NOT NULL DEFAULT 0,
    DeletedAt            DATETIME2        NULL,
    DeletedByUsuarioId   NVARCHAR(450)    NULL,
    DataCriacao          DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DataModificacao      DATETIME2        NULL,
    CriadoPor            NVARCHAR(450)    NULL,
    ModificadoPor        NVARCHAR(450)    NULL,
    CONSTRAINT PK_PedidosCompra PRIMARY KEY (Id),
    CONSTRAINT FK_PedidosCompra_Solicitante FOREIGN KEY (SolicitanteUsuarioId)
        REFERENCES AspNetUsers(Id) ON DELETE SET NULL
);
GO

-- TenantId primeiro no índice composto
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PedidosCompra_TenantId_DataInicioSemana')
    CREATE INDEX IX_PedidosCompra_TenantId_DataInicioSemana
        ON PedidosCompra (TenantId, DataInicioSemana);
GO

PRINT 'Tabela PedidosCompra criada com sucesso!';
GO
