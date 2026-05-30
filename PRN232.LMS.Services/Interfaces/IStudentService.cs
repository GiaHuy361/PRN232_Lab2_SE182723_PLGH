using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<(IEnumerable<StudentModel> Items, int TotalItems)> GetAllAsync(QueryParameters query);
    Task<StudentDetailModel?> GetByIdAsync(int id);
    /// <summary>Returns (newId, isDuplicateCode). isDuplicateCode=true means StudentCode is already taken.</summary>
    Task<(int Id, bool IsDuplicateCode)> CreateAsync(StudentModel model);
    /// <summary>Returns (found, isDuplicateCode). found=false means student not found; isDuplicateCode=true means code conflict.</summary>
    Task<(bool Found, bool IsDuplicateCode)> UpdateAsync(int id, StudentModel model);
    Task<bool> DeleteAsync(int id);
    Task<(IEnumerable<EnrollmentModel> Items, int TotalItems)?> GetEnrollmentsByStudentIdAsync(int studentId, QueryParameters query);
}
