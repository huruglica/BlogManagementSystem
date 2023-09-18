using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModelDto.CommentDto;
using Services.Validators.CommentValidator;

namespace BlogManagementSystemTest.CommentHelpers
{
    public class CommentCreateDtoValidatorFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; }

        public CommentCreateDtoValidatorFixture()
        {
            var service = new ServiceCollection();

            service.AddScoped<IValidator<CommentCreateDto>, CommentCreateDtoValidator>();

            ServiceProvider = service.BuildServiceProvider();
        }
        public void Dispose()
        {
            ServiceProvider.Dispose();
        }
    }
}
