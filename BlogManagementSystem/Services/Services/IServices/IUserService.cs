using Domain.Entities;
using ModelDto.UserDto;
using Services.Helpers;

namespace Contratcs.IServices
{
    public interface IUserService
    {
        Task<List<UserViewDto>> GetAll();
        Task<UserViewDto> GetById(string id);
        Task<User> GetUserById(string id);
        Task<UserViewDto> GetByUsername(string username);
        Task<string> Login(UserLogin userLogin);
        Task Post(UserCreateDto userCreateDto);
        Task Update(string id, UserUpdateDto userUpdateDto);
        Task SetRole(string id, string role);
        Task Delete(string id);
    }
}
