-- sql Script/customer_features_schema.sql
-- Customer Wishlist, Addresses, and Wallet tables
-- Schema: catalog (wishlist) | identity (address + wallet)
-- Safe to re-run — all statements are idempotent.

SET NOCOUNT ON;
GO

-- ============================================================
-- TABLE: catalog.Wishlists
-- ============================================================
IF OBJECT_ID('[catalog].[Wishlists]', 'U') IS NULL
BEGIN
    CREATE TABLE [catalog].[Wishlists] (
        [Id]        UNIQUEIDENTIFIER    NOT NULL CONSTRAINT [DF_Wishlists_Id] DEFAULT NEWSEQUENTIALID(),
        [BuyerId]   UNIQUEIDENTIFIER    NOT NULL,
        [CreatedAt] DATETIME2           NOT NULL CONSTRAINT [DF_Wishlists_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2           NOT NULL CONSTRAINT [DF_Wishlists_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy] UNIQUEIDENTIFIER    NOT NULL CONSTRAINT [DF_Wishlists_CreatedBy] DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy] UNIQUEIDENTIFIER    NULL,
        [IsDeleted] BIT                 NOT NULL CONSTRAINT [DF_Wishlists_IsDeleted] DEFAULT 0,
        [DeletedAt] DATETIME2           NULL,
        CONSTRAINT [PK_Wishlists] PRIMARY KEY CLUSTERED ([Id])
    );
    CREATE UNIQUE NONCLUSTERED INDEX [UIX_Wishlists_BuyerId]
        ON [catalog].[Wishlists] ([BuyerId]) WHERE ([IsDeleted] = 0);
    PRINT 'Created [catalog].[Wishlists]';
END
ELSE PRINT '[catalog].[Wishlists] already exists — skipped.';
GO

-- ============================================================
-- TABLE: catalog.WishlistItems
-- ============================================================
IF OBJECT_ID('[catalog].[WishlistItems]', 'U') IS NULL
BEGIN
    CREATE TABLE [catalog].[WishlistItems] (
        [Id]         UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_WishlistItems_Id] DEFAULT NEWSEQUENTIALID(),
        [WishlistId] UNIQUEIDENTIFIER NOT NULL,
        [ProductId]  UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt]  DATETIME2        NOT NULL CONSTRAINT [DF_WishlistItems_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]  DATETIME2        NOT NULL CONSTRAINT [DF_WishlistItems_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]  UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_WishlistItems_CreatedBy] DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]  UNIQUEIDENTIFIER NULL,
        [IsDeleted]  BIT              NOT NULL CONSTRAINT [DF_WishlistItems_IsDeleted] DEFAULT 0,
        [DeletedAt]  DATETIME2        NULL,
        CONSTRAINT [PK_WishlistItems] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_WishlistItems_Wishlists] FOREIGN KEY ([WishlistId])
            REFERENCES [catalog].[Wishlists] ([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE NONCLUSTERED INDEX [UIX_WishlistItems_Pair]
        ON [catalog].[WishlistItems] ([WishlistId], [ProductId]);
    PRINT 'Created [catalog].[WishlistItems]';
END
ELSE PRINT '[catalog].[WishlistItems] already exists — skipped.';
GO

-- ============================================================
-- TABLE: identity.CustomerAddresses
-- ============================================================
IF OBJECT_ID('[identity].[CustomerAddresses]', 'U') IS NULL
BEGIN
    CREATE TABLE [identity].[CustomerAddresses] (
        [Id]           UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_CustAddr_Id] DEFAULT NEWSEQUENTIALID(),
        [UserId]       UNIQUEIDENTIFIER NOT NULL,
        [FullName]     NVARCHAR(150)    NOT NULL,
        [Phone]        NVARCHAR(20)     NOT NULL,
        [AddressLine1] NVARCHAR(200)    NOT NULL,
        [AddressLine2] NVARCHAR(200)    NULL,
        [City]         NVARCHAR(100)    NOT NULL,
        [State]        NVARCHAR(100)    NOT NULL,
        [PostalCode]   NVARCHAR(20)     NOT NULL,
        [Country]      NVARCHAR(100)    NOT NULL,
        [IsDefault]    BIT              NOT NULL CONSTRAINT [DF_CustAddr_IsDefault] DEFAULT 0,
        [Label]        NVARCHAR(20)     NULL,
        [CreatedAt]    DATETIME2        NOT NULL CONSTRAINT [DF_CustAddr_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]    DATETIME2        NOT NULL CONSTRAINT [DF_CustAddr_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]    UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_CustAddr_CreatedBy] DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]    UNIQUEIDENTIFIER NULL,
        [IsDeleted]    BIT              NOT NULL CONSTRAINT [DF_CustAddr_IsDeleted] DEFAULT 0,
        [DeletedAt]    DATETIME2        NULL,
        CONSTRAINT [PK_CustomerAddresses] PRIMARY KEY CLUSTERED ([Id])
    );
    CREATE NONCLUSTERED INDEX [IX_CustomerAddresses_UserId]
        ON [identity].[CustomerAddresses] ([UserId]) WHERE ([IsDeleted] = 0);
    PRINT 'Created [identity].[CustomerAddresses]';
