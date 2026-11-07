-- Tworzenie bazy danych schroniska
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AnimalShelterDB')
BEGIN
    CREATE DATABASE AnimalShelterDB;
END
GO

USE AnimalShelterDB;
GO

-- 1. Tabela główna zwierząt
IF OBJECT_ID('dbo.Animals', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Animals (
        Id INT PRIMARY KEY,                        -- ID nadawane przez system
        Name NVARCHAR(100) NOT NULL,               -- Imię zwierzęcia
        Species INT NOT NULL,                      -- Enum AnimalSpecies (0=Pies, 1=Kot, 2=Królik, 3=Inne)
        Breed NVARCHAR(100) NOT NULL,              -- Rasa
        Age INT NOT NULL,                          -- Wiek w latach
        HealthScore INT NOT NULL,                  -- Stan zdrowia (0-100)
        SocializationScore INT NOT NULL,           -- Poziom socjalizacji (0-100)
        IsVaccinated BIT NOT NULL DEFAULT 0,       -- Czy zaszczepiony (0/1)
        RequiresGarden BIT NOT NULL DEFAULT 0,     -- Czy wymaga ogrodu (0/1)
        Status INT NOT NULL,                       -- Enum AdoptionStatus
        CaretakerId INT NULL                       -- ID przypisanego opiekuna (lub NULL)
    );
END
GO

-- 2. Tabela z wpisami w karcie medyczno-behawioralnej (Relacja 1 do wielu)
IF OBJECT_ID('dbo.AnimalNotes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AnimalNotes (
        NoteId INT IDENTITY(1,1) PRIMARY KEY,
        AnimalId INT NOT NULL,
        NoteText NVARCHAR(500) NOT NULL,
        -- ZMIANA: Zapisujemy czas w formacie tekstowym RRRR-MM-DD GG:MM (bez sekund)
        CreatedAt VARCHAR(16) NOT NULL DEFAULT FORMAT(SYSDATETIME(), 'yyyy-MM-dd HH:mm'),
        CONSTRAINT FK_AnimalNotes_Animals FOREIGN KEY (AnimalId)
            REFERENCES dbo.Animals(Id)
            ON DELETE CASCADE -- Usunięcie zwierzęcia automatycznie usuwa jego notatki
    );
END
GO

-- 3. Tabela dziennika zdarzeń schroniska
IF OBJECT_ID('dbo.SystemLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemLogs (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        LogMessage NVARCHAR(500) NOT NULL,
        -- ZMIANA: Zapisujemy czas w formacie tekstowym RRRR-MM-DD GG:MM (bez sekund)
        CreatedAt VARCHAR(16) NOT NULL DEFAULT FORMAT(SYSDATETIME(), 'yyyy-MM-dd HH:mm')
    );
END
GO