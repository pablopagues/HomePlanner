-- ============================================================
-- HomePlanner — Update 06: Criador da tarefa (CriadoPorUsuarioId)
-- Suporta a regra de visibilidade: tarefas Privadas só aparecem para o criador.
-- Execute APÓS o DatabaseUpdate_05_Tarefas_Horario.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('Tarefas', 'CriadoPorUsuarioId') IS NULL
    ALTER TABLE Tarefas ADD CriadoPorUsuarioId NVARCHAR(450) NULL;
GO

-- Backfill: preserva a dona das tarefas privadas já existentes, a partir da auditoria.
-- (CriadoPor já armazena o Id do usuário criador; ignoramos registros de "system".)
UPDATE Tarefas
   SET CriadoPorUsuarioId = CriadoPor
 WHERE CriadoPorUsuarioId IS NULL
   AND CriadoPor IS NOT NULL
   AND CriadoPor <> 'system';
GO

-- Índice para a regra de visibilidade (TenantId primeiro, como nos demais índices compostos)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Tarefas_TenantId_CriadoPorUsuarioId'
      AND object_id = OBJECT_ID('Tarefas'))
    CREATE INDEX IX_Tarefas_TenantId_CriadoPorUsuarioId
        ON Tarefas (TenantId, CriadoPorUsuarioId);
GO

PRINT 'Coluna CriadoPorUsuarioId adicionada em Tarefas com sucesso!';
GO
