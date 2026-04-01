-- ============================================================
-- Orders Module — Database Schema
-- Database : EcommerceDB (YashviECommerceDb)
-- Schema   : orders
-- Tables   : Orders, OrderItems, Disputes, Carts, CartItems
-- Safe to re-run — all statements are idempotent.
-- ============================================================


SET NOCOUNT ON;
GO

-- ── Schema ───────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'orders')
    EXEC('CREATE SCHEMA [orders]');
GO

-- ============================================================
-- TABLE: orders.Orders
-- ============================================================
IF OBJECT_ID('[orders].[Orders]', 'U') IS NULL
BEGIN
    CREATE TABLE [orders].[Orders] (

        [Id]                    UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Orders_Id]
                                    DEFAULT NEWSEQUENTIALID(),

        [OrderNumber]           NVARCHAR(30)        NOT NULL,

        -- Ownership
        [BuyerId]               UNIQUEIDENTIFIER    NOT NULL,
        [StoreId]               UNIQUEIDENTIFIER    NOT NULL,
        [SellerId]              UNIQUEIDENTIFIER    NOT NULL,

        -- Status
        [Status]                NVARCHAR(20)        NOT NULL
                                    CONSTRAINT [DF_Orders_Status] DEFAULT 'Pending'
                                    CONSTRAINT [CHK_Orders_Status] CHECK ([Status] IN (
                                        'Pending', 'Confirmed', 'Shipped',
                                        'Delivered', 'Cancelled'
                                    )),

        [PaymentStatus]         NVARCHAR(25)        NOT NULL
                                    CONSTRAINT [DF_Orders_PaymentStatus] DEFAULT 'Pending'
                                    CONSTRAINT [CHK_Orders_PaymentStatus] CHECK ([PaymentStatus] IN (
                                        'Pending', 'Paid', 'Failed',
                                        'Refunded', 'PartiallyRefunded'
                                    )),

        -- Pricing
        [SubTotal]              DECIMAL(18,4)       NOT NULL    CONSTRAINT [DF_Orders_SubTotal] DEFAULT 0,
        [ShippingAmount]        DECIMAL(18,4)       NOT NULL    CONSTRAINT [DF_Orders_ShippingAmount] DEFAULT 0,
        [DiscountAmount]        DECIMAL(18,4)       NOT NULL    CONSTRAINT [DF_Orders_DiscountAmount] DEFAULT 0,
        [TotalAmount]           DECIMAL(18,4)       NOT NULL    CONSTRAINT [DF_Orders_TotalAmount] DEFAULT 0,
        [CurrencyCode]          NVARCHAR(3)         NOT NULL    CONSTRAINT [DF_Orders_CurrencyCode] DEFAULT 'INR',

        -- Shipping address snapshot
        [ShippingName]          NVARCHAR(150)       NULL,
        [ShippingAddressLine1]  NVARCHAR(200)       NULL,
        [ShippingAddressLine2]  NVARCHAR(200)       NULL,
        [ShippingCity]          NVARCHAR(100)       NULL,
        [ShippingState]         NVARCHAR(100)       NULL,
        [ShippingPostalCode]    NVARCHAR(20)        NULL,
        [ShippingCountry]       NVARCHAR(100)       NULL,
        [ShippingPhone]         NVARCHAR(20)        NULL,

        -- Fulfillment
        [TrackingNumber]        NVARCHAR(100)       NULL,
        [ShippingCarrier]       NVARCHAR(100)       NULL,
        [TrackingUrl]           NVARCHAR(500)       NULL,
        [ShippedAt]             DATETIME2(7)        NULL,
        [DeliveredAt]           DATETIME2(7)        NULL,
        [CancelledAt]           DATETIME2(7)        NULL,
        [CancellationReason]    NVARCHAR(1000)      NULL,

        -- Audit
        [CreatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Orders_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Orders_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]             UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Orders_CreatedBy]
                                    DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]             UNIQUEIDENTIFIER    NULL,

        -- Soft delete
        [IsDeleted]             BIT                 NOT NULL
                                    CONSTRAINT [DF_Orders_IsDeleted] DEFAULT 0,
        [DeletedAt]             DATETIME2(7)        NULL,

        -- Optimistic concurrency
        [RowVersion]            ROWVERSION          NOT NULL,

        CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UIX_Orders_OrderNumber]
        ON [orders].[Orders] ([OrderNumber]);

    CREATE NONCLUSTERED INDEX [IX_Orders_BuyerId]
        ON [orders].[Orders] ([BuyerId])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Orders_StoreId]
        ON [orders].[Orders] ([StoreId])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Orders_SellerId]
        ON [orders].[Orders] ([SellerId])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Orders_Status]
        ON [orders].[Orders] ([Status])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Orders_CreatedAt]
        ON [orders].[Orders] ([CreatedAt])
        WHERE ([IsDeleted] = 0);

    PRINT 'Created [orders].[Orders]';
