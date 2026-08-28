-- ============================================================
-- HomePlanner — Update 18: Catálogo de produtos recorrentes
-- Produtos que a família compra com frequência (ex.: Leite, Pão). Curado só pelos pais.
-- Cada produto guarda a loja onde é comprado (ListaId → ListasCompra); ao ser selecionado
-- na tela /compras vira um pedido (PedidosCompra) da semana atual, herdando loja e quantidade.
--   • ProdutosRecorrentes : o catálogo persistente do tenant
-- Execute APÓS o DatabaseUpdate_17_ListasCompra.sql
-- ============================================================

USE HomePlannerDb;
GO

-- ── Tabela: ProdutosRecorrentes ─────────────────────────────
IF OBJECT_ID('ProdutosRecorrentes', 'U') IS NULL
BEGIN
    CREATE TABLE ProdutosRecorrentes
    (
        Id                 INT IDENTITY(1,1) NOT NULL,
        TenantId           UNIQUEIDENTIFIER NOT NULL,
        Descricao          NVARCHAR(200) NOT NULL,
        Quantidade         NVARCHAR(100) NULL,
        ListaId            INT NULL,
        Ativo              BIT NOT NULL CONSTRAINT DF_ProdutosRecorrentes_Ativo DEFAULT (1),
        Ordem              INT NOT NULL CONSTRAINT DF_ProdutosRecorrentes_Ordem DEFAULT (0),
        IsDeleted          BIT NOT NULL CONSTRAINT DF_ProdutosRecorrentes_IsDeleted DEFAULT (0),
        DeletedAt          DATETIME2 NULL,
        DeletedByUsuarioId NVARCHAR(450) NULL,
        DataCriacao        DATETIME2 NOT NULL CONSTRAINT DF_ProdutosRecorrentes_DataCriacao DEFAULT (SYSUTCDATETIME()),
        DataModificacao    DATETIME2 NULL,
        CriadoPor          NVARCHAR(450) NULL,
        ModificadoPor      NVARCHAR(450) NULL,
        CONSTRAINT PK_ProdutosRecorrentes PRIMARY KEY (Id),
        CONSTRAINT FK_ProdutosRecorrentes_Lista FOREIGN KEY (ListaId)
            REFERENCES ListasCompra(Id) ON DELETE SET NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProdutosRecorrentes_TenantId_Ordem' AND object_id = OBJECT_ID('ProdutosRecorrentes'))
    CREATE INDEX IX_ProdutosRecorrentes_TenantId_Ordem ON ProdutosRecorrentes (TenantId, Ordem);
GO

PRINT 'Update 18 (produtos recorrentes) aplicado com sucesso!';
GO
