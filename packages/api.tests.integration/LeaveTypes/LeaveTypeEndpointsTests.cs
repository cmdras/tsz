using System.Net;
using System.Net.Http.Json;
using Api.Modules.LeaveTypes;
using Api.Tests.Integration.Common;

namespace Api.Tests.Integration.LeaveTypes;

public class LeaveTypeEndpointsShould(IntegrationFactory factory) : IClassFixture<IntegrationFactory>, IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<LeaveTypeResponse> SeedLeaveTypeAsync(string name = "Holiday", decimal defaultDays = 20m)
    {
        var request = new LeaveTypeRequest { Name = name, DefaultDays = defaultDays };
        var response = await factory.Client.PostAsJsonAsync("/api/leave-types", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LeaveTypeResponse>(IntegrationFactory.JsonOptions))!;
    }

    [Fact]
    public async Task Return_Ok_For_Leave_Type_List()
    {
        var response = await factory.Client.GetAsync("/api/leave-types");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedLeaveTypes>(IntegrationFactory.JsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Find_Leave_Type_By_Id()
    {
        var seeded = await SeedLeaveTypeAsync("ADV", 5m);

        var response = await factory.Client.GetAsync($"/api/leave-types/{seeded.Id}");

        response.EnsureSuccessStatusCode();
        var leaveType = await response.Content.ReadFromJsonAsync<LeaveTypeResponse>(IntegrationFactory.JsonOptions);
        Assert.NotNull(leaveType);
        Assert.Equal("ADV", leaveType.Name);
    }

    [Fact]
    public async Task Return_Not_Found_For_Unknown_Leave_Type()
    {
        var response = await factory.Client.GetAsync($"/api/leave-types/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Leave_Type_And_Return_Created()
    {
        var request = new LeaveTypeRequest { Name = "Ancienniteit", DefaultDays = 0m };

        var response = await factory.Client.PostAsJsonAsync("/api/leave-types", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var leaveType = await response.Content.ReadFromJsonAsync<LeaveTypeResponse>(IntegrationFactory.JsonOptions);
        Assert.NotNull(leaveType);
        Assert.Equal("Ancienniteit", leaveType.Name);
        Assert.Equal(0m, leaveType.DefaultDays);
    }

    [Fact]
    public async Task Reject_Duplicate_Name_On_Create()
    {
        await SeedLeaveTypeAsync("Sickness", 0m);

        var response = await factory.Client.PostAsJsonAsync("/api/leave-types", new LeaveTypeRequest { Name = "Sickness", DefaultDays = 5m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Duplicate_Name_Different_Case_On_Create()
    {
        await SeedLeaveTypeAsync("Holiday", 20m);

        var response = await factory.Client.PostAsJsonAsync("/api/leave-types", new LeaveTypeRequest { Name = "HOLIDAY", DefaultDays = 20m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Return_Bad_Request_For_Invalid_Leave_Type()
    {
        var request = new { Name = "" };

        var response = await factory.Client.PostAsJsonAsync("/api/leave-types", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Return_Bad_Request_For_Two_Decimal_Place_Default_Days_On_Create()
    {
        var request = new LeaveTypeRequest { Name = "Bonus", DefaultDays = 7.25m };

        var response = await factory.Client.PostAsJsonAsync("/api/leave-types", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_Leave_Type_Fields()
    {
        var seeded = await SeedLeaveTypeAsync("Holiday Replacement", 0m);
        var request = new LeaveTypeRequest { Name = "Holiday Replacement", DefaultDays = 2.5m };

        var response = await factory.Client.PutAsJsonAsync($"/api/leave-types/{seeded.Id}", request);

        response.EnsureSuccessStatusCode();
        var leaveType = await response.Content.ReadFromJsonAsync<LeaveTypeResponse>(IntegrationFactory.JsonOptions);
        Assert.NotNull(leaveType);
        Assert.Equal(2.5m, leaveType.DefaultDays);
    }

    [Fact]
    public async Task Return_Not_Found_On_Update_With_Unknown_Id()
    {
        var request = new LeaveTypeRequest { Name = "Ghost", DefaultDays = 0m };

        var response = await factory.Client.PutAsJsonAsync($"/api/leave-types/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Return_Bad_Request_For_Two_Decimal_Place_Default_Days_On_Update()
    {
        var seeded = await SeedLeaveTypeAsync("Bonus", 0m);
        var request = new LeaveTypeRequest { Name = "Bonus", DefaultDays = 7.25m };

        var response = await factory.Client.PutAsJsonAsync($"/api/leave-types/{seeded.Id}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reject_Duplicate_Name_On_Update()
    {
        await SeedLeaveTypeAsync("Holiday", 20m);
        var adv = await SeedLeaveTypeAsync("ADV", 5m);

        var response = await factory.Client.PutAsJsonAsync($"/api/leave-types/{adv.Id}", new LeaveTypeRequest { Name = "Holiday", DefaultDays = 5m });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Archive_Leave_Type()
    {
        var seeded = await SeedLeaveTypeAsync("Ancienniteit", 0m);

        var response = await factory.Client.PatchAsync($"/api/leave-types/{seeded.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Return_Not_Found_On_Archive_With_Unknown_Id()
    {
        var response = await factory.Client.PatchAsync($"/api/leave-types/{Guid.NewGuid()}/archive", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Unarchive_Leave_Type()
    {
        var seeded = await SeedLeaveTypeAsync("ADV", 5m);
        await factory.Client.PatchAsync($"/api/leave-types/{seeded.Id}/archive", null);

        var response = await factory.Client.PatchAsync($"/api/leave-types/{seeded.Id}/unarchive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Exclude_Archived_Leave_Types_By_Default()
    {
        var seeded = await SeedLeaveTypeAsync("To Be Archived", 0m);
        await factory.Client.PatchAsync($"/api/leave-types/{seeded.Id}/archive", null);

        var response = await factory.Client.GetAsync("/api/leave-types?search=To+Be+Archived");
        var result = await response.Content.ReadFromJsonAsync<PagedLeaveTypes>(IntegrationFactory.JsonOptions);

        Assert.Equal(0, result!.Total);
    }

    [Fact]
    public async Task Include_Archived_Leave_Types_When_Requested()
    {
        var seeded = await SeedLeaveTypeAsync("Archived Type", 0m);
        await factory.Client.PatchAsync($"/api/leave-types/{seeded.Id}/archive", null);

        var response = await factory.Client.GetAsync("/api/leave-types?archived=Archived&search=Archived+Type");
        var result = await response.Content.ReadFromJsonAsync<PagedLeaveTypes>(IntegrationFactory.JsonOptions);

        Assert.Equal(1, result!.Total);
    }

    [Fact]
    public async Task RoundTrip_Sort_And_Pagination_Query_String()
    {
        for (var index = 1; index <= 4; index++)
            await SeedLeaveTypeAsync($"Leave Type {index:D2}", defaultDays: index * 5m);

        var page1 = await factory.Client.GetFromJsonAsync<PagedLeaveTypes>(
            "/api/leave-types?sort=Name&sortDirection=Asc&page=1&pageSize=2",
            IntegrationFactory.JsonOptions);
        var page2 = await factory.Client.GetFromJsonAsync<PagedLeaveTypes>(
            "/api/leave-types?sort=Name&sortDirection=Asc&page=2&pageSize=2",
            IntegrationFactory.JsonOptions);

        Assert.Equal(4, page1!.Total);
        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(2, page2!.Items.Count);
        Assert.True(string.Compare(page1.Items[0].Name, page1.Items[1].Name, StringComparison.Ordinal) < 0);
        Assert.True(string.Compare(page1.Items.Last().Name, page2.Items[0].Name, StringComparison.Ordinal) < 0);
    }
}
