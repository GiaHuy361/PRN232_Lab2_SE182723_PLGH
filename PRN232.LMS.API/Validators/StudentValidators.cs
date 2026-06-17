using FluentValidation;
using PRN232.LMS.API.Models.Requests;

namespace PRN232.LMS.API.Validators;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MinimumLength(2).WithMessage("FullName must be at least 2 characters.")
            .MaximumLength(100).WithMessage("FullName must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s]+$").WithMessage("FullName must contain only letters and spaces.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address format.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("DateOfBirth is required.")
            .LessThan(DateTime.Today).WithMessage("Date of birth cannot be in the future.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

public class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required.")
            .MinimumLength(2).WithMessage("FullName must be at least 2 characters.")
            .MaximumLength(100).WithMessage("FullName must not exceed 100 characters.")
            .Matches(@"^[\p{L}\s]+$").WithMessage("FullName must contain only letters and spaces.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address format.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("DateOfBirth is required.")
            .LessThan(DateTime.Today).WithMessage("Date of birth cannot be in the future.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
