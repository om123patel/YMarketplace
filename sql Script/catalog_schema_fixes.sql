-- ============================================================
-- MarketHub — Catalog Schema Fix Script
-- Database : EcommerceDB
-- Schema   : catalog
-- Purpose  : Align SQL schema with domain entities + EF configs
--            Domain is the source of truth.
--
-- Run AFTER catalog_module.sql is already executed.
-- Safe to inspect and run section-by-section.
--
-- 16 mismatches fixed across 8 tables:
--   FIX-01  Products          — ADD CreatorType
--   FIX-02  Products          — ADD CompareAtPriceCurrency
--   FIX-03  Products          — ADD CostPriceCurrency
--   FIX-04  ProductVariants   — ADD CompareAtPriceCurrency
--   FIX-05  ProductVariants   — ADD CostPriceCurrency
--   FIX-06  ProductSeo        — ADD CreatedAt, CreatedBy
--   FIX-07  ProductImages     — ADD UpdatedAt, UpdatedBy
--   FIX-08  Tags              — ADD UpdatedAt, UpdatedBy
--   FIX-09  ProductAttributes — ADD CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
--   FIX-10  AttrTemplateItems — ADD CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
--   FIX-11  ProductStatusHist — ADD CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
--   FIX-12  ProductTags       — DROP AssignedAt, AssignedBy (not mapped by EF)
--   FIX-13  Brands            — REPLACE plain UQ on Name with filtered index
--   FIX-14  Brands            — REPLACE plain UQ on Slug with filtered index
--   FIX-15  Tags              — REPLACE plain UQ on Name+Slug with filtered indexes
--   FIX-16  AttributeTemplates — ADD UpdatedBy
-- ============================================================

USE EcommerceDB;
GO

SET NOCOUNT ON;
GO

-- ============================================================
-- FIX-01 · catalog.Products — ADD CreatorType
-- ============================================================
-- Domain: Product.CreatorType (ProductCreatorType enum)
-- EF:     .HasConversion(...).HasMaxLength(20) → column "CreatorType"
-- SQL:    Column does not exist
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.Products')
      AND name = 'CreatorType'
)
BEGIN
    ALTER TABLE catalog.Products
        ADD CreatorType NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Products_CreatorType DEFAULT 'Admin';

    -- Add check constraint matching the enum values
    ALTER TABLE catalog.Products
        ADD CONSTRAINT CHK_Products_CreatorType
            CHECK (CreatorType IN ('Admin', 'Seller'));

    PRINT 'FIX-01 applied: catalog.Products.CreatorType added.';
END
ELSE
    PRINT 'FIX-01 skipped: CreatorType already exists.';
GO

-- ============================================================
-- FIX-02 · catalog.Products — ADD CompareAtPriceCurrency
-- ============================================================
-- Domain: Product.CompareAtPrice (Money value object)
-- EF OwnsOne maps to TWO columns:
--   CompareAtPrice        DECIMAL(18,4)  (already in SQL)
--   CompareAtPriceCurrency NVARCHAR(3)   ← MISSING
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.Products')
      AND name = 'CompareAtPriceCurrency'
)
BEGIN
    ALTER TABLE catalog.Products
        ADD CompareAtPriceCurrency NVARCHAR(3) NULL;

    PRINT 'FIX-02 applied: catalog.Products.CompareAtPriceCurrency added.';
END
ELSE
    PRINT 'FIX-02 skipped: CompareAtPriceCurrency already exists.';
GO

-- ============================================================
-- FIX-03 · catalog.Products — ADD CostPriceCurrency
-- ============================================================
-- Domain: Product.CostPrice (Money value object)
-- EF OwnsOne maps to TWO columns:
--   CostPrice        DECIMAL(18,4)  (already in SQL)
--   CostPriceCurrency NVARCHAR(3)   ← MISSING
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.Products')
      AND name = 'CostPriceCurrency'
)
BEGIN
    ALTER TABLE catalog.Products
        ADD CostPriceCurrency NVARCHAR(3) NULL;

    PRINT 'FIX-03 applied: catalog.Products.CostPriceCurrency added.';
END
ELSE
    PRINT 'FIX-03 skipped: CostPriceCurrency already exists.';
GO

