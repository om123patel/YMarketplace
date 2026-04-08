-- =====================================================================
-- Inventory Schema — Idempotent
-- =====================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'inventory')
    EXEC('CREATE SCHEMA inventory');
GO

-- ------------------------------------------------------------------
-- Stocks
-- ------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'inventory' AND t.name = 'Stocks'
)
BEGIN
    CREATE TABLE inventory.Stocks (
        Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
        ProductId           UNIQUEIDENTIFIER    NOT NULL,
        VariantId           UNIQUEIDENTIFIER    NULL,
        Quantity            INT                 NOT NULL DEFAULT 0,
        ReservedQuantity    INT                 NOT NULL DEFAULT 0,
        LowStockThreshold   INT                 NOT NULL DEFAULT 5,
        TrackInventory      BIT                 NOT NULL DEFAULT 1,
        AllowBackorder      BIT                 NOT NULL DEFAULT 0,
        Status              NVARCHAR(20)        NOT NULL DEFAULT 'OutOfStock'
            CONSTRAINT CK_Stocks_Status
                CHECK (Status IN ('InStock','LowStock','OutOfStock')),
        RowVersion          ROWVERSION          NOT NULL,
        IsDeleted           BIT                 NOT NULL DEFAULT 0,
        CreatedAt           DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy           UNIQUEIDENTIFIER    NULL,
        UpdatedAt           DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
        UpdatedBy           UNIQUEIDENTIFIER    NULL,
        CONSTRAINT PK_Stocks PRIMARY KEY (Id),
        CONSTRAINT CK_Stocks_Quantity
            CHECK (Quantity >= 0),
        CONSTRAINT CK_Stocks_Reserved
            CHECK (ReservedQuantity >= 0),
        CONSTRAINT CK_Stocks_Threshold
            CHECK (LowStockThreshold >= 0)
    );

    -- Unique active stock per product+variant
    CREATE UNIQUE INDEX UX_Stocks_ProductVariant
        ON inventory.Stocks (ProductId, VariantId)
        WHERE IsDeleted = 0;

    CREATE INDEX IX_Stocks_ProductId
        ON inventory.Stocks (ProductId)
        WHERE IsDeleted = 0;

    CREATE INDEX IX_Stocks_Status
        ON inventory.Stocks (Status)
        WHERE IsDeleted = 0;

    CREATE INDEX IX_Stocks_LowStock
        ON inventory.Stocks (Status, Quantity)
        WHERE IsDeleted = 0 AND Status = 'LowStock';

    PRINT 'Created inventory.Stocks';
END
GO

-- ------------------------------------------------------------------
-- StockReservations
-- ------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'inventory' AND t.name = 'StockReservations'
)
BEGIN
    CREATE TABLE inventory.StockReservations (
        Id          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
        StockId     UNIQUEIDENTIFIER    NOT NULL,
        OrderId     UNIQUEIDENTIFIER    NOT NULL,
        Quantity    INT                 NOT NULL,
        Status      NVARCHAR(20)        NOT NULL DEFAULT 'Active'
            CONSTRAINT CK_StockReservations_Status
                CHECK (Status IN ('Active','Released','Confirmed')),
        ReleasedAt  DATETIME2           NULL,
        ConfirmedAt DATETIME2           NULL,
        CreatedAt   DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy   UNIQUEIDENTIFIER    NULL,
        UpdatedAt   DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
        UpdatedBy   UNIQUEIDENTIFIER    NULL,
        CONSTRAINT PK_StockReservations PRIMARY KEY (Id),
        CONSTRAINT FK_StockReservations_Stock
            FOREIGN KEY (StockId)
            REFERENCES inventory.Stocks(Id)
            ON DELETE CASCADE,
        CONSTRAINT CK_StockReservations_Quantity
            CHECK (Quantity > 0)
    );

    CREATE INDEX IX_StockReservations_StockId
        ON inventory.StockReservations (StockId);

    CREATE INDEX IX_StockReservations_OrderId
        ON inventory.StockReservations (OrderId);

    CREATE INDEX IX_StockReservations_OrderStatus
        ON inventory.StockReservations (OrderId, Status)
        WHERE Status = 'Active';

    PRINT 'Created inventory.StockReservations';
END
GO

-- ------------------------------------------------------------------
-- MigrationsHistory
-- ------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'inventory' AND t.name = '__MigrationsHistory'
)
BEGIN
    CREATE TABLE inventory.__MigrationsHistory (
        MigrationId    NVARCHAR(150) NOT NULL,
        ProductVersion NVARCHAR(32)  NOT NULL,
        CONSTRAINT PK_MigrationsHistory
            PRIMARY KEY (MigrationId)
    );
    PRINT 'Created inventory.__MigrationsHistory';
END
GO