-- =============================================
-- Dormitory Management System Database
-- SQL Server Database Creation Script
-- =============================================

-- Create Database
USE master;
GO

-- Drop database if exists (optional - uncomment if you want to recreate)
-- DROP DATABASE IF EXISTS DormitoryDB;
-- GO

-- Create new database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DormitoryDB')
BEGIN
    CREATE DATABASE DormitoryDB;
    PRINT 'Database DormitoryDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database DormitoryDB already exists.';
END
GO

-- Use the database
USE DormitoryDB;
GO

-- =============================================
-- Create Tables
-- =============================================

-- 1. Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(50) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Role INT NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Table Users created successfully.';
END
GO

-- 2. Rooms Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rooms')
BEGIN
    CREATE TABLE Rooms (
        Id INT PRIMARY KEY IDENTITY(1,1),
        RoomNumber NVARCHAR(10) NOT NULL UNIQUE,
        Capacity INT NOT NULL,
        CurrentOccupancy INT NOT NULL DEFAULT 0,
        Type INT NOT NULL,
        Status INT NOT NULL DEFAULT 0
    );
    PRINT 'Table Rooms created successfully.';
END
GO

-- 3. Students Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
BEGIN
    CREATE TABLE Students (
        Id INT PRIMARY KEY IDENTITY(1,1),
        StudentId NVARCHAR(20) NOT NULL UNIQUE,
        Password NVARCHAR(50) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(15) NOT NULL,
        Department NVARCHAR(100) NOT NULL,
        RoomId INT NULL,
        RegistrationDate DATETIME NOT NULL DEFAULT GETDATE(),
        CheckInDate DATETIME NULL,
        CheckOutDate DATETIME NULL,
        IsCheckedIn BIT NOT NULL DEFAULT 0,
        Status INT NOT NULL DEFAULT 1,
        CONSTRAINT FK_Students_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
    );
    PRINT 'Table Students created successfully.';
END
GO

-- 4. DailyReports Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DailyReports')
BEGIN
    CREATE TABLE DailyReports (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ReportDate DATE NOT NULL,
        PreparedBy NVARCHAR(100) NOT NULL,
        PreparedByUserId INT NOT NULL,
        TotalStudents INT NOT NULL,
        StudentsPresent INT NOT NULL,
        StudentsAbsent INT NOT NULL,
        TotalRooms INT NOT NULL,
        OccupiedRooms INT NOT NULL,
        AvailableRooms INT NOT NULL,
        TotalCapacity INT NOT NULL,
        CurrentOccupancy INT NOT NULL,
        CheckInsToday INT NOT NULL,
        CheckOutsToday INT NOT NULL,
        Remarks NVARCHAR(MAX) NULL,
        Issues NVARCHAR(MAX) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        Status INT NOT NULL DEFAULT 0,
        CONSTRAINT FK_DailyReports_Users FOREIGN KEY (PreparedByUserId) REFERENCES Users(Id)
    );
    PRINT 'Table DailyReports created successfully.';
END
GO

-- =============================================
-- Insert Default Data
-- =============================================

-- Insert Default Users (if not exists)
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'director')
BEGIN
    INSERT INTO Users (Username, Password, FullName, Role, CreatedDate)
    VALUES ('director', '1234', 'Director Admin', 2, GETDATE());
    PRINT 'Default Director user created.';
END

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'proctor')
BEGIN
    INSERT INTO Users (Username, Password, FullName, Role, CreatedDate)
    VALUES ('proctor', '5678', 'Proctor Staff', 1, GETDATE());
    PRINT 'Default Proctor user created.';
END
GO

-- Insert Default Rooms (if not exists)
IF NOT EXISTS (SELECT * FROM Rooms WHERE RoomNumber = '101')
BEGIN
    INSERT INTO Rooms (RoomNumber, Capacity, CurrentOccupancy, Type, Status)
    VALUES 
        ('101', 6, 0, 4, 0),
        ('102', 6, 0, 4, 0),
        ('103', 6, 0, 4, 0),
        ('104', 6, 0, 4, 0),
        ('105', 6, 0, 4, 0),
        ('201', 6, 0, 4, 0),
        ('202', 6, 0, 4, 0),
        ('203', 6, 0, 4, 0),
        ('204', 6, 0, 4, 0),
        ('205', 6, 0, 4, 0);
    PRINT 'Default rooms created (101-105, 201-205).';
END
GO

-- =============================================
-- Create Indexes for Better Performance
-- =============================================

-- Index on Students.StudentId for faster lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_StudentId')
BEGIN
    CREATE INDEX IX_Students_StudentId ON Students(StudentId);
    PRINT 'Index IX_Students_StudentId created.';
END
GO

-- Index on Students.RoomId for faster joins
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_RoomId')
BEGIN
    CREATE INDEX IX_Students_RoomId ON Students(RoomId);
    PRINT 'Index IX_Students_RoomId created.';
END
GO

-- Index on DailyReports.ReportDate for faster date queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DailyReports_ReportDate')
BEGIN
    CREATE INDEX IX_DailyReports_ReportDate ON DailyReports(ReportDate);
    PRINT 'Index IX_DailyReports_ReportDate created.';
END
GO

-- =============================================
-- Verify Database Creation
-- =============================================

PRINT '';
PRINT '=============================================';
PRINT 'Database Setup Complete!';
PRINT '=============================================';
PRINT '';
PRINT 'Database: DormitoryDB';
PRINT 'Tables Created: 4';
PRINT '  - Users (2 default users)';
PRINT '  - Rooms (10 default rooms)';
PRINT '  - Students (empty)';
PRINT '  - DailyReports (empty)';
PRINT '';
PRINT 'Default Users:';
PRINT '  - Username: director, Password: 1234, Role: Director';
PRINT '  - Username: proctor, Password: 5678, Role: Proctor';
PRINT '';
PRINT 'Default Rooms:';
PRINT '  - 101, 102, 103, 104, 105';
PRINT '  - 201, 202, 203, 204, 205';
PRINT '  - Each room: Capacity = 6, Type = Six, Status = Available';
PRINT '';
PRINT '=============================================';
PRINT 'You can now run your application!';
PRINT '=============================================';
GO

-- Display table counts
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'Rooms', COUNT(*) FROM Rooms
UNION ALL
SELECT 'Students', COUNT(*) FROM Students
UNION ALL
SELECT 'DailyReports', COUNT(*) FROM DailyReports;
GO
