using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implements;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repo;

    public EnrollmentService(IEnrollmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<(IEnumerable<EnrollmentModel> Items, int TotalItems)> GetAllAsync(QueryParameters query)
    {
        var queryable = _repo.GetQueryable();

        // Search by Status
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(e =>
                e.Status.ToLower().Contains(search));
        }

        // Expand
        var expand = query.ExpandList;
        if (expand.Contains("student"))
            queryable = queryable.Include(e => e.Student);
        if (expand.Contains("course"))
            queryable = queryable.Include(e => e.Course).ThenInclude(c => c.Semester);

        // Sort
        queryable = ApplySort(queryable, query.Sort);

        var total = await queryable.CountAsync();

        var enrollments = await queryable
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        var items = enrollments.Select(e => MapToModel(e));
        return (items, total);
    }

    public async Task<EnrollmentDetailModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return null;
        return MapToDetailModel(entity);
    }

    public async Task<int> CreateAsync(EnrollmentModel model)
    {
        var entity = new Enrollment
        {
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status
        };
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return entity.EnrollmentId;
    }

    public async Task<bool> UpdateAsync(int id, EnrollmentModel model)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return false;
        entity.StudentId = model.StudentId;
        entity.CourseId = model.CourseId;
        entity.EnrollDate = model.EnrollDate;
        entity.Status = model.Status;
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

    private static EnrollmentModel MapToModel(Enrollment e) => new()
    {
        EnrollmentId = e.EnrollmentId,
        StudentId = e.StudentId,
        CourseId = e.CourseId,
        EnrollDate = e.EnrollDate,
        Status = e.Status
    };

    private static EnrollmentDetailModel MapToDetailModel(Enrollment e) => new()
    {
        EnrollmentId = e.EnrollmentId,
        EnrollDate = e.EnrollDate,
        Status = e.Status,
        Student = e.Student == null ? null : new StudentSummaryModel
        {
            StudentId = e.Student.StudentId,
            FullName = e.Student.FullName,
            Email = e.Student.Email
        },
        Course = e.Course == null ? null : new CourseSummaryModel
        {
            CourseId = e.Course.CourseId,
            CourseName = e.Course.CourseName
        }
    };

    // ── Sort helper ───────────────────────────────────────────────────────────

    private static IQueryable<Enrollment> ApplySort(IQueryable<Enrollment> q, string? sort)
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
                    (true, "enrolldate") => q.OrderByDescending(e => e.EnrollDate),
                    (false, "status") => q.OrderBy(e => e.Status),
                    (true, "status") => q.OrderByDescending(e => e.Status),
                    _ => q.OrderBy(e => e.EnrollmentId)
                };
            }
            else
            {
                ordered = (desc, name) switch
                {
                    (false, "enrolldate") => ordered.ThenBy(e => e.EnrollDate),
                    (true, "enrolldate") => ordered.ThenByDescending(e => e.EnrollDate),
                    (false, "status") => ordered.ThenBy(e => e.Status),
                    (true, "status") => ordered.ThenByDescending(e => e.Status),
                    _ => ordered.ThenBy(e => e.EnrollmentId)
                };
            }
        }

        return ordered ?? q.OrderBy(e => e.EnrollmentId);
    }
}
