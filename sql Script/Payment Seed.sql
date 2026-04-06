-- ============================================================
-- SEED: Transactions (linked to order seed GUIDs)
-- ============================================================

-- Transaction for Order 1 (Pending order → Paid transaction)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[Transactions]
    WHERE OrderId = 'A1000001-0000-0000-0000-000000000001'
)
BEGIN
    INSERT INTO [payments].[Transactions] (
        OrderId, BuyerId, SellerId, StoreId,
        Amount, CommissionAmount, SellerAmount, CurrencyCode,
        [Status], Method,
        GatewayTransactionId, GatewayProvider,
        CompletedAt,
        CreatedAt, UpdatedAt, CreatedBy
    )
    VALUES (
        'A1000001-0000-0000-0000-000000000001',
        'A0000001-0000-0000-0000-000000000001',
        'A0000001-0000-0000-0000-000000000002',
        'A0000001-0000-0000-0000-000000000004',
        15098.00, 1207.84, 13890.16, 'INR',
        'Completed', 'UPI',
        N'RZP-TXN-001-UPIOK',
        N'Razorpay',
        DATEADD(MINUTE, -5, GETUTCDATE()),
        DATEADD(DAY, -1, GETUTCDATE()),
        DATEADD(DAY, -1, GETUTCDATE()),
        'A0000001-0000-0000-0000-000000000001'
    );
    PRINT 'Seeded Transaction for Order 1';
END
ELSE
    PRINT 'Transaction for Order 1 already exists — skipped.';
GO

-- Transaction for Order 2 (Confirmed)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[Transactions]
    WHERE OrderId = 'A1000001-0000-0000-0000-000000000002'
)
BEGIN
    INSERT INTO [payments].[Transactions] (
        OrderId, BuyerId, SellerId, StoreId,
        Amount, CommissionAmount, SellerAmount, CurrencyCode,
        [Status], Method,
        GatewayTransactionId, GatewayProvider,
        CompletedAt,
        CreatedAt, UpdatedAt, CreatedBy
    )
    VALUES (
        'A1000001-0000-0000-0000-000000000002',
        'A0000001-0000-0000-0000-000000000001',
        'A0000001-0000-0000-0000-000000000002',
        'A0000001-0000-0000-0000-000000000004',
        8000.00, 640.00, 7360.00, 'INR',
        'Completed', 'Card',
        N'RZP-TXN-002-CARDOK',
        N'Razorpay',
        DATEADD(DAY, -3, GETUTCDATE()),
        DATEADD(DAY, -3, GETUTCDATE()),
        DATEADD(DAY, -3, GETUTCDATE()),
        'A0000001-0000-0000-0000-000000000001'
    );
    PRINT 'Seeded Transaction for Order 2';
END
ELSE
    PRINT 'Transaction for Order 2 already exists — skipped.';
GO

-- Transaction for Order 3 (Shipped)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[Transactions]
    WHERE OrderId = 'A1000001-0000-0000-0000-000000000003'
)
BEGIN
    INSERT INTO [payments].[Transactions] (
        OrderId, BuyerId, SellerId, StoreId,
        Amount, CommissionAmount, SellerAmount, CurrencyCode,
        [Status], Method,
        GatewayTransactionId, GatewayProvider,
        CompletedAt,
        CreatedAt, UpdatedAt, CreatedBy
    )
    VALUES (
        'A1000001-0000-0000-0000-000000000003',
        'A0000001-0000-0000-0000-000000000001',
        'A0000001-0000-0000-0000-000000000002',
        'A0000001-0000-0000-0000-000000000004',
        22199.00, 1775.92, 20423.08, 'INR',
        'Completed', 'NetBanking',
        N'RZP-TXN-003-NETBANK',
        N'Razorpay',
        DATEADD(DAY, -5, GETUTCDATE()),
        DATEADD(DAY, -5, GETUTCDATE()),
        DATEADD(DAY, -5, GETUTCDATE()),
        'A0000001-0000-0000-0000-000000000001'
    );
    PRINT 'Seeded Transaction for Order 3';
END
ELSE
    PRINT 'Transaction for Order 3 already exists — skipped.';
GO

-- Transaction for Order 4 (Delivered)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[Transactions]
    WHERE OrderId = 'A1000001-0000-0000-0000-000000000004'
)
BEGIN
    INSERT INTO [payments].[Transactions] (
        OrderId, BuyerId, SellerId, StoreId,
        Amount, CommissionAmount, SellerAmount, CurrencyCode,
        [Status], Method,
        GatewayTransactionId, GatewayProvider,
        CompletedAt,
        CreatedAt, UpdatedAt, CreatedBy
    )
    VALUES (
        'A1000001-0000-0000-0000-000000000004',
        'A0000001-0000-0000-0000-000000000001',
        'A0000001-0000-0000-0000-000000000002',
        'A0000001-0000-0000-0000-000000000004',
        3299.00, 329.90, 2969.10, 'INR',
        'Completed', 'UPI',
        N'RZP-TXN-004-UPIOK2',
        N'Razorpay',
        DATEADD(DAY, -14, GETUTCDATE()),
        DATEADD(DAY, -14, GETUTCDATE()),
        DATEADD(DAY, -14, GETUTCDATE()),
        'A0000001-0000-0000-0000-000000000001'
    );
    PRINT 'Seeded Transaction for Order 4';
