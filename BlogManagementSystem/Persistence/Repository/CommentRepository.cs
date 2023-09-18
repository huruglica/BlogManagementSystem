using Contratcs.IRepository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Datebase;
using System.Linq.Expressions;

namespace Persistence.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly BlogManagementDatabase _dbContext;

        public CommentRepository(BlogManagementDatabase dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IDbContextTransaction> BeginTransaction()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task<List<Comment>> GetAll(Expression<Func<Comment, bool>> condition)
        {
            return await _dbContext.Comment
                .Where(condition)
                .Include(x => x.Reply)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetBlogComments(int blogId)
        {
            return await _dbContext.Comment
                .Where(x => x.BlogId == blogId)
                .Include(x => x.Reply)
                .ToListAsync();
        }

        public async Task<Comment> GetByCondition(Expression<Func<Comment, bool>> condition)
        {
            return await _dbContext.Comment.Where(condition)
                         .Include(x => x.Reply)
                         .FirstOrDefaultAsync()
                         ?? throw new Exception("Comment not found");
        }

        public async Task Post(Comment comment)
        {
            await _dbContext.Comment.AddAsync(comment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(Comment comment)
        {
            _dbContext.Comment.Update(comment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Comment comment)
        {
            _dbContext.Comment.Remove(comment);
            await _dbContext.SaveChangesAsync();
        }
    }
}
