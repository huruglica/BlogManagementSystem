using AutoMapper;
using BlogManagementSystem.Controllers;
using BlogManagementSystemTest.BlogHelpers;
using BlogManagementSystemTest.Helper;
using Contratcs.IServices;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using ModelDto.BlogDto;
using Persistence.Datebase;
using Persistence.Repository;
using Services.Services.IServices;
using Services.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FakeItEasy;
using Domain.Entities;
using ModelDto.SearchAndPagination;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using ModelDto.CommentDto;
using FireBase;

namespace BlogManagementSystemTest.Tests
{
    public class BlogsTest : IClassFixture<BlogCreateDtoValidatorFixture>,
                             IClassFixture<BlogUpdateDtoValidatorFixture>,
                             IClassFixture<AutoMapperFixture>,
                             IClassFixture<FireBaseFixture>
    {
        private readonly BlogService _blogService;
        private readonly BlogController _blogController;

        private readonly BlogManagementDatabase _blogManagementDatabase;
        private readonly BlogRepository _blogRepository;

        private readonly UserRepository _userRepository;
        private readonly CommentRepository _commentRepository;

        private readonly BlogCreateDtoValidatorFixture _blogCreateDtoValidatorFixture;
        private readonly BlogUpdateDtoValidatorFixture _blogUpdateDtoValidatorFixture;
        private readonly AutoMapperFixture _autoMapperFixture;
        private readonly FireBaseFixture _fireBaseFixture;

        private readonly HttpContextAccessorHelper _httpContextAccessorHelper;

        private readonly IValidator<BlogCreateDto> _blogCreateValidator;
        private readonly IValidator<BlogUpdateDto> _blogUpdateValidator;
        private readonly FireBaseNotification _fireBaseNotification;
        private readonly IMapper _mapper;
        private readonly ICommentService _commentService;
        private readonly ILikesService _likesService;
        private readonly ITagService _tagService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BlogsTest(BlogCreateDtoValidatorFixture blogCreateDtoValidatorFixture, BlogUpdateDtoValidatorFixture blogUpdateDtoValidatorFixture, AutoMapperFixture autoMapperFixture, FireBaseFixture fireBaseFixture)
        {
            DbContextOptionsBuilder<BlogManagementDatabase> dbOptions = new DbContextOptionsBuilder<BlogManagementDatabase>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString());
            _blogManagementDatabase = new BlogManagementDatabase(dbOptions.Options);
            _blogRepository = new BlogRepository(_blogManagementDatabase);

            _userRepository = new UserRepository(_blogManagementDatabase);
            _commentRepository = new CommentRepository(_blogManagementDatabase);

            _blogCreateDtoValidatorFixture = blogCreateDtoValidatorFixture;
            _blogUpdateDtoValidatorFixture = blogUpdateDtoValidatorFixture;
            _autoMapperFixture = autoMapperFixture;
            _fireBaseFixture = fireBaseFixture;

            _blogCreateValidator = _blogCreateDtoValidatorFixture.ServiceProvider.GetRequiredService<IValidator<BlogCreateDto>>();
            _blogUpdateValidator = _blogUpdateDtoValidatorFixture.ServiceProvider.GetRequiredService<IValidator<BlogUpdateDto>>();
            _mapper = _autoMapperFixture.ServiceProvider.GetRequiredService<IMapper>();
            _fireBaseNotification = _fireBaseFixture.ServiceProvider.GetRequiredService<FireBaseNotification>();

            _httpContextAccessorHelper = new HttpContextAccessorHelper();
            _httpContextAccessor = _httpContextAccessorHelper.HttpContextAccessor;

            _commentService = A.Fake<ICommentService>();
            _likesService = A.Fake<ILikesService>();
            _tagService = A.Fake<ITagService>();
            _userService = A.Fake<IUserService>();

            _blogService = new BlogService(_blogRepository,
                                           _blogCreateValidator,
                                           _blogUpdateValidator,
                                           _mapper,
                                           _commentService,
                                           _httpContextAccessor,
                                           _likesService,
                                           _tagService,
                                           _userService,
                                           _fireBaseNotification);
            _blogController = new BlogController(_blogService);
        }

        [Fact]
        public async Task Test_GetAll()
        {
            var blogOne = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64f1bd63826610e30d527560"
            };

            var blogTwo = new Blog
            {
                Id = 2,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64f1bd63826610e30d527560"
            };

            await _blogRepository.Post(blogOne);
            await _blogRepository.Post(blogTwo);

            var user = new User
            {
                Id = "64f1bd63826610e30d527560",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            var result = await _blogController.GetAll(new SearchBlog());
            Assert.IsType<OkObjectResult>(result);

            Expression<Func<Blog, bool>> expression = x => true;
            var blogs = await _blogRepository.GetAll(expression);
            Assert.Equal(2, blogs.Count);
        }

