using Contratcs.IRepository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Datebase;
using System.Linq.Expressions;

namespace Persistence.Repository
{
    public class LikesRepository : ILikesRepository
    {
        private readonly BlogManagementDatabase _dbContex;

        public LikesRepository(BlogManagementDatabase dbContex)
        {
            _dbContex = dbContex;
        }

        public Task<IDbContextTransaction> BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Likes>> GetAll()
        {
            return await _dbContex.Like.Include(x => x.User).Include(x => x.Blog).ToListAsync();
        }

        public async Task<List<Likes>> GetByCondition(Expression<Func<Likes, bool>> condition)
        {
            return await _dbContex.Like
                .Include(x => x.User).Include(x => x.Blog)
                .Where(condition).ToListAsync();
        }

        public async Task Post(Likes likes)
        {
            await _dbContex.Like.AddAsync(likes);
            await _dbContex.SaveChangesAsync();
        }

        public async Task Update(Likes likes)
        {
            _dbContex.Like.Update(likes);
            await _dbContex.SaveChangesAsync();
        }

        public async Task Delete(Likes likes)
        {
            _dbContex.Like.Remove(likes);
            await _dbContex.SaveChangesAsync();
        }
    }
}
