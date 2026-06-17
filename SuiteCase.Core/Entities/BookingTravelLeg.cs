using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class BookingTravelLeg
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public TravelLegDirection Direction { get; set; }
    public string? Location { get; set; }
    public DateTime? TravelDateTime { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
