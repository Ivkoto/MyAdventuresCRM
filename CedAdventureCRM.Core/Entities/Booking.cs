using CedAdventureCRM.Core.Enums;

namespace CedAdventureCRM.Core.Entities;

internal class Booking
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int DepartureId { get; set; }
    public DateTime BookedOn { get; set; }
    public BookingStatus Status { get; set; }
    public ParticipantType ParticipantType { get; set; } = ParticipantType.Adult;
    public decimal BasePriceAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal FinalPriceAmount { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;

}
