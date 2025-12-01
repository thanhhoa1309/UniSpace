namespace UniSpace.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {

        Task<int> SaveChangesAsync();
    }
}
