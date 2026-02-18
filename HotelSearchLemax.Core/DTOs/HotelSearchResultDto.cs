namespace HotelSearchLemax.Core.DTOs;

public class HotelSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceKm { get; set; }
    public double Score { get; set; }
}
