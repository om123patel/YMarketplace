-- ============================================================
-- ECOMMERCE MARKETPLACE PLATFORM
-- Catalog Schema -- Product Related Tables
-- Database: EcommerceDB
-- Schema:   catalog
-- Tables:   11
-- Author:   Generated
-- ============================================================

-- ============================================================
-- STEP 1: CREATE SCHEMA
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'catalog')
BEGIN
    EXEC('CREATE SCHEMA catalog');
END
GO

-- ============================================================
-- TABLE 1: catalog.Categories
-- ============================================================

IF OBJECT_ID('catalog.Categories', 'U') IS NOT NULL
    DROP TABLE catalog.Categories;
GO

CREATE TABLE catalog.Categories (
    Id              INT IDENTITY(1,1)       NOT NULL,
    ParentId        INT                     NULL,
    Name            NVARCHAR(100)           NOT NULL,
    Slug            NVARCHAR(120)           NOT NULL,
    Description     NVARCHAR(500)           NULL,
    ImageUrl        NVARCHAR(500)           NULL,
    IconUrl         NVARCHAR(500)           NULL,
    SortOrder       INT                     NOT NULL    DEFAULT 0,
    IsActive        BIT                     NOT NULL    DEFAULT 1,

    -- Audit
    CreatedAt       DATETIME2               NOT NULL    DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2               NOT NULL    DEFAULT GETUTCDATE(),
    CreatedBy       UNIQUEIDENTIFIER        NOT NULL,
    UpdatedBy       UNIQUEIDENTIFIER        NULL,
    IsDeleted       BIT                     NOT NULL    DEFAULT 0,
    DeletedAt       DATETIME2               NULL,
    RowVersion      ROWVERSION,

    CONSTRAINT PK_Categories            PRIMARY KEY (Id),
    CONSTRAINT UQ_Categories_Slug       UNIQUE      (Slug),
    CONSTRAINT FK_Categories_Parent     FOREIGN KEY (ParentId)
        REFERENCES catalog.Categories(Id)
);
GO

CREATE INDEX IX_Categories_ParentId
    ON catalog.Categories(ParentId)
    WHERE IsDeleted = 0;
GO

CREATE INDEX IX_Categories_IsActive
    ON catalog.Categories(IsActive)
    WHERE IsDeleted = 0;
GO

-- ============================================================
-- TABLE 2: catalog.Brands
-- ============================================================

IF OBJECT_ID('catalog.Brands', 'U') IS NOT NULL
    DROP TABLE catalog.Brands;
GO

CREATE TABLE catalog.Brands (
    Id              INT IDENTITY(1,1)       NOT NULL,
    Name            NVARCHAR(100)           NOT NULL,
    Slug            NVARCHAR(120)           NOT NULL,
    Description     NVARCHAR(500)           NULL,
    LogoUrl         NVARCHAR(500)           NULL,
    WebsiteUrl      NVARCHAR(300)           NULL,
    IsActive        BIT                     NOT NULL    DEFAULT 1,

    -- Audit
    CreatedAt       DATETIME2               NOT NULL    DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2               NOT NULL    DEFAULT GETUTCDATE(),
    CreatedBy       UNIQUEIDENTIFIER        NOT NULL,
    UpdatedBy       UNIQUEIDENTIFIER        NULL,
    IsDeleted       BIT                     NOT NULL    DEFAULT 0,
    DeletedAt       DATETIME2               NULL,
    RowVersion      ROWVERSION,

    CONSTRAINT PK_Brands        PRIMARY KEY (Id),
    CONSTRAINT UQ_Brands_Slug   UNIQUE      (Slug),
    CONSTRAINT UQ_Brands_Name   UNIQUE      (Name)
);
GO

CREATE INDEX IX_Brands_IsActive
    ON catalog.Brands(IsActive)
    WHERE IsDeleted = 0;
GO

-- ============================================================
-- TABLE 3: catalog.Products
-- ============================================================

IF OBJECT_ID('catalog.Products', 'U') IS NOT NULL
    DROP TABLE catalog.Products;
GO

