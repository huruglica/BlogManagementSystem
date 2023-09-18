using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModelDto.BlogDto;
using Services.Validators.BlogValidator;

namespace BlogManagementSystemTest.BlogHelpers
{
    public class BlogCreateDtoValidatorFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; }

        public BlogCreateDtoValidatorFixture()
        {
            var service = new ServiceCollection();

            service.AddScoped<IValidator<BlogCreateDto>, BlogCreateDtoValidator>();

            ServiceProvider = service.BuildServiceProvider();
        }
        public void Dispose()
        {
            ServiceProvider.Dispose();
        }
    }
}
