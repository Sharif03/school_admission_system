using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.Application.DTOs;
using SchoolAdmission.Domain.Enums;
using SchoolAdmission.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SchoolAdmission.Tests;

public class AdmissionConcurrencyTests
{
    [Fact]
    public async Task ConcurrentHoldRequests_ShouldNeverExceedClassCapacity()
    {
        // Arrange: Start a real PostgreSQL container.
        await using var postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("school_admission_test_db")
            .WithUsername("postgres")
            .WithPassword("root")
            .Build();

        await postgres.StartAsync();

        // Start the API using the Testcontainers database.
        using var factory = new TestApiFactory(
            postgres.GetConnectionString());

        using var client = factory.CreateClient();

        // Create a class with 10 seats.
        var createClassResponse = await client.PostAsJsonAsync(
            "/classes",
            new
            {
                name = "Concurrency Test Class",
                totalSeats = 10
            });

        createClassResponse.EnsureSuccessStatusCode();

        var createdClass =
            await createClassResponse.Content
                .ReadFromJsonAsync<AdmissionClassDto>();

        Assert.NotNull(createdClass);

        // Act: Prepare 20 simultaneous HTTP requests.
        const int totalRequests = 20;
        const int classCapacity = 10;

        var startGate = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var requests = Enumerable.Range(1, totalRequests)
            .Select(async i =>
            {
                // All requests wait until the same start signal.
                await startGate.Task;

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"/classes/{createdClass.Id}/holds");

                var studentId = $"100000000000000{i:D2}";

                request.Headers.Add("X-Student-Id", studentId);

                return await client.SendAsync(request);
            })
            .ToArray();

        // Release all 20 requests together.
        startGate.SetResult(true);

        var responses = await Task.WhenAll(requests);

        try
        {
            // Assert: Verify HTTP response counts.
            var successfulRequests = responses.Count(
                response => response.IsSuccessStatusCode);

            var rejectedRequests = responses.Count(
                response => response.StatusCode == HttpStatusCode.Conflict);

            Assert.Equal(10, successfulRequests);
            Assert.Equal(10, rejectedRequests);

            // Assert: Verify actual PostgreSQL database records.
            await using var scope =
                factory.Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var occupiedSeats = await dbContext.SeatHolds
                .AsNoTracking()
                .CountAsync(hold =>
                    hold.ClassId == createdClass.Id &&
                    (hold.Status == HoldStatus.Held ||
                     hold.Status == HoldStatus.Confirmed));

            Assert.Equal(classCapacity, occupiedSeats);
            Assert.True(occupiedSeats <= classCapacity);
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    private sealed class TestApiFactory
        : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public TestApiFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(
                (_, configuration) =>
                {
                    var settings = new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            _connectionString,

                        ["HOLD_DURATION_SECONDS"] = "600"
                    };

                    configuration.AddInMemoryCollection(settings);
                });
        }
    }
}