END
ELSE PRINT '[identity].[CustomerAddresses] already exists — skipped.';
GO

-- ============================================================
-- TABLE: identity.CustomerWallets
-- ============================================================
IF OBJECT_ID('[identity].[CustomerWallets]', 'U') IS NULL
BEGIN
    CREATE TABLE [identity].[CustomerWallets] (
        [Id]           UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_Wallets_Id] DEFAULT NEWSEQUENTIALID(),
        [UserId]       UNIQUEIDENTIFIER NOT NULL,
        [Balance]      DECIMAL(18,4)    NOT NULL CONSTRAINT [DF_Wallets_Balance] DEFAULT 0,
        [CurrencyCode] NVARCHAR(3)      NOT NULL CONSTRAINT [DF_Wallets_Currency] DEFAULT 'INR',
        [IsActive]     BIT              NOT NULL CONSTRAINT [DF_Wallets_IsActive] DEFAULT 1,
        [CreatedAt]    DATETIME2        NOT NULL CONSTRAINT [DF_Wallets_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]    DATETIME2        NOT NULL CONSTRAINT [DF_Wallets_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]    UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_Wallets_CreatedBy] DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]    UNIQUEIDENTIFIER NULL,
        [IsDeleted]    BIT              NOT NULL CONSTRAINT [DF_Wallets_IsDeleted] DEFAULT 0,
        [DeletedAt]    DATETIME2        NULL,
        CONSTRAINT [PK_CustomerWallets] PRIMARY KEY CLUSTERED ([Id])
    );
    CREATE UNIQUE NONCLUSTERED INDEX [UIX_Wallets_UserId]
        ON [identity].[CustomerWallets] ([UserId]) WHERE ([IsDeleted] = 0);
    PRINT 'Created [identity].[CustomerWallets]';
END
ELSE PRINT '[identity].[CustomerWallets] already exists — skipped.';
GO

-- ============================================================
-- TABLE: identity.WalletTransactions
-- ============================================================
IF OBJECT_ID('[identity].[WalletTransactions]', 'U') IS NULL
BEGIN
    CREATE TABLE [identity].[WalletTransactions] (
        [Id]           UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_WalletTxn_Id] DEFAULT NEWSEQUENTIALID(),
        [WalletId]     UNIQUEIDENTIFIER NOT NULL,
        [Amount]       DECIMAL(18,4)    NOT NULL,
        [Type]         NVARCHAR(10)     NOT NULL
                           CONSTRAINT [CHK_WalletTxn_Type] CHECK ([Type] IN ('Credit','Debit')),
        [Description]  NVARCHAR(300)    NOT NULL,
        [ReferenceId]  NVARCHAR(200)    NULL,
        [BalanceAfter] DECIMAL(18,4)    NOT NULL,
        [CreatedAt]    DATETIME2        NOT NULL CONSTRAINT [DF_WalletTxn_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]    DATETIME2        NOT NULL CONSTRAINT [DF_WalletTxn_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]    UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_WalletTxn_CreatedBy] DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]    UNIQUEIDENTIFIER NULL,
        [IsDeleted]    BIT              NOT NULL CONSTRAINT [DF_WalletTxn_IsDeleted] DEFAULT 0,
        [DeletedAt]    DATETIME2        NULL,
        CONSTRAINT [PK_WalletTransactions] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_WalletTransactions_Wallets] FOREIGN KEY ([WalletId])
            REFERENCES [identity].[CustomerWallets] ([Id]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_WalletTransactions_WalletId]
        ON [identity].[WalletTransactions] ([WalletId]);
    PRINT 'Created [identity].[WalletTransactions]';
END
ELSE PRINT '[identity].[WalletTransactions] already exists — skipped.';
GO

PRINT 'Customer features schema complete.';
GO	