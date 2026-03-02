using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sannr;

namespace Sannr.AspNetCore.Filters
{
    /// <summary>
    /// Endpoint filter that automatically validates typed parameters using Sannr.
    /// </summary>
    public class SannrValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            foreach (var argument in context.Arguments)
            {
                if (argument == null) continue;

                // Check if the type has a Sannr validator registered
                if (global::Sannr.SannrValidatorRegistry.TryGetValidator(argument.GetType(), out var validator) && validator != null)
                {
                    var sannrContext = new SannrValidationContext(argument);
                    var result = await validator(sannrContext);
                    if (!result.IsValid)
                    {
                        // Standard ProblemDetails response for validation errors
                        return Results.ValidationProblem(
                            errors: result.ToErrorsDictionary(),
                            detail: "One or more validation errors occurred.",
                            title: "Validation Failure",
                            statusCode: StatusCodes.Status400BadRequest
                        );
                    }
                }
            }

            return await next(context);
        }
    }
}
