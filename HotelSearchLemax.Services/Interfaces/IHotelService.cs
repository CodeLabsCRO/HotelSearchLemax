using HotelSearchLemax.Core.DTOs;

namespace HotelSearchLemax.Services.Interfaces;

public interface IHotelService
{
    Task<IEnumerable<HotelDto>> GetAllHotelsAsync();
    Task<HotelDto?> GetHotelByIdAsync(int id);
    Task<HotelDto> CreateHotelAsync(CreateHotelDto createDto);
    Task<HotelDto?> UpdateHotelAsync(int id, UpdateHotelDto updateDto);
    Task<bool> DeleteHotelAsync(int id);
    Task<PaginatedResult<HotelSearchResultDto>> SearchHotelsAsync(HotelSearchRequestDto searchRequest);
}