END
ELSE
    PRINT '[orders].[Orders] already exists — skipped.';
GO

-- ============================================================
-- TABLE: orders.OrderItems
-- ============================================================
IF OBJECT_ID('[orders].[OrderItems]', 'U') IS NULL
BEGIN
    CREATE TABLE [orders].[OrderItems] (

        [Id]                UNIQUEIDENTIFIER    NOT NULL
                                CONSTRAINT [DF_OrderItems_Id]
                                DEFAULT NEWSEQUENTIALID(),

        [OrderId]           UNIQUEIDENTIFIER    NOT NULL,
        [ProductId]         UNIQUEIDENTIFIER    NOT NULL,
        [VariantId]         UNIQUEIDENTIFIER    NULL,
        [ProductName]       NVARCHAR(300)       NOT NULL,
        [VariantName]       NVARCHAR(200)       NULL,
        [ProductImageUrl]   NVARCHAR(500)       NULL,
        [Quantity]          INT                 NOT NULL
                                CONSTRAINT [CHK_OrderItems_Quantity] CHECK ([Quantity] > 0),
        [UnitPrice]         DECIMAL(18,4)       NOT NULL
                                CONSTRAINT [CHK_OrderItems_UnitPrice] CHECK ([UnitPrice] >= 0),
        [CurrencyCode]      NVARCHAR(3)         NOT NULL
                                CONSTRAINT [DF_OrderItems_CurrencyCode] DEFAULT 'INR',

        -- Audit
        [CreatedAt]         DATETIME2(7)        NOT NULL
                                CONSTRAINT [DF_OrderItems_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]         DATETIME2(7)        NOT NULL
                                CONSTRAINT [DF_OrderItems_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL
                                CONSTRAINT [DF_OrderItems_CreatedBy]
                                DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]         UNIQUEIDENTIFIER    NULL,
        [IsDeleted]         BIT                 NOT NULL
                                CONSTRAINT [DF_OrderItems_IsDeleted] DEFAULT 0,
        [DeletedAt]         DATETIME2(7)        NULL,

        CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED ([Id]),

        CONSTRAINT [FK_OrderItems_Orders]
            FOREIGN KEY ([OrderId])
            REFERENCES [orders].[Orders] ([Id])
            ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderId]
        ON [orders].[OrderItems] ([OrderId]);

    CREATE NONCLUSTERED INDEX [IX_OrderItems_ProductId]
        ON [orders].[OrderItems] ([ProductId]);

    PRINT 'Created [orders].[OrderItems]';
END
ELSE
    PRINT '[orders].[OrderItems] already exists — skipped.';
GO

