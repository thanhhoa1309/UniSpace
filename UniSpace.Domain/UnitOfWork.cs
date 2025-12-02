using UniSpace.Domain.Entities;
using UniSpace.Domain.Interfaces;

namespace UniSpace.Domain
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UniSpaceDbContext _dbContext;

        public UnitOfWork(UniSpaceDbContext dbContext,
            IGenericRepository<User> userRepository

            )
        {
            _dbContext = dbContext;
            User = userRepository;

        }

        public IGenericRepository<User> User { get; }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
