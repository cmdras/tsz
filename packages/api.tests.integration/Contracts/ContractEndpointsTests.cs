using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Common.Database;
using Api.Modules.Contracts;
using Api.Modules.Users;
using Api.Tests.Integration.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Contracts;

public class ContractEndpointsShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    private Guid _customerId;
    private Guid _consultantId;

    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var customer = new Api.Modules.Customers.Customer
        {
            Id = Guid.NewGuid(), Number = 100000, Name = "Test Customer", Country = "Belgium",
        };
        context.Customers.Add(customer);
        _customerId = customer.Id;

        var consultant = new User
        {
            Id = Guid.NewGuid(), Name = "Test Consultant", Email = "consultant@test.com", Role = UserRole.User,
        };
        context.Users.Add(consultant);
        _consultantId = consultant.Id;

        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private ContractRequest BuildRequest(string subject = "Test Contract") => new()
    {
        CustomerId = _customerId,
        ConsultantId = _consultantId,
        Subject = subject,
        StartDate = new DateOnly(2026, 1, 1),
        EndDate = new DateOnly(2026, 12, 31),
        Tasks = [new ContractTaskRequest { Name = "Analysis", DayRate = 800m }],
    };

    private async Task<ContractResponse> SeedContractAsync(string subject = "Test Contract")
    {
        var response = await factory.Client.PostAsJsonAsync("/api/contracts", BuildRequest(subject));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ContractResponse>(IntegrationFactory.JsonOptions))!;
    }

    [Fact]
    public async Task Return_Ok_For_Contract_List()
    {
        var response = await factory.Client.GetAsync("/api/contracts");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedContracts>(IntegrationFactory.JsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Find_Contract_By_Id()
    {
        var seeded = await SeedContractAsync("Cloud Migration");

        var response = await factory.Client.GetAsync($"/api/contracts/{seeded.Id}");

        response.EnsureSuccessStatusCode();
        var contract = await response.Content.ReadFromJsonAsync<ContractResponse>(IntegrationFactory.JsonOptions);
        Assert.NotNull(contract);
        Assert.Equal("Cloud Migration", contract.Subject);
    }

    [Fact]
    public async Task Return_Not_Found_For_Unknown_Contract()
    {
        var response = await factory.Client.GetAsync($"/api/contracts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Contract_And_Return_Created()
    {
        var response = await factory.Client.PostAsJsonAsync("/api/contracts", BuildRequest("New Contract"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var contract = await response.Content.ReadFromJsonAsync<ContractResponse>(IntegrationFactory.JsonOptions);
        Assert.NotNull(contract);
        Assert.Equal("New Contract", contract.Subject);
        Assert.True(contract.Number >= 1);
    }

    [Fact]
    public async Task Assign_Sequential_Numbers_On_Create()
    {
        var first = await SeedContractAsync("First");
        var second = await SeedContractAsync("Second");

        Assert.Equal(first.Number + 1, second.Number);
    }

    [Fact]
    public async Task Return_Validation_Error_Contract_For_No_Tasks()
    {
        var request = BuildRequest();
        request.Tasks = [];

        var response = await factory.Client.PostAsJsonAsync("/api/contracts", request);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(IntegrationFactory.JsonOptions);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, body.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("title").GetString()));
    }

    [Fact]
    public async Task Reject_Contract_When_End_Date_Before_Start_Date()
    {
        var request = BuildRequest();
        request.StartDate = new DateOnly(2026, 6, 1);
        request.EndDate = new DateOnly(2026, 1, 1);

        var response = await factory.Client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Contract_When_Customer_Is_Archived()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var archivedCustomer = new Api.Modules.Customers.Customer
        {
            Id = Guid.NewGuid(), Number = 200000, Name = "Archived Corp", Country = "Belgium", IsArchived = true,
        };
        context.Customers.Add(archivedCustomer);
        await context.SaveChangesAsync();

        var request = BuildRequest();
        request.CustomerId = archivedCustomer.Id;

        var response = await factory.Client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Contract_When_Consultant_Is_Archived()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var archivedConsultant = new User
        {
            Id = Guid.NewGuid(), Name = "Archived User", Email = "archived@test.com", Role = UserRole.User, IsArchived = true,
        };
        context.Users.Add(archivedConsultant);
        await context.SaveChangesAsync();

        var request = BuildRequest();
        request.ConsultantId = archivedConsultant.Id;

        var response = await factory.Client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Contract_When_Consultant_Is_ClientManager_On_Create()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clientManager = new User
        {
            Id = Guid.NewGuid(), Name = "Client Manager", Email = "manager@test.com", Role = UserRole.ClientManager,
        };
        context.Users.Add(clientManager);
        await context.SaveChangesAsync();

        var request = BuildRequest();
        request.ConsultantId = clientManager.Id;

        var response = await factory.Client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Contract_When_Consultant_Is_ClientManager_On_Update()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clientManager = new User
        {
            Id = Guid.NewGuid(), Name = "Client Manager", Email = "manager@test.com", Role = UserRole.ClientManager,
        };
        context.Users.Add(clientManager);
        await context.SaveChangesAsync();

        var seeded = await SeedContractAsync("Original");
        var updateRequest = BuildRequest("Updated");
        updateRequest.ConsultantId = clientManager.Id;

        var response = await factory.Client.PutAsJsonAsync($"/api/contracts/{seeded.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Persist_Task_Add_Remove_Reorder_On_Update()
    {
        var seeded = await SeedContractAsync("Task Test");
        var originalTaskId = seeded.Tasks.First().Id;

        var updateRequest = BuildRequest("Task Test");
        updateRequest.Tasks =
        [
            new ContractTaskRequest { Id = originalTaskId, Name = "Kept Task", DayRate = 900m },
            new ContractTaskRequest { Name = "New Task", DayRate = 1000m },
        ];

        var response = await factory.Client.PutAsJsonAsync($"/api/contracts/{seeded.Id}", updateRequest);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<ContractResponse>(IntegrationFactory.JsonOptions);
        var activeTasks = updated!.Tasks.Where(task => !task.IsArchived).ToList();
        Assert.Equal(2, activeTasks.Count);
        Assert.Contains(activeTasks, task => task.Id == originalTaskId && task.Name == "Kept Task");
        Assert.Contains(activeTasks, task => task.Name == "New Task");
    }

    [Fact]
    public async Task Return_Tasks_Ordered_By_Order_Field()
    {
        var seeded = await SeedContractAsync("Order Test");
        var firstTaskId = seeded.Tasks.First().Id;

        var updateRequest = BuildRequest("Order Test");
        updateRequest.Tasks =
        [
            new ContractTaskRequest { Id = firstTaskId, Name = "First Task", DayRate = 800m },
            new ContractTaskRequest { Name = "Second Task", DayRate = 900m },
            new ContractTaskRequest { Name = "Third Task", DayRate = 1000m },
        ];
        await factory.Client.PutAsJsonAsync($"/api/contracts/{seeded.Id}", updateRequest);

        var response = await factory.Client.GetAsync($"/api/contracts/{seeded.Id}");
        var contract = await response.Content.ReadFromJsonAsync<ContractResponse>(IntegrationFactory.JsonOptions);

        var activeTasks = contract!.Tasks.Where(task => !task.IsArchived).ToList();
        Assert.Equal(3, activeTasks.Count);
        for (var index = 1; index < activeTasks.Count; index++)
            Assert.True(activeTasks[index - 1].Order <= activeTasks[index].Order);
    }

    [Fact]
    public async Task Update_Contract_Fields()
    {
        var seeded = await SeedContractAsync("Original");
        var updateRequest = BuildRequest("Updated");
        updateRequest.Tasks = [new ContractTaskRequest { Id = seeded.Tasks.First().Id, Name = "Updated Task", DayRate = 950m }];

        var response = await factory.Client.PutAsJsonAsync($"/api/contracts/{seeded.Id}", updateRequest);

        response.EnsureSuccessStatusCode();
        var contract = await response.Content.ReadFromJsonAsync<ContractResponse>(IntegrationFactory.JsonOptions);
        Assert.Equal("Updated", contract!.Subject);
    }

    [Fact]
    public async Task Return_Not_Found_On_Update_With_Unknown_Id()
    {
        var response = await factory.Client.PutAsJsonAsync($"/api/contracts/{Guid.NewGuid()}", BuildRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Archive_Contract()
    {
        var seeded = await SeedContractAsync("Archive Me");

        var response = await factory.Client.PatchAsync($"/api/contracts/{seeded.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Return_Not_Found_On_Archive_With_Unknown_Id()
    {
        var response = await factory.Client.PatchAsync($"/api/contracts/{Guid.NewGuid()}/archive", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Unarchive_Contract()
    {
        var seeded = await SeedContractAsync("Unarchive Me");
        await factory.Client.PatchAsync($"/api/contracts/{seeded.Id}/archive", null);

        var response = await factory.Client.PatchAsync($"/api/contracts/{seeded.Id}/unarchive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Exclude_Archived_Contracts_By_Default()
    {
        var seeded = await SeedContractAsync("Hidden Contract");
        await factory.Client.PatchAsync($"/api/contracts/{seeded.Id}/archive", null);

        var response = await factory.Client.GetAsync("/api/contracts?search=Hidden+Contract");
        var result = await response.Content.ReadFromJsonAsync<PagedContracts>(IntegrationFactory.JsonOptions);

        Assert.Equal(0, result!.Total);
    }

    [Fact]
    public async Task Include_Archived_Contracts_When_Requested()
    {
        var seeded = await SeedContractAsync("Archived Contract");
        await factory.Client.PatchAsync($"/api/contracts/{seeded.Id}/archive", null);

        var response = await factory.Client.GetAsync("/api/contracts?archived=Archived&search=Archived+Contract");
        var result = await response.Content.ReadFromJsonAsync<PagedContracts>(IntegrationFactory.JsonOptions);

        Assert.Contains(result!.Items, contract => contract.Subject == "Archived Contract");
    }

    [Fact]
    public async Task RoundTrip_Sort_And_Pagination_Query_String()
    {
        for (var index = 1; index <= 4; index++)
            await SeedContractAsync($"Contract {index:D2}");

        var page1 = await factory.Client.GetFromJsonAsync<PagedContracts>(
            "/api/contracts?sort=Subject&sortDirection=Asc&page=1&pageSize=2",
            IntegrationFactory.JsonOptions);
        var page2 = await factory.Client.GetFromJsonAsync<PagedContracts>(
            "/api/contracts?sort=Subject&sortDirection=Asc&page=2&pageSize=2",
            IntegrationFactory.JsonOptions);

        Assert.Equal(4, page1!.Total);
        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(2, page2!.Items.Count);
        Assert.True(string.Compare(page1.Items[0].Subject, page1.Items[1].Subject, StringComparison.Ordinal) < 0);
        Assert.True(string.Compare(page1.Items.Last().Subject, page2.Items[0].Subject, StringComparison.Ordinal) < 0);
    }
}
