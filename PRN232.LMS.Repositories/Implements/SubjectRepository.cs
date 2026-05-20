using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implements;

public class SubjectRepository : ISubjectRepository
{
    private readonly LmsDbContext _context;

    public SubjectRepository(LmsDbContext context)
    {
        _context = context;
    }

    public IQueryable<Subject> GetQueryable()
        => _context.Subjects.AsQueryable();

    public async Task<Subject?> GetByIdAsync(int id)
        => await _context.Subjects
            .FirstOrDefaultAsync(s => s.SubjectId == id);

    public async Task AddAsync(Subject entity)
        => await _context.Subjects.AddAsync(entity);

    public void Update(Subject entity)
        => _context.Subjects.Update(entity);

    public void Delete(Subject entity)
        => _context.Subjects.Remove(entity);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
