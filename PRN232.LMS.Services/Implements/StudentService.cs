using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implements;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;
    private readonly IEnrollmentRepository _enrollmentRepo;

    public StudentService(IStudentRepository repo, IEnrollmentRepository enrollmentRepo)
    {
        _repo = repo;
        _enrollmentRepo = enrollmentRepo;
    }

    public async Task<(IEnumerable<StudentModel> Items, int TotalItems)> GetAllAsync(QueryParameters query)
    {
        var queryable = _repo.GetQueryable();

        // Search: fullName, email, or studentCode
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(s =>
                s.FullName.ToLower().Contains(search) ||
                s.Email.ToLower().Contains(search) ||
                s.StudentCode.ToLower().Contains(search));
        }

        // Expand: include enrollments
        var includeEnrollments = query.ExpandList.Contains("enrollments");
        if (includeEnrollments)
        {
            queryable = queryable
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course);
        }

        // Sort
        queryable = ApplySort(queryable, query.Sort);

        // Count before paging
        var total = await queryable.CountAsync();

        // Paging
        var students = await queryable
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        var items = students.Select(s => MapToModel(s, includeEnrollments));
        return (items, total);
    }

    public async Task<StudentDetailModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return null;
        return MapToDetailModel(entity);
    }

    public async Task<(int Id, bool IsDuplicateCode)> CreateAsync(StudentModel model)
    {
        // Normalize StudentCode to uppercase before save
        var normalizedCode = model.StudentCode.Trim().ToUpperInvariant();
        var normalizedEmail = model.Email.Trim().ToLowerInvariant();

        // Check for duplicate StudentCode
        var isDuplicate = await _repo.IsStudentCodeTakenAsync(normalizedCode);
        if (isDuplicate)
            return (0, true);

        var entity = new Student
        {
            StudentCode = normalizedCode,
            FullName = model.FullName,
            Email = normalizedEmail,
            DateOfBirth = model.DateOfBirth,
            Phone = model.Phone
        };
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return (entity.StudentId, false);
    }

    public async Task<(bool Found, bool IsDuplicateCode)> UpdateAsync(int id, StudentModel model)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return (false, false);

        // Normalize StudentCode to uppercase before save
        var normalizedCode = model.StudentCode.Trim().ToUpperInvariant();
        var normalizedEmail = model.Email.Trim().ToLowerInvariant();

        // Check duplicate: exclude the current student (updating their own code is always valid)
        var isDuplicate = await _repo.IsStudentCodeTakenAsync(normalizedCode, excludeStudentId: id);
        if (isDuplicate)
            return (true, true);

        entity.StudentCode = normalizedCode;
        entity.FullName = model.FullName;
        entity.Email = normalizedEmail;
        entity.DateOfBirth = model.DateOfBirth;
        entity.Phone = model.Phone;
        _repo.Update(entity);
        await _repo.SaveChangesAsync();
        return (true, false);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return false;
        _repo.Delete(entity);
        await _repo.SaveChangesAsync();
        return true;
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static StudentModel MapToModel(Student s, bool includeEnrollments) => new()
    {
        StudentId = s.StudentId,
        StudentCode = s.StudentCode,
        FullName = s.FullName,
        Email = s.Email,
        DateOfBirth = s.DateOfBirth,
        Phone = s.Phone,
        Enrollments = includeEnrollments ? s.Enrollments.Select(e => new EnrollmentSummaryModel
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            CourseId = e.CourseId,
            EnrollDate = e.EnrollDate,
            Status = e.Status
        }).ToList() : null
    };

    private static StudentDetailModel MapToDetailModel(Student s) => new()
    {
        StudentId = s.StudentId,
        StudentCode = s.StudentCode,
        FullName = s.FullName,
        Email = s.Email,
        DateOfBirth = s.DateOfBirth,
        Phone = s.Phone,
        Enrollments = s.Enrollments.Select(e => new StudentEnrollmentModel
        {
            EnrollmentId = e.EnrollmentId,
            CourseId = e.CourseId,
            CourseName = e.Course?.CourseName ?? string.Empty,
            EnrollDate = e.EnrollDate,
            Status = e.Status
        }).ToList()
    };

    // ── Sort helper ───────────────────────────────────────────────────────────

    private static IQueryable<Student> ApplySort(IQueryable<Student> q, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return q.OrderBy(s => s.StudentId);

        var fields = sort.Split(',').Select(f => f.Trim()).ToList();
        IOrderedQueryable<Student>? ordered = null;

        foreach (var field in fields)
        {
            var desc = field.StartsWith("-");
            var name = (desc ? field[1..] : field).ToLower();
            if (ordered == null)
            {
                ordered = (desc, name) switch
                {
                    (false, "studentcode") => q.OrderBy(s => s.StudentCode),
                    (true,  "studentcode") => q.OrderByDescending(s => s.StudentCode),
                    (false, "fullname") => q.OrderBy(s => s.FullName),
                    (true,  "fullname") => q.OrderByDescending(s => s.FullName),
                    (false, "email") => q.OrderBy(s => s.Email),
                    (true,  "email") => q.OrderByDescending(s => s.Email),
                    (false, "dateofbirth") => q.OrderBy(s => s.DateOfBirth),
                    (true,  "dateofbirth") => q.OrderByDescending(s => s.DateOfBirth),
                    _ => q.OrderBy(s => s.StudentId)
                };
            }
            else
            {
                ordered = (desc, name) switch
                {
                    (false, "studentcode") => ordered.ThenBy(s => s.StudentCode),
                    (true,  "studentcode") => ordered.ThenByDescending(s => s.StudentCode),
                    (false, "fullname") => ordered.ThenBy(s => s.FullName),
                    (true,  "fullname") => ordered.ThenByDescending(s => s.FullName),
                    (false, "email") => ordered.ThenBy(s => s.Email),
                    (true,  "email") => ordered.ThenByDescending(s => s.Email),
                    (false, "dateofbirth") => ordered.ThenBy(s => s.DateOfBirth),
                    (true,  "dateofbirth") => ordered.ThenByDescending(s => s.DateOfBirth),
                    _ => ordered.ThenBy(s => s.StudentId)
                };
            }
        }

        return ordered ?? q.OrderBy(s => s.StudentId);
    }

    public async Task<(IEnumerable<EnrollmentModel> Items, int TotalItems)?> GetEnrollmentsByStudentIdAsync(int studentId, QueryParameters query)
    {
        var studentExists = await _repo.GetQueryable().AnyAsync(s => s.StudentId == studentId);
        if (!studentExists) return null;

        var queryable = _enrollmentRepo.GetQueryable().Where(e => e.StudentId == studentId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(e => e.Status.ToLower().Contains(search));
        }

        var expand = query.ExpandList;
        var includeStudent = expand.Contains("student");
        var includeCourse = expand.Contains("course");
        if (includeStudent)
            queryable = queryable.Include(e => e.Student);
        if (includeCourse)
            queryable = queryable.Include(e => e.Course).ThenInclude(c => c.Semester);

        queryable = ApplyEnrollmentSort(queryable, query.Sort);

        var total = await queryable.CountAsync();

        var enrollments = await queryable
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        var items = enrollments.Select(e => new EnrollmentModel
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            CourseId = e.CourseId,
            EnrollDate = e.EnrollDate,
            Status = e.Status,
            Student = includeStudent && e.Student != null ? new StudentSummaryModel
            {
                StudentId = e.Student.StudentId,
                StudentCode = e.Student.StudentCode,
                FullName = e.Student.FullName,
                Email = e.Student.Email,
                Phone = e.Student.Phone
            } : null,
            Course = includeCourse && e.Course != null ? new CourseSummaryModel
            {
                CourseId = e.Course.CourseId,
                CourseName = e.Course.CourseName
            } : null
        });

        return (items, total);
    }

    private static IQueryable<Enrollment> ApplyEnrollmentSort(IQueryable<Enrollment> q, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return q.OrderBy(e => e.EnrollmentId);

        var fields = sort.Split(',').Select(f => f.Trim()).ToList();
        IOrderedQueryable<Enrollment>? ordered = null;

        foreach (var field in fields)
        {
            var desc = field.StartsWith("-");
            var name = (desc ? field[1..] : field).ToLower();
            if (ordered == null)
            {
                ordered = (desc, name) switch
                {
                    (false, "enrolldate") => q.OrderBy(e => e.EnrollDate),
                    (true,  "enrolldate") => q.OrderByDescending(e => e.EnrollDate),
                    (false, "status") => q.OrderBy(e => e.Status),
                    (true,  "status") => q.OrderByDescending(e => e.Status),
                    _ => q.OrderBy(e => e.EnrollmentId)
                };
            }
            else
            {
                ordered = (desc, name) switch
                {
                    (false, "enrolldate") => ordered.ThenBy(e => e.EnrollDate),
                    (true,  "enrolldate") => ordered.ThenByDescending(e => e.EnrollDate),
                    (false, "status") => ordered.ThenBy(e => e.Status),
                    (true,  "status") => ordered.ThenByDescending(e => e.Status),
                    _ => ordered.ThenBy(e => e.EnrollmentId)
                };
            }
        }

        return ordered ?? q.OrderBy(e => e.EnrollmentId);
    }
}
