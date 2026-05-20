using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<(IEnumerable<SubjectModel> Items, int TotalItems)> GetAllAsync(QueryParameters query);
    Task<SubjectDetailModel?> GetByIdAsync(int id);
    Task<int> CreateAsync(SubjectModel model);
    Task<bool> UpdateAsync(int id, SubjectModel model);
    Task<bool> DeleteAsync(int id);
}
