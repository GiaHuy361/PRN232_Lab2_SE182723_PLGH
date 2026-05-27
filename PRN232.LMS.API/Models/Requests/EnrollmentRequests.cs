using System;
using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CreateEnrollmentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be greater than or equal to 1.")]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be greater than or equal to 1.")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "EnrollDate is required.")]
    public DateTime EnrollDate { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [StringLength(20, ErrorMessage = "Status must not exceed 20 characters.")]
    [RegularExpression("^(Active|Inactive|Completed|Dropped|Pending)$", ErrorMessage = "Status must be Active, Inactive, Completed, Dropped, or Pending.")]
    public string Status { get; set; } = string.Empty;
}

public class UpdateEnrollmentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be greater than or equal to 1.")]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be greater than or equal to 1.")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "EnrollDate is required.")]
    public DateTime EnrollDate { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [StringLength(20, ErrorMessage = "Status must not exceed 20 characters.")]
    [RegularExpression("^(Active|Inactive|Completed|Dropped|Pending)$", ErrorMessage = "Status must be Active, Inactive, Completed, Dropped, or Pending.")]
    public string Status { get; set; } = string.Empty;
}
