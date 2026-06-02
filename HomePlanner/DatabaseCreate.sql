-- ============================================================
-- HomePlanner Database Creation Script
-- Execute no SQL Server Management Studio ou Azure Data Studio
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HomePlannerDb')
    CREATE DATABASE HomePlannerDb COLLATE Latin1_General_100_CI_AI_SC_UTF8;
GO

USE HomePlannerDb;
GO

-- ============================================================
-- ASP.NET Identity Tables
-- ============================================================

IF OBJECT_ID('AspNetRoleClaims', 'U') IS NULL
CREATE TABLE AspNetRoleClaims (
    Id             INT            IDENTITY(1,1) NOT NULL,
    RoleId         NVARCHAR(450)  NOT NULL,
    ClaimType      NVARCHAR(MAX)  NULL,
    ClaimValue     NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY (Id)
);
GO

IF OBJECT_ID('AspNetRoles', 'U') IS NULL
CREATE TABLE AspNetRoles (
    Id             NVARCHAR(450)  NOT NULL,
    TenantId       UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    Tipo           INT            NOT NULL DEFAULT 1,
    NomeAmigavel   NVARCHAR(50)   NOT NULL DEFAULT '',
    Name           NVARCHAR(256)  NULL,
    NormalizedName NVARCHAR(256)  NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id)
);
GO

IF OBJECT_ID('AspNetUserClaims', 'U') IS NULL
CREATE TABLE AspNetUserClaims (
    Id             INT            IDENTITY(1,1) NOT NULL,
    UserId         NVARCHAR(450)  NOT NULL,
    ClaimType      NVARCHAR(MAX)  NULL,
    ClaimValue     NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_AspNetUserClaims PRIMARY KEY (Id)
);
GO

IF OBJECT_ID('AspNetUserLogins', 'U') IS NULL
CREATE TABLE AspNetUserLogins (
    LoginProvider       NVARCHAR(128)  NOT NULL,
    ProviderKey         NVARCHAR(128)  NOT NULL,
    ProviderDisplayName NVARCHAR(MAX)  NULL,
    UserId              NVARCHAR(450)  NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey)
);
GO

IF OBJECT_ID('AspNetUserRoles', 'U') IS NULL
CREATE TABLE AspNetUserRoles (
    UserId   NVARCHAR(450) NOT NULL,
    RoleId   NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId)
);
GO

IF OBJECT_ID('AspNetUsers', 'U') IS NULL
CREATE TABLE AspNetUsers (
    Id                   NVARCHAR(450)    NOT NULL,
    TenantId             UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    NomeCompleto         NVARCHAR(200)    NOT NULL DEFAULT '',
    Ativo                BIT              NOT NULL DEFAULT 1,
    DataCriacao          DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UltimoLogin          DATETIME2        NULL,
    IsDeleted            BIT              NOT NULL DEFAULT 0,
    DeletedAt            DATETIME2        NULL,
    DeletedByUsuarioId   NVARCHAR(450)    NULL,
    DataAceiteTermos     DATETIME2        NULL,
    VersaoTermosAceito   NVARCHAR(20)     NULL,
    UserName             NVARCHAR(256)    NULL,
    NormalizedUserName   NVARCHAR(256)    NULL,
    Email                NVARCHAR(256)    NULL,
    NormalizedEmail      NVARCHAR(256)    NULL,
    EmailConfirmed       BIT              NOT NULL DEFAULT 0,
    PasswordHash         NVARCHAR(MAX)    NULL,
    SecurityStamp        NVARCHAR(MAX)    NULL,
    ConcurrencyStamp     NVARCHAR(MAX)    NULL,
    PhoneNumber          NVARCHAR(MAX)    NULL,
    PhoneNumberConfirmed BIT              NOT NULL DEFAULT 0,
    TwoFactorEnabled     BIT              NOT NULL DEFAULT 0,
    LockoutEnd           DATETIMEOFFSET   NULL,
    LockoutEnabled       BIT              NOT NULL DEFAULT 1,
    AccessFailedCount    INT              NOT NULL DEFAULT 0,
    CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id)
);
GO

