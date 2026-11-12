
namespace SchoolAdmission.Domain.Entities;

public class AdmissionClass
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int TotalSeats { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // Required by EF Core
    private AdmissionClass()
    {
    }

    public AdmissionClass(string name, int totalSeats)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Class name is required.", nameof(name));

        if (totalSeats <= 0)
            throw new ArgumentException(
                "Total seats must be greater than zero.",
                nameof(totalSeats));

        Id = Guid.NewGuid();
        Name = name.Trim();
        TotalSeats = totalSeats;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Class name is required.");

        Name = name.Trim();
    }

    public void UpdateTotalSeats(int totalSeats)
    {
        if (totalSeats <= 0)
            throw new ArgumentException(
                "Total seats must be greater than zero.");

        TotalSeats = totalSeats;
    }
}
