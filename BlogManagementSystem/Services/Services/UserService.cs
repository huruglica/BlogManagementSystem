using AutoMapper;
using Contratcs.IRepository;
using Contratcs.IServices;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using ModelDto.UserDto;
using MongoDB.Bson;
using Services.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IValidator<UserCreateDto> _userCreateValidator;
        private readonly IValidator<UserUpdateDto> _userUpdateValidator;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string secretKey = "3RiMMI3eusj2CJu15cJQIpXP8YallpXQQj8ad_13GiLu4uS7sUxL3Wezw6HpzfLL";

        public UserService(IUserRepository userRepository, IValidator<UserCreateDto> userCreateValidator, IValidator<UserUpdateDto> userUpdateValidator, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _userCreateValidator = userCreateValidator;
            _userUpdateValidator = userUpdateValidator;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<UserViewDto>> GetAll()
        {
            var users = await _userRepository.GetAll();
            return _mapper.Map<List<UserViewDto>>(users);
        }

        public async Task<UserViewDto> GetById(string id)
        {
            Expression<Func<User, bool>> condition = x => x.Id.Equals(id);
            var user = await _userRepository.GetByCondition(condition);
            return _mapper.Map<UserViewDto>(user);
        }

        public async Task<User> GetUserById(string id)
        {
            Expression<Func<User, bool>> condition = x => x.Id.Equals(id);
            return await _userRepository.GetByCondition(condition);
        }

        public async Task<UserViewDto> GetByUsername(string username)
        {
            Expression<Func<User, bool>> condition = x => x.Username.ToLower().Equals(username.ToLower());
            var user = await _userRepository.GetByCondition(condition);
            return _mapper.Map<UserViewDto>(user);
        }

        public async Task Post(UserCreateDto userCreateDto)
        {
            var validator = _userCreateValidator.Validate(userCreateDto);

            if (!validator.IsValid)
            {
                throw new Exception(validator.ToString());
            }

            var user = _mapper.Map<User>(userCreateDto);
            GenerateHash(userCreateDto.Password, out byte[] passwordHash, out byte[] key);
            user.Id = ObjectId.GenerateNewId().ToString();
            user.PasswordHash = passwordHash;
            user.Key = key;
            user.Role = "Creator";

            await _userRepository.Post(user);
        }

        private void GenerateHash(string password, out byte[] passwordHash, out byte[] key)
        {
            var hmac = new HMACSHA256();

            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            key = hmac.Key;
        }

        #region LOGIN
        public async Task<string> Login(UserLogin userLogin)
        {
            Expression<Func<User, bool>> condition = x => x.Username.ToLower().Equals(userLogin.Username.ToLower());
            var user = await _userRepository.GetByCondition(condition);

            if (VerifyPassword(userLogin.Password, user.PasswordHash, user.Key))
            {
                return GenerateToken(user);
            }

            throw new Exception("Wrong password");
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] key)
        {
            var hmac = new HMACSHA256(key);

            var passwordHashToVerify = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            return passwordHashToVerify.SequenceEqual(passwordHash);
        }

        private string GenerateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "dev-nq3upfdndrxpn4bz.us.auth0.com",
                audience: "BlogManagementSystem",
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

        public async Task Update(string id, UserUpdateDto userUpdateDto)
        {
            var validator = _userUpdateValidator.Validate(userUpdateDto);

            if (!validator.IsValid)
            {
                throw new Exception(validator.ToString());
            }

            var userId = await GetUserId();
            var role = await GetUserRole();

            if (!userId.Equals(id) && !role.Equals("Admin"))
            {
                throw new Exception("You can not update other users data");
            }

            var user = await GetUserById(id);
            user.Email = userUpdateDto.Email;

            await _userRepository.Update(user);
        }

        private async Task<string> GetUserId()
        {
            return await Task.Run(() => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x =>
                         x.Type == ClaimTypes.NameIdentifier)?.Value
                         ?? throw new Exception("You must login first"));
        }

        private async Task<string> GetUserRole()
        {
            return await Task.Run(() => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x =>
                         x.Type == ClaimTypes.Role)?.Value
                         ?? throw new Exception("You must login first"));
        }

        public async Task SetRole(string id, string role)
        {
            if (!Enum.IsDefined(typeof(Roles), role))
            {
                throw new Exception("This role is not available");
            }

            var user = await GetUserById(id);
            user.Role = role;

            await _userRepository.Update(user);
        }

        public async Task Delete(string id)
        {
            var userId = await GetUserId();
            var role = await GetUserRole();

            if (!userId.Equals(id) && !role.Equals("Admin"))
            {
                throw new Exception("You can not delete other user");
            }

            var user = await GetUserById(id);
            await _userRepository.Delete(user);
        }
    }
}
