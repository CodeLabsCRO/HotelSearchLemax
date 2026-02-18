namespace HotelSearchLemax.Core.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}
