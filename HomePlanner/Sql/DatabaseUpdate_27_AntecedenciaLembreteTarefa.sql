-- ============================================================
-- HomePlanner — Update 27: Antecedência do lembrete por tarefa
-- Cada tarefa pode escolher quando avisar (15 min, 30 min, 1 h, 8 h, 1 dia antes).
-- NULL = usa o padrão da família (ConfiguracoesFamilia.MinutosAntecedenciaLembrete).
-- Execute APÓS o DatabaseUpdate_26_CardapioAutomaticoIA.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('Tarefas', 'MinutosAntecedenciaLembrete') IS NULL
    ALTER TABLE Tarefas ADD MinutosAntecedenciaLembrete INT NULL;
GO

PRINT 'Update 27 (antecedência do lembrete por tarefa) aplicado com sucesso!';
GO
