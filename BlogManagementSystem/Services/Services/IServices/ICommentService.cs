using Domain.Entities;
using ModelDto.CommentDto;
using ModelDto.SearchAndPagination;

namespace Contratcs.IServices
{
    public interface ICommentService
    {
        Task<PagedInfo<CommentViewDto>> GetAll(SearchComment search);
        Task<PagedInfo<CommentViewDto>> GetBlogComments(int blogId, int page);
        Task<CommentViewDto> GetById(int id);
        Task Post(int blogId, CommentCreateDto commentCreateDto);
        Task AddReply(int id, CommentCreateDto commentCreateDto);
        Task Update(int id, CommentUpdateDto commentUpdateDto);
        Task Delete(int id);
        Task Delete(Comment comment);
    }
}
