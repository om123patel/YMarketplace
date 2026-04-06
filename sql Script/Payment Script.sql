GO

SET NOCOUNT ON;
GO

-- ── Schema ───────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'payments')
    EXEC('CREATE SCHEMA [payments]');
GO

-- ============================================================
-- TABLE: payments.CommissionRules
-- Admin-configured commission rates per category.
-- NULL CategoryId = global default.
-- ============================================================
IF OBJECT_ID('[payments].[CommissionRules]', 'U') IS NULL
BEGIN
    CREATE TABLE [payments].[CommissionRules] (

        [Id]            INT                 NOT NULL IDENTITY(1,1),
        [CategoryId]    INT                 NULL,
        [Name]          NVARCHAR(100)       NOT NULL,
        [RatePercent]   DECIMAL(5,2)        NOT NULL
                            CONSTRAINT [CHK_CommissionRules_Rate]
                            CHECK ([RatePercent] >= 0 AND [RatePercent] <= 100),
        [IsActive]      BIT                 NOT NULL
                            CONSTRAINT [DF_CommissionRules_IsActive] DEFAULT 1,

        -- Audit
        [CreatedAt]     DATETIME2(7)        NOT NULL
                            CONSTRAINT [DF_CommissionRules_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]     DATETIME2(7)        NOT NULL
                            CONSTRAINT [DF_CommissionRules_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]     UNIQUEIDENTIFIER    NOT NULL
                            CONSTRAINT [DF_CommissionRules_CreatedBy]
                            DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]     UNIQUEIDENTIFIER    NULL,
        [IsDeleted]     BIT                 NOT NULL
                            CONSTRAINT [DF_CommissionRules_IsDeleted] DEFAULT 0,
        [DeletedAt]     DATETIME2(7)        NULL,

        CONSTRAINT [PK_CommissionRules] PRIMARY KEY CLUSTERED ([Id])
    );

    -- One rule per category — null = global default
    CREATE UNIQUE NONCLUSTERED INDEX [UIX_CommissionRules_CategoryId]
        ON [payments].[CommissionRules] ([CategoryId])
        WHERE ([IsDeleted] = 0);

    PRINT 'Created [payments].[CommissionRules]';
END
ELSE
    PRINT '[payments].[CommissionRules] already exists — skipped.';
GO

-- ============================================================
-- TABLE: payments.Transactions
-- One transaction per order.
-- ============================================================
IF OBJECT_ID('[payments].[Transactions]', 'U') IS NULL
BEGIN
    CREATE TABLE [payments].[Transactions] (

        [Id]                    UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Transactions_Id]
                                    DEFAULT NEWSEQUENTIALID(),

        -- References (logical — no FK across schemas)
        [OrderId]               UNIQUEIDENTIFIER    NOT NULL,
        [BuyerId]               UNIQUEIDENTIFIER    NOT NULL,
        [SellerId]              UNIQUEIDENTIFIER    NOT NULL,
        [StoreId]               UNIQUEIDENTIFIER    NOT NULL,

        -- Money
        [Amount]                DECIMAL(18,4)       NOT NULL
                                    CONSTRAINT [CHK_Transactions_Amount]
                                    CHECK ([Amount] > 0),
        [CommissionAmount]      DECIMAL(18,4)       NOT NULL
                                    CONSTRAINT [DF_Transactions_Commission] DEFAULT 0,
        [SellerAmount]          DECIMAL(18,4)       NOT NULL
                                    CONSTRAINT [DF_Transactions_SellerAmount] DEFAULT 0,
        [CurrencyCode]          NVARCHAR(3)         NOT NULL
                                    CONSTRAINT [DF_Transactions_Currency] DEFAULT 'INR',

        -- Status + method
        [Status]                NVARCHAR(30)        NOT NULL
                                    CONSTRAINT [DF_Transactions_Status] DEFAULT 'Pending'
                                    CONSTRAINT [CHK_Transactions_Status] CHECK ([Status] IN (
                                        'Pending', 'Completed', 'Failed',
                                        'Refunded', 'PartiallyRefunded'
                                    )),
        [Method]                NVARCHAR(20)        NOT NULL
                                    CONSTRAINT [CHK_Transactions_Method] CHECK ([Method] IN (
                                        'Card', 'UPI', 'NetBanking', 'Wallet', 'COD'
                                    )),

        -- Gateway
        [GatewayTransactionId]  NVARCHAR(200)       NULL,
        [GatewayProvider]       NVARCHAR(50)        NULL,
        [GatewayResponse]       NVARCHAR(MAX)       NULL,

        -- Refund
        [RefundedAmount]        DECIMAL(18,4)       NOT NULL
                                    CONSTRAINT [DF_Transactions_Refunded] DEFAULT 0,
        [RefundReason]          NVARCHAR(500)       NULL,
        [RefundedAt]            DATETIME2(7)        NULL,

        -- Timestamps
        [CompletedAt]           DATETIME2(7)        NULL,
        [FailedAt]              DATETIME2(7)        NULL,

        -- Audit
        [CreatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Transactions_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Transactions_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]             UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Transactions_CreatedBy]
                                    DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]             UNIQUEIDENTIFIER    NULL,
        [IsDeleted]             BIT                 NOT NULL
                                    CONSTRAINT [DF_Transactions_IsDeleted] DEFAULT 0,
        [DeletedAt]             DATETIME2(7)        NULL,
        [RowVersion]            ROWVERSION          NOT NULL,

        CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED ([Id])
    );

    -- One transaction per order
    CREATE UNIQUE NONCLUSTERED INDEX [UIX_Transactions_OrderId]
        ON [payments].[Transactions] ([OrderId])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Transactions_SellerId]
        ON [payments].[Transactions] ([SellerId])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Transactions_BuyerId]
        ON [payments].[Transactions] ([BuyerId])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Transactions_Status]
        ON [payments].[Transactions] ([Status])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Transactions_CreatedAt]
        ON [payments].[Transactions] ([CreatedAt])
        WHERE ([IsDeleted] = 0);

    PRINT 'Created [payments].[Transactions]';
