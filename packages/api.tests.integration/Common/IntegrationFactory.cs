using Api.Common.Database;
using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.LeaveTypes;
using Api.Modules.UserLeaveAllowances;
using Api.Modules.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Tests.Integration.Common;

public class IntegrationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"IntegrationTests-{Guid.NewGuid()}";

    public HttpClient Client { get; private set; } = null!;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });
            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            var toRemove = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    descriptor.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>) ||
                    descriptor.ServiceType == typeof(AppDbContext))
                .ToList();
            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options => options
                .UseInMemoryDatabase(_databaseName)
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }

    public async Task InitializeAsync()
    {
        Client = CreateClient();
        await ResetDatabaseAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => base.DisposeAsync().AsTask();

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.TimeEntries.RemoveRange(context.TimeEntries);
        context.WeekSubmissions.RemoveRange(context.WeekSubmissions);
        context.ContractTasks.RemoveRange(context.ContractTasks);
        context.Contracts.RemoveRange(context.Contracts);
        context.UserLeaveAllowances.RemoveRange(context.UserLeaveAllowances);
        context.Users.RemoveRange(context.Users);
        context.Customers.RemoveRange(context.Customers);
        context.LeaveTypes.RemoveRange(context.LeaveTypes);
        await context.SaveChangesAsync();

        context.Users.Add(new User
        {
            Id = TestAuthHandler.CurrentUserId,
            Name = "System Test User",
            Email = "system-test@test.com",
            Role = UserRole.User,
            IsArchived = false,
        });
        await context.SaveChangesAsync();
    }

    public async Task<(User User, Guid ContractTaskId)> SeedUserWithContractTaskAsync(DateOnly weekStart)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = context.Users.Find(TestAuthHandler.CurrentUserId)!;

        var customer = new Customer { Id = Guid.NewGuid(), Number = 1, Name = "Test Customer" };
        context.Customers.Add(customer);

        var contract = new Contract
        {
            Id = Guid.NewGuid(), Number = 1, CustomerId = customer.Id, ConsultantId = user.Id,
            Subject = "Test Contract", StartDate = weekStart,
        };
        context.Contracts.Add(contract);

        var task = new ContractTask { Id = Guid.NewGuid(), ContractId = contract.Id, Name = "Test Task" };
        context.ContractTasks.Add(task);

        await context.SaveChangesAsync();
        return (user, task.Id);
    }

    public async Task<LeaveType> SeedLeaveTypeAsync(string name, decimal defaultDays = 20m, AllowanceMode defaultMode = AllowanceMode.Limited)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = name,
            DefaultDays = defaultDays,
            DefaultMode = defaultMode,
        };
        context.LeaveTypes.Add(leaveType);
        await context.SaveChangesAsync();
        return leaveType;
    }

    public async Task SeedTimeEntryAsync(DateOnly date, Guid? contractTaskId, Guid? leaveTypeId, decimal hours)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.TimeEntries.Add(new Api.Modules.TimeEntries.TimeEntry
        {
            Id = Guid.NewGuid(),
            UserId = TestAuthHandler.CurrentUserId,
            Date = date,
            ContractTaskId = contractTaskId,
            LeaveTypeId = leaveTypeId,
            Hours = hours,
            UpdatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();
    }
}
