using FluentValidation;
using FluentValidation.Validators;
using ModelDto.UserDto;

namespace Services.Validators.UserValidator
{
    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator()
        {
            RuleFor(user => user.Name)
                .NotEmpty().WithMessage("Name is required")
                .NotNull().WithMessage("Name is required")
                .MaximumLength(15).WithMessage("Name is to long");

            RuleFor(user => user.Surname)
                .NotEmpty().WithMessage("Surname is required")
                .NotNull().WithMessage("Surname is required")
                .MaximumLength(15).WithMessage("Surname is to long");

            RuleFor(user => user.Username)
                .NotEmpty().WithMessage("Username is required")
                .NotNull().WithMessage("Username is required")
                .MaximumLength(15).WithMessage("Username is to long");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password is required")
                .NotNull().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password is to short");

            RuleFor(user => user.Email)
                .EmailAddress(EmailValidationMode.Net4xRegex)
                .MaximumLength(120).WithMessage("Email address is to long");
        }
    }
}
