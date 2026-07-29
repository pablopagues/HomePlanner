-- ============================================================
-- HomePlanner — Update 19: Foto da receita (upload no banco)
-- Permite anexar UMA foto por receita cadastrada manualmente. A imagem é
-- redimensionada/comprimida na aplicação (SixLabors.ImageSharp) antes de
-- ser gravada aqui. Tem precedência sobre UrlImagem na exibição; UrlImagem
-- continua servindo receitas importadas de sites (imagem hotlinkada).
--   • Foto             : bytes da imagem já processada (VARBINARY(MAX))
--   • FotoContentType  : MIME final (image/jpeg ou image/png)
--   • FotoAtualizadaEm : carimbo do último upload (cache-busting/ETag)
-- Execute APÓS o DatabaseUpdate_18_ProdutosRecorrentes.sql
-- ============================================================

USE HomePlannerDb;
GO

IF COL_LENGTH('Receitas', 'Foto') IS NULL
    ALTER TABLE Receitas ADD Foto VARBINARY(MAX) NULL;
GO

IF COL_LENGTH('Receitas', 'FotoContentType') IS NULL
    ALTER TABLE Receitas ADD FotoContentType NVARCHAR(100) NULL;
GO

IF COL_LENGTH('Receitas', 'FotoAtualizadaEm') IS NULL
    ALTER TABLE Receitas ADD FotoAtualizadaEm DATETIME2 NULL;
GO

PRINT 'Colunas de foto da receita adicionadas em Receitas com sucesso!';
GO
