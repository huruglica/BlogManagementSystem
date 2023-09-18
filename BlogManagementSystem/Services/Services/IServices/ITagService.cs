using Domain.Entities;

namespace Services.Services.IServices
{
    public interface ITagService
    {
        Task<Tag> GetById(int blogId, string userId);
        Task<Tag> Post(int blogId, string userId);
        Task Delete(Tag tag);
    }
}
