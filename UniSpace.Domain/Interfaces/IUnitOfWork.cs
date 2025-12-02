using UniSpace.Domain.Entities;

namespace UniSpace.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> User { get; }
        IGenericRepository<Campus> Campus { get; }
        IGenericRepository<Room> Room { get; }
        Task<int> SaveChangesAsync();
    }
}
