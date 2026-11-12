
namespace SchoolAdmission.Application.DTOs;

public record AdmissionClassDto(
    Guid Id,
    string Name,
    int TotalSeats,
    int HeldSeats,
    int ConfirmedSeats,
    int AvailableSeats);
