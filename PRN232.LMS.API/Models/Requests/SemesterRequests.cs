using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CreateSemesterRequest
{
    [Required(ErrorMessage = "SemesterName is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "SemesterName must be between 2 and 50 characters.")]
    [RegularExpression(@"^[A-Za-z0-9\s\-_]+$", ErrorMessage = "SemesterName contains invalid characters.")]
    public string SemesterName { get; set; } = string.Empty;

    [Required(ErrorMessage = "StartDate is required.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "EndDate is required.")]
    public DateTime EndDate { get; set; }
}

public class UpdateSemesterRequest
{
    [Required(ErrorMessage = "SemesterName is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "SemesterName must be between 2 and 50 characters.")]
    [RegularExpression(@"^[A-Za-z0-9\s\-_]+$", ErrorMessage = "SemesterName contains invalid characters.")]
    public string SemesterName { get; set; } = string.Empty;

    [Required(ErrorMessage = "StartDate is required.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "EndDate is required.")]
    public DateTime EndDate { get; set; }
}
