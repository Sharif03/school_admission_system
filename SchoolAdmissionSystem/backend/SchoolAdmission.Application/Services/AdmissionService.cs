
using SchoolAdmission.Application.DTOs;
using SchoolAdmission.Application.Interfaces;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Application.Services;

public class AdmissionService : IAdmissionService
{
    private readonly IAdmissionRepository _repository;

    private readonly TimeSpan _holdDuration;

    public AdmissionService(
        IAdmissionRepository repository,
        TimeSpan holdDuration)
    {
        _repository = repository;
        _holdDuration = holdDuration;

        if (holdDuration <= TimeSpan.Zero)
            throw new ArgumentException(
                "Hold duration must be greater than zero.");
    }

    public async Task<AdmissionClassDto> CreateClassAsync(
        CreateClassRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var admissionClass = new AdmissionClass(
            request.Name,
            request.TotalSeats);

        await _repository.CreateClassAsync(admissionClass);

        return await MapClassAsync(admissionClass);
    }

    public async Task<List<AdmissionClassDto>> GetClassesAsync()
    {
        var now = DateTimeOffset.UtcNow;

        await _repository.ExpireHoldsAsync(now);

        var classes = await _repository.GetClassesAsync();

        var results = new List<AdmissionClassDto>();

        foreach (var admissionClass in classes)
        {
            results.Add(await MapClassAsync(admissionClass));
        }

        return results;
    }

    public async Task<AdmissionClassDto> GetClassAsync(Guid classId)
    {
        if (classId == Guid.Empty)
            throw new ArgumentException("Invalid class ID.");

        await _repository.ExpireHoldsAsync(
            DateTimeOffset.UtcNow);

        var admissionClass =
            await _repository.GetClassAsync(classId);

        if (admissionClass is null)
            throw new KeyNotFoundException("Class not found.");

        return await MapClassAsync(admissionClass);
    }

    public async Task<SeatHoldDto> CreateHoldAsync(
        Guid classId,
        string studentId)
    {
        ValidateStudentId(studentId);

        if (classId == Guid.Empty)
            throw new ArgumentException("Invalid class ID.");

        var admissionClass =
            await _repository.GetClassAsync(classId);

        if (admissionClass is null)
            throw new KeyNotFoundException("Class not found.");

        var now = DateTimeOffset.UtcNow;

        var hold = new SeatHold(
            classId,
            studentId,
            now.Add(_holdDuration));

        var created = await _repository.TryCreateHoldAsync(hold);

        if (!created)
            throw new InvalidOperationException(
                "No seats available or an active application already exists.");

        return MapHold(hold);
    }

    public async Task<SeatHoldDto> GetHoldAsync(
        Guid holdId,
        string studentId)
    {
        ValidateStudentId(studentId);

        if (holdId == Guid.Empty)
            throw new ArgumentException("Invalid hold ID.");

        await _repository.ExpireHoldsAsync(
            DateTimeOffset.UtcNow);

        var hold = await _repository.GetHoldAsync(holdId);

        if (hold is null || hold.StudentId != studentId.Trim())
            throw new KeyNotFoundException("Hold not found.");

        return MapHold(hold);
    }

    public async Task<ConfirmHoldResponse> ConfirmHoldAsync(
        Guid holdId,
        string studentId)
    {
        ValidateStudentId(studentId);

        if (holdId == Guid.Empty)
            throw new ArgumentException("Invalid hold ID.");

        var hold = await _repository.ConfirmHoldAsync(
            holdId,
            studentId.Trim(),
            DateTimeOffset.UtcNow);

        if (hold is null)
            throw new KeyNotFoundException(
                "Hold not found or it cannot be confirmed.");

        return new ConfirmHoldResponse(
            hold.Id,
            hold.Status.ToString().ToUpperInvariant());
    }

    public async Task CancelHoldAsync(
        Guid holdId,
        string studentId)
    {
        ValidateStudentId(studentId);

        if (holdId == Guid.Empty)
            throw new ArgumentException("Invalid hold ID.");

        var cancelled = await _repository.CancelHoldAsync(
            holdId,
            studentId.Trim(),
            DateTimeOffset.UtcNow);

        if (!cancelled)
            throw new KeyNotFoundException(
                "Hold not found or it cannot be cancelled.");
    }

    public async Task<List<MyHoldDto>> GetMyHoldsAsync(
        string studentId)
    {
        ValidateStudentId(studentId);

        await _repository.ExpireHoldsAsync(
            DateTimeOffset.UtcNow);

        var holds = await _repository.GetStudentHoldsAsync(
            studentId.Trim());

        var results = new List<MyHoldDto>();

        foreach (var hold in holds)
        {
            var admissionClass =
                await _repository.GetClassAsync(hold.ClassId);

            if (admissionClass is null)
                continue;

            results.Add(new MyHoldDto(
                hold.Id,
                hold.ClassId,
                admissionClass.Name,
                hold.Status.ToString().ToUpperInvariant(),
                hold.ExpiresAt));
        }

        return results;
    }

    private async Task<AdmissionClassDto> MapClassAsync(
        AdmissionClass admissionClass)
    {
        var counts = await _repository.GetSeatCountsAsync(
            admissionClass.Id);

        return new AdmissionClassDto(
            admissionClass.Id,
            admissionClass.Name,
            admissionClass.TotalSeats,
            counts.HeldSeats,
            counts.ConfirmedSeats,
            Math.Max(
                0,
                admissionClass.TotalSeats - counts.OccupiedSeats));
    }

    private static SeatHoldDto MapHold(SeatHold hold)
    {
        return new SeatHoldDto(
            hold.Id,
            hold.ClassId,
            hold.StudentId,
            hold.Status.ToString().ToUpperInvariant(),
            hold.ExpiresAt);
    }

    private static void ValidateStudentId(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException(
                "Student ID is required.");

        studentId = studentId.Trim();

        if (studentId.Length != 17)
            throw new ArgumentException(
                "Student ID must be exactly 17 digits.");

        if (!studentId.All(char.IsDigit))
            throw new ArgumentException(
                "Student ID must contain digits only.");
    }
}
