namespace HotelSearchLemax.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IHotelRepository Hotels { get; }
    Task<int> SaveChangesAsync();
}
