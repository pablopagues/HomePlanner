-- ============================================================
-- HomePlanner — Update 05: Horário das tarefas (HoraInicio/HoraFim)
-- Execute APÓS o DatabaseUpdate_02_Tarefas.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('Tarefas', 'HoraInicio') IS NULL
    ALTER TABLE Tarefas ADD HoraInicio TIME NULL;
GO

IF COL_LENGTH('Tarefas', 'HoraFim') IS NULL
    ALTER TABLE Tarefas ADD HoraFim TIME NULL;
GO

PRINT 'Colunas HoraInicio/HoraFim adicionadas em Tarefas com sucesso!';
GO
