-- ============================================================
-- Identity Module — Database Schema
-- EcommerceMarketplace
--
-- Schema:   identity
-- Tables:   Users, RefreshTokens
-- Patterns: Soft delete, audit columns, optimistic concurrency,
--           string enums for Role/Status, sequential GUIDs,
--           indexed email (unique, non-deleted only)
-- ============================================================

-- ── Create schema ────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.schemas WHERE name = 'identity'
)
BEGIN
    EXEC('CREATE SCHEMA [identity]');
END
GO

-- ============================================================
-- TABLE: identity.Users
-- ============================================================
IF OBJECT_ID('[identity].[Users]', 'U') IS NULL
BEGIN
    CREATE TABLE [identity].[Users] (

        -- Primary key
        [Id]                    UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Users_Id]
                                    DEFAULT NEWSEQUENTIALID(),

        -- Profile
        [FirstName]             NVARCHAR(100)       NOT NULL,
        [LastName]              NVARCHAR(100)       NOT NULL,
        [Email]                 NVARCHAR(255)       NOT NULL,
        [PasswordHash]          NVARCHAR(500)       NOT NULL,
        [PhoneNumber]           NVARCHAR(20)        NULL,
        [AvatarUrl]             NVARCHAR(500)       NULL,

        -- Role & status (stored as string for readability)
        -- Domain: UserRole  — Admin | Seller | Buyer
        -- Domain: UserStatus — Active | Inactive | Suspended | PendingVerification
        [Role]                  NVARCHAR(20)        NOT NULL
                                    CONSTRAINT [CHK_Users_Role]
                                    CHECK ([Role] IN ('Admin', 'Seller', 'Buyer')),

        [Status]                NVARCHAR(30)        NOT NULL
                                    CONSTRAINT [DF_Users_Status]
                                    DEFAULT 'Active'
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
                                    CONSTRAINT [DF_Users_FailedLoginAttempts]
                                    DEFAULT 0,
        [LockedUntil]           DATETIME2(7)        NULL,

        -- Audit
        [CreatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Users_CreatedAt]
                                    DEFAULT GETUTCDATE(),
        [UpdatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Users_UpdatedAt]
                                    DEFAULT GETUTCDATE(),
        [CreatedBy]             UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Users_CreatedBy]
                                    DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]             UNIQUEIDENTIFIER    NULL,

        -- Soft delete
        [IsDeleted]             BIT                 NOT NULL
                                    CONSTRAINT [DF_Users_IsDeleted]
                                    DEFAULT 0,
        [DeletedAt]             DATETIME2(7)        NULL,

        -- Optimistic concurrency
        [RowVersion]            ROWVERSION          NOT NULL,

        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id])
    );

    -- Unique email per non-deleted user
    CREATE UNIQUE NONCLUSTERED INDEX [UIX_Users_Email_Active]
        ON [identity].[Users] ([Email])
        WHERE ([IsDeleted] = 0);

    -- Filter by role (common admin queries)
    CREATE NONCLUSTERED INDEX [IX_Users_Role]
        ON [identity].[Users] ([Role])
        WHERE ([IsDeleted] = 0);

    -- Filter by status
    CREATE NONCLUSTERED INDEX [IX_Users_Status]
        ON [identity].[Users] ([Status])
        WHERE ([IsDeleted] = 0);

    PRINT 'Created table [identity].[Users]';
END
ELSE
    PRINT 'Table [identity].[Users] already exists — skipped.';
GO

