using GeoCoordinatePortable;
using HotelSearchLemax.Core.DTOs;
using HotelSearchLemax.Core.Entities;
using HotelSearchLemax.Core.Interfaces;
using HotelSearchLemax.Services.Interfaces;
using HotelSearchLemax.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace HotelSearchLemax.Services.Implementations;

public class HotelService : IHotelService
{
    private readonly IUnitOfWork _unitOfWork;

    public HotelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<HotelDto>> GetAllHotelsAsync()
    {
        var hotels = await _unitOfWork.Hotels.GetAllAsync();
        return hotels.Select(h => h.ToDto());
    }

    public async Task<HotelDto?> GetHotelByIdAsync(int id)
    {
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
        return hotel?.ToDto();
    }

    public async Task<HotelDto> CreateHotelAsync(CreateHotelDto createDto)
    {
        var hotel = createDto.ToEntity();
        await _unitOfWork.Hotels.AddAsync(hotel);
        await _unitOfWork.SaveChangesAsync();
        return hotel.ToDto();
    }

    public async Task<HotelDto?> UpdateHotelAsync(int id, UpdateHotelDto updateDto)
    {
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
        if (hotel == null)
            return null;

        hotel.UpdateFrom(updateDto);
        _unitOfWork.Hotels.Update(hotel);
        await _unitOfWork.SaveChangesAsync();
        return hotel.ToDto();
    }

    public async Task<bool> DeleteHotelAsync(int id)
    {
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
        if (hotel == null)
            return false;

        _unitOfWork.Hotels.Remove(hotel);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<PaginatedResult<HotelSearchResultDto>> SearchHotelsAsync(HotelSearchRequestDto request)
    {
        // Load hotels with optional keyword filter (SQL)
        var query = _unitOfWork.Hotels.Query();

        if (!string.IsNullOrWhiteSpace(request.SearchKeywords))
        {
            var keyword = request.SearchKeywords.Trim().ToLower();
            query = query.Where(h => h.Name.ToLower().Contains(keyword));
        }

        var hotels = await query.ToListAsync();

        if (hotels.Count == 0)
        {
            return new PaginatedResult<HotelSearchResultDto>
            {
                Items = [],
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = 0
            };
        }

        // Calculate distances using GeoCoordinate 
        var userLocation = new GeoCoordinate(request.UserLatitude, request.UserLongitude);

        var hotelsWithDistance = hotels
            .Select(h => new
            {
                Hotel = h,
                Distance = new GeoCoordinate(h.Latitude, h.Longitude).GetDistanceTo(userLocation) / 1000.0
            })
            .Where(x => !request.RadiusKm.HasValue || x.Distance <= request.RadiusKm.Value)
            .ToList();

        if (hotelsWithDistance.Count == 0)
        {
            return new PaginatedResult<HotelSearchResultDto>
            {
                Items = [],
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = 0
            };
        }

        // Calculate normalized scores
        var maxPrice = hotelsWithDistance.Max(x => (double)x.Hotel.Price);
        var minPrice = hotelsWithDistance.Min(x => (double)x.Hotel.Price);
        var maxDistance = hotelsWithDistance.Max(x => x.Distance);
        var minDistance = hotelsWithDistance.Min(x => x.Distance);

        var priceRange = maxPrice - minPrice;
        var distanceRange = maxDistance - minDistance;

        var results = hotelsWithDistance
            .Select(x =>
            {
                var normalizedPrice = priceRange > 0 ? ((double)x.Hotel.Price - minPrice) / priceRange : 0;
                var normalizedDistance = distanceRange > 0 ? (x.Distance - minDistance) / distanceRange : 0;
                var score = (normalizedPrice + normalizedDistance) / 2;

                return x.Hotel.ToSearchResultDto(x.Distance, score);
            })
            .ToList();

        // Sort
        results = request.SortBy switch
        {
            SortBy.Name => request.SortDirection == SortDirection.Asc
                ? results.OrderBy(x => x.Name).ToList()
                : results.OrderByDescending(x => x.Name).ToList(),
            SortBy.Price => request.SortDirection == SortDirection.Asc
                ? results.OrderBy(x => x.Price).ToList()
                : results.OrderByDescending(x => x.Price).ToList(),
            SortBy.Distance => request.SortDirection == SortDirection.Asc
                ? results.OrderBy(x => x.DistanceKm).ToList()
                : results.OrderByDescending(x => x.DistanceKm).ToList(),
            _ => request.SortDirection == SortDirection.Asc
                ? results.OrderBy(x => x.Score).ToList()
                : results.OrderByDescending(x => x.Score).ToList()
        };

        var totalCount = results.Count;

        // Paginate
        var pagedResults = results
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResult<HotelSearchResultDto>
        {
            Items = pagedResults,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
