using Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Contratcs.IRepository
{
    public interface ILikesRepository
    {
        Task<IDbContextTransaction> BeginTransaction();
        Task<List<Likes>> GetAll();
        Task<List<Likes>> GetByCondition(Expression<Func<Likes, bool>> condition);
        Task Post(Likes likes);
        Task Update(Likes likes);
        Task Delete(Likes likes);
    }
}
