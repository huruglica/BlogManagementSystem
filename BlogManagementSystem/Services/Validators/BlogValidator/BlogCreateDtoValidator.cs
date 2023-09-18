using FluentValidation;
using Microsoft.AspNetCore.Http;
using ModelDto.BlogDto;
using System.Linq;

namespace Services.Validators.BlogValidator
{
    public class BlogCreateDtoValidator : AbstractValidator<BlogCreateDto>
    {
        public BlogCreateDtoValidator()
        {
            RuleFor(blog => blog.Title)
                .NotEmpty().WithMessage("Title is required")
                .NotNull().WithMessage("Title is required")
                .MaximumLength(20).WithMessage("Titlse is to long");

            RuleFor(blog => blog.Content)
                .NotEmpty().WithMessage("Content is required")
                .NotNull().WithMessage("Content is required");

            RuleFor(blog => blog.FormFile)
                .NotEmpty().WithMessage("Image is required")
                .NotNull().WithMessage("Image is required")
                .Must(IsImage).WithMessage("The file should be image");
        }

        private bool IsImage(IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
            {
                return false;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };

            var fileExtension = Path.GetExtension(formFile.FileName).ToLower();
            var contentType = formFile.ContentType.ToLower();

            return allowedExtensions.Contains(fileExtension) &&
                   allowedMimeTypes.Contains(contentType);
        }
    }
}
