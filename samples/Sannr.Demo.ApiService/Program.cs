using Sannr;
using Sannr.AspNetCore;
using Sannr.Demo.ApiService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// ============================================================================
// SANNR VALIDATION - THE MAGIC HAPPENS HERE!
// ============================================================================
// AddSannr() automatically:
// 1. Discovers all models with validation attributes (Required, StringLength, Range, etc.)
// 2. Registers compile-time generated validators (no reflection, 100% AoT compatible!)
// 3. Integrates with ASP.NET Core model binding - validation happens automatically
// 4. Generates client-side validation code (TypeScript/JavaScript) for each model
// 5. Creates OpenAPI schema filters for Swagger documentation
//
// ZERO manual validation code needed - it's all source-generated at compile-time!
// ============================================================================
builder.Services.AddSannr();

// Add services to the container.
builder.Services.AddProblemDetails();

// Enable advanced Sannr features for the demo
builder.Services.AddSannr(options =>
{
    options.EnableMetrics = true;
    options.EnableEnhancedErrorResponses = true;
    options.IncludeValidationDuration = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.MapDefaultEndpoints();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// ============================================================================
// SANNR VALIDATED ROUTE GROUP
// ============================================================================
// By grouping endpoints and adding WithSannrValidation(), all models
// are automatically validated and sanitized before the handler runs!
// ============================================================================
var api = app.MapGroup("/api").WithSannrValidation();

// User Registration - Basic validation with sanitization
api.MapPost("/users/register", (UserRegistrationRequest request) =>
{
    return Results.Ok(new
    {
        message = "User registered successfully!",
        user = new
        {
            request.Username,
            request.Email,
            request.Age,
            request.PhoneNumber,
            request.Website
        }
    });
});

// User Profile Update - Conditional validation
api.MapPost("/users/profile", (UserProfileUpdateRequest request) =>
{
    return Results.Ok(new
    {
        message = "Profile updated successfully!",
        changes = new
        {
            request.Email,
            request.PhoneNumber,
            request.CurrentPassword
        }
    });
});

// Product Creation - Complex validation with ranges and file extensions
api.MapPost("/products", (ProductCreateRequest request) =>
{
    return Results.Ok(new
    {
        message = "Product created successfully!",
        product = new
        {
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.Category,
            request.ImageFileName
        }
    });
});

// Order Creation - Payment validation and business rules
api.MapPost("/orders", (OrderCreateRequest request) =>
{
    return Results.Ok(new
    {
        message = "Order created successfully!",
        order = new
        {
            request.CustomerId,
            request.PaymentMethod,
            request.ShippingAddress,
            ItemCount = request.Items?.Count ?? 0
        }
    });
});

// Contact Form - Sanitization and spam protection
api.MapPost("/contact", (ContactFormRequest request) =>
{
    return Results.Ok(new
    {
        message = "Message sent successfully!",
        contact = new
        {
            request.Name,
            request.Email,
            request.Subject,
            request.Message
        }
    });
});

// Newsletter Subscription - Email validation with preferences
api.MapPost("/newsletter/subscribe", (NewsletterSubscriptionRequest request) =>
{
    return Results.Ok(new
    {
        message = "Subscribed successfully!",
        subscription = new
        {
            request.Email,
            request.Preferences,
            request.MarketingConsent
        }
    });
});

// Simple validation demo endpoint (legacy)
api.MapPost("/validate", (UserRegistrationRequest request) =>
{
    return Results.Ok(new { message = "Validation passed!", user = request });
});

// TEST VALIDATION ENDPOINTS - Comprehensive Sannr Validation Demo
var testApi = api.MapGroup("/test");

// 1. Comprehensive Validation Test - All attribute types
testApi.MapPost("/validation", (ValidationTestModel request) =>
{
    return Results.Ok(new
    {
        message = "All validations passed!",
        sanitizedData = new
        {
            request.Username,
            request.DisplayName,
            request.Age,
            request.Price,
            request.ContactEmail,
            request.UserId // Shows sanitization (trimmed + uppercase)
        }
    });
});

// 2. User Profile Validation - Sanitization and conditional validation
testApi.MapPost("/user-profile", (UserProfileModel request) =>
{
    return Results.Ok(new
    {
        message = "User profile created successfully!",
        profile = new
        {
            request.Username, // Shows sanitization
            request.Email,
            request.Age,
            request.ZipCode // Only required if Country is USA
        }
    });
});

// 3. Advanced Validation - Custom validators and complex rules
testApi.MapPost("/advanced", (AdvancedValidationModel request) =>
{
    return Results.Ok(new
    {
        message = "Advanced validation passed!",
        securityCode = request.SecurityCode,
        hasValidPassword = !string.IsNullOrEmpty(request.Password)
    });
});

// 4. Business Rules - Order validation with future dates and ranges
testApi.MapPost("/order", (OrderModel request) =>
{
    return Results.Ok(new
    {
        message = "Order validated successfully!",
        order = new
        {
            request.CustomerId,
            request.DeliveryDate,
            request.Currency,
            request.Amount
        }
    });
});

// 5. Product Validation - Business rules with status-based ranges
testApi.MapPost("/product", (ProductModel request) =>
{
    return Results.Ok(new
    {
        message = "Product validated successfully!",
        product = new
        {
            request.Name,
            request.Status,
            request.ReleaseDate,
            request.Price
        }
    });
});

// 6. Appointment Validation - Time-based business rules
testApi.MapPost("/appointment", (AppointmentModel request) =>
{
    return Results.Ok(new
    {
        message = "Appointment booked successfully!",
        appointment = new
        {
            request.CustomerName,
            request.AppointmentDate,
            request.Status,
            request.DurationMinutes
        }
    });
});

// 7. Client-side Validation Rules - Returns the generated JS code
// 8. Client-side Validation Rules - Returns the generated JS code
testApi.MapGet("/client-validation/rules", () => Results.Content(ClientValidationModel.ValidationRulesJavaScript, "application/javascript"));

testApi.MapPost("/client-validation", (ClientValidationModel request) =>
{
    return Results.Ok(new
    {
        message = "Client validation passed!",
        data = new
        {
            request.Name,
            request.Email,
            request.Age,
            request.HasAddress,
            request.StreetAddress
        }
    });
});

app.Run();
