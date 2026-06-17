using System.ComponentModel.DataAnnotations;
using PRN232.LMS.API.Infrastructure;

namespace PRN232.LMS.API.Models.Requests;

public class CreateStudentRequest
{
    [Required(ErrorMessage = "StudentCode is required.")]
    [FptStudentCode(ErrorMessage = "Student code must contain 2 letters followed by 5 digits (e.g. SE19886).")]
    public string StudentCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "FullName is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "FullName must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "DateOfBirth is required.")]
    [PastOrPresentDate(ErrorMessage = "Date of birth cannot be in the future.")]
    public DateTime DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters.")]
    public string? Phone { get; set; }
}

public class UpdateStudentRequest
{
    [Required(ErrorMessage = "StudentCode is required.")]
    [FptStudentCode(ErrorMessage = "Student code must contain 2 letters followed by 5 digits (e.g. SE19886).")]
    public string StudentCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "FullName is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "FullName must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "DateOfBirth is required.")]
    [PastOrPresentDate(ErrorMessage = "Date of birth cannot be in the future.")]
    public DateTime DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters.")]
    public string? Phone { get; set; }
}
