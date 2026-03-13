-- ============================================================
-- Identity Module — Database Schema
-- Database: YashviECommerceDb
-- Schema:   identity
--
-- Safe to re-run — all statements are idempotent.
-- Tables:   Users, Sellers, RefreshTokens, __MigrationsHistory
-- Seed:     Default Admin user (admin@markethub.com / Admin@123456)
-- ============================================================

-- ── Schema ───────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'identity')
    EXEC('CREATE SCHEMA [identity]');
GO

-- ============================================================
-- TABLE: identity.Users
-- Covers all roles: Admin, Seller, Buyer
-- ============================================================
IF OBJECT_ID('[identity].[Users]', 'U') IS NULL
BEGIN
    CREATE TABLE [identity].[Users] (

        -- PK
        [Id]                    UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Users_Id] DEFAULT NEWSEQUENTIALID(),

        -- Profile
        [FirstName]             NVARCHAR(100)       NOT NULL,
        [LastName]              NVARCHAR(100)       NOT NULL,
        [Email]                 NVARCHAR(255)       NOT NULL,
        [PasswordHash]          NVARCHAR(500)       NOT NULL,
        [PhoneNumber]           NVARCHAR(20)        NULL,
        [AvatarUrl]             NVARCHAR(500)       NULL,

        -- Role & Status stored as string (matches C# enum .ToString())
        [Role]                  NVARCHAR(20)        NOT NULL
                                    CONSTRAINT [CHK_Users_Role]
                                    CHECK ([Role] IN ('Admin', 'Seller', 'Buyer')),

        [Status]                NVARCHAR(30)        NOT NULL
                                    CONSTRAINT [DF_Users_Status]   DEFAULT 'Active'
                                    CONSTRAINT [CHK_Users_Status]
                                    CHECK ([Status] IN (
                                        'Active',
                                        'Inactive',
                                        'Suspended',
                                        'PendingVerification'
                                    )),

        -- Login tracking
        [LastLoginAt]           DATETIME2(7)        NULL,
        [FailedLoginAttempts]   INT                 NOT NULL
                                    CONSTRAINT [DF_Users_FailedAttempts] DEFAULT 0,
        [LockedUntil]           DATETIME2(7)        NULL,

        -- Audit (matches Shared.Domain.Abstractions.Entity<T>)
        [CreatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Users_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Users_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]             UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Users_CreatedBy]
                                    DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]             UNIQUEIDENTIFIER    NULL,

        -- Soft delete
        [IsDeleted]             BIT                 NOT NULL
                                    CONSTRAINT [DF_Users_IsDeleted] DEFAULT 0,
        [DeletedAt]             DATETIME2(7)        NULL,

        -- Optimistic concurrency (EF Core IsConcurrencyToken)
        [RowVersion]            ROWVERSION          NOT NULL,

        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id])
    );

    -- Unique email among non-deleted users only
    CREATE UNIQUE NONCLUSTERED INDEX [UIX_Users_Email_Active]
        ON [identity].[Users] ([Email])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Users_Role]
        ON [identity].[Users] ([Role])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Users_Status]
        ON [identity].[Users] ([Status])
        WHERE ([IsDeleted] = 0);

    PRINT 'Created [identity].[Users]';
END
ELSE
    PRINT '[identity].[Users] already exists — skipped.';
GO

