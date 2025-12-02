using UniSpace.Domain.Entities;
using UniSpace.Domain.Interfaces;

namespace UniSpace.Domain
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UniSpaceDbContext _dbContext;

        public UnitOfWork(
            UniSpaceDbContext dbContext,
            IGenericRepository<User> userRepository,
            IGenericRepository<Campus> campusRepository,
            IGenericRepository<Room> roomRepository)
        {
            _dbContext = dbContext;
            User = userRepository;
            Campus = campusRepository;
            Room = roomRepository;
        }

        public IGenericRepository<User> User { get; }
        public IGenericRepository<Campus> Campus { get; }
        public IGenericRepository<Room> Room { get; }

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
