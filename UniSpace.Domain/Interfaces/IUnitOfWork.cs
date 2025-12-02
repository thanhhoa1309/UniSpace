using UniSpace.Domain.Entities;

namespace UniSpace.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> User { get; }
        Task<int> SaveChangesAsync();
    }
}
