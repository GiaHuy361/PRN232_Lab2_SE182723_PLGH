using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CreateSubjectRequest
{
    [Required(ErrorMessage = "SubjectCode is required.")]
    [StringLength(20, ErrorMessage = "SubjectCode must not exceed 20 characters.")]
    [RegularExpression("^[a-zA-Z]{3}[0-9]{3}$", ErrorMessage = "SubjectCode must follow the format of 3 letters followed by 3 digits (e.g., PRN232).")]
    public string SubjectCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "SubjectName is required.")]
    [StringLength(100, ErrorMessage = "SubjectName must not exceed 100 characters.")]
    public string SubjectName { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Credit must be between 1 and 10.")]
    public int Credit { get; set; }
}

public class UpdateSubjectRequest
{
    [Required(ErrorMessage = "SubjectCode is required.")]
    [StringLength(20, ErrorMessage = "SubjectCode must not exceed 20 characters.")]
    [RegularExpression("^[a-zA-Z]{3}[0-9]{3}$", ErrorMessage = "SubjectCode must follow the format of 3 letters followed by 3 digits (e.g., PRN232).")]
    public string SubjectCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "SubjectName is required.")]
    [StringLength(100, ErrorMessage = "SubjectName must not exceed 100 characters.")]
    public string SubjectName { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Credit must be between 1 and 10.")]
    public int Credit { get; set; }
}