END
ELSE
    PRINT 'Transaction for Order 4 already exists — skipped.';
GO

-- Transaction for Order 5 (Cancelled → Refunded)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[Transactions]
    WHERE OrderId = 'A1000001-0000-0000-0000-000000000005'
)
BEGIN
    INSERT INTO [payments].[Transactions] (
        OrderId, BuyerId, SellerId, StoreId,
        Amount, CommissionAmount, SellerAmount, CurrencyCode,
        [Status], Method,
        GatewayTransactionId, GatewayProvider,
        RefundedAmount, RefundReason, RefundedAt,
        CompletedAt,
        CreatedAt, UpdatedAt, CreatedBy
    )
    VALUES (
        'A1000001-0000-0000-0000-000000000005',
        'A0000001-0000-0000-0000-000000000001',
        'A0000001-0000-0000-0000-000000000002',
        'A0000001-0000-0000-0000-000000000004',
        12599.00, 1259.90, 11339.10, 'INR',
        'Refunded', 'Card',
        N'RZP-TXN-005-CARDOK',
        N'Razorpay',
        12599.00,
        N'Order cancelled by buyer — full refund processed.',
        DATEADD(DAY, -8, GETUTCDATE()),
        DATEADD(DAY, -9, GETUTCDATE()),
        DATEADD(DAY, -9, GETUTCDATE()),
        DATEADD(DAY, -8, GETUTCDATE()),
        'A0000001-0000-0000-0000-000000000001'
    );
    PRINT 'Seeded Transaction for Order 5 (Refunded)';
END
ELSE
    PRINT 'Transaction for Order 5 already exists — skipped.';
GO

-- ============================================================
-- SEED: Payout (one completed, one pending)
-- ============================================================
DECLARE @AdminId UNIQUEIDENTIFIER = (
    SELECT TOP 1 Id FROM [identity].[Users]
    WHERE Email = 'admin@markethub.com' AND IsDeleted = 0
);

-- Payout 1 — Completed (covers Orders 2, 3)
IF NOT EXISTS (
    SELECT 1 FROM [payments].[Payouts]
    WHERE Id = 'F6000001-0000-0000-0000-000000000001'
)
BEGIN
    INSERT INTO [payments].[Payouts] (
        Id, SellerId, Amount, CurrencyCode,
        [Status],
        UpiId,
        ProcessedByAdminId, AdminNote,
        GatewayReference,
ProcessedAt, CompletedAt,
CreatedAt, UpdatedAt, CreatedBy
)
VALUES (
'F6000001-0000-0000-0000-000000000001',
'A0000001-0000-0000-0000-000000000002',
27783.08,   -- 7360 + 20423.08 seller amounts
'INR', 'Completed',
N'priya@upi',
@AdminId,
N'Verified transactions. Processed via NEFT.',
N'NEFT-REF-20250110-001',
DATEADD(DAY, -4, GETUTCDATE()),
DATEADD(DAY, -3, GETUTCDATE()),
DATEADD(DAY, -6, GETUTCDATE()),
DATEADD(DAY, -3, GETUTCDATE()),
'A0000001-0000-0000-0000-000000000002'
);
PRINT 'Seeded Payout 1 — Completed';
END
ELSE
PRINT 'Payout 1 already exists — skipped.';
GO
-- Payout 2 — Pending (covers Orders 1, 4)
IF NOT EXISTS (
SELECT 1 FROM [payments].[Payouts]
WHERE Id = 'F6000001-0000-0000-0000-000000000002'
)
BEGIN
INSERT INTO [payments].[Payouts] (
Id, SellerId, Amount, CurrencyCode,
[Status],
BankAccountNumber, BankIfscCode, BankAccountName,
CreatedAt, UpdatedAt, CreatedBy
)
VALUES (
'F6000001-0000-0000-0000-000000000002',
'A0000001-0000-0000-0000-000000000002',
16859.26,   -- 13890.16 + 2969.10 seller amounts
'INR', 'Pending',
N'1234567890', N'SBIN0001234', N'Priya Shah',
DATEADD(HOUR, -2, GETUTCDATE()),
DATEADD(HOUR, -2, GETUTCDATE()),
'A0000001-0000-0000-0000-000000000002'
);
PRINT 'Seeded Payout 2 — Pending';
END
ELSE
PRINT 'Payout 2 already exists — skipped.';
GO
-- ── Verification ─────────────────────────────────────────────
SELECT 'CommissionRules' AS [Table], COUNT() AS [Count]
FROM [payments].[CommissionRules]
UNION ALL SELECT 'Transactions', COUNT() FROM [payments].[Transactions]
UNION ALL SELECT 'Payouts',      COUNT() FROM [payments].[Payouts]
UNION ALL SELECT 'PayoutTransactions', COUNT() FROM [payments].[PayoutTransactions];
GO
SELECT [Status], COUNT(*) AS Cnt
FROM [payments].[Transactions]
GROUP BY [Status];
GO
PRINT 'Payments module schema + seed complete.';
GO
