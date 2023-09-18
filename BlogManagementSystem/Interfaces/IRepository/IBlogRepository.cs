using Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Contratcs.IRepository
{
    public interface IBlogRepository
    {
        Task<IDbContextTransaction> BeginTransaction();
        Task<List<Blog>> GetAll(Expression<Func<Blog, bool>> condition);
        Task<Blog> GetByCondition(Expression<Func<Blog, bool>> condition);
        Task Post(Blog blog);
        Task Update(Blog blog);
        Task Delete(Blog blog);
    }
}
