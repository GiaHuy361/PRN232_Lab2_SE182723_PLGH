using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implements;

public class CourseRepository : ICourseRepository
{
    private readonly LmsDbContext _context;

    public CourseRepository(LmsDbContext context)
    {
        _context = context;
    }

    public IQueryable<Course> GetQueryable()
        => _context.Courses.AsQueryable();

    public async Task<Course?> GetByIdAsync(int id)
        => await _context.Courses
            .Include(c => c.Semester)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.CourseId == id);

    public async Task AddAsync(Course entity)
        => await _context.Courses.AddAsync(entity);

    public void Update(Course entity)
        => _context.Courses.Update(entity);

    public void Delete(Course entity)
        => _context.Courses.Remove(entity);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