-- ============================================================
-- FIX-04 · catalog.ProductVariants — ADD CompareAtPriceCurrency
-- ============================================================
-- Domain: ProductVariant.CompareAtPrice (Money value object)
-- EF OwnsOne:
--   CompareAtPrice         DECIMAL(18,4)  (already in SQL)
--   CompareAtPriceCurrency NVARCHAR(3)    ← MISSING
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.ProductVariants')
      AND name = 'CompareAtPriceCurrency'
)
BEGIN
    ALTER TABLE catalog.ProductVariants
        ADD CompareAtPriceCurrency NVARCHAR(3) NULL;

    PRINT 'FIX-04 applied: catalog.ProductVariants.CompareAtPriceCurrency added.';
END
ELSE
    PRINT 'FIX-04 skipped: CompareAtPriceCurrency already exists.';
GO

-- ============================================================
-- FIX-05 · catalog.ProductVariants — ADD CostPriceCurrency
-- ============================================================
-- Domain: ProductVariant.CostPrice (Money value object)
-- EF OwnsOne:
--   CostPrice         DECIMAL(18,4)  (already in SQL)
--   CostPriceCurrency NVARCHAR(3)    ← MISSING
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.ProductVariants')
      AND name = 'CostPriceCurrency'
)
BEGIN
    ALTER TABLE catalog.ProductVariants
        ADD CostPriceCurrency NVARCHAR(3) NULL;

    PRINT 'FIX-05 applied: catalog.ProductVariants.CostPriceCurrency added.';
END
ELSE
    PRINT 'FIX-05 skipped: CostPriceCurrency already exists.';
GO

-- ============================================================
-- FIX-06 · catalog.ProductSeo — ADD CreatedAt, CreatedBy
-- ============================================================
-- Domain: ProductSeo : Entity<int> — Create() sets CreatedAt + CreatedBy
-- SQL table only has UpdatedAt + UpdatedBy.
-- Missing: CreatedAt DATETIME2 NOT NULL, CreatedBy UNIQUEIDENTIFIER NOT NULL
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.ProductSeo')
      AND name = 'CreatedAt'
)
BEGIN
    ALTER TABLE catalog.ProductSeo
        ADD CreatedAt  DATETIME2        NOT NULL
                CONSTRAINT DF_ProductSeo_CreatedAt DEFAULT GETUTCDATE(),
            CreatedBy  UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_ProductSeo_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000000';

    PRINT 'FIX-06 applied: catalog.ProductSeo.CreatedAt and CreatedBy added.';
END
ELSE
    PRINT 'FIX-06 skipped: CreatedAt already exists.';
GO

-- ============================================================
-- FIX-07 · catalog.ProductImages — ADD UpdatedAt, UpdatedBy
-- ============================================================
-- Domain: ProductImage : Entity<int> — Create() sets UpdatedAt
-- SQL table has CreatedAt + CreatedBy but not UpdatedAt or UpdatedBy.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.ProductImages')
      AND name = 'UpdatedAt'
)
BEGIN
    ALTER TABLE catalog.ProductImages
        ADD UpdatedAt  DATETIME2        NOT NULL
                CONSTRAINT DF_ProductImages_UpdatedAt DEFAULT GETUTCDATE(),
            UpdatedBy  UNIQUEIDENTIFIER NULL;

    PRINT 'FIX-07 applied: catalog.ProductImages.UpdatedAt and UpdatedBy added.';
END
ELSE
    PRINT 'FIX-07 skipped: UpdatedAt already exists.';
GO

-- ============================================================
-- FIX-08 · catalog.Tags — ADD UpdatedAt, UpdatedBy
-- ============================================================
-- Domain: Tag : Entity<int> — Update() calls SetUpdatedBy()
-- EF TagConfiguration maps UpdatedAt
-- SQL has CreatedAt + CreatedBy + IsDeleted — missing UpdatedAt, UpdatedBy
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.Tags')
      AND name = 'UpdatedAt'
)
BEGIN
    ALTER TABLE catalog.Tags
        ADD UpdatedAt  DATETIME2        NOT NULL
                CONSTRAINT DF_Tags_UpdatedAt DEFAULT GETUTCDATE(),
            UpdatedBy  UNIQUEIDENTIFIER NULL;

    PRINT 'FIX-08 applied: catalog.Tags.UpdatedAt and UpdatedBy added.';
END
ELSE
    PRINT 'FIX-08 skipped: UpdatedAt already exists.';
