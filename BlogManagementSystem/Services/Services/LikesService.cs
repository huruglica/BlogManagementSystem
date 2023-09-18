using Contratcs.IRepository;
using Domain.Entities;
using Services.Services.IServices;
using System.Linq.Expressions;

namespace Services.Services
{
    public class LikesService : ILikesService
    {
        private readonly ILikesRepository _likesRepository;

        public LikesService(ILikesRepository likesRepository)
        {
            _likesRepository = likesRepository;
        }

        public async Task<List<Likes>> GetAll()
        {
            return await _likesRepository.GetAll();
        }

        public async Task<Likes> GetById(int blogId, string userId)
        {
            Expression<Func<Likes, bool>> condition = x => x.BlogId == blogId && x.UserId.Equals(userId);
            var like = await _likesRepository.GetByCondition(condition);

            return like.FirstOrDefault() ?? throw new Exception("You have not liked this blog");
        }

        public async Task Post(int blogId, string userId)
        {
            Expression<Func<Likes, bool>> condition = x => x.BlogId == blogId;
            var like = await _likesRepository.GetByCondition(condition);

            if (like.Count() > 0)
            {
                if (like.Where(x => x.UserId.Equals(userId)).Count() > 0)
                { 
                    throw new Exception("You have liked this post");
                }
            }

            var newLike = new Likes()
            {
                UserId = userId,
                BlogId = blogId
            };

            await _likesRepository.Post(newLike);
        }

        public async Task Delete(Likes like)
        {
            await _likesRepository.Delete(like);
        }
    }
}