IF OBJECT_ID('AspNetUserTokens', 'U') IS NULL
CREATE TABLE AspNetUserTokens (
    UserId        NVARCHAR(450) NOT NULL,
    LoginProvider NVARCHAR(128) NOT NULL,
    Name          NVARCHAR(128) NOT NULL,
    Value         NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name)
);
GO

-- ============================================================
-- SaaS Tables
-- ============================================================

IF OBJECT_ID('Tenants', 'U') IS NULL
CREATE TABLE Tenants (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    NomeResponsavel  NVARCHAR(200)    NOT NULL,
    Email            NVARCHAR(256)    NOT NULL,
    PaisId           NVARCHAR(2)      NULL,
    Ativo            BIT              NOT NULL DEFAULT 1,
    DataCriacao      DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    OwnerUsuarioId   NVARCHAR(450)    NOT NULL,
    CONSTRAINT PK_Tenants PRIMARY KEY (Id)
);
GO

CREATE UNIQUE INDEX IX_Tenants_Email ON Tenants (Email);
GO

IF OBJECT_ID('TenantDadosBrasil', 'U') IS NULL
CREATE TABLE TenantDadosBrasil (
    TenantId  UNIQUEIDENTIFIER NOT NULL,
    Cpf       NVARCHAR(20)     NULL,
    CONSTRAINT PK_TenantDadosBrasil PRIMARY KEY (TenantId),
    CONSTRAINT FK_TenantDadosBrasil_Tenants FOREIGN KEY (TenantId)
        REFERENCES Tenants(Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('TenantDadosCanada', 'U') IS NULL
CREATE TABLE TenantDadosCanada (
    TenantId  UNIQUEIDENTIFIER NOT NULL,
    Province  NVARCHAR(100)    NULL,
    CONSTRAINT PK_TenantDadosCanada PRIMARY KEY (TenantId),
    CONSTRAINT FK_TenantDadosCanada_Tenants FOREIGN KEY (TenantId)
        REFERENCES Tenants(Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('TenantDelecoesSolicitadas', 'U') IS NULL
CREATE TABLE TenantDelecoesSolicitadas (
    Id                      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId                UNIQUEIDENTIFIER NOT NULL,
    DataSolicitacao         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DataExecucaoAgendada    DATETIME2        NOT NULL,
    Executado               BIT              NOT NULL DEFAULT 0,
    DataExecucao            DATETIME2        NULL,
    SolicitadoPorUsuarioId  NVARCHAR(450)    NOT NULL,
    CONSTRAINT PK_TenantDelecoesSolicitadas PRIMARY KEY (Id),
    CONSTRAINT FK_TenantDelecoes_Tenants FOREIGN KEY (TenantId)
        REFERENCES Tenants(Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('ConfiguracoesAssinatura', 'U') IS NULL
CREATE TABLE ConfiguracoesAssinatura (
    Id                   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId             UNIQUEIDENTIFIER NOT NULL,
    Plano                INT              NOT NULL DEFAULT 0,
    Status               INT              NOT NULL DEFAULT 0,
    DataInicioTrial      DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DataFimTrial         DATETIME2        NULL,
    DataProximaCobranca  DATETIME2        NULL,
    DataCancelamento     DATETIME2        NULL,
    StripeCustomerId     NVARCHAR(100)    NULL,
    StripeSubscriptionId NVARCHAR(100)    NULL,
    StripePriceId        NVARCHAR(100)    NULL,
    UltimoCartaoFinal    NVARCHAR(4)      NULL,
    DataCriacao          DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DataModificacao      DATETIME2        NULL,
    CONSTRAINT PK_ConfiguracoesAssinatura PRIMARY KEY (Id),
    CONSTRAINT FK_ConfigAssinatura_Tenants FOREIGN KEY (TenantId)
        REFERENCES Tenants(Id) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX IX_ConfigAssinatura_TenantId ON ConfiguracoesAssinatura (TenantId);
CREATE INDEX IX_ConfigAssinatura_StripeCustomerId ON ConfiguracoesAssinatura (StripeCustomerId);
CREATE INDEX IX_ConfigAssinatura_StripeSubscriptionId ON ConfiguracoesAssinatura (StripeSubscriptionId);
GO

IF OBJECT_ID('HistoricosPagamento', 'U') IS NULL
CREATE TABLE HistoricosPagamento (
    Id                    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    Plano                 INT              NOT NULL,
    Valor                 DECIMAL(10,2)    NOT NULL,
    Moeda                 NVARCHAR(3)      NOT NULL DEFAULT 'BRL',
    DataPagamento         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    StripeInvoiceId       NVARCHAR(100)    NULL,
    StripePaymentIntentId NVARCHAR(100)    NULL,
    Sucesso               BIT              NOT NULL DEFAULT 1,
    MotivoFalha           NVARCHAR(500)    NULL,
    CONSTRAINT PK_HistoricosPagamento PRIMARY KEY (Id),
    CONSTRAINT FK_HistPagamento_Tenants FOREIGN KEY (TenantId)
        REFERENCES Tenants(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_HistPagamento_TenantId_DataPagamento ON HistoricosPagamento (TenantId, DataPagamento);
GO

IF OBJECT_ID('AuditLogs', 'U') IS NULL
CREATE TABLE AuditLogs (
    Id         BIGINT          IDENTITY(1,1) NOT NULL,
    TenantId   UNIQUEIDENTIFIER NULL,
    UsuarioId  NVARCHAR(450)   NULL,
    Acao       NVARCHAR(100)   NOT NULL,
    Entidade   NVARCHAR(100)   NULL,
    EntidadeId NVARCHAR(100)   NULL,
    DadosAntes NVARCHAR(MAX)   NULL,
    DadosDepois NVARCHAR(MAX)  NULL,
    IpAddress  NVARCHAR(50)    NULL,
    UserAgent  NVARCHAR(500)   NULL,
    DataHora   DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_AuditLogs PRIMARY KEY (Id)
);
GO

CREATE INDEX IX_AuditLogs_TenantId_DataHora ON AuditLogs (TenantId, DataHora);
GO

IF OBJECT_ID('Parametros', 'U') IS NULL
CREATE TABLE Parametros (
    Chave           NVARCHAR(100)  NOT NULL,
    Valor           NVARCHAR(2000) NOT NULL,
    Descricao       NVARCHAR(500)  NULL,
    DataModificacao DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Parametros PRIMARY KEY (Chave)
);
GO

-- ============================================================
-- Cardápio Tables
-- ============================================================

IF OBJECT_ID('UnidadesMedida', 'U') IS NULL
CREATE TABLE UnidadesMedida (
    Id              INT           IDENTITY(1,1) NOT NULL,
    Codigo          NVARCHAR(10)  NOT NULL,
    Nome            NVARCHAR(50)  NOT NULL,
    ChaveTraducao   NVARCHAR(100) NOT NULL,
    Tipo            INT           NOT NULL,
    FatorParaBase   DECIMAL(18,6) NOT NULL DEFAULT 1,
    IsAtivo         BIT           NOT NULL DEFAULT 1,
    CONSTRAINT PK_UnidadesMedida PRIMARY KEY (Id)
);
GO

CREATE UNIQUE INDEX IX_UnidadesMedida_Codigo ON UnidadesMedida (Codigo);
GO

IF OBJECT_ID('Ingredientes', 'U') IS NULL
CREATE TABLE Ingredientes (
    Id                     INT              IDENTITY(1,1) NOT NULL,
    TenantId               UNIQUEIDENTIFIER NOT NULL,
    Nome                   NVARCHAR(200)    NOT NULL,
    NomeNormalizado        NVARCHAR(200)    NOT NULL,
    Categoria              NVARCHAR(100)    NULL,
    UnidadeMedidaPadraoId  INT              NULL,
    IsDeleted              BIT              NOT NULL DEFAULT 0,
    DeletedAt              DATETIME2        NULL,
    DeletedByUsuarioId     NVARCHAR(450)    NULL,
    DataCriacao            DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DataModificacao        DATETIME2        NULL,
    CriadoPor              NVARCHAR(450)    NULL,
    ModificadoPor          NVARCHAR(450)    NULL,
    CONSTRAINT PK_Ingredientes PRIMARY KEY (Id),
    CONSTRAINT FK_Ingredientes_UnidadeMedida FOREIGN KEY (UnidadeMedidaPadraoId)
        REFERENCES UnidadesMedida(Id) ON DELETE SET NULL
);
GO

CREATE INDEX IX_Ingredientes_TenantId_NomeNormalizado ON Ingredientes (TenantId, NomeNormalizado);
GO

IF OBJECT_ID('Receitas', 'U') IS NULL
CREATE TABLE Receitas (
    Id                  INT              IDENTITY(1,1) NOT NULL,
    TenantId            UNIQUEIDENTIFIER NOT NULL,
    Nome                NVARCHAR(300)    NOT NULL,
    NomeNormalizado     NVARCHAR(300)    NOT NULL,
    ModoPreparo         NVARCHAR(MAX)    NULL,
    NumeroPorcoesBase   INT              NOT NULL DEFAULT 4,
    TempoPreparoMinutos INT              NULL,
    UrlOrigem           NVARCHAR(2000)   NULL,
    UrlImagem           NVARCHAR(2000)   NULL,
    Observacoes         NVARCHAR(MAX)    NULL,
    IsDeleted           BIT              NOT NULL DEFAULT 0,
    DeletedAt           DATETIME2        NULL,
    DeletedByUsuarioId  NVARCHAR(450)    NULL,
    DataCriacao         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DataModificacao     DATETIME2        NULL,
    CriadoPor           NVARCHAR(450)    NULL,
    ModificadoPor       NVARCHAR(450)    NULL,
    CONSTRAINT PK_Receitas PRIMARY KEY (Id)
);
GO

CREATE INDEX IX_Receitas_TenantId_NomeNormalizado ON Receitas (TenantId, NomeNormalizado);
GO

IF OBJECT_ID('ReceitasIngredientes', 'U') IS NULL
CREATE TABLE ReceitasIngredientes (
    Id              INT           IDENTITY(1,1) NOT NULL,
    ReceitaId       INT           NOT NULL,
    IngredienteId   INT           NOT NULL,
    Quantidade      DECIMAL(10,3) NOT NULL,
    UnidadeMedidaId INT           NOT NULL,
    Observacao      NVARCHAR(500) NULL,
    Opcional        BIT           NOT NULL DEFAULT 0,
    Ordem           INT           NOT NULL DEFAULT 0,
    IsDeleted       BIT           NOT NULL DEFAULT 0,
    CONSTRAINT PK_ReceitasIngredientes PRIMARY KEY (Id),
    CONSTRAINT FK_ReceitasIngredientes_Receita FOREIGN KEY (ReceitaId)
        REFERENCES Receitas(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ReceitasIngredientes_Ingrediente FOREIGN KEY (IngredienteId)
        REFERENCES Ingredientes(Id),
    CONSTRAINT FK_ReceitasIngredientes_UnidadeMedida FOREIGN KEY (UnidadeMedidaId)
        REFERENCES UnidadesMedida(Id)
);
GO

CREATE UNIQUE INDEX IX_ReceitasIngredientes_Unico
    ON ReceitasIngredientes (ReceitaId, IngredienteId)
    WHERE IsDeleted = 0;
GO

IF OBJECT_ID('ModelosSemana', 'U') IS NULL
CREATE TABLE ModelosSemana (
    Id                 INT              IDENTITY(1,1) NOT NULL,
    TenantId           UNIQUEIDENTIFIER NOT NULL,
    Nome               NVARCHAR(200)    NOT NULL,
    Descricao          NVARCHAR(500)    NULL,
    IsDeleted          BIT              NOT NULL DEFAULT 0,
    DeletedAt          DATETIME2        NULL,
    DeletedByUsuarioId NVARCHAR(450)    NULL,
    DataCriacao        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DataModificacao    DATETIME2        NULL,
    CriadoPor          NVARCHAR(450)    NULL,
    ModificadoPor      NVARCHAR(450)    NULL,
    CONSTRAINT PK_ModelosSemana PRIMARY KEY (Id)
);
GO

CREATE INDEX IX_ModelosSemana_TenantId_Nome ON ModelosSemana (TenantId, Nome);
GO

IF OBJECT_ID('RefeicoesModelo', 'U') IS NULL
CREATE TABLE RefeicoesModelo (
    Id              INT  IDENTITY(1,1) NOT NULL,
    ModeloSemanaId  INT  NOT NULL,
    DiaSemana       INT  NOT NULL,
    TipoRefeicao    INT  NOT NULL,
    ReceitaId       INT  NULL,
    PorcoesDesejadas INT NULL,
    Observacao      NVARCHAR(500) NULL,
    IsDeleted       BIT           NOT NULL DEFAULT 0,
    CONSTRAINT PK_RefeicoesModelo PRIMARY KEY (Id),
    CONSTRAINT FK_RefeicoesModelo_ModeloSemana FOREIGN KEY (ModeloSemanaId)
        REFERENCES ModelosSemana(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RefeicoesModelo_Receita FOREIGN KEY (ReceitaId)
        REFERENCES Receitas(Id) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX IX_RefeicoesModelo_Unico
    ON RefeicoesModelo (ModeloSemanaId, DiaSemana, TipoRefeicao)
    WHERE IsDeleted = 0;
GO

IF OBJECT_ID('PlanejamentosSemanais', 'U') IS NULL
CREATE TABLE PlanejamentosSemanais (
    Id                     INT              IDENTITY(1,1) NOT NULL,
    TenantId               UNIQUEIDENTIFIER NOT NULL,
    DataInicio             DATE             NOT NULL,
    Nome                   NVARCHAR(200)    NULL,
    ModeloSemanaOrigemId   INT              NULL,
    IsDeleted              BIT              NOT NULL DEFAULT 0,
    DeletedAt              DATETIME2        NULL,
    DeletedByUsuarioId     NVARCHAR(450)    NULL,
    DataCriacao            DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DataModificacao        DATETIME2        NULL,
    CriadoPor              NVARCHAR(450)    NULL,
    ModificadoPor          NVARCHAR(450)    NULL,
    CONSTRAINT PK_PlanejamentosSemanais PRIMARY KEY (Id),
    CONSTRAINT FK_PlanejamentosSemanais_ModeloSemana FOREIGN KEY (ModeloSemanaOrigemId)
        REFERENCES ModelosSemana(Id) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX IX_PlanejamentosSemanais_TenantId_DataInicio
    ON PlanejamentosSemanais (TenantId, DataInicio)
    WHERE IsDeleted = 0;
GO

IF OBJECT_ID('RefeicoesDia', 'U') IS NULL
CREATE TABLE RefeicoesDia (
    Id                    INT  IDENTITY(1,1) NOT NULL,
    PlanejamentoSemanalId INT  NOT NULL,
    DiaSemana             INT  NOT NULL,
    TipoRefeicao          INT  NOT NULL,
    ReceitaId             INT  NULL,
    PorcoesDesejadas      INT  NULL,
    Observacao            NVARCHAR(500) NULL,
    IsDeleted             BIT           NOT NULL DEFAULT 0,
    CONSTRAINT PK_RefeicoesDia PRIMARY KEY (Id),
    CONSTRAINT FK_RefeicoesDia_PlanejamentoSemanal FOREIGN KEY (PlanejamentoSemanalId)
        REFERENCES PlanejamentosSemanais(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RefeicoesDia_Receita FOREIGN KEY (ReceitaId)
        REFERENCES Receitas(Id) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX IX_RefeicoesDia_Unico
    ON RefeicoesDia (PlanejamentoSemanalId, DiaSemana, TipoRefeicao)
    WHERE IsDeleted = 0;
GO

IF OBJECT_ID('ConfiguracoesFamilia', 'U') IS NULL
CREATE TABLE ConfiguracoesFamilia (
    TenantId              UNIQUEIDENTIFIER NOT NULL,
    TiposRefeicaoAtivos   NVARCHAR(200)    NOT NULL DEFAULT 'Almoco',
    TamanhoFamiliaPadrao  INT              NOT NULL DEFAULT 4,
    FusoHorario           NVARCHAR(100)    NOT NULL DEFAULT 'America/Toronto',
    DataCriacao           DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DataModificacao       DATETIME2        NULL,
    CriadoPor             NVARCHAR(450)    NULL,
    ModificadoPor         NVARCHAR(450)    NULL,
    CONSTRAINT PK_ConfiguracoesFamilia PRIMARY KEY (TenantId)
);
GO

-- ============================================================
-- Foreign Keys Identity Tables
-- ============================================================

ALTER TABLE AspNetRoleClaims ADD CONSTRAINT FK_AspNetRoleClaims_Roles
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE;
GO

ALTER TABLE AspNetUserClaims ADD CONSTRAINT FK_AspNetUserClaims_Users
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE;
GO

ALTER TABLE AspNetUserLogins ADD CONSTRAINT FK_AspNetUserLogins_Users
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE;
GO

ALTER TABLE AspNetUserRoles ADD CONSTRAINT FK_AspNetUserRoles_Roles
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE;
ALTER TABLE AspNetUserRoles ADD CONSTRAINT FK_AspNetUserRoles_Users
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE;
GO

ALTER TABLE AspNetUserTokens ADD CONSTRAINT FK_AspNetUserTokens_Users
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE;
GO

-- ============================================================
-- Índices Identity
-- ============================================================

CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles (NormalizedName) WHERE NormalizedName IS NOT NULL;
CREATE INDEX IX_AspNetRoles_TenantId ON AspNetRoles (TenantId);
CREATE INDEX EmailIndex ON AspNetUsers (NormalizedEmail);
CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers (NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
CREATE INDEX IX_AspNetUsers_TenantId ON AspNetUsers (TenantId);
GO

-- ============================================================
-- Seed: UnidadesMedida
-- ============================================================

SET IDENTITY_INSERT UnidadesMedida ON;

MERGE INTO UnidadesMedida AS target
USING (VALUES
    (1,  'g',      'Grama',          'UnidadeMedida_Grama',       1, 1.000000),
    (2,  'kg',     'Quilograma',     'UnidadeMedida_Quilograma',  1, 1000.000000),
    (3,  'ml',     'Mililitro',      'UnidadeMedida_Mililitro',   2, 1.000000),
    (4,  'l',      'Litro',          'UnidadeMedida_Litro',       2, 1000.000000),
    (5,  'xic',    'Xícara (240ml)', 'UnidadeMedida_Xicara',      2, 240.000000),
    (6,  'cs',     'Colher de sopa', 'UnidadeMedida_ColherSopa',  2, 15.000000),
    (7,  'cc',     'Colher de chá',  'UnidadeMedida_ColherCha',   2, 5.000000),
    (8,  'pitada', 'Pitada',         'UnidadeMedida_Pitada',      2, 0.300000),
    (9,  'un',     'Unidade',        'UnidadeMedida_Unidade',     3, 1.000000),
    (10, 'dente',  'Dente',          'UnidadeMedida_Dente',       3, 1.000000),
    (11, 'fatia',  'Fatia',          'UnidadeMedida_Fatia',       3, 1.000000),
    (12, 'pacote', 'Pacote',         'UnidadeMedida_Pacote',      3, 1.000000)
) AS source (Id, Codigo, Nome, ChaveTraducao, Tipo, FatorParaBase)
ON target.Id = source.Id
WHEN NOT MATCHED THEN
    INSERT (Id, Codigo, Nome, ChaveTraducao, Tipo, FatorParaBase, IsAtivo)
    VALUES (source.Id, source.Codigo, source.Nome, source.ChaveTraducao, source.Tipo, source.FatorParaBase, 1);

SET IDENTITY_INSERT UnidadesMedida OFF;
GO

-- ============================================================
-- Seed: Parametros
-- ============================================================

MERGE INTO Parametros AS target
USING (VALUES
    ('versao_termos', '1.0', 'Versão atual dos termos de uso', GETUTCDATE()),
    ('dias_trial',    '30',  'Dias de período trial gratuito',  GETUTCDATE())
) AS source (Chave, Valor, Descricao, DataModificacao)
ON target.Chave = source.Chave
WHEN NOT MATCHED THEN
    INSERT (Chave, Valor, Descricao, DataModificacao)
    VALUES (source.Chave, source.Valor, source.Descricao, source.DataModificacao);
GO

PRINT 'HomePlannerDb criado e populado com sucesso!';
GO
