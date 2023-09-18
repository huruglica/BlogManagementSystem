using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModelDto.UserDto;
using Services.Validators.UserValidator;

namespace BlogManagementSystemTest.UserHelpers
{
    public class UserUpdateDtoValidatorFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; }

        public UserUpdateDtoValidatorFixture()
        {
            var service = new ServiceCollection();

            service.AddScoped<IValidator<UserUpdateDto>, UserUpdateDtoValidator>();

            ServiceProvider = service.BuildServiceProvider();
        }
        public void Dispose()
        {
            ServiceProvider.Dispose();
        }
    }
}
