using HotelSearchLemax.Core.Interfaces;
using HotelSearchLemax.DataAccess.Data;
using HotelSearchLemax.DataAccess.Repositories;

namespace HotelSearchLemax.DataAccess;

public class UnitOfWork : IUnitOfWork
{
    private readonly HotelDbContext _context;
    private IHotelRepository? _hotels;
    private IUserRepository? _users;

    public UnitOfWork(HotelDbContext context)
    {
        _context = context;
    }

    public IHotelRepository Hotels => _hotels ??= new HotelRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
