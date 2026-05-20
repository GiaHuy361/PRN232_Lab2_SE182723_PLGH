using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ISubjectRepository
{
    IQueryable<Subject> GetQueryable();
    Task<Subject?> GetByIdAsync(int id);
    Task AddAsync(Subject entity);
    void Update(Subject entity);
    void Delete(Subject entity);
    Task<int> SaveChangesAsync();
}
