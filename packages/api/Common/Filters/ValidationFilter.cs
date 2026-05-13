using System.ComponentModel.DataAnnotations;

namespace Api.Common.Filters;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
            return Results.BadRequest("Invalid request body.");

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);

        if (!Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = validationResults
                .Where(validationResult => validationResult.ErrorMessage is not null)
                .ToDictionary(
                    validationResult => validationResult.MemberNames.FirstOrDefault() ?? string.Empty,
                    validationResult => new[] { validationResult.ErrorMessage! }
                );
            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}