-- ============================================================
-- TABLE: identity.Sellers
-- One-to-one with Users where Role = 'Seller'.
-- Stores seller-specific profile and approval state.
-- ============================================================
IF OBJECT_ID('[identity].[Sellers]', 'U') IS NULL
BEGIN
    CREATE TABLE [identity].[Sellers] (

        -- PK (same GUID as the Users row — 1:1 relationship)
        [Id]                    UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Sellers_Id] DEFAULT NEWSEQUENTIALID(),

        -- FK → Users
        [UserId]                UNIQUEIDENTIFIER    NOT NULL,

        -- Business profile
        [BusinessName]          NVARCHAR(200)       NOT NULL,
        [BusinessEmail]         NVARCHAR(255)       NULL,
        [BusinessPhone]         NVARCHAR(20)        NULL,
        [Description]           NVARCHAR(1000)      NULL,
        [LogoUrl]               NVARCHAR(500)       NULL,
        [WebsiteUrl]            NVARCHAR(500)       NULL,

        -- Address
        [AddressLine1]          NVARCHAR(200)       NULL,
        [AddressLine2]          NVARCHAR(200)       NULL,
        [City]                  NVARCHAR(100)       NULL,
        [State]                 NVARCHAR(100)       NULL,
        [PostalCode]            NVARCHAR(20)        NULL,
        [Country]               NVARCHAR(100)       NULL,

        -- Approval workflow
        -- Domain: SellerStatus — PendingApproval | Active | Suspended | Rejected
        [SellerStatus]          NVARCHAR(30)        NOT NULL
                                    CONSTRAINT [DF_Sellers_SellerStatus] DEFAULT 'PendingApproval'
                                    CONSTRAINT [CHK_Sellers_SellerStatus]
                                    CHECK ([SellerStatus] IN (
                                        'PendingApproval',
                                        'Active',
                                        'Suspended',
                                        'Rejected'
                                    )),

        [ApprovedByAdminId]     UNIQUEIDENTIFIER    NULL,
        [ApprovedAt]            DATETIME2(7)        NULL,
        [RejectedByAdminId]     UNIQUEIDENTIFIER    NULL,
        [RejectedAt]            DATETIME2(7)        NULL,
        [RejectionReason]       NVARCHAR(500)       NULL,

        -- Stats (denormalised for fast dashboard reads — updated via app layer)
        [TotalProducts]         INT                 NOT NULL
                                    CONSTRAINT [DF_Sellers_TotalProducts] DEFAULT 0,
        [TotalOrders]           INT                 NOT NULL
                                    CONSTRAINT [DF_Sellers_TotalOrders]   DEFAULT 0,
        [TotalRevenue]          DECIMAL(18, 4)      NOT NULL
                                    CONSTRAINT [DF_Sellers_TotalRevenue]  DEFAULT 0,
        [Rating]                DECIMAL(3, 2)       NULL,

        -- Audit
        [CreatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Sellers_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Sellers_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]             UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Sellers_CreatedBy]
                                    DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]             UNIQUEIDENTIFIER    NULL,

        -- Soft delete
        [IsDeleted]             BIT                 NOT NULL
                                    CONSTRAINT [DF_Sellers_IsDeleted] DEFAULT 0,
        [DeletedAt]             DATETIME2(7)        NULL,

        -- Optimistic concurrency
        [RowVersion]            ROWVERSION          NOT NULL,

        CONSTRAINT [PK_Sellers] PRIMARY KEY CLUSTERED ([Id]),

        CONSTRAINT [FK_Sellers_Users]
            FOREIGN KEY ([UserId])
            REFERENCES [identity].[Users] ([Id])
            ON DELETE CASCADE
    );

    -- Each user has at most one seller profile
    CREATE UNIQUE NONCLUSTERED INDEX [UIX_Sellers_UserId]
        ON [identity].[Sellers] ([UserId])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Sellers_SellerStatus]
        ON [identity].[Sellers] ([SellerStatus])
        WHERE ([IsDeleted] = 0);

    PRINT 'Created [identity].[Sellers]';
END
ELSE
    PRINT '[identity].[Sellers] already exists — skipped.';
GO

