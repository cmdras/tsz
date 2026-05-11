using System.ComponentModel.DataAnnotations;
using Api.Common.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Tests.Common.Filters;

public class ValidationFilterTests
{
    private sealed class TestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        [Range(1, 100, ErrorMessage = "Age must be between 1 and 100")]
        public int Age { get; set; }
    }

    private static EndpointFilterInvocationContext CreateContext(params object?[] arguments)
    {
        var httpContext = new DefaultHttpContext();
        return new DefaultEndpointFilterInvocationContext(httpContext, arguments);
    }

    [Fact]
    public async Task InvokeAsync_NoMatchingArgument_ReturnsBadRequest()
    {
        var filter = new ValidationFilter<TestModel>();
        var context = CreateContext("not a model", 42);
        var nextCalled = false;
        ValueTask<object?> Next(EndpointFilterInvocationContext _) { nextCalled = true; return ValueTask.FromResult<object?>(null); }

        var result = await filter.InvokeAsync(context, Next);

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("Invalid request body.", badRequest.Value);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_NullArgument_ReturnsBadRequest()
    {
        var filter = new ValidationFilter<TestModel>();
        var context = CreateContext(new object?[] { null });
        var nextCalled = false;
        ValueTask<object?> Next(EndpointFilterInvocationContext _) { nextCalled = true; return ValueTask.FromResult<object?>(null); }

        var result = await filter.InvokeAsync(context, Next);

        Assert.IsType<BadRequest<string>>(result);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ValidModel_CallsNext()
    {
        var filter = new ValidationFilter<TestModel>();
        var model = new TestModel { Name = "Alice", Age = 30 };
        var context = CreateContext(model);
        var expected = new object();
        ValueTask<object?> Next(EndpointFilterInvocationContext _) => ValueTask.FromResult<object?>(expected);

        var result = await filter.InvokeAsync(context, Next);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task InvokeAsync_InvalidModel_ReturnsValidationProblem()
    {
        var filter = new ValidationFilter<TestModel>();
        var model = new TestModel { Name = null, Age = 30 };
        var context = CreateContext(model);
        var nextCalled = false;
        ValueTask<object?> Next(EndpointFilterInvocationContext _) { nextCalled = true; return ValueTask.FromResult<object?>(null); }

        var result = await filter.InvokeAsync(context, Next);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        var details = Assert.IsType<HttpValidationProblemDetails>(problem.ProblemDetails);
        Assert.True(details.Errors.ContainsKey(nameof(TestModel.Name)));
        Assert.Equal("Name is required", details.Errors[nameof(TestModel.Name)].Single());
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_MultipleValidationErrors_AllReturned()
    {
        var filter = new ValidationFilter<TestModel>();
        var model = new TestModel { Name = null, Age = 999 };
        var context = CreateContext(model);
        ValueTask<object?> Next(EndpointFilterInvocationContext _) => ValueTask.FromResult<object?>(null);

        var result = await filter.InvokeAsync(context, Next);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        var details = Assert.IsType<HttpValidationProblemDetails>(problem.ProblemDetails);
        var errors = details.Errors;
        Assert.Equal("Name is required", errors[nameof(TestModel.Name)].Single());
        Assert.Equal("Age must be between 1 and 100", errors[nameof(TestModel.Age)].Single());
    }

    [Fact]
    public async Task InvokeAsync_FindsMatchingArgumentAmongOthers()
    {
        var filter = new ValidationFilter<TestModel>();
        var model = new TestModel { Name = "Bob", Age = 25 };
        var context = CreateContext("extra", 7, model, new object());
        var expected = new object();
        ValueTask<object?> Next(EndpointFilterInvocationContext _) => ValueTask.FromResult<object?>(expected);

        var result = await filter.InvokeAsync(context, Next);

        Assert.Same(expected, result);
    }
}