END
ELSE
    PRINT '[payments].[Transactions] already exists — skipped.';
GO

-- ============================================================
-- TABLE: payments.Payouts
-- Seller earnings payout lifecycle.
-- ============================================================
IF OBJECT_ID('[payments].[Payouts]', 'U') IS NULL
BEGIN
    CREATE TABLE [payments].[Payouts] (

        [Id]                    UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Payouts_Id]
                                    DEFAULT NEWSEQUENTIALID(),

        [SellerId]              UNIQUEIDENTIFIER    NOT NULL,
        [Amount]                DECIMAL(18,4)       NOT NULL
                                    CONSTRAINT [CHK_Payouts_Amount]
                                    CHECK ([Amount] > 0),
        [CurrencyCode]          NVARCHAR(3)         NOT NULL
                                    CONSTRAINT [DF_Payouts_Currency] DEFAULT 'INR',

        [Status]                NVARCHAR(20)        NOT NULL
                                    CONSTRAINT [DF_Payouts_Status] DEFAULT 'Pending'
                                    CONSTRAINT [CHK_Payouts_Status] CHECK ([Status] IN (
                                        'Pending', 'Processing', 'Completed', 'Failed', 'Cancelled'
                                    )),

        -- Payment details
        [BankAccountNumber]     NVARCHAR(20)        NULL,
        [BankIfscCode]          NVARCHAR(15)        NULL,
        [BankAccountName]       NVARCHAR(150)       NULL,
        [UpiId]                 NVARCHAR(100)       NULL,

        -- Admin
        [ProcessedByAdminId]    UNIQUEIDENTIFIER    NULL,
        [AdminNote]             NVARCHAR(500)       NULL,
        [FailureReason]         NVARCHAR(500)       NULL,
        [GatewayReference]      NVARCHAR(200)       NULL,

        -- Timestamps
        [ProcessedAt]           DATETIME2(7)        NULL,
        [CompletedAt]           DATETIME2(7)        NULL,
        [FailedAt]              DATETIME2(7)        NULL,
        [CancelledAt]           DATETIME2(7)        NULL,

        -- Audit
        [CreatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Payouts_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Payouts_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]             UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Payouts_CreatedBy]
                                    DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]             UNIQUEIDENTIFIER    NULL,
        [IsDeleted]             BIT                 NOT NULL
                                    CONSTRAINT [DF_Payouts_IsDeleted] DEFAULT 0,
        [DeletedAt]             DATETIME2(7)        NULL,
        [RowVersion]            ROWVERSION          NOT NULL,

        CONSTRAINT [PK_Payouts] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_Payouts_SellerId]
        ON [payments].[Payouts] ([SellerId])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Payouts_Status]
        ON [payments].[Payouts] ([Status])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Payouts_CreatedAt]
        ON [payments].[Payouts] ([CreatedAt])
        WHERE ([IsDeleted] = 0);

    PRINT 'Created [payments].[Payouts]';
END
ELSE
    PRINT '[payments].[Payouts] already exists — skipped.';
GO

