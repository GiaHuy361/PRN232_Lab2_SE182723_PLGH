using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<(IEnumerable<StudentModel> Items, int TotalItems)> GetAllAsync(QueryParameters query);
    Task<StudentDetailModel?> GetByIdAsync(int id);
    Task<int> CreateAsync(StudentModel model);
    Task<bool> UpdateAsync(int id, StudentModel model);
    Task<bool> DeleteAsync(int id);
    Task<(IEnumerable<EnrollmentModel> Items, int TotalItems)?> GetEnrollmentsByStudentIdAsync(int studentId, QueryParameters query);
}
