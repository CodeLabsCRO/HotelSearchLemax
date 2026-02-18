using HotelSearchLemax.Core.Entities;
using HotelSearchLemax.Core.Interfaces;
using HotelSearchLemax.DataAccess.Data;

namespace HotelSearchLemax.DataAccess.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(HotelDbContext context) : base(context)
    {
    }
}
