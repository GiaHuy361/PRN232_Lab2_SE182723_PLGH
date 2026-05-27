using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<(IEnumerable<CourseModel> Items, int TotalItems)> GetAllAsync(QueryParameters query);
    Task<CourseDetailModel?> GetByIdAsync(int id);
    Task<int> CreateAsync(CourseModel model);
    Task<bool> UpdateAsync(int id, CourseModel model);
    Task<bool> DeleteAsync(int id);
    Task<(IEnumerable<EnrollmentModel> Items, int TotalItems)?> GetEnrollmentsByCourseIdAsync(int courseId, QueryParameters query);
    Task<(IEnumerable<StudentModel> Items, int TotalItems)?> GetStudentsByCourseIdAsync(int courseId, QueryParameters query);
}
