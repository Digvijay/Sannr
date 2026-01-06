using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Configure HTTP client for API service
builder.Services.AddHttpClient("apiservice", (serviceProvider, client) =>
{
    // Aspire handles base address automatically via service discovery
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

// Serve the main demo page from wwwroot/index.html (default)
app.MapGet("/", () => Results.File(Path.Combine(app.Environment.WebRootPath, "index.html"), "text/html"));

// Serve the interactive demo page
app.MapGet("/demo", () => Results.File(Path.Combine(app.Environment.WebRootPath, "demo.html"), "text/html"));

// Serve configuration for the client
app.MapGet("/config.js", (IConfiguration configuration) =>
{
    // window.apiBase is empty to use the local proxy
    return Results.Content("window.apiBase = \"\";", "application/javascript");
});

app.MapDefaultEndpoints();

// API proxy endpoints to forward requests to the API service
// This allows the browser to call /api/... and have it routed to the Sannr.Demo.ApiService
app.Map("/api/{**catch-all}", async (HttpContext context, IHttpClientFactory httpClientFactory, IConfiguration config) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("apiservice");
        var path = context.Request.Path.ToString();
        var queryString = context.Request.QueryString.ToString();

        // Aspire service discovery provides the URL
        var apiBaseUrl = config["services:apiservice:http:0"];
        if (string.IsNullOrEmpty(apiBaseUrl))
        {
            return Results.Problem("API Service (apiservice) not found in configuration.", statusCode: 500);
        }

        var apiUrl = $"{apiBaseUrl}{path}{queryString}";
        var apiRequest = new HttpRequestMessage(new HttpMethod(context.Request.Method), apiUrl);

        // Copy headers (excluding Host)
        foreach (var header in context.Request.Headers)
        {
            if (!header.Key.StartsWith("Host", StringComparison.OrdinalIgnoreCase))
                apiRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }

        // Copy body
        if (context.Request.Method != "GET" && context.Request.Method != "HEAD")
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            apiRequest.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        }

        var response = await client.SendAsync(apiRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        return Results.Content(responseContent, contentType, statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
});

app.Run();
