
namespace SchoolAdmission.Application.DTOs;

public record SeatHoldDto(
    Guid HoldId,
    Guid ClassId,
    string StudentId,
    string Status,
    DateTimeOffset ExpiresAt);