CREATE TABLE catalog.Products (
    Id                          UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),

    -- Ownership (one of SellerId OR CreatedByAdminId must be set)
    SellerId                    UNIQUEIDENTIFIER    NULL,   -- LOGICAL FK → identity.Sellers
    StoreId                     UNIQUEIDENTIFIER    NULL,   -- LOGICAL FK → store.Stores
    CreatedByAdminId            UNIQUEIDENTIFIER    NULL,   -- set when admin creates directly

    -- Classification
    CategoryId                  INT                 NOT NULL,
    BrandId                     INT                 NULL,

    -- Core
    Name                        NVARCHAR(300)       NOT NULL,
    Slug                        NVARCHAR(350)       NOT NULL,
    ShortDescription            NVARCHAR(500)       NULL,
    Description                 NVARCHAR(MAX)       NULL,

    -- Pricing
    BasePrice                   DECIMAL(18,4)       NOT NULL,
    CurrencyCode                CHAR(3)             NOT NULL    DEFAULT 'USD',
    CompareAtPrice              DECIMAL(18,4)       NULL,   -- strikethrough price
    CostPrice                   DECIMAL(18,4)       NULL,   -- internal only, never exposed

    -- Identifiers
    Sku                         NVARCHAR(100)       NULL,
    Barcode                     NVARCHAR(100)       NULL,

    -- Physical dimensions
    WeightKg                    DECIMAL(10,3)       NULL,
    LengthCm                    DECIMAL(10,2)       NULL,
    WidthCm                     DECIMAL(10,2)       NULL,
    HeightCm                    DECIMAL(10,2)       NULL,

    -- Flags
    IsDigital                   BIT                 NOT NULL    DEFAULT 0,
    RequiresShipping            BIT                 NOT NULL    DEFAULT 1,
    IsActive                    BIT                 NOT NULL    DEFAULT 0, -- stays 0 until Approved
    IsFeatured                  BIT                 NOT NULL    DEFAULT 0, -- admin can feature

    -- Approval workflow
    Status                      NVARCHAR(30)        NOT NULL    DEFAULT 'Draft',
    -- Allowed: Draft | PendingApproval | Active | Rejected | Archived
    SubmittedForApprovalAt      DATETIME2           NULL,
    ApprovedByAdminId           UNIQUEIDENTIFIER    NULL,
    ApprovedAt                  DATETIME2           NULL,
    RejectedByAdminId           UNIQUEIDENTIFIER    NULL,
    RejectedAt                  DATETIME2           NULL,
    RejectionReason             NVARCHAR(1000)      NULL,

    -- Audit
    CreatedAt                   DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),
    UpdatedAt                   DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),
    CreatedBy                   UNIQUEIDENTIFIER    NOT NULL,
    UpdatedBy                   UNIQUEIDENTIFIER    NULL,
    IsDeleted                   BIT                 NOT NULL    DEFAULT 0,
    DeletedAt                   DATETIME2           NULL,
    RowVersion                  ROWVERSION,

    CONSTRAINT PK_Products          PRIMARY KEY (Id),
    CONSTRAINT UQ_Products_Slug     UNIQUE      (Slug),
    CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId)
        REFERENCES catalog.Categories(Id),
    CONSTRAINT FK_Products_Brand    FOREIGN KEY (BrandId)
        REFERENCES catalog.Brands(Id),
    CONSTRAINT CHK_Products_Status  CHECK (
        Status IN ('Draft', 'PendingApproval', 'Active', 'Rejected', 'Archived')
    ),
    CONSTRAINT CHK_Products_Ownership CHECK (
        -- Product must belong to a seller OR be admin-created — never neither
        SellerId IS NOT NULL OR CreatedByAdminId IS NOT NULL
    ),
    CONSTRAINT CHK_Products_BasePrice CHECK (
        BasePrice >= 0
    ),
    CONSTRAINT CHK_Products_CompareAtPrice CHECK (
        CompareAtPrice IS NULL OR CompareAtPrice >= BasePrice
    )
);
GO

CREATE INDEX IX_Products_SellerId
    ON catalog.Products(SellerId)
    WHERE IsDeleted = 0;
