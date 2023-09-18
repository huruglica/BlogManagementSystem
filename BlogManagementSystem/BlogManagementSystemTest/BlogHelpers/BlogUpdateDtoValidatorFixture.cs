using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModelDto.BlogDto;
using Services.Validators.BlogValidator;

namespace BlogManagementSystemTest.BlogHelpers
{
    public class BlogUpdateDtoValidatorFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; }

        public BlogUpdateDtoValidatorFixture()
        {
            var service = new ServiceCollection();

            service.AddScoped<IValidator<BlogUpdateDto>, BlogUpdateDtoValidator>();

            ServiceProvider = service.BuildServiceProvider();
        }
        public void Dispose()
        {
            ServiceProvider.Dispose();
        }
    }
}
