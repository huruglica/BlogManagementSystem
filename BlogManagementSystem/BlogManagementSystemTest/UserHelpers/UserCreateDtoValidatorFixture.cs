using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModelDto.UserDto;
using Services.Validators.UserValidator;

namespace BlogManagementSystemTest.UserHelpers
{
    public class UserCreateDtoValidatorFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; }

        public UserCreateDtoValidatorFixture()
        {
            var service = new ServiceCollection();

            service.AddScoped<IValidator<UserCreateDto>, UserCreateDtoValidator>();

            ServiceProvider = service.BuildServiceProvider();
        }
        public void Dispose()
        {
            ServiceProvider.Dispose();
        }
    }
}
