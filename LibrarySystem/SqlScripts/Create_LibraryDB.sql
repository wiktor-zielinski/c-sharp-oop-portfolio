-- Tworzenie bazy danych biblioteki
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'LibraryDB')
BEGIN
    CREATE DATABASE LibraryDB;
END
GO

USE LibraryDB;
GO

-- 1. Tabela główna książek (kluczem głównym jest numer ISBN)
IF OBJECT_ID('dbo.Books', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Books (
        Isbn NVARCHAR(30) PRIMARY KEY,             -- Unikalny numer ISBN książki
        Title NVARCHAR(200) NOT NULL,              -- Tytuł książki
        Author NVARCHAR(150) NOT NULL,             -- Autor książki
        PublicationDate DATE NOT NULL,             -- Data wydania (tylko data, bez godzin)
        Status INT NOT NULL DEFAULT 0,             -- Enum BookStatus (0=Available, 1=Borrowed, 2=Reserved, 3=Lost)
        DueDate DATE NULL,                         -- Termin zwrotu (lub NULL jeśli niewypożyczona)
        BorrowedByUserId INT NULL,                 -- ID czytelnika, który wypożyczył książkę
        ReservedByUserId INT NULL                  -- ID czytelnika, który zarezerwował książkę
    );
END
GO

-- 2. Tabela z opiniami czytelników o książkach (Relacja 1 do wielu)
IF OBJECT_ID('dbo.BookFeedbacks', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BookFeedbacks (
        FeedbackId INT IDENTITY(1,1) PRIMARY KEY,
        BookIsbn NVARCHAR(30) NOT NULL,
        FeedbackText NVARCHAR(500) NOT NULL,
        CreatedAt VARCHAR(16) NOT NULL DEFAULT FORMAT(SYSDATETIME(), 'yyyy-MM-dd HH:mm'),
        CONSTRAINT FK_BookFeedbacks_Books FOREIGN KEY (BookIsbn)
            REFERENCES dbo.Books(Isbn)
            ON DELETE CASCADE -- Usunięcie książki automatycznie usuwa jej opinie
    );
END
GO