GO

CREATE INDEX IX_Products_StoreId
    ON catalog.Products(StoreId)
    WHERE IsDeleted = 0;
GO

CREATE INDEX IX_Products_CategoryId
    ON catalog.Products(CategoryId)
    WHERE IsDeleted = 0;
GO

CREATE INDEX IX_Products_Status
    ON catalog.Products(Status)
    WHERE IsDeleted = 0;
GO

CREATE INDEX IX_Products_IsFeatured
    ON catalog.Products(IsFeatured)
    WHERE IsDeleted = 0 AND IsActive = 1;
GO

-- ============================================================
-- TABLE 4: catalog.ProductSeo
-- One-to-one with Products
-- ============================================================

IF OBJECT_ID('catalog.ProductSeo', 'U') IS NOT NULL
    DROP TABLE catalog.ProductSeo;
GO

CREATE TABLE catalog.ProductSeo (
    Id                  INT IDENTITY(1,1)   NOT NULL,
    ProductId           UNIQUEIDENTIFIER    NOT NULL,

    -- Standard SEO
    MetaTitle           NVARCHAR(70)        NULL,   -- recommended max 60-70 chars
    MetaDescription     NVARCHAR(165)       NULL,   -- recommended max 150-165 chars
    MetaKeywords        NVARCHAR(500)       NULL,   -- comma-separated keywords

    -- Canonical
    CanonicalUrl        NVARCHAR(500)       NULL,

    -- Open Graph (social sharing preview)
    OgTitle             NVARCHAR(200)       NULL,
    OgDescription       NVARCHAR(300)       NULL,
    OgImageUrl          NVARCHAR(500)       NULL,

    -- Audit
    UpdatedAt           DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),
    UpdatedBy           UNIQUEIDENTIFIER    NULL,

    CONSTRAINT PK_ProductSeo            PRIMARY KEY (Id),
    CONSTRAINT UQ_ProductSeo_ProductId  UNIQUE      (ProductId),  -- strict 1:1
    CONSTRAINT FK_ProductSeo_Products   FOREIGN KEY (ProductId)
        REFERENCES catalog.Products(Id)
);
GO

-- ============================================================
-- TABLE 5: catalog.ProductImages
-- ============================================================

IF OBJECT_ID('catalog.ProductImages', 'U') IS NOT NULL
    DROP TABLE catalog.ProductImages;
GO

CREATE TABLE catalog.ProductImages (
    Id              INT IDENTITY(1,1)   NOT NULL,
    ProductId       UNIQUEIDENTIFIER    NOT NULL,
    ImageUrl        NVARCHAR(500)       NOT NULL,
    ThumbnailUrl    NVARCHAR(500)       NULL,       -- pre-generated smaller version
    AltText         NVARCHAR(200)       NULL,
    SortOrder       INT                 NOT NULL    DEFAULT 0,
    IsPrimary       BIT                 NOT NULL    DEFAULT 0,  -- main display image

    -- Audit
    CreatedAt       DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),
    CreatedBy       UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT PK_ProductImages         PRIMARY KEY (Id),
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId)
        REFERENCES catalog.Products(Id)
);
GO

CREATE INDEX IX_ProductImages_ProductId
    ON catalog.ProductImages(ProductId);
GO

-- ============================================================
-- TABLE 6: catalog.Tags
-- ============================================================

IF OBJECT_ID('catalog.Tags', 'U') IS NOT NULL
    DROP TABLE catalog.Tags;
GO

CREATE TABLE catalog.Tags (
    Id          INT IDENTITY(1,1)   NOT NULL,
    Name        NVARCHAR(100)       NOT NULL,
    Slug        NVARCHAR(120)       NOT NULL,

    -- Audit
    CreatedAt   DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),
    CreatedBy   UNIQUEIDENTIFIER    NOT NULL,
    IsDeleted   BIT                 NOT NULL    DEFAULT 0,

    CONSTRAINT PK_Tags      PRIMARY KEY (Id),
    CONSTRAINT UQ_Tags_Slug UNIQUE      (Slug),
    CONSTRAINT UQ_Tags_Name UNIQUE      (Name)
);
GO