-- ============================================================
-- TABLE: identity.RefreshTokens
-- ============================================================
IF OBJECT_ID('[identity].[RefreshTokens]', 'U') IS NULL
BEGIN
    CREATE TABLE [identity].[RefreshTokens] (

        -- Primary key (identity int — refresh tokens are numerous)
        [Id]                    INT                 NOT NULL
                                    IDENTITY(1, 1),

        -- Foreign key to Users
        [UserId]                UNIQUEIDENTIFIER    NOT NULL,

        -- Token value (base64 of 64 random bytes = 88 chars)
        [Token]                 NVARCHAR(500)       NOT NULL,

        -- Expiry
        [ExpiresAt]             DATETIME2(7)        NOT NULL,

        -- Revocation
        [IsRevoked]             BIT                 NOT NULL
                                    CONSTRAINT [DF_RefreshTokens_IsRevoked]
                                    DEFAULT 0,
        [RevokedAt]             DATETIME2(7)        NULL,
        [RevokedReason]         NVARCHAR(200)       NULL,
        [ReplacedByToken]       NVARCHAR(500)       NULL,

        -- Metadata
        [CreatedByIp]           NVARCHAR(50)        NULL,

        -- Audit
        [CreatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_RefreshTokens_CreatedAt]
                                    DEFAULT GETUTCDATE(),
        [UpdatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_RefreshTokens_UpdatedAt]
                                    DEFAULT GETUTCDATE(),
        [CreatedBy]             UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_RefreshTokens_CreatedBy]
                                    DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]             UNIQUEIDENTIFIER    NULL,

        -- Soft delete (inherited from Entity<TId> base class)
        [IsDeleted]             BIT                 NOT NULL
                                    CONSTRAINT [DF_RefreshTokens_IsDeleted]
                                    DEFAULT 0,
        [DeletedAt]             DATETIME2(7)        NULL,

        CONSTRAINT [PK_RefreshTokens]
            PRIMARY KEY CLUSTERED ([Id]),

        CONSTRAINT [FK_RefreshTokens_Users]
            FOREIGN KEY ([UserId])
            REFERENCES [identity].[Users] ([Id])
            ON DELETE CASCADE   -- delete tokens when user is hard-deleted
    );

    -- Unique token value
    CREATE UNIQUE NONCLUSTERED INDEX [UIX_RefreshTokens_Token]
        ON [identity].[RefreshTokens] ([Token]);

    -- Look up active tokens by user
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId]
        ON [identity].[RefreshTokens] ([UserId])
        INCLUDE ([Token], [ExpiresAt], [IsRevoked]);

    -- Clean-up queries: find expired + revoked tokens
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_Expiry_Revoked]
        ON [identity].[RefreshTokens] ([ExpiresAt], [IsRevoked]);

    PRINT 'Created table [identity].[RefreshTokens]';
END
ELSE
    PRINT 'Table [identity].[RefreshTokens] already exists — skipped.';
GO

-- ============================================================
-- EF Core migrations history table for identity schema
-- (created automatically by EF Core on first migration,
--  but added here for completeness / manual setup)
-- ============================================================
IF OBJECT_ID('[identity].[__MigrationsHistory]', 'U') IS NULL
BEGIN
    CREATE TABLE [identity].[__MigrationsHistory] (
        [MigrationId]    NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32)  NOT NULL,
        CONSTRAINT [PK___MigrationsHistory]
            PRIMARY KEY ([MigrationId])
    );

    PRINT 'Created table [identity].[__MigrationsHistory]';
END
ELSE
    PRINT 'Table [identity].[__MigrationsHistory] already exists — skipped.';
GO

-- ============================================================
-- SEED: Default Admin user
-- Password: Admin@123456  (BCrypt hash, workFactor=12)
-- Change this password immediately after first login.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM [identity].[Users]
    WHERE [Email] = 'admin@markethub.com'
      AND [IsDeleted] = 0
)
BEGIN
    -- NEWSEQUENTIALID() is only valid in DEFAULT constraints.
    -- Use NEWID() here for the one-time seed insert.
    DECLARE @adminId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO [identity].[Users] (
        [Id],
        [FirstName],
        [LastName],
        [Email],
        [PasswordHash],
        [Role],
        [Status],
        [FailedLoginAttempts],
        [CreatedAt],
        [UpdatedAt],
        [CreatedBy],
        [IsDeleted]
    )
    VALUES (
        @adminId,
        'Admin',
        'User',
        'admin@markethub.com',
        -- BCrypt hash for: Admin@123456  (workFactor=12)
        '$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',
        'Admin',
        'Active',
        0,
        GETUTCDATE(),
        GETUTCDATE(),
        '00000000-0000-0000-0000-000000000000',
        0
    );

    PRINT 'Seeded default admin user: admin@markethub.com';
END
ELSE
    PRINT 'Admin user already exists — seed skipped.';
GO

-- ============================================================
-- VERIFY
-- ============================================================
SELECT
    t.TABLE_SCHEMA  AS [Schema],
    t.TABLE_NAME    AS [Table],
    COUNT(c.COLUMN_NAME) AS [Columns]
FROM INFORMATION_SCHEMA.TABLES  t
JOIN INFORMATION_SCHEMA.COLUMNS c
    ON c.TABLE_SCHEMA = t.TABLE_SCHEMA
    AND c.TABLE_NAME  = t.TABLE_NAME
WHERE t.TABLE_SCHEMA = 'identity'
  AND t.TABLE_TYPE   = 'BASE TABLE'
GROUP BY t.TABLE_SCHEMA, t.TABLE_NAME
ORDER BY t.TABLE_NAME;
GO
