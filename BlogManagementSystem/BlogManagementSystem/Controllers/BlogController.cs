using Contratcs.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelDto.BlogDto;
using ModelDto.CommentDto;
using ModelDto.SearchAndPagination;

namespace BlogManagementSystem.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    [Authorize]
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] SearchBlog search)
        {
            try
            {
                var blogs = await _blogService.GetAll(search);
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, [FromQuery] int page = 1)
        {
            try
            {
                var blog = await _blogService.GetById(id, page);
                return Ok(blog);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] BlogCreateDto blogCreateDto)
        {
            try
            {
                await _blogService.Post(blogCreateDto);
                return Ok("Posted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/add-comment")]
        [AllowAnonymous]
        public async Task<IActionResult> AddComment(int id, CommentCreateDto commentCreateDto)
        {
            try
            {
                await _blogService.AddComment(id, commentCreateDto);
                return Ok("Comment added successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> Like(int id)
        {
            try
            {
                await _blogService.Like(id);
                return Ok("Blog is liked");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/add-tag")]
        public async Task<IActionResult> AddTag(int id, [FromQuery] string userId)
        {
            try
            {
                await _blogService.AddTag(id, userId);
                return Ok("Blog is liked");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BlogUpdateDto blogUpdateDto)
        {
            try
            {
                await _blogService.Update(id, blogUpdateDto);
                return Ok("Updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _blogService.Delete(id);
                return Ok("Deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}/delete-comment")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteComment(int id, [FromQuery] int commentId)
        {
            try
            {
                await _blogService.DeleteComment(id, commentId);
                return Ok("Comment deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}/dislike")]
        public async Task<IActionResult> Dislike(int id, [FromQuery] string userId)
        {
            try
            {
                await _blogService.Dislike(id, userId);
                return Ok("Disliked successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}/remove-tag")]
        public async Task<IActionResult> RemoveTag(int id, [FromQuery] string userId)
        {
            try
            {
                await _blogService.RemoveTag(id, userId);
                return Ok("Tag removed successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
