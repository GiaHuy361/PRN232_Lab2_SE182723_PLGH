using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implements;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repo;
    private readonly IEnrollmentRepository _enrollmentRepo;

    public CourseService(ICourseRepository repo, IEnrollmentRepository enrollmentRepo)
    {
        _repo = repo;
        _enrollmentRepo = enrollmentRepo;
    }

    public async Task<(IEnumerable<CourseModel> Items, int TotalItems)> GetAllAsync(QueryParameters query)
    {
        var queryable = _repo.GetQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(c => c.CourseName.ToLower().Contains(search));
        }

        var expand = query.ExpandList;
        var includeSemester = expand.Contains("semester");
        var includeEnrollments = expand.Contains("enrollments");
        if (includeSemester)
            queryable = queryable.Include(c => c.Semester);
        if (includeEnrollments)
            queryable = queryable.Include(c => c.Enrollments).ThenInclude(e => e.Student);

        queryable = ApplySort(queryable, query.Sort);

        var total = await queryable.CountAsync();

        var courses = await queryable
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        return (courses.Select(c => MapToModel(c, includeSemester, includeEnrollments)), total);
    }

    public async Task<CourseDetailModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return null;
        return MapToDetailModel(entity);
    }

    public async Task<int> CreateAsync(CourseModel model)
    {
        var entity = new Course { CourseName = model.CourseName, SemesterId = model.SemesterId };
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return entity.CourseId;
    }

    public async Task<bool> UpdateAsync(int id, CourseModel model)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return false;
        entity.CourseName = model.CourseName;
        entity.SemesterId = model.SemesterId;
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

    private static CourseModel MapToModel(Course c, bool includeSemester, bool includeEnrollments) => new()
    {
        CourseId = c.CourseId,
        CourseName = c.CourseName,
        SemesterId = c.SemesterId,
        Semester = includeSemester && c.Semester != null ? new SemesterSummaryModel
        {
            SemesterId = c.Semester.SemesterId,
            SemesterName = c.Semester.SemesterName
        } : null,
        Enrollments = includeEnrollments ? c.Enrollments.Select(e => new EnrollmentSummaryModel
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            CourseId = e.CourseId,
            EnrollDate = e.EnrollDate,
            Status = e.Status
        }).ToList() : null
    };

    private static CourseDetailModel MapToDetailModel(Course c) => new()
    {
        CourseId = c.CourseId,
        CourseName = c.CourseName,
        Semester = c.Semester == null ? null : new SemesterSummaryModel
        {
            SemesterId = c.Semester.SemesterId,
            SemesterName = c.Semester.SemesterName
        },
        Enrollments = c.Enrollments.Select(e => new CourseEnrollmentModel
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            StudentName = e.Student?.FullName ?? string.Empty,
            EnrollDate = e.EnrollDate,
            Status = e.Status
        }).ToList()
    };

    private static IQueryable<Course> ApplySort(IQueryable<Course> q, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return q.OrderBy(c => c.CourseId);
        var fields = sort.Split(',').Select(f => f.Trim()).ToList();
        IOrderedQueryable<Course>? ordered = null;
        foreach (var field in fields)
        {
            var desc = field.StartsWith("-");
            var name = (desc ? field[1..] : field).ToLower();
            if (ordered == null)
                ordered = (desc, name) switch
                {
                    (false, "coursename") => q.OrderBy(c => c.CourseName),
                    (true, "coursename") => q.OrderByDescending(c => c.CourseName),
                    _ => q.OrderBy(c => c.CourseId)
                };
            else
                ordered = (desc, name) switch
                {
                    (false, "coursename") => ordered.ThenBy(c => c.CourseName),
                    (true, "coursename") => ordered.ThenByDescending(c => c.CourseName),
                    _ => ordered.ThenBy(c => c.CourseId)
                };
        }
        return ordered ?? q.OrderBy(c => c.CourseId);
    }

    public async Task<(IEnumerable<EnrollmentModel> Items, int TotalItems)?> GetEnrollmentsByCourseIdAsync(int courseId, QueryParameters query)
    {
        var courseExists = await _repo.GetQueryable().AnyAsync(c => c.CourseId == courseId);
        if (!courseExists) return null;

        var queryable = _enrollmentRepo.GetQueryable().Where(e => e.CourseId == courseId);

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
                FullName = e.Student.FullName,
                Email = e.Student.Email
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
