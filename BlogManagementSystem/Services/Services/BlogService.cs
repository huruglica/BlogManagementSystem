using AutoMapper;
using Contratcs.IRepository;
using Contratcs.IServices;
using Domain.Entities;
using FireBase;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using ModelDto.BlogDto;
using ModelDto.CommentDto;
using ModelDto.SearchAndPagination;
using Services.Services.IServices;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace Services.Services
{
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IValidator<BlogCreateDto> _blogCreateValidator;
        private readonly IValidator<BlogUpdateDto> _blogUpdateValidator;
        private readonly IMapper _mapper;
        private readonly ICommentService _commentService;
        private readonly ILikesService _likesService;
        private readonly ITagService _tagService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FireBaseNotification _fireBaseNotification;

        public BlogService(IBlogRepository blogRepository, IValidator<BlogCreateDto> blogCreateValidator, IValidator<BlogUpdateDto> blogUpdateValidator, IMapper mapper, ICommentService commentService, IHttpContextAccessor httpContextAccessor, ILikesService likesService, ITagService tagService, IUserService userService, FireBaseNotification fireBaseNotification)
        {
            _blogRepository = blogRepository;
            _blogCreateValidator = blogCreateValidator;
            _blogUpdateValidator = blogUpdateValidator;
            _mapper = mapper;
            _commentService = commentService;
            _httpContextAccessor = httpContextAccessor;
            _likesService = likesService;
            _tagService = tagService;
            _userService = userService;
            _fireBaseNotification = fireBaseNotification;
        }

        public async Task<PagedInfo<BlogViewDto>> GetAll(SearchBlog search)
        {
            var condition = GetCondition(search);

            var blogs = await _blogRepository.GetAll(condition);
            var blogsViewDto = _mapper.Map<List<BlogViewDto>>(blogs);

            var pagedInfo = new PagedInfo<BlogViewDto>
            {
                Page = search.Page,
                PageSize = search.PageSize,
                TotalCount = blogsViewDto.Count,
                Data = blogsViewDto
                           .Skip((search.Page - 1) * search.PageSize)
                           .Take(search.PageSize)
                           .ToList()
            };

            return pagedInfo;
        }

        private Expression<Func<Blog, bool>> GetCondition(SearchBlog search)
        {
            Expression<Func<Blog, bool>> condition = x =>
            ((string.IsNullOrEmpty(search.Title) || x.Title.ToLower().Contains(search.Title.ToLower()))
            && (string.IsNullOrEmpty(search.Content) || x.Content.ToLower().Contains(search.Content))
            && (string.IsNullOrEmpty(search.Username) || x.User.Username.ToLower().Equals(search.Username.ToLower()))
            && (search.BeforeDate == null || x.PublicationDate < search.BeforeDate)
            && (search.AfterDate == null || x.PublicationDate > search.AfterDate));

            return condition;
        }

        public async Task<BlogViewDto> GetById(int id, int page)
        {
            if (page < 1)
            {
                page = 1;
            }

            Expression<Func<Blog, bool>> condition = x => x.Id == id;
            var blog = await _blogRepository.GetByCondition(condition);
            var blogViewDto = _mapper.Map<BlogViewDto>(blog);
            blogViewDto.CommentsPaged = await _commentService.GetBlogComments(id, page);
            return blogViewDto;
        }

        public async Task<Blog> GetBlogById(int id)
        {
            Expression<Func<Blog, bool>> condition = x => x.Id == id;
            return await _blogRepository.GetByCondition(condition);
        }

        public async Task Post(BlogCreateDto blogCreateDto)
        {
            var validator = _blogCreateValidator.Validate(blogCreateDto);
            if (!validator.IsValid)
            {
                throw new Exception(validator.ToString());
            }

            var memoryStream = new MemoryStream();
            await blogCreateDto.FormFile.CopyToAsync(memoryStream);

            var blog = _mapper.Map<Blog>(blogCreateDto);
            blog.Image = memoryStream.ToArray();
            blog.PublicationDate = DateTime.Now;
            blog.UserId = await GetUserId();

            await _blogRepository.Post(blog);
        }

        private async Task<string> GetUserId()
        {
            return await Task.Run(() => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x =>
                         x.Type == ClaimTypes.NameIdentifier)?.Value
                         ?? throw new Exception("You must login first"));
        }

        public async Task AddComment(int id, CommentCreateDto commentCreateDto)
        {
            var username = SetUsername(commentCreateDto.Username);

            await _commentService.Post(id, commentCreateDto);

            var title = "Someone commented on your blog";
            var body = username + " commentet on your blog";

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

        public async Task Like(int id)
        {
            var userId = await GetUserId();
            await _likesService.Post(id, userId);
            var blog = await GetBlogById(id);

            await _blogRepository.Update(blog);
        }

        public async Task AddTag(int id, string userId)
        {
            var ownerId = await GetUserId();
            var blog = await GetBlogById(id);

            if (!ownerId.Equals(blog.UserId))
            {
                throw new Exception("This is not your blog");
            }

            var user = await _userService.GetUserById(userId);

            if (user == null || blog == null)
            {
                throw new Exception("Tag is not added");
            }

            await _tagService.Post(id, userId);

            var username = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x =>
             x.Type == ClaimTypes.Name)?.Value;

            var title = "Someone taged you";
            var body = "You are taged in " + username + " blog";

            _fireBaseNotification.PushNotificationFireBase(title, body);
        }

        public async Task Update(int id, BlogUpdateDto blogUpdateDto)
        {
            var validator = _blogUpdateValidator.Validate(blogUpdateDto);
            if (!validator.IsValid)
            {
                throw new Exception(validator.ToString());
            }

            var blog = await GetBlogById(id);

            var userId = await GetUserId();
            var userRole = await GetUserRole();

            if (!userId.Equals(blog.UserId) && !userRole.Equals("Admin"))
            {
                throw new Exception("You can not update other users blog");
            }

            if (!string.IsNullOrEmpty(blogUpdateDto.Title))
            {
                blog.Title = blogUpdateDto.Title;
            }

            if (!string.IsNullOrEmpty(blogUpdateDto.Content))
            {
                blog.Content = blogUpdateDto.Content;
            }

            await _blogRepository.Update(blog);
        }

        private async Task<string> GetUserRole()
        {
            return await Task.Run(() => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x =>
                         x.Type == ClaimTypes.Role)?.Value
                         ?? throw new Exception("You must login first"));
        }

        public async Task Delete(int id)
        {
            var blog = await GetBlogById(id);

            var userId = await GetUserId();
            var userRole = await GetUserRole();

            if (!userId.Equals(blog.UserId) && !userRole.Equals("Admin"))
            {
                throw new Exception("You can not delete other users blog");
            }

            await _blogRepository.Delete(blog);
        }

        public async Task DeleteComment(int id, int commentId)
        {
            var blog = await GetBlogById(id);
            var comment = blog.Comments?.Where(x => x.Id == commentId).FirstOrDefault();
            if (comment == null)
            {
                throw new Exception("This comment is not at this blog");
            }

            var userRole = await GetUserRole();

            var username = GetUsername();

            if(!username.ToLower().Equals(comment.Username.ToLower()) && !userRole.Equals("Admin"))
            {
                throw new Exception("This is not your comment");
            }

            await _commentService.Delete(comment);
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

        public async Task Dislike(int id, string userId)
        {
            var like = await _likesService.GetById(id, userId);

            await _likesService.Delete(like);
        }

        public async Task RemoveTag(int id, string userId)
        {
            var blog = await GetBlogById(id);
            var ownerId = await GetUserId();

            if (!ownerId.Equals(blog.UserId))
            {
                throw new Exception("This is not your blog");
            }

            var tag = await _tagService.GetById(id, userId);

            await _tagService.Delete(tag);
        }
    }
}
