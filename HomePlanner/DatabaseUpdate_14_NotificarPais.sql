-- ============================================================
-- HomePlanner — Update 14: "Notificar pai/mãe" na tarefa
-- Quando ligado, o lembrete de horário também vai para os pais (Owner/Membro)
-- da família, além do responsável.
-- Execute APÓS o DatabaseUpdate_13_IdiomaUsuario.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('Tarefas', 'NotificarResponsaveis') IS NULL
    ALTER TABLE Tarefas ADD NotificarResponsaveis BIT NOT NULL
        CONSTRAINT DF_Tarefas_NotificarResponsaveis DEFAULT (0);
GO

PRINT 'Update 14 (notificar pai/mãe) aplicado com sucesso!';
GO
