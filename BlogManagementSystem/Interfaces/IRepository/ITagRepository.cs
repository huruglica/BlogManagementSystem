using Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Contratcs.IRepository
{
    public interface ITagRepository
    {
        Task<IDbContextTransaction> BeginTransaction();
        Task<List<Tag>> GetAll();
        Task<List<Tag>> GetByCondition(Expression<Func<Tag, bool>> condition);
        Task Post(Tag tag);
        Task Update(Tag tag);
        Task Delete(Tag tag);
    }
}
