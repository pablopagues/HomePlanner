-- ============================================================
-- HomePlanner — Update 17: Listas de compra customizadas (lojas)
-- Cria as listas/lojas do tenant (ex.: Walmart, Costco, Farmácia) para onde os itens
-- do cardápio e os pedidos de membros podem ser remanejados.
--   • ListasCompra                : a loja em si (persistente, reutilizável entre semanas)
--   • PreferenciasLojaIngrediente : loja padrão "aprendida" por ingrediente (auto-atribuição)
--   • PedidosCompra.ListaId       : loja para onde o pedido de um membro foi remanejado
-- Execute APÓS o DatabaseUpdate_16_UnidadesImperiais.sql
-- ============================================================

USE HomePlannerDb;
GO

-- ── Tabela: ListasCompra ────────────────────────────────────
IF OBJECT_ID('ListasCompra', 'U') IS NULL
BEGIN
    CREATE TABLE ListasCompra
    (
        Id                 INT IDENTITY(1,1) NOT NULL,
        TenantId           UNIQUEIDENTIFIER NOT NULL,
        Nome               NVARCHAR(100) NOT NULL,
        Icone              NVARCHAR(60)  NULL,
        Cor                NVARCHAR(20)  NULL,
        Ordem              INT NOT NULL CONSTRAINT DF_ListasCompra_Ordem DEFAULT (0),
        IsDeleted          BIT NOT NULL CONSTRAINT DF_ListasCompra_IsDeleted DEFAULT (0),
        DeletedAt          DATETIME2 NULL,
        DeletedByUsuarioId NVARCHAR(450) NULL,
        DataCriacao        DATETIME2 NOT NULL CONSTRAINT DF_ListasCompra_DataCriacao DEFAULT (SYSUTCDATETIME()),
        DataModificacao    DATETIME2 NULL,
        CriadoPor          NVARCHAR(450) NULL,
        ModificadoPor      NVARCHAR(450) NULL,
        CONSTRAINT PK_ListasCompra PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ListasCompra_TenantId_Ordem' AND object_id = OBJECT_ID('ListasCompra'))
    CREATE INDEX IX_ListasCompra_TenantId_Ordem ON ListasCompra (TenantId, Ordem);
GO

-- ── Tabela: PreferenciasLojaIngrediente ─────────────────────
IF OBJECT_ID('PreferenciasLojaIngrediente', 'U') IS NULL
BEGIN
    CREATE TABLE PreferenciasLojaIngrediente
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        IngredienteId   INT NOT NULL,
        ListaId         INT NOT NULL,
        DataCriacao     DATETIME2 NOT NULL CONSTRAINT DF_PrefLojaIng_DataCriacao DEFAULT (SYSUTCDATETIME()),
        DataModificacao DATETIME2 NULL,
        CriadoPor       NVARCHAR(450) NULL,
        ModificadoPor   NVARCHAR(450) NULL,
        CONSTRAINT PK_PreferenciasLojaIngrediente PRIMARY KEY (Id),
        CONSTRAINT FK_PrefLojaIng_Lista FOREIGN KEY (ListaId)
            REFERENCES ListasCompra(Id)
    );
END
GO

-- Uma preferência por ingrediente dentro do tenant
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PrefLojaIng_Unico' AND object_id = OBJECT_ID('PreferenciasLojaIngrediente'))
    CREATE UNIQUE INDEX IX_PrefLojaIng_Unico ON PreferenciasLojaIngrediente (TenantId, IngredienteId);
GO

-- ── PedidosCompra.ListaId ───────────────────────────────────
IF COL_LENGTH('PedidosCompra', 'ListaId') IS NULL
    ALTER TABLE PedidosCompra ADD ListaId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PedidosCompra_Lista')
    ALTER TABLE PedidosCompra
        ADD CONSTRAINT FK_PedidosCompra_Lista FOREIGN KEY (ListaId)
            REFERENCES ListasCompra(Id) ON DELETE SET NULL;
GO

PRINT 'Update 17 (listas de compra customizadas) aplicado com sucesso!';
GO
