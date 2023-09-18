using Contratcs.IRepository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Datebase;
using System.Linq.Expressions;

namespace Persistence.Repository
{
    public class TagRepository : ITagRepository
    {
        private readonly BlogManagementDatabase _dbContext;

        public TagRepository(BlogManagementDatabase dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IDbContextTransaction> BeginTransaction()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task<List<Tag>> GetAll()
        {
            return await _dbContext.Tag.Include(x => x.User).Include(x => x.Blog).ToListAsync();
        }

        public async Task<List<Tag>> GetByCondition(Expression<Func<Tag, bool>> condition)
        {
            return await _dbContext.Tag
                .Include(x => x.User).Include(x => x.Blog)
                .Where(condition).ToListAsync();
        }

        public async Task Post(Tag tag)
        {
            await _dbContext.Tag.AddAsync(tag);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(Tag tag)
        {
            _dbContext.Tag.Update(tag);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Tag tag)
        {
            _dbContext.Tag.Remove(tag);
            await _dbContext.SaveChangesAsync();
        }
    }
}
