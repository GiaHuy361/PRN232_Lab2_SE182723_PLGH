-- PRN232_LMS Full Database Script
-- Run this script against your SQL Server instance to create and seed the database.

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PRN232_LMS')
BEGIN
    CREATE DATABASE PRN232_LMS;
END
GO

USE PRN232_LMS;
GO

-- ============================================================
-- DROP TABLES (if re-running)
-- ============================================================
IF OBJECT_ID('dbo.RefreshToken', 'U') IS NOT NULL DROP TABLE dbo.RefreshToken;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Enrollment', 'U') IS NOT NULL DROP TABLE dbo.Enrollment;
IF OBJECT_ID('dbo.Course', 'U') IS NOT NULL DROP TABLE dbo.Course;
IF OBJECT_ID('dbo.Semester', 'U') IS NOT NULL DROP TABLE dbo.Semester;
IF OBJECT_ID('dbo.Student', 'U') IS NOT NULL DROP TABLE dbo.Student;
IF OBJECT_ID('dbo.Subject', 'U') IS NOT NULL DROP TABLE dbo.Subject;
GO

-- ============================================================
-- CREATE TABLES
-- ============================================================
CREATE TABLE Semester (
    SemesterId   INT PRIMARY KEY IDENTITY(1,1),
    SemesterName NVARCHAR(100) NOT NULL,
    StartDate    DATETIME NOT NULL,
    EndDate      DATETIME NOT NULL
);

CREATE TABLE Course (
    CourseId   INT PRIMARY KEY IDENTITY(1,1),
    CourseName NVARCHAR(100) NOT NULL,
    SemesterId INT NOT NULL,
    CONSTRAINT FK_Course_Semester FOREIGN KEY (SemesterId) REFERENCES Semester(SemesterId)
);

CREATE TABLE Subject (
    SubjectId   INT PRIMARY KEY IDENTITY(1,1),
    SubjectCode VARCHAR(20) NOT NULL,
    SubjectName NVARCHAR(100) NOT NULL,
    Credit      INT NOT NULL
);

CREATE TABLE Student (
    StudentId   INT PRIMARY KEY IDENTITY(1,1),
    FullName    NVARCHAR(100) NOT NULL,
    Email       VARCHAR(100) NOT NULL,
    DateOfBirth DATETIME NOT NULL,
    Phone       VARCHAR(20) NULL
);

CREATE TABLE Enrollment (
    EnrollmentId INT PRIMARY KEY IDENTITY(1,1),
    StudentId    INT NOT NULL,
    CourseId     INT NOT NULL,
    EnrollDate   DATETIME NOT NULL,
    Status       VARCHAR(20) NOT NULL,
    CONSTRAINT FK_Enrollment_Student FOREIGN KEY (StudentId) REFERENCES Student(StudentId),
    CONSTRAINT FK_Enrollment_Course  FOREIGN KEY (CourseId)  REFERENCES Course(CourseId)
);

CREATE TABLE [Users] (
    UserId       INT PRIMARY KEY IDENTITY(1,1),
    Username     VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role         VARCHAR(20) NOT NULL
);

CREATE TABLE RefreshToken (
    RefreshTokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId         INT NOT NULL,
    Token          VARCHAR(500) NOT NULL,
    ExpiresAt      DATETIME2 NOT NULL,
    CreatedAt      DATETIME2 NOT NULL,
    RevokedAt      DATETIME2 NULL,
    IsUsed         BIT NOT NULL,
    IsRevoked      BIT NOT NULL,
    CONSTRAINT FK_RefreshToken_Users FOREIGN KEY (UserId) REFERENCES [Users](UserId)
);
GO

-- ============================================================
-- SEED: Semesters (5)
-- ============================================================
INSERT INTO Semester (SemesterName, StartDate, EndDate) VALUES
(N'Spring 2024', '2024-01-15', '2024-05-31'),
(N'Summer 2024', '2024-06-01', '2024-08-31'),
(N'Fall 2024',   '2024-09-01', '2025-01-15'),
(N'Spring 2025', '2025-01-20', '2025-05-31'),
(N'Fall 2025',   '2025-09-01', '2026-01-15');
GO

