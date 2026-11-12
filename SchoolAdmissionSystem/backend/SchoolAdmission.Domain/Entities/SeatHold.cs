
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Domain.Entities;

public class SeatHold
{
    public Guid Id { get; private set; }

    public Guid ClassId { get; private set; }

    public string StudentId { get; private set; } = string.Empty;

    public HoldStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }

    // Required by EF Core
    private SeatHold()
    {
    }

    public SeatHold(
        Guid classId,
        string studentId,
        DateTimeOffset expiresAt)
    {
        if (classId == Guid.Empty)
            throw new ArgumentException("Class ID is required.");

        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID is required.");

        var now = DateTimeOffset.UtcNow;

        if (expiresAt <= now)
            throw new ArgumentException(
                "Expiry time must be in the future.");

        Id = Guid.NewGuid();
        ClassId = classId;
        StudentId = studentId.Trim();
        Status = HoldStatus.Held;
        CreatedAt = now;
        ExpiresAt = expiresAt;
    }

    public bool ExpireIfNeeded(DateTimeOffset now)
    {
        if (Status != HoldStatus.Held || now < ExpiresAt)
            return false;

        Status = HoldStatus.Expired;
        return true;
    }

    public void Confirm(DateTimeOffset now)
    {
        if (Status == HoldStatus.Confirmed)
            return;

        ExpireIfNeeded(now);

        if (Status != HoldStatus.Held)
            throw new InvalidOperationException(
                "Only active seat holds can be confirmed.");

        Status = HoldStatus.Confirmed;
        ConfirmedAt = now;
    }

    public void Cancel(DateTimeOffset now)
    {
        ExpireIfNeeded(now);

        if (Status == HoldStatus.Cancelled)
            return;

        if (Status != HoldStatus.Held)
            throw new InvalidOperationException(
                "Only active seat holds can be cancelled.");

        Status = HoldStatus.Cancelled;
    }

    public bool IsActiveAt(DateTimeOffset now)
    {
        return Status == HoldStatus.Held && now < ExpiresAt;
    }
}