GO

-- ============================================================
-- FIX-09 · catalog.ProductAttributes — ADD all audit columns
-- ============================================================
-- Domain: ProductAttribute : Entity<int>
-- Create() sets: CreatedAt, UpdatedAt, CreatedBy = Guid.Empty
-- SQL table: Id, VariantId, Name, Value, SortOrder — NO audit columns
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.ProductAttributes')
      AND name = 'CreatedAt'
)
BEGIN
    ALTER TABLE catalog.ProductAttributes
        ADD CreatedAt  DATETIME2        NOT NULL
                CONSTRAINT DF_ProductAttributes_CreatedAt DEFAULT GETUTCDATE(),
            UpdatedAt  DATETIME2        NOT NULL
                CONSTRAINT DF_ProductAttributes_UpdatedAt DEFAULT GETUTCDATE(),
            CreatedBy  UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_ProductAttributes_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000000',
            UpdatedBy  UNIQUEIDENTIFIER NULL;

    PRINT 'FIX-09 applied: catalog.ProductAttributes audit columns added.';
END
ELSE
    PRINT 'FIX-09 skipped: CreatedAt already exists.';
GO

-- ============================================================
-- FIX-10 · catalog.AttributeTemplateItems — ADD all audit columns
-- ============================================================
-- Domain: AttributeTemplateItem : Entity<int>
-- Create() sets: CreatedAt, UpdatedAt, CreatedBy = Guid.Empty
-- SQL table has: Id, TemplateId, AttributeName, InputType, Options, IsRequired, SortOrder
-- None of the audit fields are present.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.AttributeTemplateItems')
      AND name = 'CreatedAt'
)
BEGIN
    ALTER TABLE catalog.AttributeTemplateItems
        ADD CreatedAt  DATETIME2        NOT NULL
                CONSTRAINT DF_AttrTemplateItems_CreatedAt DEFAULT GETUTCDATE(),
            UpdatedAt  DATETIME2        NOT NULL
                CONSTRAINT DF_AttrTemplateItems_UpdatedAt DEFAULT GETUTCDATE(),
            CreatedBy  UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_AttrTemplateItems_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000000',
            UpdatedBy  UNIQUEIDENTIFIER NULL;

    PRINT 'FIX-10 applied: catalog.AttributeTemplateItems audit columns added.';
END
ELSE
    PRINT 'FIX-10 skipped: CreatedAt already exists.';
GO

-- ============================================================
-- FIX-11 · catalog.ProductStatusHistory — ADD audit columns
-- ============================================================
-- Domain: ProductStatusHistory : Entity<int>
-- Create() sets: CreatedAt, UpdatedAt, CreatedBy = changedBy
-- SQL table has: Id, ProductId, FromStatus, ToStatus, Note, ChangedBy, ChangedAt
-- EF entity base properties (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) not in SQL
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.ProductStatusHistory')
      AND name = 'CreatedAt'
)
BEGIN
    ALTER TABLE catalog.ProductStatusHistory
        ADD CreatedAt  DATETIME2        NOT NULL
                CONSTRAINT DF_ProductStatusHistory_CreatedAt DEFAULT GETUTCDATE(),
            UpdatedAt  DATETIME2        NOT NULL
                CONSTRAINT DF_ProductStatusHistory_UpdatedAt DEFAULT GETUTCDATE(),
            CreatedBy  UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT DF_ProductStatusHistory_CreatedBy DEFAULT '00000000-0000-0000-0000-000000000000',
            UpdatedBy  UNIQUEIDENTIFIER NULL;

    PRINT 'FIX-11 applied: catalog.ProductStatusHistory audit columns added.';
END
ELSE
    PRINT 'FIX-11 skipped: CreatedAt already exists.';
GO

-- ============================================================
-- FIX-12 · catalog.ProductTags — DROP AssignedAt, AssignedBy
-- ============================================================
-- EF config: builder.HasMany(Tags).WithMany().UsingEntity(j => j.ToTable("ProductTags","catalog"))
-- Simple UsingEntity generates ONLY ProductsId + TagsId junction — no extra columns.
-- SQL has AssignedAt and AssignedBy which EF will never write/read → data integrity risk.
--
-- DECISION: Drop the extra columns so EF controls the table.
-- If you want AssignedAt/AssignedBy tracked, a full ProductTag entity is needed
-- (separate task — see comment at end of script).
-- ============================================================

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.ProductTags')
      AND name = 'AssignedAt'
)
BEGIN
    ALTER TABLE catalog.ProductTags
        DROP COLUMN AssignedAt;

    PRINT 'FIX-12a applied: catalog.ProductTags.AssignedAt dropped.';
