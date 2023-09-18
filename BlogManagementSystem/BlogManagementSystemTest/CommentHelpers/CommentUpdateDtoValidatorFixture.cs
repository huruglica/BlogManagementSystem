using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModelDto.CommentDto;
using Services.Validators.CommentValidator;

namespace BlogManagementSystemTest.CommentHelpers
{
    public class CommentUpdateDtoValidatorFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; }

        public CommentUpdateDtoValidatorFixture()
        {
            var service = new ServiceCollection();

            service.AddScoped<IValidator<CommentUpdateDto>, CommentUpdateDtoValidator>();

            ServiceProvider = service.BuildServiceProvider();
        }
        public void Dispose()
        {
            ServiceProvider.Dispose();
        }
    }
}
