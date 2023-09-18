using FluentValidation;
using FluentValidation.Validators;
using ModelDto.CommentDto;

namespace Services.Validators.CommentValidator
{
    public class CommentCreateDtoValidator : AbstractValidator<CommentCreateDto>
    {
        public CommentCreateDtoValidator()
        {
            RuleFor(comment => comment.Username)
                .NotEmpty().WithMessage("Username is required")
                .NotNull().WithMessage("Username is required")
                .MaximumLength(15).WithMessage("Username is to long");

            RuleFor(comment => comment.Content)
                .NotEmpty().WithMessage("Content is required")
                .NotNull().WithMessage("Content is required");

            RuleFor(comment => comment.Email)
                    .EmailAddress(EmailValidationMode.Net4xRegex)
                    .MaximumLength(120).WithMessage("Email address is to long");
        }
    }
}
