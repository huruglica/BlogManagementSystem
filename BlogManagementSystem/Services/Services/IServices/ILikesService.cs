using Domain.Entities;
using ModelDto.LikesDto;

namespace Services.Services.IServices
{
    public interface ILikesService
    {
        Task<Likes> GetById(int blogId, string userId);
        Task Post(int blogId, string userId);
        Task Delete(Likes like);
    }
}
