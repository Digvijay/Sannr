using Sannr;

namespace Sannr.ConsoleDemo;

/// <summary>
/// User registration model demonstrating basic validation attributes.
/// All validation logic is auto-generated at compile-time!
/// </summary>
public partial class UserRegistration
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [Sanitize(Trim = true, ToUpper = true)]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Sanitize(Trim = true, ToLower = true)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Age is required")]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Please confirm your password")]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "You must accept the terms and conditions")]
    public bool AcceptTerms { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }
    
    [Url(ErrorMessage = "Invalid URL format")]
    public string? Website { get; set; }
}

/// <summary>
/// Order request model demonstrating business rules and conditional validation.
/// Shows RequiredIf, future date validation, and range constraints.
/// </summary>
public partial class OrderRequest
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 3)]
    [Sanitize(Trim = true)]
    public string? ProductName { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Delivery date is required")]
    [FutureDate(ErrorMessage = "Delivery date must be in the future")]
    public DateTime DeliveryDate { get; set; }

    [Required(ErrorMessage = "Shipping country is required")]
    [AllowedValues("USA", "Canada", "Mexico", "UK", ErrorMessage = "We only ship to USA, Canada, Mexico, or UK")]
    public string? ShippingCountry { get; set; }

    [RequiredIf(nameof(ShippingCountry), "USA", ErrorMessage = "Zip code is required for USA shipments")]
    [StringLength(10, MinimumLength = 5, ErrorMessage = "Invalid zip code format")]
    public string? ZipCode { get; set; }
    
    [CreditCard(ErrorMessage = "Invalid credit card number")]
    public string? PaymentCard { get; set; }
}
