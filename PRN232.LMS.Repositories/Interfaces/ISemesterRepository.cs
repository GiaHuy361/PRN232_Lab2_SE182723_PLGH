using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ISemesterRepository
{
    IQueryable<Semester> GetQueryable();
    Task<Semester?> GetByIdAsync(int id);
    Task AddAsync(Semester entity);
    void Update(Semester entity);
    void Delete(Semester entity);
    Task<int> SaveChangesAsync();
}
