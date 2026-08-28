-- ============================================================
-- HomePlanner — Update 03: Foto de perfil do usuário (avatar)
-- Execute APÓS o DatabaseCreate.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('AspNetUsers', 'Foto') IS NULL
    ALTER TABLE AspNetUsers ADD Foto VARBINARY(MAX) NULL;
GO

IF COL_LENGTH('AspNetUsers', 'FotoContentType') IS NULL
    ALTER TABLE AspNetUsers ADD FotoContentType NVARCHAR(100) NULL;
GO

IF COL_LENGTH('AspNetUsers', 'FotoAtualizadaEm') IS NULL
    ALTER TABLE AspNetUsers ADD FotoAtualizadaEm DATETIME2 NULL;
GO

PRINT 'Colunas de foto de perfil adicionadas em AspNetUsers com sucesso!';
GO
