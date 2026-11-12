
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SchoolAdmission.Application.DTOs;
using SchoolAdmission.Application.Interfaces;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;
using SchoolAdmission.Infrastructure.Persistence;

namespace SchoolAdmission.Infrastructure.Repositories;

public class AdmissionRepository : IAdmissionRepository
{
    private readonly ApplicationDbContext _context;

    public AdmissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdmissionClass> CreateClassAsync(
        AdmissionClass admissionClass)
    {
        _context.AdmissionClasses.Add(admissionClass);

        await _context.SaveChangesAsync();

        return admissionClass;
    }

    public async Task<List<AdmissionClass>> GetClassesAsync()
    {
        return await _context.AdmissionClasses
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<AdmissionClass?> GetClassAsync(Guid classId)
    {
        return await _context.AdmissionClasses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == classId);
    }

    public async Task<SeatCountsDto> GetSeatCountsAsync(Guid classId)
    {
        var now = DateTimeOffset.UtcNow;

        var heldSeats = await _context.SeatHolds.CountAsync(x =>
            x.ClassId == classId &&
            x.Status == HoldStatus.Held &&
            x.ExpiresAt > now);

        var confirmedSeats = await _context.SeatHolds.CountAsync(x =>
            x.ClassId == classId &&
            x.Status == HoldStatus.Confirmed);

        return new SeatCountsDto(heldSeats, confirmedSeats);
    }

    // Atomically reserves a seat using a PostgreSQL row-level lock.
    public async Task<bool> TryCreateHoldAsync(SeatHold hold)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Lock the admission class row.
            // Other reservation requests for this class must wait.
            var admissionClass = await _context.AdmissionClasses
                .FromSqlInterpolated($"""
                    SELECT *
                    FROM admission_classes
                    WHERE "Id" = {hold.ClassId}
                    FOR UPDATE
                    """)
                .AsNoTracking()
                .SingleOrDefaultAsync();

            if (admissionClass is null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            var now = hold.CreatedAt;

            // Release expired holds before checking capacity
            // and the student's existing application.
            await ExpireClassHoldsAsync(hold.ClassId, now);

            var occupiedSeats = await _context.SeatHolds.CountAsync(x =>
                x.ClassId == hold.ClassId &&
                (
                    x.Status == HoldStatus.Confirmed ||
                    (x.Status == HoldStatus.Held && x.ExpiresAt > now)
                ));

            if (occupiedSeats >= admissionClass.TotalSeats)
            {
                await transaction.RollbackAsync();
                return false;
            }

            _context.SeatHolds.Add(hold);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return true;
        }
        catch (DbUpdateException exception)
            when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync();

            return false;
        }
    }

    public async Task<SeatHold?> GetHoldAsync(Guid holdId)
    {
        return await _context.SeatHolds
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == holdId);
    }

    // Locks the hold row and safely handles repeated confirmation.
    public async Task<SeatHold?> ConfirmHoldAsync(
        Guid holdId,
        string studentId,
        DateTimeOffset now)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var hold = await _context.SeatHolds
            .FromSqlInterpolated($"""
                SELECT *
                FROM seat_holds
                WHERE "Id" = {holdId}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync();

        if (hold is null ||
            hold.StudentId != studentId)
        {
            await transaction.RollbackAsync();
            return null;
        }

        // Repeated confirmation is idempotent.
        if (hold.Status == HoldStatus.Confirmed)
        {
            await transaction.CommitAsync();
            return hold;
        }

        if (!hold.IsActiveAt(now))
        {
            hold.ExpireIfNeeded(now);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return null;
        }

        hold.Confirm(now);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return hold;
    }

    public async Task<bool> CancelHoldAsync(
        Guid holdId,
        string studentId,
        DateTimeOffset now)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var hold = await _context.SeatHolds
            .FromSqlInterpolated($"""
                SELECT *
                FROM seat_holds
                WHERE "Id" = {holdId}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync();

        if (hold is null ||
            hold.StudentId != studentId)
        {
            await transaction.RollbackAsync();
            return false;
        }

        // Repeated cancellation is safe.
        if (hold.Status == HoldStatus.Cancelled)
        {
            await transaction.CommitAsync();
            return true;
        }

        if (!hold.IsActiveAt(now))
        {
            hold.ExpireIfNeeded(now);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return false;
        }

        hold.Cancel(now);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return true;
    }

    public async Task<List<SeatHold>> GetStudentHoldsAsync(
        string studentId)
    {
        return await _context.SeatHolds
            .AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    // Marks every expired HELD reservation as EXPIRED.
    public async Task ExpireHoldsAsync(DateTimeOffset now)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE seat_holds
            SET "Status" = {(int)HoldStatus.Expired}
            WHERE "Status" = {(int)HoldStatus.Held}
              AND "ExpiresAt" <= {now}
            """);
    }

    private async Task ExpireClassHoldsAsync(
        Guid classId,
        DateTimeOffset now)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE seat_holds
            SET "Status" = {(int)HoldStatus.Expired}
            WHERE "ClassId" = {classId}
              AND "Status" = {(int)HoldStatus.Held}
              AND "ExpiresAt" <= {now}
            """);
    }

    private static bool IsUniqueViolation(Exception exception)
    {
        while (exception is not null)
        {
            if (exception is PostgresException postgresException &&
                postgresException.SqlState ==
                PostgresErrorCodes.UniqueViolation)
            {
                return true;
            }

            exception = exception.InnerException!;
        }

        return false;
    }
}
