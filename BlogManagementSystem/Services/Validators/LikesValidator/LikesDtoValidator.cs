using FluentValidation;
using ModelDto.LikesDto;

namespace Services.Validators.LikesValidator
{
    public class LikesDtoValidator : AbstractValidator<LikesViewDto>
    {
        public LikesDtoValidator()
        {
        }
    }
}
