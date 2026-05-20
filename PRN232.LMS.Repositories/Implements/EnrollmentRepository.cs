using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implements;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly LmsDbContext _context;

    public EnrollmentRepository(LmsDbContext context)
    {
        _context = context;
    }

    public IQueryable<Enrollment> GetQueryable()
        => _context.Enrollments.AsQueryable();

    public async Task<Enrollment?> GetByIdAsync(int id)
        => await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
                .ThenInclude(c => c.Semester)
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);

    public async Task AddAsync(Enrollment entity)
        => await _context.Enrollments.AddAsync(entity);

    public void Update(Enrollment entity)
        => _context.Enrollments.Update(entity);

    public void Delete(Enrollment entity)
        => _context.Enrollments.Remove(entity);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