-- ============================================================
-- TABLE: identity.RefreshTokens
-- ============================================================
IF OBJECT_ID('[identity].[RefreshTokens]', 'U') IS NULL
BEGIN
    CREATE TABLE [identity].[RefreshTokens] (

        [Id]                    INT                 NOT NULL IDENTITY(1, 1),
        [UserId]                UNIQUEIDENTIFIER    NOT NULL,

        -- Token (base64url of 64 random bytes)
        [Token]                 NVARCHAR(500)       NOT NULL,
        [ExpiresAt]             DATETIME2(7)        NOT NULL,

        -- Revocation
        [IsRevoked]             BIT                 NOT NULL
                                    CONSTRAINT [DF_RefreshTokens_IsRevoked] DEFAULT 0,
        [RevokedAt]             DATETIME2(7)        NULL,
        [RevokedReason]         NVARCHAR(200)       NULL,
        -- Token that replaced this one (refresh-token rotation chain)
        [ReplacedByToken]       NVARCHAR(500)       NULL,

        -- Request metadata
        [CreatedByIp]           NVARCHAR(50)        NULL,
        [UserAgent]             NVARCHAR(500)       NULL,

        -- Audit
        [CreatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_RefreshTokens_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_RefreshTokens_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]             UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_RefreshTokens_CreatedBy]
                                    DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]             UNIQUEIDENTIFIER    NULL,

        -- Soft delete
        [IsDeleted]             BIT                 NOT NULL
                                    CONSTRAINT [DF_RefreshTokens_IsDeleted] DEFAULT 0,
        [DeletedAt]             DATETIME2(7)        NULL,

        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED ([Id]),

        CONSTRAINT [FK_RefreshTokens_Users]
            FOREIGN KEY ([UserId])
            REFERENCES [identity].[Users] ([Id])
            ON DELETE CASCADE
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UIX_RefreshTokens_Token]
        ON [identity].[RefreshTokens] ([Token]);

    -- Fast look-up of active tokens for a user
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId_Active]
        ON [identity].[RefreshTokens] ([UserId], [IsRevoked])
        INCLUDE ([Token], [ExpiresAt])
        WHERE ([IsRevoked] = 0 AND [IsDeleted] = 0);

    -- Background clean-up job (prune expired + revoked tokens)
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_Expiry_Revoked]
        ON [identity].[RefreshTokens] ([ExpiresAt], [IsRevoked]);

    PRINT 'Created [identity].[RefreshTokens]';
END
ELSE
    PRINT '[identity].[RefreshTokens] already exists — skipped.';
GO

-- ============================================================
-- TABLE: identity.__MigrationsHistory
-- EF Core creates this automatically but included for manual setup.
-- ============================================================
IF OBJECT_ID('[identity].[__MigrationsHistory]', 'U') IS NULL
BEGIN
    CREATE TABLE [identity].[__MigrationsHistory] (
        [MigrationId]    NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32)  NOT NULL,
        CONSTRAINT [PK___MigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT 'Created [identity].[__MigrationsHistory]';
END
ELSE
    PRINT '[identity].[__MigrationsHistory] already exists — skipped.';
GO

-- ============================================================
-- SEED: Default Admin user
-- Password: Admin@123456  (BCrypt, workFactor = 12)
-- !! Change this password immediately after first login !!
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM [identity].[Users]
    WHERE [Email] = 'admin@markethub.com' AND [IsDeleted] = 0
)
BEGIN
    DECLARE @adminId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO [identity].[Users] (
        [Id], [FirstName], [LastName], [Email], [PasswordHash],
        [Role], [Status], [FailedLoginAttempts],
        [CreatedAt], [UpdatedAt], [CreatedBy], [IsDeleted]
    )
    VALUES (
        @adminId,
        'Admin', 'User',
        'admin@markethub.com',
        '$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',
        'Admin', 'Active', 0,
        GETUTCDATE(), GETUTCDATE(),
        '00000000-0000-0000-0000-000000000000',
        0
    );

    PRINT 'Seeded admin user: admin@markethub.com';
END
ELSE
    PRINT 'Admin user already exists — seed skipped.';
GO

-- ============================================================
-- VERIFY
-- ============================================================
SELECT
    t.TABLE_SCHEMA          AS [Schema],
    t.TABLE_NAME            AS [Table],
    COUNT(c.COLUMN_NAME)    AS [Columns]
FROM INFORMATION_SCHEMA.TABLES  t
JOIN INFORMATION_SCHEMA.COLUMNS c
    ON  c.TABLE_SCHEMA = t.TABLE_SCHEMA
    AND c.TABLE_NAME   = t.TABLE_NAME
WHERE t.TABLE_SCHEMA = 'identity'
  AND t.TABLE_TYPE   = 'BASE TABLE'
GROUP BY t.TABLE_SCHEMA, t.TABLE_NAME
ORDER BY t.TABLE_NAME;
GO
