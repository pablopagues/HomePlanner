-- ============================================================
-- HomePlanner — Update 09: Marcações compartilhadas da lista de compras
-- "Já tenho/comprado" dos itens gerados do cardápio, agora no banco (compartilhado
-- pela família) em vez de localStorage. Chave: semana + ingrediente (produto base).
-- Execute APÓS o DatabaseUpdate_08_ReceitaComponente.sql
-- ============================================================

USE HomePlannerDb;
GO

IF OBJECT_ID('MarcacoesCompra', 'U') IS NULL
BEGIN
    CREATE TABLE MarcacoesCompra
    (
        Id               INT IDENTITY(1,1) NOT NULL,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        DataInicioSemana DATE NOT NULL,
        IngredienteId    INT NOT NULL,
        Comprado         BIT NOT NULL CONSTRAINT DF_MarcacoesCompra_Comprado DEFAULT (0),
        DataCompra       DATETIME2 NULL,
        DataCriacao      DATETIME2 NOT NULL CONSTRAINT DF_MarcacoesCompra_DataCriacao DEFAULT (SYSUTCDATETIME()),
        DataModificacao  DATETIME2 NULL,
        CriadoPor        NVARCHAR(450) NULL,
        ModificadoPor    NVARCHAR(450) NULL,
        CONSTRAINT PK_MarcacoesCompra PRIMARY KEY (Id)
    );
END
GO

-- Uma marcação por ingrediente/semana dentro do tenant
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_MarcacoesCompra_Unico'
      AND object_id = OBJECT_ID('MarcacoesCompra'))
    CREATE UNIQUE INDEX IX_MarcacoesCompra_Unico
        ON MarcacoesCompra (TenantId, DataInicioSemana, IngredienteId);
GO

PRINT 'Tabela MarcacoesCompra criada com sucesso!';
GO
