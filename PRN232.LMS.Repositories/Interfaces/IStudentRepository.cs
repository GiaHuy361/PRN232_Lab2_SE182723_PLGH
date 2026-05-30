using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IStudentRepository
{
    IQueryable<Student> GetQueryable();
    Task<Student?> GetByIdAsync(int id);
    Task<bool> IsStudentCodeTakenAsync(string studentCode, int? excludeStudentId = null);
    Task AddAsync(Student entity);
    void Update(Student entity);
    void Delete(Student entity);
    Task<int> SaveChangesAsync();
}
