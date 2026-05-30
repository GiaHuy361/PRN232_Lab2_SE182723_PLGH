using FluentValidation;
using PRN232.LMS.API.Models.Requests;

namespace PRN232.LMS.API.Validators;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.Email)
            .Must(email => string.IsNullOrWhiteSpace(email) || email.EndsWith("@fpt.edu.vn", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Email must use the @fpt.edu.vn domain.");
    }
}

public class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.Email)
            .Must(email => string.IsNullOrWhiteSpace(email) || email.EndsWith("@fpt.edu.vn", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Email must use the @fpt.edu.vn domain.");
    }
}
