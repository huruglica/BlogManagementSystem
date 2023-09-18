using FluentValidation;
using FluentValidation.Validators;
using ModelDto.UserDto;

namespace Services.Validators.UserValidator
{
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateDtoValidator()
        {
            RuleFor(user => user.Email)
                .EmailAddress(EmailValidationMode.Net4xRegex)
                .NotEmpty().WithMessage("You must set an email address")
                .NotNull().WithMessage("You must set an email address")
                .MaximumLength(50).WithMessage("Email address is to long");
        }
    }
}
