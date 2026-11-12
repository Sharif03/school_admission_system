
namespace SchoolAdmission.Application.DTOs;

public record MyHoldDto(
    Guid HoldId,
    Guid ClassId,
    string ClassName,
    string Status,
    DateTimeOffset ExpiresAt);
