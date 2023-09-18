using FluentValidation;
using ModelDto.CommentDto;

namespace Services.Validators.CommentValidator
{
    public class CommentUpdateDtoValidator : AbstractValidator<CommentUpdateDto>
    {
        public CommentUpdateDtoValidator()
        {
            RuleFor(comment => comment.Content)
                .NotEmpty().WithMessage("Content is required")
                .NotNull().WithMessage("Content is required");
        }
    }
}
