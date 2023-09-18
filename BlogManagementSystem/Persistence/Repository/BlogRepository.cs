using Contratcs.IRepository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Datebase;
using System.Linq.Expressions;

namespace Persistence.Repository
{
    public class BlogRepository : IBlogRepository
    {
        private readonly BlogManagementDatabase _dbContext;

        public BlogRepository(BlogManagementDatabase dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IDbContextTransaction> BeginTransaction()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task<List<Blog>> GetAll(Expression<Func<Blog, bool>> condition)
        {
            return await _dbContext.Blog
                .Where(condition)
                .Include(x => x.User)
                .Include(x => x.Comments)
                .ThenInclude(x => x.Reply)
                .ToListAsync();
        }

        public async Task<Blog> GetByCondition(Expression<Func<Blog, bool>> condition)
        {
            return await _dbContext.Blog.Where(condition)
                .Include(x => x.User)
                .Include(x => x.Tags)
                .ThenInclude(x => x.User)
                .Include(x => x.Likes)
                .ThenInclude(x => x.User)
                .Include(x => x.Comments)
                .ThenInclude(x => x.Reply)
                .FirstOrDefaultAsync()
                ?? throw new Exception("Blog not found");
        }

        public async Task Post(Blog blog)
        {
            await _dbContext.Blog.AddAsync(blog);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(Blog blog)
        {
            _dbContext.Blog.Update(blog);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Blog blog)
        {
            _dbContext.Blog.Remove(blog);
            await _dbContext.SaveChangesAsync();
        }
    }
}
