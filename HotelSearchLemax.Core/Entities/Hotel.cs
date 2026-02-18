namespace HotelSearchLemax.Core.Entities;

public class Hotel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
