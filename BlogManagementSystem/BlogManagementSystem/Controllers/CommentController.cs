using Contratcs.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelDto.CommentDto;
using ModelDto.SearchAndPagination;
using Services.Helpers;

namespace BlogManagementSystem.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] SearchComment search)
        {
            try
            {
                var comments = await _commentService.GetAll(search);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var comment = await _commentService.GetById(id);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{blogId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post(int blogId, CommentCreateDto commentCreateDto)
        {
            try
            {
                await _commentService.Post(blogId, commentCreateDto);
                return Ok("Posted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/add-reply")]
        public async Task<IActionResult> AddReply(int id, CommentCreateDto commentCreateDto)
        {
            try
            {
                await _commentService.AddReply(id, commentCreateDto);
                return Ok("Posted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CommentUpdateDto commentUpdateDto)
        {
            try
            {
                await _commentService.Update(id, commentUpdateDto);
                return Ok("Posted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _commentService.Delete(id);
                return Ok("Posted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
