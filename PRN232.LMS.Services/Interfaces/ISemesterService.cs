using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<(IEnumerable<SemesterModel> Items, int TotalItems)> GetAllAsync(QueryParameters query);
    Task<SemesterDetailModel?> GetByIdAsync(int id);
    Task<int> CreateAsync(SemesterModel model);
    Task<bool> UpdateAsync(int id, SemesterModel model);
    Task<bool> DeleteAsync(int id);
}
