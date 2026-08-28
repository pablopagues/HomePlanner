-- ============================================================
-- HomePlanner — Update 22: Rastreio do convite de membro
-- Guarda quando o convite de definição de senha foi enviado pela última vez,
-- para exibir "convite enviado em ..." na tela Família e limitar reenvios
-- seguidos (throttle de 2 minutos aplicado no FamiliaService).
-- Execute APÓS o DatabaseUpdate_21_DispositivosPush.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('AspNetUsers', 'UltimoConviteEnviadoEm') IS NULL
BEGIN
    ALTER TABLE AspNetUsers ADD UltimoConviteEnviadoEm DATETIME2 NULL;
    PRINT 'Coluna AspNetUsers.UltimoConviteEnviadoEm criada.';
END
ELSE
    PRINT 'Coluna AspNetUsers.UltimoConviteEnviadoEm já existe — nada a fazer.';
GO

-- Convites já enviados antes desta atualização ficam com NULL: a tela mostra
-- apenas "convite pendente" (sem data) e o primeiro reenvio passa sem throttle.

PRINT 'Update 22 concluído com sucesso!';
GO
