
using Moq;
using SchoolAdmission.Application.Interfaces;
using SchoolAdmission.Application.Services;
using SchoolAdmission.Domain.Entities;
using Xunit;

namespace SchoolAdmission.Tests;

public class AdmissionServiceTests
{
    private readonly Mock<IAdmissionRepository> _repositoryMock;
    private readonly AdmissionService _service;

    private const string ValidStudentId = "12345678901234567";

    public AdmissionServiceTests()
    {
        _repositoryMock = new Mock<IAdmissionRepository>();

        _service = new AdmissionService(
            _repositoryMock.Object,
            TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task CreateHoldAsync_InvalidStudentId_ShouldThrowException()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var invalidStudentId = "12345";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateHoldAsync(classId, invalidStudentId));

        _repositoryMock.Verify(
            x => x.GetClassAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateHoldAsync_ExistingActiveApplication_ShouldThrowConflict()
    {
        // Arrange
        var admissionClass = new AdmissionClass("Class One", 1);

        _repositoryMock
            .Setup(x => x.GetClassAsync(admissionClass.Id))
            .ReturnsAsync(admissionClass);

        _repositoryMock
            .Setup(x => x.TryCreateHoldAsync(It.IsAny<SeatHold>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateHoldAsync(
                admissionClass.Id,
                ValidStudentId));

        Assert.Contains("active application", exception.Message);

        _repositoryMock.Verify(
            x => x.TryCreateHoldAsync(It.IsAny<SeatHold>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateHoldAsync_ValidRequest_ShouldCreateHold()
    {
        // Arrange
        var admissionClass = new AdmissionClass("Class One", 2);

        _repositoryMock
            .Setup(x => x.GetClassAsync(admissionClass.Id))
            .ReturnsAsync(admissionClass);

        _repositoryMock
            .Setup(x => x.TryCreateHoldAsync(It.IsAny<SeatHold>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateHoldAsync(
            admissionClass.Id,
            ValidStudentId);

        // Assert
        Assert.NotEqual(Guid.Empty, result.HoldId);
        Assert.Equal(admissionClass.Id, result.ClassId);
        Assert.Equal(ValidStudentId, result.StudentId);
        Assert.Equal("HELD", result.Status);
        Assert.True(result.ExpiresAt > DateTimeOffset.UtcNow);

        _repositoryMock.Verify(
            x => x.TryCreateHoldAsync(It.IsAny<SeatHold>()),
            Times.Once);
    }
}