-- ============================================================
-- TABLE: orders.Disputes
-- ============================================================
IF OBJECT_ID('[orders].[Disputes]', 'U') IS NULL
BEGIN
    CREATE TABLE [orders].[Disputes] (

        [Id]                    UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Disputes_Id]
                                    DEFAULT NEWSEQUENTIALID(),

        [OrderId]               UNIQUEIDENTIFIER    NOT NULL,
        [BuyerId]               UNIQUEIDENTIFIER    NOT NULL,
        [Reason]                NVARCHAR(1000)      NOT NULL,
        [BuyerEvidence]         NVARCHAR(2000)      NULL,
        [SellerResponse]        NVARCHAR(2000)      NULL,
        [AdminNote]             NVARCHAR(2000)      NULL,
        [Resolution]            NVARCHAR(2000)      NULL,

        [Status]                NVARCHAR(20)        NOT NULL
                                    CONSTRAINT [DF_Disputes_Status] DEFAULT 'Open'
                                    CONSTRAINT [CHK_Disputes_Status] CHECK ([Status] IN (
                                        'Open', 'UnderReview', 'Resolved', 'Escalated'
                                    )),

        [ResolvedByAdminId]     UNIQUEIDENTIFIER    NULL,
        [ResolvedAt]            DATETIME2(7)        NULL,

        -- Audit
        [CreatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Disputes_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]             DATETIME2(7)        NOT NULL
                                    CONSTRAINT [DF_Disputes_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]             UNIQUEIDENTIFIER    NOT NULL
                                    CONSTRAINT [DF_Disputes_CreatedBy]
                                    DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]             UNIQUEIDENTIFIER    NULL,
        [IsDeleted]             BIT                 NOT NULL
                                    CONSTRAINT [DF_Disputes_IsDeleted] DEFAULT 0,
        [DeletedAt]             DATETIME2(7)        NULL,

        CONSTRAINT [PK_Disputes] PRIMARY KEY CLUSTERED ([Id]),

        CONSTRAINT [FK_Disputes_Orders]
            FOREIGN KEY ([OrderId])
            REFERENCES [orders].[Orders] ([Id])
            ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_Disputes_OrderId]
        ON [orders].[Disputes] ([OrderId]);

    CREATE NONCLUSTERED INDEX [IX_Disputes_BuyerId]
        ON [orders].[Disputes] ([BuyerId])
        WHERE ([IsDeleted] = 0);

    CREATE NONCLUSTERED INDEX [IX_Disputes_Status]
        ON [orders].[Disputes] ([Status])
        WHERE ([IsDeleted] = 0);

    PRINT 'Created [orders].[Disputes]';
END
ELSE
    PRINT '[orders].[Disputes] already exists — skipped.';
GO

-- ============================================================
-- TABLE: orders.Carts
-- ============================================================
IF OBJECT_ID('[orders].[Carts]', 'U') IS NULL
BEGIN
    CREATE TABLE [orders].[Carts] (

        [Id]            UNIQUEIDENTIFIER    NOT NULL
                            CONSTRAINT [DF_Carts_Id]
                            DEFAULT NEWSEQUENTIALID(),

        [BuyerId]       UNIQUEIDENTIFIER    NOT NULL,

        -- Audit
        [CreatedAt]     DATETIME2(7)        NOT NULL
                            CONSTRAINT [DF_Carts_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]     DATETIME2(7)        NOT NULL
                            CONSTRAINT [DF_Carts_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]     UNIQUEIDENTIFIER    NOT NULL
                            CONSTRAINT [DF_Carts_CreatedBy]
                            DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]     UNIQUEIDENTIFIER    NULL,
        [IsDeleted]     BIT                 NOT NULL
                            CONSTRAINT [DF_Carts_IsDeleted] DEFAULT 0,
        [DeletedAt]     DATETIME2(7)        NULL,

        CONSTRAINT [PK_Carts] PRIMARY KEY CLUSTERED ([Id])
    );

    -- One active cart per buyer
    CREATE UNIQUE NONCLUSTERED INDEX [UIX_Carts_BuyerId]
        ON [orders].[Carts] ([BuyerId])
        WHERE ([IsDeleted] = 0);

    PRINT 'Created [orders].[Carts]';
END
ELSE
    PRINT '[orders].[Carts] already exists — skipped.';
GO

