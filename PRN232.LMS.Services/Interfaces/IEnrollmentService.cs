using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<(IEnumerable<EnrollmentModel> Items, int TotalItems)> GetAllAsync(QueryParameters query);
    Task<EnrollmentDetailModel?> GetByIdAsync(int id);
    Task<int> CreateAsync(EnrollmentModel model);
    Task<bool> UpdateAsync(int id, EnrollmentModel model);
    Task<bool> DeleteAsync(int id);
}
