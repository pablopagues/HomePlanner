-- ============================================================
-- HomePlanner — Update 12: Lembretes de tarefa por horário
-- Coluna de dedup na Tarefas (já avisado?) e a antecedência configurável por família.
-- Execute APÓS o DatabaseUpdate_11_Push.sql
-- ============================================================

USE HomePlannerDb;
GO

-- Marca quando o lembrete de horário desta ocorrência foi disparado (NULL = ainda não)
IF COL_LENGTH('Tarefas', 'LembreteEnviadoEm') IS NULL
    ALTER TABLE Tarefas ADD LembreteEnviadoEm DATETIME2 NULL;
GO

-- Antecedência (minutos) com que os lembretes são enviados antes da hora da tarefa
IF COL_LENGTH('ConfiguracoesFamilia', 'MinutosAntecedenciaLembrete') IS NULL
    ALTER TABLE ConfiguracoesFamilia
        ADD MinutosAntecedenciaLembrete INT NOT NULL
            CONSTRAINT DF_ConfiguracoesFamilia_MinAntecedencia DEFAULT (15);
GO

PRINT 'Update 12 (lembretes de tarefa) aplicado com sucesso!';
GO
