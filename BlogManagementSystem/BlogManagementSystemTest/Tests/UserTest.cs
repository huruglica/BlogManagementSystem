using AutoMapper;
using BlogManagementSystem.Controllers;
using BlogManagementSystemTest.Helper;
using BlogManagementSystemTest.UserHelpers;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelDto.UserDto;
using Persistence.Datebase;
using Persistence.Repository;
using Services.Services;

namespace BlogManagementSystemTest.Tests
{
    public class UserTest : IClassFixture<UserCreateDtoValidatorFixture>,
                            IClassFixture<UserUpdateDtoValidatorFixture>,
                            IClassFixture<AutoMapperFixture>
    {
        private readonly UserController _userController;
        private readonly UserService _userService;

        private readonly UserCreateDtoValidatorFixture _userCreateDtoValidatorFixture;
        private readonly UserUpdateDtoValidatorFixture _userUpdateDtoValidatorFixture;
        private readonly AutoMapperFixture _autoMapperFixture;

        private readonly HttpContextAccessorHelper _httpContextAccessorHelper;

        private readonly BlogManagementDatabase _blogManagementDatabase;
        private readonly UserRepository _userRepository;

        private readonly IValidator<UserCreateDto> _userCreateValidator;
        private readonly IValidator<UserUpdateDto> _userUpdateValidator;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserTest(UserCreateDtoValidatorFixture userCreateDtoValidatorFixture, UserUpdateDtoValidatorFixture userUpdateDtoValidatorFixture, AutoMapperFixture autoMapperFixture)
        {
            DbContextOptionsBuilder<BlogManagementDatabase> dbOptions = new DbContextOptionsBuilder<BlogManagementDatabase>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString());

            _blogManagementDatabase = new BlogManagementDatabase(dbOptions.Options);
            _userRepository = new UserRepository(_blogManagementDatabase);

            _userCreateDtoValidatorFixture = userCreateDtoValidatorFixture;
            _userUpdateDtoValidatorFixture = userUpdateDtoValidatorFixture;
            _autoMapperFixture = autoMapperFixture;

            _httpContextAccessorHelper = new HttpContextAccessorHelper();
            _httpContextAccessor = _httpContextAccessorHelper.HttpContextAccessor;

            _userCreateValidator = _userCreateDtoValidatorFixture.ServiceProvider.GetRequiredService<IValidator<UserCreateDto>>();
            _userUpdateValidator = _userUpdateDtoValidatorFixture.ServiceProvider.GetRequiredService<IValidator<UserUpdateDto>>();
            _mapper = _autoMapperFixture.ServiceProvider.GetRequiredService<IMapper>();

            _userService = new UserService(_userRepository, _userCreateValidator, _userUpdateValidator, _mapper, _httpContextAccessor);
            _userController = new UserController(_userService);
        }

        [Fact]
        public async Task Test_GetAll()
        {
            var userOne = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            var userTwo = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(userOne);
            await _userRepository.Post(userTwo);

            var result = await _userController.GetAll();
            Assert.IsType<OkObjectResult>(result);

            var users = await _userRepository.GetAll();
            Assert.Equal(2, users.Count);
        }

        [Theory]
        [InlineData("64f1bd63826610e30d527560", "64f1bd63826610e30d527561")]
        public async Task Test_GetById(string okId, string notOkId)
        {
            var user = new User
            {
                Id = okId,
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            var result = await _userController.GetById(okId);
            var notOkResult = await _userController.GetById(notOkId);

            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<BadRequestObjectResult>(notOkResult);
        }

        [Theory]
        [InlineData("Name", "Surname", "Username", "email@email.ee", "Password")]
        [InlineData("Name", "Surname", "Username", null, "Password")]
        public async Task Test_PostOk(string name, string surname, string username, string email, string password)
        {
            var userCreateDto = new UserCreateDto
            {
                Name = name,
                Surname = surname,
                Username = username,
                Email = email,
                Password = password
            };

            var result = await _userController.Post(userCreateDto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData("", "Surname", "Username", "email@email.ee", "Password")]
        [InlineData(null, "Surname", "Username", "email@email.ee", "Password")]
        [InlineData("Name", "", "Username", "email@email.ee", "Password")]
        [InlineData("Name", null, "Username", "email@email.ee", "Password")]
        [InlineData("Name", "Surname", "", "email@email.ee", "Password")]
        [InlineData("Name", "Surname", null, "email@email.ee", "Password")]
        [InlineData("Name", "Surname", "Username", "", "Password")]
        [InlineData("Name", "Surname", "Username", "email@email", "Password")]
        [InlineData("Name", "Surname", "Username", "email@.ee", "Password")]
        [InlineData("Name", "Surname", "Username", "email@.email.ee", "Password")]
        [InlineData("Name", "Surname", "Username", "@email.ee", "Password")]
        [InlineData("Name", "Surname", "Username", "email@email.ee", "")]
        [InlineData("Name", "Surname", "Username", "email@email.ee", null)]
        [InlineData("Name", "Surname", "Username", "email@email.ee", "Pass")]
        public async Task Test_PostNotOk(string name, string surname, string username, string email, string password)
        {
            var userCreateDto = new UserCreateDto
            {
                Name = name,
                Surname = surname,
                Username = username,
                Email = email,
                Password = password
            };

            var result = await _userController.Post(userCreateDto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Creator")]
        public async Task Test_SetRoleOk(string role)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            var result = await _userController.SetRole(user.Id, role);
            Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("Supervisor")]
        [InlineData("Regular")]
        [InlineData("Manager")]
        public async Task Test_SetRoleNotOk(string role)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            var result = await _userController.SetRole(user.Id, role);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("email@email.ee")]
        public async Task Test_UpdateOk(string email)
        {
            var user = new User
            {
                Id = "64dcd34fe55c1e2ee8460991",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            var userUpdateDto = new UserUpdateDto
            {
                Email = email
            };

            var result = await _userController.Update(user.Id, userUpdateDto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("email@email.")]
        [InlineData("email@.ee")]
        [InlineData("@.email.ee")]
        public async Task Test_UpdateNotOk(string email)
        {
            var user = new User
            {
                Id = "64dcd34fe55c1e2ee8460991",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            var userUpdateDto = new UserUpdateDto
            {
                Email = email
            };

            var result = await _userController.Update(user.Id, userUpdateDto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("64f1bd63826610e30d527560", "64f1bd63826610e30d527561")]
        public async Task Test_Delete(string okId, string notOkId)
        {
            var user = new User
            {
                Id = okId,
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            var result = await _userController.Delete(okId);
            var notOkResult = await _userController.Delete(notOkId);

            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<BadRequestObjectResult>(notOkResult);
        }
    }
}
