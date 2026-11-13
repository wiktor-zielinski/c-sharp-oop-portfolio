-- Tworzenie bazy danych systemu magazynowego WMS
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WarehouseDB')
BEGIN
    CREATE DATABASE WarehouseDB;
END
GO

USE WarehouseDB;
GO

-- 1. Tabela główna asortymentu magazynowego (kluczem głównym jest kod SKU)
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Sku VARCHAR(30) PRIMARY KEY,               -- Unikalny kod magazynowy produktu (np. ALU-PRF-200)
        Name NVARCHAR(150) NOT NULL,               -- Nazwa towaru
        Category INT NOT NULL,                     -- Enum ProductCategory (0..3)
        RackLocation VARCHAR(20) NOT NULL,         -- Lokalizacja w alejce (np. A-01-04)
        Quantity INT NOT NULL DEFAULT 0,           -- Aktualny stan magazynowy (szt.)
        MinStockThreshold INT NOT NULL DEFAULT 10, -- Próg alarmowy niskiego stanu
        UnitNetPrice DECIMAL(10,2) NOT NULL,       -- Cena jednostkowa netto (PLN)
        Status INT NOT NULL DEFAULT 0              -- Enum StockStatus (0=Optymalny, 1=NiskiStan, 2=Wyczerpany, 3=ZablokowanyQA)
    );
END
GO

-- 2. Tabela historii audytów jakościowych i operacji na towarze (Relacja 1 do wielu)
IF OBJECT_ID('dbo.ProductAuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductAuditLogs (
        AuditId INT IDENTITY(1,1) PRIMARY KEY,
        ProductSku VARCHAR(30) NOT NULL,
        AuditNote NVARCHAR(500) NOT NULL,
        CreatedAt VARCHAR(16) NOT NULL DEFAULT FORMAT(SYSDATETIME(), 'yyyy-MM-dd HH:mm'),
        CONSTRAINT FK_ProductAuditLogs_Products FOREIGN KEY (ProductSku)
            REFERENCES dbo.Products(Sku)
            ON DELETE CASCADE
    );
END
GO

-- 3. Tabela dziennika zdarzeń i operacji logistycznych
IF OBJECT_ID('dbo.SystemLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemLogs (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        LogMessage NVARCHAR(500) NOT NULL,
        CreatedAt VARCHAR(16) NOT NULL DEFAULT FORMAT(SYSDATETIME(), 'yyyy-MM-dd HH:mm')
    );
END
GO