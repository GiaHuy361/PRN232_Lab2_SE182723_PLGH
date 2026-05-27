using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CreateCourseRequest
{
    [Required(ErrorMessage = "CourseName is required.")]
    [StringLength(100, ErrorMessage = "CourseName must not exceed 100 characters.")]
    public string CourseName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "SemesterId must be greater than or equal to 1.")]
    public int SemesterId { get; set; }
}

public class UpdateCourseRequest
{
    [Required(ErrorMessage = "CourseName is required.")]
    [StringLength(100, ErrorMessage = "CourseName must not exceed 100 characters.")]
    public string CourseName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "SemesterId must be greater than or equal to 1.")]
    public int SemesterId { get; set; }
}