-- ============================================================
-- TABLE: payments.PayoutTransactions
-- Junction: which Transactions are included in a Payout.
-- ============================================================
IF OBJECT_ID('[payments].[PayoutTransactions]', 'U') IS NULL
BEGIN
    CREATE TABLE [payments].[PayoutTransactions] (

        [Id]                INT                 NOT NULL IDENTITY(1,1),
        [PayoutId]          UNIQUEIDENTIFIER    NOT NULL,
        [TransactionId]     UNIQUEIDENTIFIER    NOT NULL,

        -- Audit
        [CreatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt]         DATETIME2(7)        NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL
                                DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]         UNIQUEIDENTIFIER    NULL,
        [IsDeleted]         BIT                 NOT NULL DEFAULT 0,
        [DeletedAt]         DATETIME2(7)        NULL,

        CONSTRAINT [PK_PayoutTransactions] PRIMARY KEY ([Id]),

        CONSTRAINT [FK_PayoutTransactions_Payouts]
            FOREIGN KEY ([PayoutId])
            REFERENCES [payments].[Payouts] ([Id])
            ON DELETE CASCADE,

        CONSTRAINT [FK_PayoutTransactions_Transactions]
            FOREIGN KEY ([TransactionId])
            REFERENCES [payments].[Transactions] ([Id])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UIX_PayoutTransactions_Pair]
        ON [payments].[PayoutTransactions] ([PayoutId], [TransactionId]);

    CREATE NONCLUSTERED INDEX [IX_PayoutTransactions_TransactionId]
        ON [payments].[PayoutTransactions] ([TransactionId]);

    PRINT 'Created [payments].[PayoutTransactions]';
END
ELSE
    PRINT '[payments].[PayoutTransactions] already exists — skipped.';
GO

-- ============================================================
-- EF Core migrations history
-- ============================================================
IF OBJECT_ID('[payments].[__MigrationsHistory]', 'U') IS NULL
BEGIN
    CREATE TABLE [payments].[__MigrationsHistory] (
        [MigrationId]    NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32)  NOT NULL,
        CONSTRAINT [PK_payments___MigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT 'Created [payments].[__MigrationsHistory]';
END
ELSE
    PRINT '[payments].[__MigrationsHistory] already exists — skipped.';
GO

-- ============================================================
-- SEED: Commission rules (5 fixed GUIDs — idempotent MERGE)
-- ============================================================
DECLARE @AdminId UNIQUEIDENTIFIER = (
    SELECT TOP 1 Id FROM [identity].[Users]
    WHERE Email = 'admin@markethub.com' AND IsDeleted = 0
);

-- Global default (no category)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[CommissionRules]
    WHERE CategoryId IS NULL AND IsDeleted = 0
)
BEGIN
    INSERT INTO [payments].[CommissionRules]
        (Name, CategoryId, RatePercent, IsActive, CreatedBy)
    VALUES
        (N'Global Default', NULL, 10.00, 1, @AdminId);
    PRINT 'Seeded Global Default commission rule (10%)';
END

-- Electronics (CategoryId = 1 per catalog seed)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[CommissionRules]
    WHERE CategoryId = 1 AND IsDeleted = 0
)
BEGIN
    INSERT INTO [payments].[CommissionRules]
        (Name, CategoryId, RatePercent, IsActive, CreatedBy)
    VALUES
        (N'Electronics', 1, 8.00, 1, @AdminId);
    PRINT 'Seeded Electronics commission rule (8%)';
END

-- Clothing (CategoryId = 2)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[CommissionRules]
    WHERE CategoryId = 2 AND IsDeleted = 0
)
BEGIN
    INSERT INTO [payments].[CommissionRules]
        (Name, CategoryId, RatePercent, IsActive, CreatedBy)
    VALUES
        (N'Clothing', 2, 12.00, 1, @AdminId);
    PRINT 'Seeded Clothing commission rule (12%)';
END

-- Home & Garden (CategoryId = 3)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[CommissionRules]
    WHERE CategoryId = 3 AND IsDeleted = 0
)
BEGIN
    INSERT INTO [payments].[CommissionRules]
        (Name, CategoryId, RatePercent, IsActive, CreatedBy)
    VALUES
        (N'Home & Garden', 3, 9.00, 1, @AdminId);
    PRINT 'Seeded Home & Garden commission rule (9%)';
END

-- Sports (CategoryId = 4)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[CommissionRules]
    WHERE CategoryId = 4 AND IsDeleted = 0
)
BEGIN
    INSERT INTO [payments].[CommissionRules]
        (Name, CategoryId, RatePercent, IsActive, CreatedBy)
    VALUES
        (N'Sports', 4, 10.00, 1, @AdminId);
    PRINT 'Seeded Sports commission rule (10%)';
END
GO

