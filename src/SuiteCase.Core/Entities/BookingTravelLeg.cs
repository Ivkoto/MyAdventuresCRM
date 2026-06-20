using SuiteCase.Core.Enums;

namespace SuiteCase.Core.Entities;

public class BookingTravelLeg
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public TravelLegDirection Direction { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset? TravelDateTime { get; set; }
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
