using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    IQueryable<Enrollment> GetQueryable();
    Task<Enrollment?> GetByIdAsync(int id);
    Task AddAsync(Enrollment entity);
    void Update(Enrollment entity);
    void Delete(Enrollment entity);
    Task<int> SaveChangesAsync();
}
