using FluentValidation;
using ModelDto.BlogDto;
using Microsoft.IdentityModel.Tokens;

namespace Services.Validators.BlogValidator
{
    public class BlogUpdateDtoValidator : AbstractValidator<BlogUpdateDto>
    {
        public BlogUpdateDtoValidator()
        {
            RuleFor(blog => blog.Title)
                .MaximumLength(20).WithMessage("Titlse is to long");

            RuleFor(blog => blog)
                .Must(CheckIfOneNotNull).WithMessage("One field is required Title/Content");
        }

        private bool CheckIfOneNotNull(BlogUpdateDto blogUpdateDto)
        {
            if (blogUpdateDto.Content.IsNullOrEmpty() && blogUpdateDto.Title.IsNullOrEmpty())
            {
                return false;
            }

            return true;
        }
    }
}
