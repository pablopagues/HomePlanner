-- ============================================================
-- HomePlanner — Update 08: Receitas compostas (ReceitasComponentes)
-- Liga um prato composto a outras receitas usadas como componentes
-- (referência viva, dimensionada por porções).
-- Execute APÓS o DatabaseUpdate_07_Ingrediente_Base.sql
-- ============================================================

USE HomePlannerDb;
GO

IF OBJECT_ID('ReceitasComponentes', 'U') IS NULL
BEGIN
    CREATE TABLE ReceitasComponentes
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        ReceitaPaiId        INT NOT NULL,
        ReceitaComponenteId INT NOT NULL,
        PorcoesDesejadas    INT NOT NULL,
        Ordem               INT NOT NULL,
        IsDeleted           BIT NOT NULL CONSTRAINT DF_ReceitasComponentes_IsDeleted DEFAULT (0),
        CONSTRAINT PK_ReceitasComponentes PRIMARY KEY (Id),
        CONSTRAINT FK_ReceitasComponentes_Pai
            FOREIGN KEY (ReceitaPaiId) REFERENCES Receitas (Id) ON DELETE CASCADE,
        CONSTRAINT FK_ReceitasComponentes_Componente
            FOREIGN KEY (ReceitaComponenteId) REFERENCES Receitas (Id) ON DELETE NO ACTION
    );
END
GO

-- Um componente só uma vez por prato (ignorando os deletados)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ReceitasComponentes_Unico'
      AND object_id = OBJECT_ID('ReceitasComponentes'))
    CREATE UNIQUE INDEX IX_ReceitasComponentes_Unico
        ON ReceitasComponentes (ReceitaPaiId, ReceitaComponenteId)
        WHERE IsDeleted = 0;
GO

PRINT 'Tabela ReceitasComponentes criada com sucesso!';
GO
