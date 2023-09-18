using Contratcs.IRepository;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Datebase;
using System.Linq.Expressions;

namespace Persistence.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly BlogManagementDatabase _dbContext;

        public UserRepository(BlogManagementDatabase dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IDbContextTransaction> BeginTransaction()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task<List<User>> GetAll()
        {
            return await _dbContext.User.ToListAsync();
        }

        public async Task<User> GetByCondition(Expression<Func<User, bool>> condition)
        {
            return await _dbContext.User.Where(condition)
                .FirstOrDefaultAsync() ?? throw new Exception("User not found");
        }

        public async Task Post(User user)
        {
            await _dbContext.User.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(User user)
        {
            _dbContext.User.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(User user)
        {
            _dbContext.User.Remove(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
