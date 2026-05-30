using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implements;

public class StudentRepository : IStudentRepository
{
    private readonly LmsDbContext _context;

    public StudentRepository(LmsDbContext context)
    {
        _context = context;
    }

    public IQueryable<Student> GetQueryable()
        => _context.Students.AsQueryable();

    public async Task<Student?> GetByIdAsync(int id)
        => await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.StudentId == id);

    public async Task<bool> IsStudentCodeTakenAsync(string studentCode, int? excludeStudentId = null)
        => await _context.Students.AnyAsync(s =>
            s.StudentCode == studentCode &&
            (excludeStudentId == null || s.StudentId != excludeStudentId.Value));

    public async Task AddAsync(Student entity)
        => await _context.Students.AddAsync(entity);

    public void Update(Student entity)
        => _context.Students.Update(entity);

    public void Delete(Student entity)
        => _context.Students.Remove(entity);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
