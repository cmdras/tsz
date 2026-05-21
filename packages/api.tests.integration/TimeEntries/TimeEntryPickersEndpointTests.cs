using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Common.Database;
using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.LeaveTypes;
using Api.Modules.UserLeaveAllowances;
using Api.Modules.Users;
using Api.Tests.Integration.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.TimeEntries;

public class TimeEntryPickersEndpointShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedContractAsync(Guid consultantId, string customerName, string subject, DateOnly startDate, DateOnly? endDate = null)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var customer = new Customer { Id = Guid.NewGuid(), Number = new Random().Next(100000, 999999), Name = customerName, Country = "Belgium" };
        context.Customers.Add(customer);

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Number = new Random().Next(100000, 999999),
            CustomerId = customer.Id,
            Customer = customer,
            ConsultantId = consultantId,
            Subject = subject,
            StartDate = startDate,
            EndDate = endDate,
            IsArchived = false,
            Tasks = [new ContractTask { Id = Guid.NewGuid(), Name = "Development", DayRate = 800m }],
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Return_Ok_With_Empty_Lists_When_No_Data()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/pickers?weekStart=2026-05-18");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        Assert.Equal(0, json.GetProperty("availableTasks").GetArrayLength());
        Assert.Equal(0, json.GetProperty("availableLeaveTypes").GetArrayLength());
    }

    [Fact]
    public async Task Return_Bad_Request_When_WeekStart_Is_Not_Monday()
    {
        var response = await factory.Client.GetAsync("/api/time-entries/pickers?weekStart=2026-05-19");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Return_Leave_Types_When_Present()
    {
        await factory.SeedLeaveTypeAsync("Annual Leave");

        var response = await factory.Client.GetAsync("/api/time-entries/pickers?weekStart=2026-05-18");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        Assert.Equal(1, json.GetProperty("availableLeaveTypes").GetArrayLength());
    }

    [Fact]
    public async Task Return_Task_For_Active_Contract_Belonging_To_Current_User()
    {
        await SeedContractAsync(
            consultantId: Guid.Empty,
            customerName: "Acme",
            subject: "Cloud Migration",
            startDate: new DateOnly(2026, 1, 1));

        var response = await factory.Client.GetAsync("/api/time-entries/pickers?weekStart=2026-05-18");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        Assert.Equal(1, json.GetProperty("availableTasks").GetArrayLength());
        var task = json.GetProperty("availableTasks")[0];
        Assert.Equal("Acme", task.GetProperty("customerName").GetString());
        Assert.Equal("Cloud Migration", task.GetProperty("contractSubject").GetString());
    }

    [Fact]
    public async Task Exclude_Task_From_Contract_Belonging_To_Other_User()
    {
        await SeedContractAsync(
            consultantId: Guid.NewGuid(),
            customerName: "Acme",
            subject: "Other Project",
            startDate: new DateOnly(2026, 1, 1));

        var response = await factory.Client.GetAsync("/api/time-entries/pickers?weekStart=2026-05-18");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);
        Assert.Equal(0, json.GetProperty("availableTasks").GetArrayLength());
    }
}
