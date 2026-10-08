-- ============================================================================
-- Student Management Mini-ERP: Database Creation Script
-- Target Database: Microsoft SQL Server (2019 / 2022 / LocalDB)
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'StudentManagementDb')
BEGIN
    CREATE DATABASE StudentManagementDb;
END
GO

USE StudentManagementDb;
GO

-- Drop existing tables in reverse dependency order if recreating
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.Fees', 'U') IS NOT NULL DROP TABLE dbo.Fees;
IF OBJECT_ID('dbo.Students', 'U') IS NOT NULL DROP TABLE dbo.Students;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- 1. Users Table
CREATE TABLE dbo.Users (
    Id INT IDENTITY(1,1) NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT 'Staff',
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT UQ_Users_Username UNIQUE NONCLUSTERED (Username ASC),
    CONSTRAINT UQ_Users_Email UNIQUE NONCLUSTERED (Email ASC)
);
GO

-- 2. Students Table
CREATE TABLE dbo.Students (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentCode NVARCHAR(30) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Address NVARCHAR(255) NULL,
    AdmissionDate DATE NOT NULL,
    Course NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Students_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Students_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_Students PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT UQ_Students_StudentCode UNIQUE NONCLUSTERED (StudentCode ASC),
    CONSTRAINT UQ_Students_Email UNIQUE NONCLUSTERED (Email ASC)
);
GO

CREATE NONCLUSTERED INDEX IX_Students_Search ON dbo.Students (LastName, FirstName) INCLUDE (StudentCode, Email, Course, IsActive);
CREATE NONCLUSTERED INDEX IX_Students_Course_Status ON dbo.Students (Course, IsActive);
GO

-- 3. Fees Table
CREATE TABLE dbo.Fees (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentId INT NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Fees_PaidAmount DEFAULT 0.00,
    OutstandingAmount DECIMAL(18,2) NOT NULL,
    DueDate DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Fees_Status DEFAULT 'Pending',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Fees_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_Fees PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_Fees_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students (Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Fees_TotalAmount CHECK (TotalAmount >= 0),
    CONSTRAINT CK_Fees_PaidAmount CHECK (PaidAmount >= 0),
    CONSTRAINT CK_Fees_OutstandingAmount CHECK (OutstandingAmount >= 0),
    CONSTRAINT CK_Fees_BalanceLogic CHECK (PaidAmount <= TotalAmount)
);
GO

CREATE NONCLUSTERED INDEX IX_Fees_StudentId ON dbo.Fees (StudentId ASC);
CREATE NONCLUSTERED INDEX IX_Fees_Status ON dbo.Fees (Status ASC);
GO

-- 4. Payments Table
CREATE TABLE dbo.Payments (
    Id INT IDENTITY(1,1) NOT NULL,
    FeeId INT NOT NULL,
    ReceiptNumber NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME2 NOT NULL CONSTRAINT DF_Payments_PaymentDate DEFAULT SYSUTCDATETIME(),
    PaymentMethod NVARCHAR(30) NOT NULL,
    Notes NVARCHAR(255) NULL,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Payments PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_Payments_Fees FOREIGN KEY (FeeId) REFERENCES dbo.Fees (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Payments_Users FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users (Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_Payments_ReceiptNumber UNIQUE NONCLUSTERED (ReceiptNumber ASC),
    CONSTRAINT CK_Payments_Amount CHECK (Amount > 0)
);
GO

CREATE NONCLUSTERED INDEX IX_Payments_FeeId ON dbo.Payments (FeeId ASC);
GO