-- ============================================================
-- TABLE 7: catalog.ProductTags  (junction)
-- ============================================================

IF OBJECT_ID('catalog.ProductTags', 'U') IS NOT NULL
    DROP TABLE catalog.ProductTags;
GO

CREATE TABLE catalog.ProductTags (
    ProductId   UNIQUEIDENTIFIER    NOT NULL,
    TagId       INT                 NOT NULL,
    AssignedAt  DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),
    AssignedBy  UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT PK_ProductTags          PRIMARY KEY (ProductId, TagId),
    CONSTRAINT FK_ProductTags_Products FOREIGN KEY (ProductId)
        REFERENCES catalog.Products(Id),
    CONSTRAINT FK_ProductTags_Tags     FOREIGN KEY (TagId)
        REFERENCES catalog.Tags(Id)
);
GO

CREATE INDEX IX_ProductTags_TagId
    ON catalog.ProductTags(TagId);
GO

-- ============================================================
-- TABLE 8: catalog.ProductVariants
-- ============================================================

IF OBJECT_ID('catalog.ProductVariants', 'U') IS NOT NULL
    DROP TABLE catalog.ProductVariants;
GO

CREATE TABLE catalog.ProductVariants (
    Id              UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    ProductId       UNIQUEIDENTIFIER    NOT NULL,
    Name            NVARCHAR(200)       NOT NULL,   -- e.g. "Red / XL"
    Sku             NVARCHAR(100)       NULL,
    Barcode         NVARCHAR(100)       NULL,
    Price           DECIMAL(18,4)       NOT NULL,
    CurrencyCode    CHAR(3)             NOT NULL    DEFAULT 'USD',
    CompareAtPrice  DECIMAL(18,4)       NULL,
    CostPrice       DECIMAL(18,4)       NULL,
    WeightKg        DECIMAL(10,3)       NULL,
    ImageUrl        NVARCHAR(500)       NULL,       -- variant-specific image override
    SortOrder       INT                 NOT NULL    DEFAULT 0,
    IsActive        BIT                 NOT NULL    DEFAULT 1,

    -- Audit
    CreatedAt       DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),
    CreatedBy       UNIQUEIDENTIFIER    NOT NULL,
    UpdatedBy       UNIQUEIDENTIFIER    NULL,
    IsDeleted       BIT                 NOT NULL    DEFAULT 0,
    DeletedAt       DATETIME2           NULL,
    RowVersion      ROWVERSION,

    CONSTRAINT PK_ProductVariants           PRIMARY KEY (Id),
    CONSTRAINT FK_ProductVariants_Products  FOREIGN KEY (ProductId)
        REFERENCES catalog.Products(Id),
    CONSTRAINT CHK_ProductVariants_Price    CHECK (Price >= 0)
);
GO

CREATE INDEX IX_ProductVariants_ProductId
    ON catalog.ProductVariants(ProductId)
    WHERE IsDeleted = 0;
GO

-- ============================================================
-- TABLE 9: catalog.ProductAttributes
-- Key-value attribute values per variant
-- ============================================================

IF OBJECT_ID('catalog.ProductAttributes', 'U') IS NOT NULL
    DROP TABLE catalog.ProductAttributes;
GO

CREATE TABLE catalog.ProductAttributes (
    Id          INT IDENTITY(1,1)   NOT NULL,
    VariantId   UNIQUEIDENTIFIER    NOT NULL,
    Name        NVARCHAR(100)       NOT NULL,   -- e.g. Color | Size | Material
    Value       NVARCHAR(200)       NOT NULL,   -- e.g. Red  | XL   | Cotton
    SortOrder   INT                 NOT NULL    DEFAULT 0,

    CONSTRAINT PK_ProductAttributes             PRIMARY KEY (Id),
    CONSTRAINT UQ_ProductAttributes_Variant_Name UNIQUE (VariantId, Name),
    CONSTRAINT FK_ProductAttributes_Variants    FOREIGN KEY (VariantId)
        REFERENCES catalog.ProductVariants(Id)
);
GO

