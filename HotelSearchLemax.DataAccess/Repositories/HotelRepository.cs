using HotelSearchLemax.Core.Entities;
using HotelSearchLemax.Core.Interfaces;
using HotelSearchLemax.DataAccess.Data;

namespace HotelSearchLemax.DataAccess.Repositories;

public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public HotelRepository(HotelDbContext context) : base(context)
    {
    }
}
