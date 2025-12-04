using UniSpace.Domain.Entities;

namespace UniSpace.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> User { get; }
        IGenericRepository<Campus> Campus { get; }
        IGenericRepository<Room> Room { get; }
        IGenericRepository<Schedule> Schedule { get; }
        IGenericRepository<Booking> Booking { get; }
        Task<int> SaveChangesAsync();
    }
}
