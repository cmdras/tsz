using System.Reflection;
using Api.Common.Database;
using Api.Common.Exceptions;
using Api.Common.Extensions;
using Api.Common.OpenApi;
using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.LeaveTypes;
using Api.Modules.Stats;
using Api.Modules.UserLeaveAllowances;
using Api.Modules.Users;
using Microsoft.AspNetCore.HttpLogging;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTszAuthentication(builder.Configuration, builder.Environment);

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestQuery
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddTszJson();
builder.Services.AddTszOpenApi();

builder.Services.AddAppDatabase(builder.Configuration);
builder.Services.AddCustomersModule();
builder.Services.AddUserLeaveAllowancesModule();
builder.Services.AddUsersModule();
builder.Services.AddContractsModule();
builder.Services.AddLeaveTypesModule();
builder.Services.AddStatsModule();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.InitializeDatabaseAsync();
}

app.UseHttpLogging();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi("/openapi/{documentName}.json");
app.MapScalarApiReference("/openapi", options =>
{
    options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
});

app.MapGet("/", () => new
{
    name = "TSZ API",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
});

app.MapCustomersModule();
app.MapUsersModule();
app.MapContractsModule();
app.MapLeaveTypesModule();
app.MapStatsModule();

app.Run();
