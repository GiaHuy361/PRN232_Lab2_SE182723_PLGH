using FluentValidation;
using PRN232.LMS.API.Models.Requests;

namespace PRN232.LMS.API.Validators;

// Email domain validation removed - [Required] + [EmailAddress] on the request model is sufficient.
// StudentCode format (FptStudentCode attribute) is still enforced on the request model.

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator() { }
}

public class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator() { }
}
