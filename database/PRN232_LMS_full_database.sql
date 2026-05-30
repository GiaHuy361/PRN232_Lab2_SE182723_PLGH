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
    StudentCode VARCHAR(8) NOT NULL UNIQUE,
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
-- SEED: Students (50) with unique StudentCode (2 letters + 6 digits)
-- Format: SE18xxxx — valid FPTU-style codes, all uppercase, all unique.
-- ============================================================
INSERT INTO Student (StudentCode, FullName, Email, DateOfBirth, Phone) VALUES
('SE180001', N'Nguyen Van Student 1',  'student1@fpt.edu.vn',  '2001-01-08', NULL),
('SE180002', N'Nguyen Van Student 2',  'student2@fpt.edu.vn',  '2001-03-15', NULL),
('SE180003', N'Nguyen Van Student 3',  'student3@fpt.edu.vn',  '2000-11-22', NULL),
('SE180004', N'Nguyen Van Student 4',  'student4@fpt.edu.vn',  '2002-06-01', NULL),
('SE180005', N'Nguyen Van Student 5',  'student5@fpt.edu.vn',  '2001-09-10', NULL),
('SE180006', N'Nguyen Van Student 6',  'student6@fpt.edu.vn',  '2000-04-18', NULL),
('SE180007', N'Nguyen Van Student 7',  'student7@fpt.edu.vn',  '2001-12-25', NULL),
('SE180008', N'Nguyen Van Student 8',  'student8@fpt.edu.vn',  '2002-02-14', NULL),
('SE180009', N'Nguyen Van Student 9',  'student9@fpt.edu.vn',  '2000-07-30', NULL),
('SE180010', N'Nguyen Van Student 10', 'student10@fpt.edu.vn', '2001-05-05', NULL),
('SE180011', N'Nguyen Van Student 11', 'student11@fpt.edu.vn', '2001-01-11', NULL),
('SE180012', N'Nguyen Van Student 12', 'student12@fpt.edu.vn', '2000-08-20', NULL),
('SE180013', N'Nguyen Van Student 13', 'student13@fpt.edu.vn', '2002-03-03', NULL),
('SE180014', N'Nguyen Van Student 14', 'student14@fpt.edu.vn', '2001-10-17', NULL),
('SE180015', N'Nguyen Van Student 15', 'student15@fpt.edu.vn', '2000-12-12', NULL),
('SE180016', N'Nguyen Van Student 16', 'student16@fpt.edu.vn', '2002-07-07', NULL),
('SE180017', N'Nguyen Van Student 17', 'student17@fpt.edu.vn', '2001-04-24', NULL),
('SE180018', N'Nguyen Van Student 18', 'student18@fpt.edu.vn', '2000-09-09', NULL),
('SE180019', N'Nguyen Van Student 19', 'student19@fpt.edu.vn', '2002-01-28', NULL),
('SE180020', N'Nguyen Van Student 20', 'student20@fpt.edu.vn', '2001-06-16', NULL),
('HE180021', N'Nguyen Van Student 21', 'student21@fpt.edu.vn', '2000-11-03', NULL),
('HE180022', N'Nguyen Van Student 22', 'student22@fpt.edu.vn', '2002-04-11', NULL),
('HE180023', N'Nguyen Van Student 23', 'student23@fpt.edu.vn', '2001-07-19', NULL),
('HE180024', N'Nguyen Van Student 24', 'student24@fpt.edu.vn', '2000-02-26', NULL),
('HE180025', N'Nguyen Van Student 25', 'student25@fpt.edu.vn', '2002-09-04', NULL),
('HE180026', N'Nguyen Van Student 26', 'student26@fpt.edu.vn', '2001-03-12', NULL),
('HE180027', N'Nguyen Van Student 27', 'student27@fpt.edu.vn', '2000-06-20', NULL),
('HE180028', N'Nguyen Van Student 28', 'student28@fpt.edu.vn', '2002-11-27', NULL),
('HE180029', N'Nguyen Van Student 29', 'student29@fpt.edu.vn', '2001-08-05', NULL),
('HE180030', N'Nguyen Van Student 30', 'student30@fpt.edu.vn', '2000-01-13', NULL),
('CE180031', N'Nguyen Van Student 31', 'student31@fpt.edu.vn', '2002-06-21', NULL),
('CE180032', N'Nguyen Van Student 32', 'student32@fpt.edu.vn', '2001-09-28', NULL),
('CE180033', N'Nguyen Van Student 33', 'student33@fpt.edu.vn', '2000-04-06', NULL),
('CE180034', N'Nguyen Van Student 34', 'student34@fpt.edu.vn', '2002-12-14', NULL),
('CE180035', N'Nguyen Van Student 35', 'student35@fpt.edu.vn', '2001-05-22', NULL),
('CE180036', N'Nguyen Van Student 36', 'student36@fpt.edu.vn', '2000-10-29', NULL),
('CE180037', N'Nguyen Van Student 37', 'student37@fpt.edu.vn', '2002-02-07', NULL),
('CE180038', N'Nguyen Van Student 38', 'student38@fpt.edu.vn', '2001-07-15', NULL),
('CE180039', N'Nguyen Van Student 39', 'student39@fpt.edu.vn', '2000-12-23', NULL),
('CE180040', N'Nguyen Van Student 40', 'student40@fpt.edu.vn', '2002-05-01', NULL),
('DE180041', N'Nguyen Van Student 41', 'student41@fpt.edu.vn', '2001-02-09', NULL),
('DE180042', N'Nguyen Van Student 42', 'student42@fpt.edu.vn', '2000-07-17', NULL),
('DE180043', N'Nguyen Van Student 43', 'student43@fpt.edu.vn', '2002-10-24', NULL),
('DE180044', N'Nguyen Van Student 44', 'student44@fpt.edu.vn', '2001-04-03', NULL),
('DE180045', N'Nguyen Van Student 45', 'student45@fpt.edu.vn', '2000-09-11', NULL),
('DE180046', N'Nguyen Van Student 46', 'student46@fpt.edu.vn', '2002-01-18', NULL),
('DE180047', N'Nguyen Van Student 47', 'student47@fpt.edu.vn', '2001-06-26', NULL),
('DE180048', N'Nguyen Van Student 48', 'student48@fpt.edu.vn', '2000-03-05', NULL),
('DE180049', N'Nguyen Van Student 49', 'student49@fpt.edu.vn', '2002-08-13', NULL),
('DE180050', N'Nguyen Van Student 50', 'student50@fpt.edu.vn', '2001-11-21', NULL);
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
