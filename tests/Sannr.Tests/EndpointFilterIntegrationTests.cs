// ----------------------------------------------------------------------------------
// MIT License
//
// Copyright (c) 2025 Sannr contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sannr.AspNetCore;
using Xunit;

namespace Sannr.Tests;

/// <summary>
/// A model the Sannr generator produces a validator for.
/// </summary>
public partial class CheckoutRequest
{
    [Required(ErrorMessage = "Order code is required")]
    [StringLength(6, MinimumLength = 4)]
    [Sanitize(Trim = true, ToUpper = true)]
    public string? OrderCode { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; }
}

/// <summary>
/// A model with no validation attributes, so no validator is generated or registered for it.
/// </summary>
public class UnregisteredRequest
{
    public string? Anything { get; set; }
}

/// <summary>
/// Drives <c>WithSannrValidation</c> through a real ASP.NET Core pipeline over real HTTP.
/// </summary>
/// <remarks>
/// <para>
/// These tests exist because of a defect that shipped through 1.6.0: the endpoint filter resolved
/// validators from <c>Sannr.AspNetCore.SannrValidatorRegistry</c>, a second registry that nothing
/// ever wrote to, which shadowed the real <c>Sannr.SannrValidatorRegistry</c> inside the
/// <c>Sannr.AspNetCore</c> namespace. Every lookup missed, so every request skipped validation and
/// the endpoint returned success.
/// </para>
/// <para>
/// The existing suite could not catch it. <c>MinimalApiIntegrationTests</c> is named for the
/// integration but exercises <c>Validated&lt;T&gt;</c> directly and never issues an HTTP request,
/// and the unit tests call the generated validator directly, where behaviour was always correct.
/// The defect lived entirely in the wiring between them. Anything asserting on this filter must
/// therefore go through the pipeline, as these tests do.
/// </para>
/// </remarks>
public class EndpointFilterIntegrationTests
{
    private static async Task<(WebApplication App, HttpClient Client)> StartAsync(
        Action<WebApplication> configure,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();
        configure(app);
        await app.StartAsync();

        var address = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();

        return (app, new HttpClient { BaseAddress = new Uri(address) });
    }

    /// <summary>
    /// Builds an application and forces its endpoints to be constructed, which is when endpoint
    /// filter factories run.
    /// </summary>
    private static void MaterializeEndpoints(
        Action<WebApplication> configure,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();
        configure(app);

        foreach (var dataSource in ((IEndpointRouteBuilder)app).DataSources)
        {
            _ = dataSource.Endpoints;
        }
    }

    [Fact]
    public async Task InvalidPayload_IsRejectedWithBadRequest()
    {
        var (app, client) = await StartAsync(app =>
            app.MapPost("/checkout", (CheckoutRequest request) => Results.Ok(new { ok = true }))
               .WithSannrValidation());

        await using var host = app;

        var response = await client.PostAsJsonAsync("/checkout", new { orderCode = "X", quantity = 500 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty(nameof(CheckoutRequest.OrderCode), out var ignored));
        Assert.True(errors.TryGetProperty(nameof(CheckoutRequest.Quantity), out var quantityErrors));
    }

    [Fact]
    public async Task InvalidPayload_OnAGroup_IsRejectedWithBadRequest()
    {
        var (app, client) = await StartAsync(app =>
        {
            var group = app.MapGroup("/api").WithSannrValidation();
            group.MapPost("/checkout", (CheckoutRequest request) => Results.Ok(new { ok = true }));
        });

        await using var host = app;

        var response = await client.PostAsJsonAsync("/api/checkout", new { orderCode = "X", quantity = 500 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidPayload_IsAcceptedAndSanitized()
    {
        var (app, client) = await StartAsync(app =>
            app.MapPost("/checkout", (CheckoutRequest request) => Results.Ok(new { code = request.OrderCode }))
               .WithSannrValidation());

        await using var host = app;

        // Deliberately untrimmed and lower case: [Sanitize] must normalise it before the handler runs.
        var response = await client.PostAsJsonAsync("/checkout", new { orderCode = "  ab12 ", quantity = 3 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("AB12", body.GetProperty("code").GetString());
    }

    [Fact]
    public void EndpointBindingAnUnregisteredModel_FailsClosed()
    {
        // The whole point: asking for validation on a model with no validator must be loud.
        // Previously this produced an endpoint that accepted every request without validating it.
        // Endpoint construction is where this is detectable, so materialise the endpoints rather
        // than issuing a request; a request would surface it only as an opaque 500.
        var exception = Assert.ThrowsAny<Exception>(() =>
            MaterializeEndpoints(app =>
                app.MapPost("/unregistered", (UnregisteredRequest request) => Results.Ok())
                   .WithSannrValidation()));

        var messages = Flatten(exception).ToList();
        Assert.Contains(messages, m => m.Contains(nameof(UnregisteredRequest), StringComparison.Ordinal));
        Assert.Contains(messages, m => m.Contains("no Sannr validator is registered", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void EndpointBindingAnUnregisteredModel_CanOptOutDeliberately()
    {
        var exception = Record.Exception(() =>
            MaterializeEndpoints(
                app => app.MapPost("/unregistered", (UnregisteredRequest request) => Results.Ok())
                          .WithSannrValidation(),
                services => services.AddSingleton(new SannrValidationOptions { RequireValidator = false })));

        Assert.Null(exception);
    }

    [Fact]
    public void EndpointBindingAnUnregisteredModel_CanOptOutWithConfiguredOptions()
    {
        var exception = Record.Exception(() =>
            MaterializeEndpoints(
                app => app.MapPost("/unregistered", (UnregisteredRequest request) => Results.Ok())
                          .WithSannrValidation(),
                services => services.Configure<SannrValidationOptions>(o => o.RequireValidator = false)));

        Assert.Null(exception);
    }

    [Fact]
    public void EndpointBindingAValidatedModel_BuildsWithoutComplaint()
    {
        // Guards against the fail-closed check becoming over-eager and rejecting good endpoints.
        var exception = Record.Exception(() =>
            MaterializeEndpoints(app =>
                app.MapPost("/checkout", (CheckoutRequest request) => Results.Ok())
                   .WithSannrValidation()));

        Assert.Null(exception);
    }

    [Fact]
    public void ObsoleteAspNetCoreRegistry_SharesStorageWithTheRealRegistry()
    {
        // The two registries used to be independent, which is what made the filter fail open.
        // The shim must now read through to the registry the generator writes to.
#pragma warning disable CS0618 // deliberately exercising the obsolete forwarder
        Assert.True(SannrValidatorRegistry.TryGetValidator(typeof(CheckoutRequest), out var shim));
#pragma warning restore CS0618
        Assert.NotNull(shim);
        Assert.True(global::Sannr.SannrValidatorRegistry.TryGetValidator(typeof(CheckoutRequest), out var real));
        Assert.NotNull(real);
    }

    private static IEnumerable<string> Flatten(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            yield return current.Message;
        }
    }
}
