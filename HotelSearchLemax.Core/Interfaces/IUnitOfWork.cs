namespace HotelSearchLemax.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IHotelRepository Hotels { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync();
}
