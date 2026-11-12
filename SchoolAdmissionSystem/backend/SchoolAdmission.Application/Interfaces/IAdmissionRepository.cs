
using SchoolAdmission.Application.DTOs;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Application.Interfaces;

public interface IAdmissionRepository
{
    Task<AdmissionClass> CreateClassAsync(
        AdmissionClass admissionClass);

    Task<List<AdmissionClass>> GetClassesAsync();

    Task<AdmissionClass?> GetClassAsync(Guid classId);

    Task<SeatCountsDto> GetSeatCountsAsync(Guid classId);

    // Must be implemented atomically using a PostgreSQL transaction
    // and a row-level lock on the admission class.
    Task<bool> TryCreateHoldAsync(SeatHold hold);

    Task<SeatHold?> GetHoldAsync(Guid holdId);

    // Must lock the hold row and make confirmation idempotent.
    Task<SeatHold?> ConfirmHoldAsync(
        Guid holdId,
        string studentId,
        DateTimeOffset now);

    Task<bool> CancelHoldAsync(
        Guid holdId,
        string studentId,
        DateTimeOffset now);

    Task<List<SeatHold>> GetStudentHoldsAsync(string studentId);

    // Updates HELD records whose ExpiresAt <= now to EXPIRED.
    Task ExpireHoldsAsync(DateTimeOffset now);
}
