using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Sannr.AspNetCore.Filters;

namespace Sannr.AspNetCore
{
    /// <summary>
    /// Extensions for <see cref="RouteHandlerBuilder"/> to enable Sannr validation.
    /// </summary>
    public static class RouteHandlerBuilderExtensions
    {
        /// <summary>
        /// Adds Sannr validation to the endpoint(s).
        /// </summary>
        /// <param name="builder">The route handler builder.</param>
        /// <returns>The builder instance.</returns>
        public static RouteHandlerBuilder WithSannrValidation(this RouteHandlerBuilder builder)
        {
            return builder.AddEndpointFilter<SannrValidationFilter>();
        }

        /// <summary>
        /// Adds Sannr validation to the endpoint group.
        /// </summary>
        /// <param name="builder">The route group builder.</param>
        /// <returns>The builder instance.</returns>
        public static RouteGroupBuilder WithSannrValidation(this RouteGroupBuilder builder)
        {
            return builder.AddEndpointFilter<SannrValidationFilter>();
        }
    }
}
