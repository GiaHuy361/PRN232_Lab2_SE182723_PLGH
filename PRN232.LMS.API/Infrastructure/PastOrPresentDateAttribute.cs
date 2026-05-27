using System;
using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Infrastructure;

public class PastOrPresentDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateTime)
        {
            if (dateTime > DateTime.UtcNow)
            {
                return new ValidationResult(ErrorMessage ?? "Date of birth cannot be in the future.");
            }
        }
        return ValidationResult.Success;
    }
}
