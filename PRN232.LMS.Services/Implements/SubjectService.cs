using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implements;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repo;

    public SubjectService(ISubjectRepository repo)
    {
        _repo = repo;
    }

    public async Task<(IEnumerable<SubjectModel> Items, int TotalItems)> GetAllAsync(QueryParameters query)
    {
        var queryable = _repo.GetQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(s =>
                s.SubjectCode.ToLower().Contains(search) ||
                s.SubjectName.ToLower().Contains(search));
        }

        queryable = ApplySort(queryable, query.Sort);

        var total = await queryable.CountAsync();

        var subjects = await queryable
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        return (subjects.Select(MapToModel), total);
    }

    public async Task<SubjectDetailModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return null;
        return MapToDetailModel(entity);
    }

    public async Task<int> CreateAsync(SubjectModel model)
    {
        var entity = new Subject
        {
            SubjectCode = model.SubjectCode,
            SubjectName = model.SubjectName,
            Credit = model.Credit
        };
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return entity.SubjectId;
    }

    public async Task<bool> UpdateAsync(int id, SubjectModel model)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return false;
        entity.SubjectCode = model.SubjectCode;
        entity.SubjectName = model.SubjectName;
        entity.Credit = model.Credit;
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

    private static SubjectModel MapToModel(Subject s) => new()
    {
        SubjectId = s.SubjectId,
        SubjectCode = s.SubjectCode,
        SubjectName = s.SubjectName,
        Credit = s.Credit
    };

    private static SubjectDetailModel MapToDetailModel(Subject s) => new()
    {
        SubjectId = s.SubjectId,
        SubjectCode = s.SubjectCode,
        SubjectName = s.SubjectName,
        Credit = s.Credit
    };

    private static IQueryable<Subject> ApplySort(IQueryable<Subject> q, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return q.OrderBy(s => s.SubjectId);
        var fields = sort.Split(',').Select(f => f.Trim()).ToList();
        IOrderedQueryable<Subject>? ordered = null;
        foreach (var field in fields)
        {
            var desc = field.StartsWith("-");
            var name = (desc ? field[1..] : field).ToLower();
            if (ordered == null)
                ordered = (desc, name) switch
                {
                    (false, "subjectcode") => q.OrderBy(s => s.SubjectCode),
                    (true, "subjectcode") => q.OrderByDescending(s => s.SubjectCode),
                    (false, "subjectname") => q.OrderBy(s => s.SubjectName),
                    (true, "subjectname") => q.OrderByDescending(s => s.SubjectName),
                    (false, "credit") => q.OrderBy(s => s.Credit),
                    (true, "credit") => q.OrderByDescending(s => s.Credit),
                    _ => q.OrderBy(s => s.SubjectId)
                };
            else
                ordered = (desc, name) switch
                {
                    (false, "subjectcode") => ordered.ThenBy(s => s.SubjectCode),
                    (true, "subjectcode") => ordered.ThenByDescending(s => s.SubjectCode),
                    (false, "subjectname") => ordered.ThenBy(s => s.SubjectName),
                    (true, "subjectname") => ordered.ThenByDescending(s => s.SubjectName),
                    (false, "credit") => ordered.ThenBy(s => s.Credit),
                    (true, "credit") => ordered.ThenByDescending(s => s.Credit),
                    _ => ordered.ThenBy(s => s.SubjectId)
                };
        }
        return ordered ?? q.OrderBy(s => s.SubjectId);
    }
}
