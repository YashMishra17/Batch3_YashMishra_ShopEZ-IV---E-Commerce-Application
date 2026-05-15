-- =============================================================================
-- ShopEZ — Product Service Database Schema
-- Database : ShopEZ_ProductDb
-- =============================================================================

USE master;
GO

IF NOT EXISTS (
    SELECT name FROM sys.databases WHERE name = N'ShopEZ_ProductDb'
)
BEGIN
    CREATE DATABASE ShopEZ_ProductDb
        COLLATE SQL_Latin1_General_CP1_CI_AS;
END
GO

USE ShopEZ_ProductDb;
GO

-- ── Products table ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products
    (
        ProductId   INT            NOT NULL IDENTITY(1,1),
        Name        NVARCHAR(200)  NOT NULL,
        Description NVARCHAR(1000) NOT NULL
            CONSTRAINT DF_Products_Description DEFAULT (N''),
        Price       DECIMAL(18,2)  NOT NULL,
        ImageUrl    NVARCHAR(500)  NOT NULL
            CONSTRAINT DF_Products_ImageUrl DEFAULT (N''),
        Stock       INT            NOT NULL
            CONSTRAINT DF_Products_Stock DEFAULT (0),

        CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (ProductId ASC),
        CONSTRAINT CK_Products_Price CHECK (Price > 0),
        CONSTRAINT CK_Products_Stock CHECK (Stock >= 0)
    );
END
GO

-- ── Index: Name + Price for keyword search and price-range queries ────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  name = 'IX_Products_Name_Price'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_Name_Price
        ON Products (Name ASC, Price ASC)
        INCLUDE (ProductId, Description, ImageUrl, Stock);
END
GO

-- ── Index: Price only ─────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE  name = 'IX_Products_Price'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_Price
        ON Products (Price ASC)
        INCLUDE (ProductId, Name, Description, ImageUrl, Stock);
END
GO

-- ── Seed data — mirrors monolith EF seed ─────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    SET IDENTITY_INSERT Products ON;

    INSERT INTO Products
        (ProductId, Name, Description, Price, ImageUrl, Stock)
    VALUES
    (
        1,
        N'Wireless Mouse',
        N'Ergonomic wireless mouse with USB receiver',
        29.99,
        N'https://via.placeholder.com/300?text=Wireless+Mouse',
        100
    ),
    (
        2,
        N'Mechanical Keyboard',
        N'RGB mechanical keyboard with blue switches',
        79.99,
        N'https://via.placeholder.com/300?text=Mechanical+Keyboard',
        50
    ),
    (
        3,
        N'USB-C Hub',
        N'7-in-1 USB-C hub with HDMI and SD card reader',
        49.99,
        N'https://via.placeholder.com/300?text=USB-C+Hub',
        75
    );

    SET IDENTITY_INSERT Products OFF;
END
GO

SELECT * FROM Products;