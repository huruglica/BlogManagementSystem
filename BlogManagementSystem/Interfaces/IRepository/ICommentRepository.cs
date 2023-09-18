using Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Contratcs.IRepository
{
    public interface ICommentRepository
    {
        Task<IDbContextTransaction> BeginTransaction();
        Task<List<Comment>> GetAll(Expression<Func<Comment, bool>> condition);
        Task<List<Comment>> GetBlogComments(int blogId);
        Task<Comment> GetByCondition(Expression<Func<Comment, bool>> condition);
        Task Post (Comment comment);
        Task Update(Comment comment);
        Task Delete (Comment comment);
    }
}
