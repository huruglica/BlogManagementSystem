using Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Contratcs.IRepository
{
    public interface IUserRepository
    {
        Task<IDbContextTransaction> BeginTransaction();
        Task<List<User>> GetAll();
        Task<User> GetByCondition(Expression<Func<User, bool>> condition);
        Task Post(User user);
        Task Update(User user);
        Task Delete(User user);
    }
}
