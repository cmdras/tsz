using Api.Common.Database;
using Api.Modules.UserLeaveAllowances;
using Api.Modules.Users;
using Api.Tests.Integration.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Users;

public class OAuthProvisioningShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_User_With_Default_Allowances_On_First_Login()
    {
        using var setupScope = factory.Services.CreateScope();
        var setupContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
        setupContext.LeaveTypes.Add(new() { Id = Guid.NewGuid(), Name = "Holiday", DefaultDays = 20m, DefaultMode = AllowanceMode.Limited });
        setupContext.LeaveTypes.Add(new() { Id = Guid.NewGuid(), Name = "Sickness", DefaultDays = 0m, DefaultMode = AllowanceMode.Unlimited });
        await setupContext.SaveChangesAsync();

        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<UserService>();

        var user = await service.GetOrProvisionAsync("Alice", "alice@example.com");

        Assert.Equal("Alice", user.Name);
        Assert.Equal("alice@example.com", user.Email);

        using var verifyScope = factory.Services.CreateScope();
        var context = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var allowances = await context.UserLeaveAllowances
            .Where(allowance => allowance.UserId == user.Id)
            .ToListAsync();
        Assert.Equal(2, allowances.Count);
    }

    [Fact]
    public async Task Return_Existing_User_On_Repeated_Provision()
    {
        using var firstScope = factory.Services.CreateScope();
        var service = firstScope.ServiceProvider.GetRequiredService<UserService>();
        var first = await service.GetOrProvisionAsync("Bob", "bob@example.com");

        using var secondScope = factory.Services.CreateScope();
        var service2 = secondScope.ServiceProvider.GetRequiredService<UserService>();
        var second = await service2.GetOrProvisionAsync("Bob", "bob@example.com");

        Assert.Equal(first.Id, second.Id);
    }

    [Fact]
    public async Task Match_Existing_User_By_Email_Case_Insensitively()
    {
        using var firstScope = factory.Services.CreateScope();
        var service = firstScope.ServiceProvider.GetRequiredService<UserService>();
        var first = await service.GetOrProvisionAsync("Carol", "carol@example.com");

        using var secondScope = factory.Services.CreateScope();
        var service2 = secondScope.ServiceProvider.GetRequiredService<UserService>();
        var second = await service2.GetOrProvisionAsync("Carol", "CAROL@EXAMPLE.COM");

        Assert.Equal(first.Id, second.Id);
    }
}
