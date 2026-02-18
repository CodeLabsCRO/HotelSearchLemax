using HotelSearchLemax.Core.DTOs;
using HotelSearchLemax.Core.Entities;

namespace HotelSearchLemax.Services.Mapping;

public static class HotelMappingExtensions
{
    public static HotelDto ToDto(this Hotel hotel)
    {
        return new HotelDto
        {
            Id = hotel.Id,
            Name = hotel.Name,
            Price = hotel.Price,
            Latitude = hotel.Latitude,
            Longitude = hotel.Longitude
        };
    }

    public static Hotel ToEntity(this CreateHotelDto dto)
    {
        return new Hotel
        {
            Name = dto.Name,
            Price = dto.Price,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };
    }

    public static void UpdateFrom(this Hotel hotel, UpdateHotelDto dto)
    {
        hotel.Name = dto.Name;
        hotel.Price = dto.Price;
        hotel.Latitude = dto.Latitude;
        hotel.Longitude = dto.Longitude;
    }

    public static HotelSearchResultDto ToSearchResultDto(this Hotel hotel, double distanceKm, double score)
    {
        return new HotelSearchResultDto
        {
            Id = hotel.Id,
            Name = hotel.Name,
            Price = hotel.Price,
            Latitude = hotel.Latitude,
            Longitude = hotel.Longitude,
            DistanceKm = Math.Round(distanceKm, 2),
            Score = Math.Round(score, 4)
        };
    }
}
