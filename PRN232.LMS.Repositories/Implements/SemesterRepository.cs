using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implements;

public class SemesterRepository : ISemesterRepository
{
    private readonly LmsDbContext _context;

    public SemesterRepository(LmsDbContext context)
    {
        _context = context;
    }

    public IQueryable<Semester> GetQueryable()
        => _context.Semesters.AsQueryable();

    public async Task<Semester?> GetByIdAsync(int id)
        => await _context.Semesters
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.SemesterId == id);

    public async Task AddAsync(Semester entity)
        => await _context.Semesters.AddAsync(entity);

    public void Update(Semester entity)
        => _context.Semesters.Update(entity);

    public void Delete(Semester entity)
        => _context.Semesters.Remove(entity);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
