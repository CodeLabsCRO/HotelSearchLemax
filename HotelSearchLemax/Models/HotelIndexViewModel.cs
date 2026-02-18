using HotelSearchLemax.Core.Entities;

namespace HotelSearchLemax.Models;

public class HotelIndexViewModel
{
    public IEnumerable<Hotel> Hotels { get; set; } = [];
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "Name";
    public string SortDirection { get; set; } = "Asc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static int[] PageSizeOptions => [5, 10, 25, 50, 100];
}