-- ============================================================
-- SEED: Subjects (10)
-- ============================================================
INSERT INTO Subject (SubjectCode, SubjectName, Credit) VALUES
('PRN211', N'Basic Cross-Platform Application Programming With .NET', 3),
('PRN231', N'Advanced Cross-Platform Application Programming With .NET', 3),
('PRN232', N'Web API With .NET', 3),
('SWD391', N'Software Architecture and Design', 3),
('SWE201', N'Introduction to Software Engineering', 3),
('DBI202', N'Database Systems', 3),
('MAE101', N'Mathematics for Engineering', 3),
('OSG202', N'Operating Systems', 3),
('NWC203', N'Computer Networking', 3),
('WDP301', N'Web Design & Development', 3);
GO

-- ============================================================
-- SEED: Courses (20 — 4 per semester)
-- ============================================================
INSERT INTO Course (CourseName, SemesterId) VALUES
(N'PRN211 - Spring 2024', 1), (N'PRN232 - Spring 2024', 1),
(N'SWD391 - Spring 2024', 1), (N'DBI202 - Spring 2024', 1),

(N'PRN231 - Summer 2024', 2), (N'SWE201 - Summer 2024', 2),
(N'MAE101 - Summer 2024', 2), (N'WDP301 - Summer 2024', 2),

(N'PRN232 - Fall 2024',   3), (N'OSG202 - Fall 2024',   3),
(N'NWC203 - Fall 2024',   3), (N'DBI202 - Fall 2024',   3),

(N'PRN231 - Spring 2025', 4), (N'PRN232 - Spring 2025', 4),
(N'SWD391 - Spring 2025', 4), (N'WDP301 - Spring 2025', 4),

(N'PRN211 - Fall 2025',   5), (N'PRN232 - Fall 2025',   5),
(N'MAE101 - Fall 2025',   5), (N'NWC203 - Fall 2025',   5);
GO

-- ============================================================
-- SEED: Students (50)
-- ============================================================
DECLARE @i INT = 1;
WHILE @i <= 50
BEGIN
    INSERT INTO Student (FullName, Email, DateOfBirth)
    VALUES (
        CONCAT(N'Nguyen Van Student ', @i),
        CONCAT('student', @i, '@lms.edu.vn'),
        DATEADD(YEAR, -18 - (@i % 5), DATEADD(DAY, @i * 7, '2000-01-01'))
    );
    SET @i = @i + 1;
END
GO

-- ============================================================
-- SEED: Enrollments (500 — 10 per student, spread across courses)
-- ============================================================
DECLARE @sid INT = 1;
DECLARE @cid INT;
DECLARE @statuses TABLE (s VARCHAR(20));
INSERT INTO @statuses VALUES ('Active'), ('Inactive'), ('Completed'), ('Dropped'), ('Pending');

WHILE @sid <= 50
BEGIN
    DECLARE @j INT = 1;
    WHILE @j <= 10
    BEGIN
        SET @cid = ((@sid + @j - 2) % 20) + 1;
        INSERT INTO Enrollment (StudentId, CourseId, EnrollDate, Status)
        VALUES (
            @sid,
            @cid,
            DATEADD(DAY, (@sid * 3 + @j), '2024-01-01'),
            (SELECT TOP 1 s FROM @statuses ORDER BY NEWID())
        );
        SET @j = @j + 1;
    END
    SET @sid = @sid + 1;
END
GO

-- ============================================================
-- SEED: Users (Admin & Student)
-- ============================================================
INSERT INTO [Users] (Username, PasswordHash, Role) VALUES
('admin', '$2a$11$KOrKPxHkCwrGVF/U4RqBeeoBAahmIFVFvVGa1RpSx1szrK2iX2dLi', 'Admin'),
('student', '$2a$11$E/rdbALLsHRU0QFb569DyeQm6UPeezJuRgq7mkRtf9uclLGufiQSm', 'Student');
GO

PRINT 'PRN232_LMS database seeded successfully.';
GO