        [Theory]
        [InlineData(1, 2)]
        public async Task Test_GetById(int okId, int notOkId)
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64f1bd63826610e30d527560"
            };

            await _blogRepository.Post(blog);

            var user = new User
            {
                Id = "64f1bd63826610e30d527560",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            var result = await _blogController.GetById(okId);
            var notOkResult = await _blogController.GetById(notOkId);

            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<BadRequestObjectResult>(notOkResult);
        }

        [Theory]
        [InlineData("Title", "Content")]
        public async Task Test_PostOk(string title, string content)
        {
            var fakeFromFile = new FakeFormFile
            {
                FileName = "image.jpeg",
                Length = 100,
                ContentType = "image/jpeg"
            };

            var blogCreateDto = new BlogCreateDto
            {
                Title = title,
                Content = content,
                FormFile = fakeFromFile
            };

            var result = await _blogController.Post(blogCreateDto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData("", "Content")]
        [InlineData(null, "Content")]
        [InlineData("Title", "")]
        [InlineData("Title", null)]
        [InlineData(null, null)]
        [InlineData("", "")]
        public async Task Test_PostNotOk(string title, string content)
        {
            var blogCreateDto = new BlogCreateDto
            {
                Title = title,
                Content = content,
                FormFile = new FakeFormFile()
            };

            var result = await _blogController.Post(blogCreateDto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("Username", "Content")]
        public async Task Test_AddComment(string username, string content)
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64f1bd63826610e30d527560"
            };

            var user = new User
            {
                Id = "64f1bd63826610e30d527560",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            await _blogRepository.Post(blog);

            var commentCreateDto = new CommentCreateDto
            {
                Username = username,
                Content = content
            };

            var result = await _blogController.AddComment(blog.Id, commentCreateDto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Test_AddTagOk()
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64dcd34fe55c1e2ee8460991"
            };

            var userOne = new User
            {
                Id = "64dcd34fe55c1e2ee8460991",
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
                Id = "64f1bd63826610e30d527560",
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

            await _blogRepository.Post(blog);
            var userId = "64f1bd63826610e30d527560";

            var result = await _blogController.AddTag(blog.Id, userId);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Test_AddTagNotOk()
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64f1bd63826610e30d527560"
            };

            var user = new User
            {
                Id = "64f1bd63826610e30d527560",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            await _blogRepository.Post(blog);
            var userId = "UserId";

            var result = await _blogController.AddTag(2, userId);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("Title", "Content")]
        [InlineData("", "Content")]
        [InlineData(null, "Content")]
        [InlineData("Title", "")]
        [InlineData("Title", null)]
        public async Task Test_UpdateOk(string title, string content)
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[128],
                UserId = "64f1bd63826610e30d527560"
            };

            var user = new User
            {
                Id = "64f1bd63826610e30d527560",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            await _blogRepository.Post(blog);

            var blogUpdateDto = new BlogUpdateDto
            {
                Title = title,
                Content = content
            };

            var result = await _blogController.Update(blog.Id, blogUpdateDto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData(2, "Title", "Content")]
        [InlineData(1, "", "")]
        [InlineData(1, null, null)]
        public async Task Test_UpdateNotOk(int id, string title, string content)
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[128],
                UserId = "64f1bd63826610e30d527560"
            };

            var user = new User
            {
                Id = "64f1bd63826610e30d527560",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            await _blogRepository.Post(blog);

            var blogUpdateDto = new BlogUpdateDto
            {
                Title = title,
                Content = content
            };

            var result = await _blogController.Update(id, blogUpdateDto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData(1, 2)]
        public async Task Test_Delete(int okId, int notOkId)
        {
            var blog = new Blog
            {
                Id = okId,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64f1bd63826610e30d527560"
            };

            var user = new User
            {
                Id = "64f1bd63826610e30d527560",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            await _userRepository.Post(user);

            await _blogRepository.Post(blog);

            var result = await _blogController.Delete(okId);
            var notOkResult = await _blogController.Delete(notOkId);

            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<BadRequestObjectResult>(notOkResult);
        }

        [Fact]
        public async Task Test_DeleteCommentOk()
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64f1bd63826610e30d527560"
            };

            var user = new User
            {
                Id = "64f1bd63826610e30d527560",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            var comment = new Comment
            {
                Id = 1,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            await _userRepository.Post(user);

            await _commentRepository.Post(comment);

            await _blogRepository.Post(blog);

            var commentId = 1;

            var result = await _blogController.DeleteComment(blog.Id, commentId);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Test_DeleteCommentNotOk()
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64f1bd63826610e30d527560"
            };

            var user = new User
            {
                Id = "64f1bd63826610e30d527560",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            var comment = new Comment
            {
                Id = 2,
                Username = "Username",
                Content = "Content",
                BlogId = 2
            };

            await _commentRepository.Post(comment);

            await _userRepository.Post(user);

            await _blogRepository.Post(blog);

            var result = await _blogController.DeleteComment(blog.Id, comment.Id);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Test_Dislike()
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64f1bd63826610e30d527560"
            };

            var user = new User
            {
                Id = "64f1bd63826610e30d527560",
                Name = "Name",
                Surname = "Surname",
                Username = "Username",
                Email = "email@email.ee",
                PasswordHash = new byte[10],
                Key = new byte[10],
                Role = "Creator"
            };

            var comment = new Comment
            {
                Id = 1,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            await _userRepository.Post(user);

            await _commentRepository.Post(comment);

            await _blogRepository.Post(blog);

            await _likesService.Post(blog.Id, user.Id);

            var result = await _blogController.Dislike(blog.Id, user.Id);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Test_RemoveTagOk()
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64dcd34fe55c1e2ee8460991"
            };

            var userOne = new User
            {
                Id = "64dcd34fe55c1e2ee8460991",
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
                Id = "64f1bd63826610e30d527560",
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

            await _blogRepository.Post(blog);
            var userId = "64f1bd63826610e30d527560";

            await _tagService.Post(blog.Id, userId);

            var result = await _blogController.RemoveTag(blog.Id, userId);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Test_RemoveTagNotOk()
        {
            var blog = new Blog
            {
                Id = 1,
                Title = "Title",
                Content = "Content",
                PublicationDate = DateTime.Now,
                Image = new byte[5],
                UserId = "64dcd34fe55c1e2ee8460991"
            };

            var userOne = new User
            {
                Id = "64dcd34fe55c1e2ee8460991",
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
                Id = "64f1bd63826610e30d527560",
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

            await _blogRepository.Post(blog);
            var userId = "64f1bd63826610e30d527560";

            await _tagService.Post(blog.Id, userId);

            var result = await _blogController.RemoveTag(3, userId);
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