CREATE INDEX IX_ProductAttributes_VariantId
    ON catalog.ProductAttributes(VariantId);
GO

-- ============================================================
-- TABLE 10: catalog.AttributeTemplates
-- Admin defines expected attributes per category
-- e.g. Category "Phones" → template with Color, Storage, RAM
-- ============================================================

IF OBJECT_ID('catalog.AttributeTemplates', 'U') IS NOT NULL
    DROP TABLE catalog.AttributeTemplates;
GO

CREATE TABLE catalog.AttributeTemplates (
    Id          INT IDENTITY(1,1)   NOT NULL,
    CategoryId  INT                 NOT NULL,
    Name        NVARCHAR(100)       NOT NULL,   -- e.g. "Phone Attributes"
    IsActive    BIT                 NOT NULL    DEFAULT 1,

    -- Audit
    CreatedAt   DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),
    CreatedBy   UNIQUEIDENTIFIER    NOT NULL,
    IsDeleted   BIT                 NOT NULL    DEFAULT 0,

    CONSTRAINT PK_AttributeTemplates            PRIMARY KEY (Id),
    CONSTRAINT FK_AttributeTemplates_Categories FOREIGN KEY (CategoryId)
        REFERENCES catalog.Categories(Id)
);
GO

CREATE INDEX IX_AttributeTemplates_CategoryId
    ON catalog.AttributeTemplates(CategoryId)
    WHERE IsDeleted = 0;
GO

-- ============================================================
-- TABLE 11: catalog.AttributeTemplateItems
-- Individual attribute fields within a template
-- ============================================================

IF OBJECT_ID('catalog.AttributeTemplateItems', 'U') IS NOT NULL
    DROP TABLE catalog.AttributeTemplateItems;
GO

CREATE TABLE catalog.AttributeTemplateItems (
    Id              INT IDENTITY(1,1)   NOT NULL,
    TemplateId      INT                 NOT NULL,
    AttributeName   NVARCHAR(100)       NOT NULL,   -- e.g. Color | Storage | RAM
    InputType       NVARCHAR(20)        NOT NULL    DEFAULT 'Text',
    -- Allowed: Text | Select | MultiSelect | Number | Boolean
    Options         NVARCHAR(MAX)       NULL,
    -- JSON array, used when InputType = Select or MultiSelect
    -- e.g. ["Red","Blue","Green"] or ["64GB","128GB","256GB"]
    IsRequired      BIT                 NOT NULL    DEFAULT 0,
    SortOrder       INT                 NOT NULL    DEFAULT 0,

    CONSTRAINT PK_AttributeTemplateItems            PRIMARY KEY (Id),
    CONSTRAINT FK_AttributeTemplateItems_Templates  FOREIGN KEY (TemplateId)
        REFERENCES catalog.AttributeTemplates(Id),
    CONSTRAINT CHK_AttributeTemplateItems_InputType CHECK (
        InputType IN ('Text', 'Select', 'MultiSelect', 'Number', 'Boolean')
    )
);
GO

CREATE INDEX IX_AttributeTemplateItems_TemplateId
    ON catalog.AttributeTemplateItems(TemplateId);
GO

-- ============================================================
-- TABLE 12: catalog.ProductStatusHistory
-- Full audit trail of every status change
-- ============================================================

IF OBJECT_ID('catalog.ProductStatusHistory', 'U') IS NOT NULL
    DROP TABLE catalog.ProductStatusHistory;
GO

CREATE TABLE catalog.ProductStatusHistory (
    Id          INT IDENTITY(1,1)   NOT NULL,
    ProductId   UNIQUEIDENTIFIER    NOT NULL,
    FromStatus  NVARCHAR(30)        NULL,       -- NULL on first status set
    ToStatus    NVARCHAR(30)        NOT NULL,
    Note        NVARCHAR(1000)      NULL,       -- rejection reason, approval notes
    ChangedBy   UNIQUEIDENTIFIER    NOT NULL,   -- AdminId or SellerId
    ChangedAt   DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),

    CONSTRAINT PK_ProductStatusHistory          PRIMARY KEY (Id),
    CONSTRAINT FK_ProductStatusHistory_Products FOREIGN KEY (ProductId)
        REFERENCES catalog.Products(Id)
);
GO