END
ELSE
    PRINT 'FIX-12a skipped: AssignedAt already absent.';
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.ProductTags')
      AND name = 'AssignedBy'
)
BEGIN
    ALTER TABLE catalog.ProductTags
        DROP COLUMN AssignedBy;

    PRINT 'FIX-12b applied: catalog.ProductTags.AssignedBy dropped.';
END
ELSE
    PRINT 'FIX-12b skipped: AssignedBy already absent.';
GO

-- ============================================================
-- FIX-13 · catalog.Brands — Replace UQ_Brands_Name with filtered index
-- ============================================================
-- EF: HasIndex(x => x.Name).IsUnique().HasFilter("[IsDeleted] = 0")
-- SQL: UQ_Brands_Name is a plain UNIQUE constraint (no filter)
-- Problem: Plain UNIQUE blocks re-use of a brand name after soft-delete
-- ============================================================

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE name = 'UQ_Brands_Name'
      AND type = 'UQ'
      AND parent_object_id = OBJECT_ID('catalog.Brands')
)
BEGIN
    ALTER TABLE catalog.Brands
        DROP CONSTRAINT UQ_Brands_Name;

    PRINT 'FIX-13a: UQ_Brands_Name constraint dropped.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_Brands_Name'
      AND object_id = OBJECT_ID('catalog.Brands')
)
BEGIN
    CREATE UNIQUE INDEX UX_Brands_Name
        ON catalog.Brands (Name)
        WHERE IsDeleted = 0;

    PRINT 'FIX-13b applied: Filtered unique index UX_Brands_Name created.';
END
ELSE
    PRINT 'FIX-13b skipped: UX_Brands_Name already exists.';
GO

-- ============================================================
-- FIX-14 · catalog.Brands — Replace UQ_Brands_Slug with filtered index
-- ============================================================
-- EF: HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0")
-- SQL: UQ_Brands_Slug is a plain UNIQUE constraint (no filter)
-- ============================================================

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE name = 'UQ_Brands_Slug'
      AND type = 'UQ'
      AND parent_object_id = OBJECT_ID('catalog.Brands')
)
BEGIN
    ALTER TABLE catalog.Brands
        DROP CONSTRAINT UQ_Brands_Slug;

    PRINT 'FIX-14a: UQ_Brands_Slug constraint dropped.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_Brands_Slug'
      AND object_id = OBJECT_ID('catalog.Brands')
)
BEGIN
    CREATE UNIQUE INDEX UX_Brands_Slug
        ON catalog.Brands (Slug)
        WHERE IsDeleted = 0;

    PRINT 'FIX-14b applied: Filtered unique index UX_Brands_Slug created.';
END
ELSE
    PRINT 'FIX-14b skipped: UX_Brands_Slug already exists.';
GO

-- ============================================================
-- FIX-15 · catalog.Tags — Replace plain UQ constraints with filtered indexes
-- ============================================================
-- EF: HasIndex(Slug).IsUnique().HasFilter("[IsDeleted]=0")
--     HasIndex(Name).IsUnique().HasFilter("[IsDeleted]=0")
-- SQL: UQ_Tags_Slug and UQ_Tags_Name are plain UNIQUE constraints
-- ============================================================

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE name = 'UQ_Tags_Slug'
      AND type = 'UQ'
      AND parent_object_id = OBJECT_ID('catalog.Tags')
)
BEGIN
    ALTER TABLE catalog.Tags DROP CONSTRAINT UQ_Tags_Slug;
    PRINT 'FIX-15a: UQ_Tags_Slug constraint dropped.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_Tags_Slug'
      AND object_id = OBJECT_ID('catalog.Tags')
)
BEGIN
    CREATE UNIQUE INDEX UX_Tags_Slug
        ON catalog.Tags (Slug)
        WHERE IsDeleted = 0;

    PRINT 'FIX-15b applied: Filtered unique index UX_Tags_Slug created.';
