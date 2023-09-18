using Contratcs.IRepository;
using Domain.Entities;
using Services.Services.IServices;
using System.Linq.Expressions;

namespace Services.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<Tag> GetById(int blogId, string userId)
        {
            Expression<Func<Tag, bool>> condition = x => x.BlogId == blogId && x.UserId.Equals(userId);
            var tag = await _tagRepository.GetByCondition(condition);

            return tag.FirstOrDefault() ?? throw new Exception("This user is not taged");
        }

        public async Task<Tag> Post(int blogId, string userId)
        {
            var tag = new Tag
            {
                BlogId = blogId,
                UserId = userId
            };

            await _tagRepository.Post(tag);

            return tag;
        }

        public async Task Delete(Tag tag)
        {
            await _tagRepository.Delete(tag);
        }
    }
}
