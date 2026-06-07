-- ============================================================
-- HomePlanner — Update 07: Produto base do ingrediente (IngredienteBaseId)
-- Permite agrupar variantes ("Cebola picada", "Cebolas grandes") sob um
-- ingrediente base ("Cebola") para consolidar a lista de compras.
-- Execute APÓS o DatabaseUpdate_06_Tarefas_Criador.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('Ingredientes', 'IngredienteBaseId') IS NULL
    ALTER TABLE Ingredientes ADD IngredienteBaseId INT NULL;
GO

-- FK auto-referenciada (NO ACTION evita ciclo de cascata no SQL Server)
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_Ingredientes_Ingredientes_IngredienteBaseId')
    ALTER TABLE Ingredientes
        ADD CONSTRAINT FK_Ingredientes_Ingredientes_IngredienteBaseId
        FOREIGN KEY (IngredienteBaseId) REFERENCES Ingredientes (Id)
        ON DELETE NO ACTION;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Ingredientes_IngredienteBaseId'
      AND object_id = OBJECT_ID('Ingredientes'))
    CREATE INDEX IX_Ingredientes_IngredienteBaseId
        ON Ingredientes (IngredienteBaseId);
GO

PRINT 'Coluna IngredienteBaseId adicionada em Ingredientes com sucesso!';
GO
