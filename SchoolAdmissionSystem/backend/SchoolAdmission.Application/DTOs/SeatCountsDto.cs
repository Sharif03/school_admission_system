
namespace SchoolAdmission.Application.DTOs;

public record SeatCountsDto(
    int HeldSeats,
    int ConfirmedSeats)
{
    public int OccupiedSeats => HeldSeats + ConfirmedSeats;
}
