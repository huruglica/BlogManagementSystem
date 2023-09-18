using ModelDto.BlogDto;
using ModelDto.CommentDto;
using ModelDto.SearchAndPagination;

namespace Contratcs.IServices
{
    public interface IBlogService
    {
        Task<PagedInfo<BlogViewDto>> GetAll(SearchBlog search);
        Task<BlogViewDto> GetById(int id, int page);
        Task Post(BlogCreateDto blogCreateDto);
        Task AddComment(int id, CommentCreateDto commentCreateDto);
        Task Like(int id);
        Task AddTag(int id, string userId);
        Task Update(int id, BlogUpdateDto blogUpdateDto);
        Task Delete(int id);
        Task DeleteComment(int id, int commentId);
        Task Dislike(int id, string userId);
        Task RemoveTag(int id, string userId);
    }
}
