-- ============================================================
-- HomePlanner — Update 13: Idioma preferido do usuário
-- Guarda o idioma (pt/en/es) escolhido pelo usuário, para que as notificações
-- enviadas fora de uma requisição (background) saiam no idioma certo.
-- Execute APÓS o DatabaseUpdate_12_Lembretes.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('AspNetUsers', 'Idioma') IS NULL
    ALTER TABLE AspNetUsers ADD Idioma NVARCHAR(5) NULL;
GO

PRINT 'Update 13 (idioma do usuário) aplicado com sucesso!';
GO
