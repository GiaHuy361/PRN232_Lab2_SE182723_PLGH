using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PRN232.LMS.API.Infrastructure;

/// <summary>
/// Custom validation attribute that enforces the FPTU student code format:
/// exactly 2 letters (case-insensitive) followed by exactly 6 digits.
/// Examples of valid codes: SE182723, se182723, HE180001, ce230099
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class FptStudentCodeAttribute : ValidationAttribute
{
    // 2 letters (upper or lower) + exactly 6 digits
    private static readonly Regex _pattern = new(@"^[A-Za-z]{2}\d{6}$", RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // If null or empty, let [Required] handle it
        if (value is not string code || string.IsNullOrWhiteSpace(code))
            return ValidationResult.Success;

        if (!_pattern.IsMatch(code.Trim()))
        {
            return new ValidationResult(
                ErrorMessage ?? "Student code must contain 2 letters followed by 6 digits.");
        }

        return ValidationResult.Success;
    }
}