CREATE INDEX IX_ProductStatusHistory_ProductId
    ON catalog.ProductStatusHistory(ProductId);
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- System admin placeholder for seeding
-- In production replace this GUID with your actual admin user Id
DECLARE @SystemAdminId UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';

-- Root Categories
INSERT INTO catalog.Categories (Name, Slug, Description, SortOrder, IsActive, CreatedBy)
VALUES
    ('Electronics',     'electronics',      'Electronic devices and accessories',   1, 1, @SystemAdminId),
    ('Clothing',        'clothing',          'Men, Women and Kids fashion',          2, 1, @SystemAdminId),
    ('Home & Garden',   'home-garden',       'Furniture, decor and garden',          3, 1, @SystemAdminId),
    ('Sports',          'sports',            'Sports equipment and apparel',         4, 1, @SystemAdminId),
    ('Books',           'books',             'Books, eBooks and audiobooks',         5, 1, @SystemAdminId);
GO

-- Subcategories under Electronics (ParentId = 1)
INSERT INTO catalog.Categories (ParentId, Name, Slug, SortOrder, IsActive, CreatedBy)
VALUES
    (1, 'Phones',       'phones',       1, 1, '00000000-0000-0000-0000-000000000001'),
    (1, 'Laptops',      'laptops',      2, 1, '00000000-0000-0000-0000-000000000001'),
    (1, 'Tablets',      'tablets',      3, 1, '00000000-0000-0000-0000-000000000001'),
    (1, 'Accessories',  'accessories',  4, 1, '00000000-0000-0000-0000-000000000001');
GO

-- Subcategories under Clothing (ParentId = 2)
INSERT INTO catalog.Categories (ParentId, Name, Slug, SortOrder, IsActive, CreatedBy)
VALUES
    (2, 'Men',      'men-clothing',     1, 1, '00000000-0000-0000-0000-000000000001'),
    (2, 'Women',    'women-clothing',   2, 1, '00000000-0000-0000-0000-000000000001'),
    (2, 'Kids',     'kids-clothing',    3, 1, '00000000-0000-0000-0000-000000000001');
GO

-- Brands
INSERT INTO catalog.Brands (Name, Slug, IsActive, CreatedBy)
VALUES
    ('Apple',       'apple',        1, '00000000-0000-0000-0000-000000000001'),
    ('Samsung',     'samsung',      1, '00000000-0000-0000-0000-000000000001'),
    ('Nike',        'nike',         1, '00000000-0000-0000-0000-000000000001'),
    ('Sony',        'sony',         1, '00000000-0000-0000-0000-000000000001'),
    ('Dell',        'dell',         1, '00000000-0000-0000-0000-000000000001'),
    ('Generic',     'generic',      1, '00000000-0000-0000-0000-000000000001');
GO

-- Attribute Template for Phones (CategoryId = 6)
INSERT INTO catalog.AttributeTemplates (CategoryId, Name, IsActive, CreatedBy)
VALUES (6, 'Phone Attributes', 1, '00000000-0000-0000-0000-000000000001');
GO

INSERT INTO catalog.AttributeTemplateItems (TemplateId, AttributeName, InputType, Options, IsRequired, SortOrder)
VALUES
    (1, 'Color',    'Select',   '["Black","White","Blue","Red","Gold","Silver"]',    1, 1),
    (1, 'Storage',  'Select',   '["64GB","128GB","256GB","512GB","1TB"]',            1, 2),
    (1, 'RAM',      'Select',   '["4GB","6GB","8GB","12GB","16GB"]',                 1, 3),
    (1, 'Network',  'Select',   '["4G","5G"]',                                       0, 4),
    (1, 'OS',       'Text',     NULL,                                                0, 5);
GO

-- Attribute Template for Laptops (CategoryId = 7)
INSERT INTO catalog.AttributeTemplates (CategoryId, Name, IsActive, CreatedBy)
VALUES (7, 'Laptop Attributes', 1, '00000000-0000-0000-0000-000000000001');
GO

