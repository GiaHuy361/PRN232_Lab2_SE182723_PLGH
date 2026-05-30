using System;
using System.Collections.Generic;
using System.Linq;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Data;

public static class DbSeeder
{
    public static void Seed(LmsDbContext context)
    {
        if (context.Users.Any())
        {
            return; // Already seeded
        }

        // 1. Seed Semesters
        var semesters = new List<Semester>
        {
            new() { SemesterName = "Spring 2024", StartDate = DateTime.Parse("2024-01-15"), EndDate = DateTime.Parse("2024-05-31") },
            new() { SemesterName = "Summer 2024", StartDate = DateTime.Parse("2024-06-01"), EndDate = DateTime.Parse("2024-08-31") },
            new() { SemesterName = "Fall 2024",   StartDate = DateTime.Parse("2024-09-01"), EndDate = DateTime.Parse("2025-01-15") },
            new() { SemesterName = "Spring 2025", StartDate = DateTime.Parse("2025-01-20"), EndDate = DateTime.Parse("2025-05-31") },
            new() { SemesterName = "Fall 2025",   StartDate = DateTime.Parse("2025-09-01"), EndDate = DateTime.Parse("2026-01-15") }
        };
        context.Semesters.AddRange(semesters);
        context.SaveChanges();

        // 2. Seed Subjects
        var subjects = new List<Subject>
        {
            new() { SubjectCode = "PRN211", SubjectName = "Basic Cross-Platform Application Programming With .NET", Credit = 3 },
            new() { SubjectCode = "PRN231", SubjectName = "Advanced Cross-Platform Application Programming With .NET", Credit = 3 },
            new() { SubjectCode = "PRN232", SubjectName = "Web API With .NET", Credit = 3 },
            new() { SubjectCode = "SWD391", SubjectName = "Software Architecture and Design", Credit = 3 },
            new() { SubjectCode = "SWE201", SubjectName = "Introduction to Software Engineering", Credit = 3 },
            new() { SubjectCode = "DBI202", SubjectName = "Database Systems", Credit = 3 },
            new() { SubjectCode = "MAE101", SubjectName = "Mathematics for Engineering", Credit = 3 },
            new() { SubjectCode = "OSG202", SubjectName = "Operating Systems", Credit = 3 },
            new() { SubjectCode = "NWC203", SubjectName = "Computer Networking", Credit = 3 },
            new() { SubjectCode = "WDP301", SubjectName = "Web Design & Development", Credit = 3 }
        };
        context.Subjects.AddRange(subjects);
        context.SaveChanges();

        // 3. Seed Courses
        var courses = new List<Course>
        {
            new() { CourseName = "PRN211 - Spring 2024", SemesterId = semesters[0].SemesterId },
            new() { CourseName = "PRN232 - Spring 2024", SemesterId = semesters[0].SemesterId },
            new() { CourseName = "SWD391 - Spring 2024", SemesterId = semesters[0].SemesterId },
            new() { CourseName = "DBI202 - Spring 2024", SemesterId = semesters[0].SemesterId },

            new() { CourseName = "PRN231 - Summer 2024", SemesterId = semesters[1].SemesterId },
            new() { CourseName = "SWE201 - Summer 2024", SemesterId = semesters[1].SemesterId },
            new() { CourseName = "MAE101 - Summer 2024", SemesterId = semesters[1].SemesterId },
            new() { CourseName = "WDP301 - Summer 2024", SemesterId = semesters[1].SemesterId },

            new() { CourseName = "PRN232 - Fall 2024",   SemesterId = semesters[2].SemesterId },
            new() { CourseName = "OSG202 - Fall 2024",   SemesterId = semesters[2].SemesterId },
            new() { CourseName = "NWC203 - Fall 2024",   SemesterId = semesters[2].SemesterId },
            new() { CourseName = "DBI202 - Fall 2024",   SemesterId = semesters[2].SemesterId },

            new() { CourseName = "PRN231 - Spring 2025", SemesterId = semesters[3].SemesterId },
            new() { CourseName = "PRN232 - Spring 2025", SemesterId = semesters[3].SemesterId },
            new() { CourseName = "SWD391 - Spring 2025", SemesterId = semesters[3].SemesterId },
            new() { CourseName = "WDP301 - Spring 2025", SemesterId = semesters[3].SemesterId },

            new() { CourseName = "PRN211 - Fall 2025",   SemesterId = semesters[4].SemesterId },
            new() { CourseName = "PRN232 - Fall 2025",   SemesterId = semesters[4].SemesterId },
            new() { CourseName = "MAE101 - Fall 2025",   SemesterId = semesters[4].SemesterId },
            new() { CourseName = "NWC203 - Fall 2025",   SemesterId = semesters[4].SemesterId }
        };
        context.Courses.AddRange(courses);
        context.SaveChanges();

        // 4. Seed Users
        var users = new List<User>
        {
            new() { Username = "admin", PasswordHash = "$2a$11$KOrKPxHkCwrGVF/U4RqBeeoBAahmIFVFvVGa1RpSx1szrK2iX2dLi", Role = "Admin" },
            new() { Username = "student", PasswordHash = "$2a$11$E/rdbALLsHRU0QFb569DyeQm6UPeezJuRgq7mkRtf9uclLGufiQSm", Role = "Student" }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        // 5. Seed Students (50)
        var students = new List<Student>();
        var dobList = new[] { "2001-01-08", "2001-03-15", "2000-11-22", "2002-06-01", "2001-09-10", "2000-04-18", "2001-12-25", "2002-02-14", "2000-07-30", "2001-05-05", 
                              "2001-01-11", "2000-08-20", "2002-03-03", "2001-10-17", "2000-12-12", "2002-07-07", "2001-04-24", "2000-09-09", "2002-01-28", "2001-06-16",
                              "2000-11-03", "2002-04-11", "2001-07-19", "2000-02-26", "2002-09-04", "2001-03-12", "2000-06-20", "2002-11-27", "2001-08-05", "2000-01-13",
                              "2002-06-21", "2001-09-28", "2000-04-06", "2002-12-14", "2001-05-22", "2000-10-29", "2002-02-07", "2001-07-15", "2000-12-23", "2002-05-01",
                              "2001-02-09", "2000-07-17", "2002-10-24", "2001-04-03", "2000-09-11", "2002-01-18", "2001-06-26", "2000-03-05", "2002-08-13", "2001-11-21" };

        for (int i = 1; i <= 50; i++)
        {
            string code;
            if (i <= 20) code = $"SE18{i:D4}";
            else if (i <= 30) code = $"HE18{i:D4}";
            else if (i <= 40) code = $"CE{180000 + i}";
            else code = $"DE{180000 + i}";

            students.Add(new Student
            {
                StudentCode = code,
                FullName = $"Nguyen Van Student {i}",
                Email = $"student{i}@fpt.edu.vn",
                DateOfBirth = DateTime.Parse(dobList[i - 1]),
                Phone = null
            });
        }
        context.Students.AddRange(students);
        context.SaveChanges();

        // 6. Seed Enrollments (500)
        var enrollments = new List<Enrollment>();
        var statuses = new[] { "Active", "Inactive", "Completed", "Dropped", "Pending" };

        for (int sid = 1; sid <= 50; sid++)
        {
            var studentEntity = students[sid - 1];
            for (int j = 1; j <= 10; j++)
            {
                int cidIndex = ((sid + j - 2) % 20);
                var courseEntity = courses[cidIndex];
                var status = statuses[(sid * 3 + j) % 5];
                var enrollDate = DateTime.Parse("2024-01-01").AddDays(sid * 3 + j);

                enrollments.Add(new Enrollment
                {
                    StudentId = studentEntity.StudentId,
                    CourseId = courseEntity.CourseId,
                    EnrollDate = enrollDate,
                    Status = status
                });
            }
        }
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}
