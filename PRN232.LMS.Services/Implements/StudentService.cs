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

    public StudentService(IStudentRepository repo)
    {
        _repo = repo;
    }

    public async Task<(IEnumerable<StudentModel> Items, int TotalItems)> GetAllAsync(QueryParameters query)
    {
        var queryable = _repo.GetQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(s =>
                s.FullName.ToLower().Contains(search) ||
                s.Email.ToLower().Contains(search));
        }

        // Expand: include enrollments
        if (query.ExpandList.Contains("enrollments"))
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

        var items = students.Select(s => MapToModel(s));
        return (items, total);
    }

    public async Task<StudentDetailModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return null;
        return MapToDetailModel(entity);
    }

    public async Task<int> CreateAsync(StudentModel model)
    {
        var entity = new Student
        {
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth
        };
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return entity.StudentId;
    }

    public async Task<bool> UpdateAsync(int id, StudentModel model)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return false;
        entity.FullName = model.FullName;
        entity.Email = model.Email;
        entity.DateOfBirth = model.DateOfBirth;
        _repo.Update(entity);
        await _repo.SaveChangesAsync();
        return true;
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

    private static StudentModel MapToModel(Student s) => new()
    {
        StudentId = s.StudentId,
        FullName = s.FullName,
        Email = s.Email,
        DateOfBirth = s.DateOfBirth
    };

    private static StudentDetailModel MapToDetailModel(Student s) => new()
    {
        StudentId = s.StudentId,
        FullName = s.FullName,
        Email = s.Email,
        DateOfBirth = s.DateOfBirth,
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
                    (false, "fullname") => q.OrderBy(s => s.FullName),
                    (true, "fullname") => q.OrderByDescending(s => s.FullName),
                    (false, "email") => q.OrderBy(s => s.Email),
                    (true, "email") => q.OrderByDescending(s => s.Email),
                    (false, "dateofbirth") => q.OrderBy(s => s.DateOfBirth),
                    (true, "dateofbirth") => q.OrderByDescending(s => s.DateOfBirth),
                    _ => q.OrderBy(s => s.StudentId)
                };
            }
            else
            {
                ordered = (desc, name) switch
                {
                    (false, "fullname") => ordered.ThenBy(s => s.FullName),
                    (true, "fullname") => ordered.ThenByDescending(s => s.FullName),
                    (false, "email") => ordered.ThenBy(s => s.Email),
                    (true, "email") => ordered.ThenByDescending(s => s.Email),
                    (false, "dateofbirth") => ordered.ThenBy(s => s.DateOfBirth),
                    (true, "dateofbirth") => ordered.ThenByDescending(s => s.DateOfBirth),
                    _ => ordered.ThenBy(s => s.StudentId)
                };
            }
        }

        return ordered ?? q.OrderBy(s => s.StudentId);
    }
}