END
ELSE
    PRINT 'FIX-15b skipped: UX_Tags_Slug already exists.';
GO

IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE name = 'UQ_Tags_Name'
      AND type = 'UQ'
      AND parent_object_id = OBJECT_ID('catalog.Tags')
)
BEGIN
    ALTER TABLE catalog.Tags DROP CONSTRAINT UQ_Tags_Name;
    PRINT 'FIX-15c: UQ_Tags_Name constraint dropped.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_Tags_Name'
      AND object_id = OBJECT_ID('catalog.Tags')
)
BEGIN
    CREATE UNIQUE INDEX UX_Tags_Name
        ON catalog.Tags (Name)
        WHERE IsDeleted = 0;

    PRINT 'FIX-15d applied: Filtered unique index UX_Tags_Name created.';
END
ELSE
    PRINT 'FIX-15d skipped: UX_Tags_Name already exists.';
GO

-- ============================================================
-- FIX-16 · catalog.AttributeTemplates — ADD UpdatedBy
-- ============================================================
-- Domain: AttributeTemplate : Entity<int> — Update() calls SetUpdatedBy()
-- SQL has CreatedAt, UpdatedAt, CreatedBy, IsDeleted — missing UpdatedBy
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('catalog.AttributeTemplates')
      AND name = 'UpdatedBy'
)
BEGIN
    ALTER TABLE catalog.AttributeTemplates
        ADD UpdatedBy UNIQUEIDENTIFIER NULL;

    PRINT 'FIX-16 applied: catalog.AttributeTemplates.UpdatedBy added.';
END
ELSE
    PRINT 'FIX-16 skipped: UpdatedBy already exists.';
GO

-- ============================================================
-- VERIFICATION — Run after all fixes to confirm the schema
-- ============================================================

PRINT '';
PRINT '=== VERIFICATION QUERIES ===';
PRINT '';

-- Products new columns
SELECT
    c.name      AS ColumnName,
    t.name      AS DataType,
    c.max_length,
    c.is_nullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('catalog.Products')
  AND c.name IN ('CreatorType', 'CompareAtPriceCurrency', 'CostPriceCurrency')
ORDER BY c.name;
GO

-- ProductVariants new columns
SELECT
    c.name      AS ColumnName,
    t.name      AS DataType,
    c.is_nullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('catalog.ProductVariants')
  AND c.name IN ('CompareAtPriceCurrency', 'CostPriceCurrency')
ORDER BY c.name;
GO

-- Check audit columns across all fixed tables
SELECT
    OBJECT_NAME(c.object_id)    AS TableName,
    c.name                      AS ColumnName,
    t.name                      AS DataType,
    c.is_nullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE OBJECT_NAME(c.object_id) IN (
    'ProductSeo', 'ProductImages', 'Tags',
    'ProductAttributes', 'AttributeTemplateItems',
    'ProductStatusHistory', 'AttributeTemplates'
)
  AND c.name IN ('CreatedAt','UpdatedAt','CreatedBy','UpdatedBy')
  AND OBJECT_SCHEMA_NAME(c.object_id) = 'catalog'
ORDER BY TableName, ColumnName;
GO

-- Confirm ProductTags has no extra columns
SELECT c.name AS ColumnName
FROM sys.columns c
WHERE c.object_id = OBJECT_ID('catalog.ProductTags')
ORDER BY c.column_id;
GO

-- Confirm filtered indexes exist on Brands and Tags
SELECT
    t.name      AS TableName,
    i.name      AS IndexName,
    i.is_unique,
    i.filter_definition
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = 'catalog'
  AND i.name IN (
      'UX_Brands_Name', 'UX_Brands_Slug',
      'UX_Tags_Name',   'UX_Tags_Slug'
  )
ORDER BY TableName, IndexName;
GO

-- ============================================================
-- NOTE ON FIX-12 (ProductTags): If you want AssignedAt/AssignedBy
-- tracked going forward, replace the simple UsingEntity() call
-- in ProductConfiguration.cs with a full junction entity:
--
--   public class ProductTag
--   {
--       public Guid     ProductId  { get; set; }
--       public int      TagId      { get; set; }
--       public DateTime AssignedAt { get; set; }
--       public Guid     AssignedBy { get; set; }
--   }
--
-- Then map it in ProductTagConfiguration and re-add the columns.
-- ============================================================
