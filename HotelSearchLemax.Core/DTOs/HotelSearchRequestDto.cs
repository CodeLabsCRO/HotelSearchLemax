using System.ComponentModel.DataAnnotations;

namespace HotelSearchLemax.Core.DTOs;

public class HotelSearchRequestDto
{
    [Required]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double UserLatitude { get; set; }

    [Required]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double UserLongitude { get; set; }

    /// <summary>
    /// Optional radius in kilometers. If specified, only hotels within this radius are returned.
    /// </summary>
    [Range(0.1, 20000, ErrorMessage = "Radius must be between 0.1 and 20000 km")]
    public double? RadiusKm { get; set; }

    /// <summary>
    /// Optional search keywords to filter hotels by name (comma-separated).
    /// </summary>
    public string? SearchKeywords { get; set; }

    /// <summary>
    /// Sort by field: Score (default), Name, Price, Distance
    /// </summary>
    public SortBy SortBy { get; set; } = SortBy.Score;

    /// <summary>
    /// Sort direction: Asc or Desc
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Asc;

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
}

public enum SortBy
{
    Score,
    Name,
    Price,
    Distance
}

public enum SortDirection
{
    Asc,
    Desc
}
