using AutoMapper;
using Contratcs.IRepository;
using Contratcs.IServices;
using Domain.Entities;
using FireBase;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using ModelDto.CommentDto;
using ModelDto.SearchAndPagination;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace Services.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IValidator<CommentCreateDto> _commentCreateValidator;
        private readonly IValidator<CommentUpdateDto> _commentUpdateValidator;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FireBaseNotification _fireBaseNotification;

        public CommentService(ICommentRepository commentRepository, IValidator<CommentCreateDto> commentCreateValidator, IValidator<CommentUpdateDto> commentUpdateValidator, IMapper mapper, IHttpContextAccessor httpContextAccessor, FireBaseNotification fireBaseNotification)
        {
            _commentRepository = commentRepository;
            _commentCreateValidator = commentCreateValidator;
            _commentUpdateValidator = commentUpdateValidator;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _fireBaseNotification = fireBaseNotification;
        }

        public async Task<PagedInfo<CommentViewDto>> GetAll(SearchComment search)
        {
            var condition = GetCondition(search.Content, search.Username);

            var comments = await _commentRepository.GetAll(condition);
            var commentsViewDto = _mapper.Map<List<CommentViewDto>>(comments);

            var pagedInfo = new PagedInfo<CommentViewDto>
            {
                Page = search.Page,
                PageSize = search.PageSize,
                TotalCount = commentsViewDto.Count,
                Data = commentsViewDto
                           .Skip((search.Page - 1) * search.PageSize)
                           .Take(search.PageSize)
                           .ToList()
            };

            return pagedInfo;
        }

        private Expression<Func<Comment, bool>> GetCondition(string? content, string? username)
        {
            Expression<Func<Comment, bool>> condition = x =>
            ((string.IsNullOrEmpty(content) || x.Content.ToLower().Contains(content.ToLower()))
            && (string.IsNullOrEmpty(username) || x.Username.ToLower().Equals(username.ToLower())));

            return condition;
        }

        public async Task<PagedInfo<CommentViewDto>> GetBlogComments(int blogId, int page)
        {
            var comments = await _commentRepository.GetBlogComments(blogId);
            var commentsViewDto = _mapper.Map<List<CommentViewDto>>(comments);

            var pagedInfo = new PagedInfo<CommentViewDto>
            {
                Page = page,
                PageSize = 5,
                TotalCount = commentsViewDto.Count,
                Data = commentsViewDto
                           .Skip((page - 1) * 5)
                           .Take(5)
                           .ToList()
            };

            return pagedInfo;
        }

        public async Task<CommentViewDto> GetById(int id)
        {
           Expression<Func<Comment, bool>> condition = x => x.Id == id;
            var comment = await _commentRepository.GetByCondition(condition);
            var commentViewDto = _mapper.Map<CommentViewDto>(comment);

            return commentViewDto;
        }

        private async Task<Comment> GetCommentById(int id)
        {
            Expression<Func<Comment, bool>> condition = x => x.Id == id;
            return await _commentRepository.GetByCondition(condition);
        }

        public async Task Post(int blogId, CommentCreateDto commentCreateDto)
        {
            var validator = _commentCreateValidator.Validate(commentCreateDto);
            if (!validator.IsValid)
            {
                throw new Exception(validator.ToString());
            }

            var comment = _mapper.Map<Comment>(commentCreateDto);
            comment.BlogId = blogId;
            await _commentRepository.Post(comment);
        }

        public async Task AddReply(int id, CommentCreateDto commentCreateDto)
        {
            var validator = _commentCreateValidator.Validate(commentCreateDto);
            if (!validator.IsValid)
            {
                throw new Exception(validator.ToString());
            }

            var username = SetUsername(commentCreateDto.Username);

            if (username != null)
            {
                commentCreateDto.Username = username;
            }

            var comment = _mapper.Map<Comment>(commentCreateDto);
            comment.CommentId = id;
            await _commentRepository.Post(comment);

            var title = "Someone replyed on your comment";
            var body = username + " replyed to your comment";

            _fireBaseNotification.PushNotificationFireBase(title, body);
        }

        private string SetUsername(string commentUsername)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x =>
             x.Type == ClaimTypes.Name)?.Value;

            var session = _httpContextAccessor.HttpContext?.Session;

            if (username == null)
            {
                var usernameByte = Encoding.UTF8.GetBytes(commentUsername);

                session?.Set("Username", usernameByte);
                return commentUsername;
            }
            else
            {
                var usernameByte = Encoding.UTF8.GetBytes(username);

                session?.Set("Username", usernameByte);
                return username;
            }
        }

        public async Task Update(int id, CommentUpdateDto commentUpdateDto)
        {
            var validator = _commentUpdateValidator.Validate(commentUpdateDto);
            if (!validator.IsValid)
            {
                throw new Exception(validator.ToString());
            }

            var comment = await GetCommentById(id);

            var userRole = await GetUserRole();
            var username = GetUsername();

            if (!username.ToLower().Equals(comment.Username.ToLower()) && !userRole.Equals("Admin"))
            {
                throw new Exception("This is not your comment");
            }

            comment.Content = commentUpdateDto.Content;
            await _commentRepository.Update(comment);
        }

        private async Task<string> GetUserRole()
        {
            return await Task.Run(() => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x =>
                         x.Type == ClaimTypes.Role)?.Value
                         ?? throw new Exception("You must login first"));
        }

        public async Task Delete(int id)
        {
            var comment = await GetCommentById(id);
            await _commentRepository.Delete(comment);
        }

        public async Task Delete(Comment comment)
        {
            await _commentRepository.Delete(comment);
        }

        private string GetUsername()
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x =>
             x.Type == ClaimTypes.Name)?.Value;

            if (username == null)
            {
                var session = _httpContextAccessor.HttpContext?.Session;

                if (session == null)
                {
                    throw new Exception("We can not identify you, please check your username");
                }

                session.TryGetValue("Username", out byte[] usernameByte);
                username = Encoding.UTF8.GetString(usernameByte);

                return username;
            }

            return username;
        }
    }
}
