using AutoMapper;
using BlogManagementSystem.Controllers;
using BlogManagementSystemTest.CommentHelpers;
using BlogManagementSystemTest.Helper;
using Domain.Entities;
using FakeItEasy;
using FireBase;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelDto.CommentDto;
using ModelDto.SearchAndPagination;
using Persistence.Datebase;
using Persistence.Repository;
using Services.Services;
using System.Linq.Expressions;

namespace BlogManagementSystemTest.Tests
{
    public class CommentTest : IClassFixture<CommentCreateDtoValidatorFixture>,
                               IClassFixture<CommentUpdateDtoValidatorFixture>,
                               IClassFixture<AutoMapperFixture>,
                               IClassFixture<FireBaseFixture>
    {
        private readonly CommentService _commentService;
        private readonly CommentController _commentController;

        private readonly CommentCreateDtoValidatorFixture _commentCreateDtoValidatorFixture;
        private readonly CommentUpdateDtoValidatorFixture _commentUpdatetoValidatorFixture;
        private readonly AutoMapperFixture _autoMapperFixture;
        private readonly FireBaseFixture _fireBaseFixture;

        private readonly HttpContextAccessorHelper _httpContextAccessorHelper;

        private readonly BlogManagementDatabase _blogManagementDatabase;
        private readonly CommentRepository _commentRepository;

        private readonly IValidator<CommentCreateDto> _commentCreateValidator;
        private readonly IValidator<CommentUpdateDto> _commentUpdateValidator;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FireBaseNotification _fireBaseNotification;

        public CommentTest(CommentCreateDtoValidatorFixture commentCreateDtoValidatorFixture, CommentUpdateDtoValidatorFixture commentUpdatetoValidatorFixture, AutoMapperFixture autoMapperFixture, FireBaseFixture fireBaseFixture)
        {
            DbContextOptionsBuilder<BlogManagementDatabase> dbOptions = new DbContextOptionsBuilder<BlogManagementDatabase>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString());

            _blogManagementDatabase = new BlogManagementDatabase(dbOptions.Options);
            _commentRepository = new CommentRepository(_blogManagementDatabase);

            _commentCreateDtoValidatorFixture = commentCreateDtoValidatorFixture;
            _commentUpdatetoValidatorFixture = commentUpdatetoValidatorFixture;
            _autoMapperFixture = autoMapperFixture;
            _fireBaseFixture = fireBaseFixture;

            _httpContextAccessorHelper = new HttpContextAccessorHelper();
            _httpContextAccessor = _httpContextAccessorHelper.HttpContextAccessor;

            _commentCreateValidator = _commentCreateDtoValidatorFixture.ServiceProvider.GetRequiredService<IValidator<CommentCreateDto>>();
            _commentUpdateValidator = _commentUpdatetoValidatorFixture.ServiceProvider.GetRequiredService<IValidator<CommentUpdateDto>>();
            _mapper = _autoMapperFixture.ServiceProvider.GetRequiredService<IMapper>();
            _fireBaseNotification = _fireBaseFixture.ServiceProvider.GetRequiredService<FireBaseNotification>();

            _commentService = new CommentService(_commentRepository, _commentCreateValidator, _commentUpdateValidator,
                _mapper, _httpContextAccessor, _fireBaseNotification);
            _commentController = new CommentController(_commentService);
        }

        [Fact]
        public async Task Test_GetAll()
        {
            var commentOne = new Comment
            {
                Id = 1,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            var commentTwo = new Comment
            {
                Id = 2,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            await _commentRepository.Post(commentOne);
            await _commentRepository.Post(commentTwo);

            var result = await _commentController.GetAll(new SearchComment());
            Assert.IsType<OkObjectResult>(result);

            Expression<Func<Comment, bool>> expression = x => true;
            var comments = await _commentRepository.GetAll(expression);
            Assert.Equal(2, comments.Count);
        }

        [Theory]
        [InlineData(1, 2)]
        public async Task Test_GetById(int okId, int notOkId)
        {
            var comment = new Comment
            {
                Id = okId,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            await _commentRepository.Post(comment);

            var result = await _commentController.GetById(okId);
            var notOkResult = await _commentController.GetById(notOkId);

            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<BadRequestObjectResult>(notOkResult);
        }

        [Theory]
        [InlineData(1, "Username", "Content", null)]
        [InlineData(1, "Username", "Content", "email@email.ee")]
        public async Task Test_PostOk(int bolgId, string username, string content, string email)
        {
            var commentCreateDto = new CommentCreateDto
            {
                Username = username,
                Content = content,
                Email = email
            };

            var result = await _commentController.Post(bolgId, commentCreateDto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData(1, "Username", "Content", "")]
        [InlineData(1, "Username", "Content", "@email.ee")]
        [InlineData(1, "Username", "Content", "email@.ee")]
        [InlineData(1, "Username", "Content", "email@email.")]
        [InlineData(1, "Username", "Content", "email@.email.ee")]
        [InlineData(1, "Username", "", "email@email.ee")]
        [InlineData(1, "Username", null, "email@email.ee")]
        [InlineData(1, "", "Content", "email@email.ee")]
        [InlineData(1, null, "Content", "email@email.ee")]
        public async Task Test_PostNotOk(int bolgId, string username, string content, string email)
        {
            var commentCreateDto = new CommentCreateDto
            {
                Username = username,
                Content = content,
                Email = email
            };

            var result = await _commentController.Post(bolgId, commentCreateDto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("Username", "Content", null)]
        [InlineData("Username", "Content", "email@email.ee")]
        public async Task Test_AddReplyOk(string username, string content, string email)
        {
            var comment = new Comment
            {
                Id = 1,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            await _commentRepository.Post(comment);

            var commentCreateDto = new CommentCreateDto
            {
                Username = username,
                Content = content,
                Email = email
            };

            var result = await _commentController.AddReply(1, commentCreateDto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData(1, "", "Content")]
        [InlineData(1, null, "Content")]
        [InlineData(1, "Username", "")]
        [InlineData(1, "Username", null)]
        public async Task Test_AddReplyNotOk(int id, string username, string content)
        {
            var comment = new Comment
            {
                Id = 1,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            await _commentRepository.Post(comment);

            var commentCreateDto = new CommentCreateDto
            {
                Username = username,
                Content = content
            };

            var result = await _commentController.AddReply(id, commentCreateDto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("New Contetnt")]
        public async Task Test_UpdateOk(string content)
        {
            var comment = new Comment
            {
                Id = 1,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            await _commentRepository.Post(comment);

            var commentUpdateDto = new CommentUpdateDto
            {
                Content = content
            };

            var result = await _commentController.Update(comment.Id, commentUpdateDto);
            Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(1, "")]
        [InlineData(2, "New Contetnt")]
        [InlineData(2, null)]
        public async Task Test_UpdateNotOk(int id, string content)
        {
            var comment = new Comment
            {
                Id = 1,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            await _commentRepository.Post(comment);

            var commentUpdateDto = new CommentUpdateDto
            {
                Content = content
            };

            var result = await _commentController.Update(id, commentUpdateDto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData(1, 2)]
        public async Task Test_Delete(int okId, int notOkId)
        {
            var commentOne = new Comment
            {
                Id = okId,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            var commentTwo = new Comment
            {
                Id = 3,
                Username = "Username",
                Content = "Content",
                BlogId = 1
            };

            await _commentRepository.Post(commentOne);
            await _commentRepository.Post(commentTwo);

            var result = await _commentController.GetById(okId);
            var notOkResult = await _commentController.GetById(notOkId);

            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<BadRequestObjectResult>(notOkResult);
        }
    }
}
