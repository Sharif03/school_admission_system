
using SchoolAdmission.Application.DTOs;

namespace SchoolAdmission.Application.Interfaces;

public interface IAdmissionService
{
    Task<AdmissionClassDto> CreateClassAsync(
        CreateClassRequest request);

    Task<List<AdmissionClassDto>> GetClassesAsync();

    Task<AdmissionClassDto> GetClassAsync(Guid classId);

    Task<SeatHoldDto> CreateHoldAsync(
        Guid classId,
        string studentId);

    Task<SeatHoldDto> GetHoldAsync(
        Guid holdId,
        string studentId);

    Task<ConfirmHoldResponse> ConfirmHoldAsync(
        Guid holdId,
        string studentId);

    Task CancelHoldAsync(
        Guid holdId,
        string studentId);

    Task<List<MyHoldDto>> GetMyHoldsAsync(
        string studentId);
}