INSERT INTO catalog.AttributeTemplateItems (TemplateId, AttributeName, InputType, Options, IsRequired, SortOrder)
VALUES
    (2, 'Color',        'Select',   '["Silver","Space Grey","Black","White"]',           1, 1),
    (2, 'RAM',          'Select',   '["8GB","16GB","32GB","64GB"]',                      1, 2),
    (2, 'Storage',      'Select',   '["256GB SSD","512GB SSD","1TB SSD","2TB SSD"]',     1, 3),
    (2, 'Processor',    'Text',     NULL,                                                 1, 4),
    (2, 'Screen Size',  'Select',   '["13 inch","14 inch","15 inch","16 inch"]',          0, 5);
GO

-- Attribute Template for Clothing (Men/Women/Kids — CategoryId 10,11,12)
INSERT INTO catalog.AttributeTemplates (CategoryId, Name, IsActive, CreatedBy)
VALUES (10, 'Clothing Attributes', 1, '00000000-0000-0000-0000-000000000001');
GO

INSERT INTO catalog.AttributeTemplateItems (TemplateId, AttributeName, InputType, Options, IsRequired, SortOrder)
VALUES
    (3, 'Size',     'Select',   '["XS","S","M","L","XL","XXL","XXXL"]',     1, 1),
    (3, 'Color',    'Select',   '["Black","White","Red","Blue","Green","Yellow","Grey","Navy"]', 1, 2),
    (3, 'Material', 'Text',     NULL,                                         0, 3);
GO

-- Common Tags
INSERT INTO catalog.Tags (Name, Slug, CreatedBy)
VALUES
    ('New Arrival',     'new-arrival',      '00000000-0000-0000-0000-000000000001'),
    ('Best Seller',     'best-seller',      '00000000-0000-0000-0000-000000000001'),
    ('On Sale',         'on-sale',          '00000000-0000-0000-0000-000000000001'),
    ('Limited Edition', 'limited-edition',  '00000000-0000-0000-0000-000000000001'),
    ('Eco Friendly',    'eco-friendly',     '00000000-0000-0000-0000-000000000001'),
    ('Imported',        'imported',         '00000000-0000-0000-0000-000000000001');
GO

-- ============================================================
-- VERIFICATION QUERIES
-- Run these after execution to verify everything created correctly
-- ============================================================

-- Check all tables created
SELECT
    t.name          AS TableName,
    s.name          AS SchemaName,
    p.rows          AS RowCount
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.partitions p ON t.object_id = p.object_id AND p.index_id IN (0,1)
WHERE s.name = 'catalog'
ORDER BY t.name;
GO

-- Check all indexes
SELECT
    t.name      AS TableName,
    i.name      AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = 'catalog'
  AND i.name IS NOT NULL
ORDER BY t.name, i.name;
GO

-- Check all foreign keys
SELECT
    fk.name                         AS ForeignKeyName,
    tp.name                         AS ParentTable,
    cp.name                         AS ParentColumn,
    tr.name                         AS ReferencedTable,
    cr.name                         AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp            ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr            ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns  fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp           ON fkc.parent_object_id = cp.object_id
    AND fkc.parent_column_id = cp.column_id
INNER JOIN sys.columns cr           ON fkc.referenced_object_id = cr.object_id
    AND fkc.referenced_column_id = cr.column_id
INNER JOIN sys.schemas s            ON tp.schema_id = s.schema_id
WHERE s.name = 'catalog'
ORDER BY tp.name;
GO

-- Check seed data
SELECT 'Categories' AS [Table], COUNT(*) AS [Count] FROM catalog.Categories
UNION ALL
SELECT 'Brands',                COUNT(*) FROM catalog.Brands
UNION ALL
SELECT 'Tags',                  COUNT(*) FROM catalog.Tags
UNION ALL
SELECT 'AttributeTemplates',    COUNT(*) FROM catalog.AttributeTemplates
UNION ALL
SELECT 'AttributeTemplateItems',COUNT(*) FROM catalog.AttributeTemplateItems;
GO