-- ============================================================
-- TABLE: orders.CartItems
-- ============================================================
IF OBJECT_ID('[orders].[CartItems]', 'U') IS NULL
BEGIN
    CREATE TABLE [orders].[CartItems] (

        [Id]                UNIQUEIDENTIFIER    NOT NULL
                                CONSTRAINT [DF_CartItems_Id]
                                DEFAULT NEWSEQUENTIALID(),

        [CartId]            UNIQUEIDENTIFIER    NOT NULL,
        [ProductId]         UNIQUEIDENTIFIER    NOT NULL,
        [VariantId]         UNIQUEIDENTIFIER    NULL,
        [ProductName]       NVARCHAR(300)       NOT NULL,
        [VariantName]       NVARCHAR(200)       NULL,
        [ProductImageUrl]   NVARCHAR(500)       NULL,
        [UnitPrice]         DECIMAL(18,4)       NOT NULL
                                CONSTRAINT [CHK_CartItems_UnitPrice] CHECK ([UnitPrice] >= 0),
        [CurrencyCode]      NVARCHAR(3)         NOT NULL
                                CONSTRAINT [DF_CartItems_CurrencyCode] DEFAULT 'INR',
        [Quantity]          INT                 NOT NULL
                                CONSTRAINT [CHK_CartItems_Quantity] CHECK ([Quantity] > 0),

        -- Audit
        [CreatedAt]         DATETIME2(7)        NOT NULL
                                CONSTRAINT [DF_CartItems_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt]         DATETIME2(7)        NOT NULL
                                CONSTRAINT [DF_CartItems_UpdatedAt] DEFAULT GETUTCDATE(),
        [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL
                                CONSTRAINT [DF_CartItems_CreatedBy]
                                DEFAULT '00000000-0000-0000-0000-000000000000',
        [UpdatedBy]         UNIQUEIDENTIFIER    NULL,
        [IsDeleted]         BIT                 NOT NULL
                                CONSTRAINT [DF_CartItems_IsDeleted] DEFAULT 0,
        [DeletedAt]         DATETIME2(7)        NULL,

        CONSTRAINT [PK_CartItems] PRIMARY KEY CLUSTERED ([Id]),

        CONSTRAINT [FK_CartItems_Carts]
            FOREIGN KEY ([CartId])
            REFERENCES [orders].[Carts] ([Id])
            ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_CartItems_CartId]
        ON [orders].[CartItems] ([CartId]);

    PRINT 'Created [orders].[CartItems]';
END
ELSE
    PRINT '[orders].[CartItems] already exists — skipped.';
GO

-- ============================================================
-- EF Core migrations history table for orders schema
-- ============================================================
IF OBJECT_ID('[orders].[__MigrationsHistory]', 'U') IS NULL
BEGIN
    CREATE TABLE [orders].[__MigrationsHistory] (
        [MigrationId]    NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32)  NOT NULL,
        CONSTRAINT [PK_orders___MigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT 'Created [orders].[__MigrationsHistory]';
END
ELSE
    PRINT '[orders].[__MigrationsHistory] already exists — skipped.';
GO

-- ============================================================
-- VERIFICATION
-- ============================================================
SELECT
    t.TABLE_SCHEMA      AS [Schema],
    t.TABLE_NAME        AS [Table],
    COUNT(c.COLUMN_NAME) AS [Columns]
FROM INFORMATION_SCHEMA.TABLES  t
JOIN INFORMATION_SCHEMA.COLUMNS c
    ON c.TABLE_SCHEMA = t.TABLE_SCHEMA
    AND c.TABLE_NAME  = t.TABLE_NAME
WHERE t.TABLE_SCHEMA = 'orders'
  AND t.TABLE_TYPE   = 'BASE TABLE'
GROUP BY t.TABLE_SCHEMA, t.TABLE_NAME
ORDER BY t.TABLE_NAME;
GO

-- Check indexes
SELECT
    t.name      AS TableName,
    i.name      AS IndexName,
    i.is_unique,
    i.filter_definition
FROM sys.indexes i
INNER JOIN sys.tables t  ON i.object_id  = t.object_id
INNER JOIN sys.schemas s ON t.schema_id  = s.schema_id
WHERE s.name = 'orders'
  AND i.name IS NOT NULL
ORDER BY t.name, i.name;
GO