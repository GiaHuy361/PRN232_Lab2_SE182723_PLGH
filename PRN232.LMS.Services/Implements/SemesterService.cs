using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implements;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _repo;

    public SemesterService(ISemesterRepository repo)
    {
        _repo = repo;
    }

    public async Task<(IEnumerable<SemesterModel> Items, int TotalItems)> GetAllAsync(QueryParameters query)
    {
        var queryable = _repo.GetQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(s => s.SemesterName.ToLower().Contains(search));
        }

        var includeCourses = query.ExpandList.Contains("courses");
        if (includeCourses)
            queryable = queryable.Include(s => s.Courses);

        queryable = ApplySort(queryable, query.Sort);

        var total = await queryable.CountAsync();

        var semesters = await queryable
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        return (semesters.Select(s => MapToModel(s, includeCourses)), total);
    }

    public async Task<SemesterDetailModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return null;
        return MapToDetailModel(entity);
    }

    public async Task<int> CreateAsync(SemesterModel model)
    {
        var entity = new Semester
        {
            SemesterName = model.SemesterName,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return entity.SemesterId;
    }

    public async Task<bool> UpdateAsync(int id, SemesterModel model)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return false;
        entity.SemesterName = model.SemesterName;
        entity.StartDate = model.StartDate;
        entity.EndDate = model.EndDate;
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

    private static SemesterModel MapToModel(Semester s, bool includeCourses) => new()
    {
        SemesterId = s.SemesterId,
        SemesterName = s.SemesterName,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        Courses = includeCourses ? s.Courses.Select(c => new CourseSummaryModel
        {
            CourseId = c.CourseId,
            CourseName = c.CourseName
        }).ToList() : null
    };

    private static SemesterDetailModel MapToDetailModel(Semester s) => new()
    {
        SemesterId = s.SemesterId,
        SemesterName = s.SemesterName,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        Courses = s.Courses.Select(c => new CourseSummaryModel
        {
            CourseId = c.CourseId,
            CourseName = c.CourseName
        }).ToList()
    };

    private static IQueryable<Semester> ApplySort(IQueryable<Semester> q, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return q.OrderBy(s => s.SemesterId);
        var fields = sort.Split(',').Select(f => f.Trim()).ToList();
        IOrderedQueryable<Semester>? ordered = null;
        foreach (var field in fields)
        {
            var desc = field.StartsWith("-");
            var name = (desc ? field[1..] : field).ToLower();
            if (ordered == null)
                ordered = (desc, name) switch
                {
                    (false, "semestername") => q.OrderBy(s => s.SemesterName),
                    (true, "semestername") => q.OrderByDescending(s => s.SemesterName),
                    (false, "startdate") => q.OrderBy(s => s.StartDate),
                    (true, "startdate") => q.OrderByDescending(s => s.StartDate),
                    (false, "enddate") => q.OrderBy(s => s.EndDate),
                    (true, "enddate") => q.OrderByDescending(s => s.EndDate),
                    _ => q.OrderBy(s => s.SemesterId)
                };
            else
                ordered = (desc, name) switch
                {
                    (false, "semestername") => ordered.ThenBy(s => s.SemesterName),
                    (true, "semestername") => ordered.ThenByDescending(s => s.SemesterName),
                    (false, "startdate") => ordered.ThenBy(s => s.StartDate),
                    (true, "startdate") => ordered.ThenByDescending(s => s.StartDate),
                    (false, "enddate") => ordered.ThenBy(s => s.EndDate),
                    (true, "enddate") => ordered.ThenByDescending(s => s.EndDate),
                    _ => ordered.ThenBy(s => s.SemesterId)
                };
        }
        return ordered ?? q.OrderBy(s => s.SemesterId);
    }
}
