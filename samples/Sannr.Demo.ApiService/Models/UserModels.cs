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

using Sannr;

namespace Sannr.Demo.ApiService.Models;

/// <summary>
/// User registration request - demonstrates comprehensive validation.
/// Features: Required, StringLength, EmailAddress, Range, Sanitize, Display
/// </summary>
public partial class UserRegistrationRequest
{
    /// <summary>Username with length constraints and sanitization.</summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be 3-50 characters")]
    [Sanitize(Trim = true, ToLower = true)]
    [Display(Name = "Username")]
    public string? Username { get; set; }

    /// <summary>Email address with format validation.</summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    /// <summary>Password with complex validation rules.</summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be 8-100 characters")]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    /// <summary>Age with range validation.</summary>
    [Required(ErrorMessage = "Age is required")]
    [Range(13, 120, ErrorMessage = "Age must be between 13 and 120")]
    [Display(Name = "Age")]
    public int? Age { get; set; }

    /// <summary>Phone number with format validation.</summary>
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    /// <summary>Website URL validation.</summary>
    [Url(ErrorMessage = "Please enter a valid website URL")]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    /// <summary>Credit card for premium accounts.</summary>
    [CreditCard(ErrorMessage = "Please enter a valid credit card number")]
    [Display(Name = "Credit Card (Optional)")]
    public string? CreditCard { get; set; }
}

/// <summary>
/// User profile update - demonstrates conditional validation.
/// Features: RequiredIf, business rules, sanitization
/// </summary>
public partial class UserProfileUpdateRequest
{
    /// <summary>Current password (required for sensitive changes).</summary>
    [RequiredIf("Email", "*", ErrorMessage = "Current password is required when changing email")]
    [Display(Name = "Current Password")]
    public string? CurrentPassword { get; set; }

    /// <summary>New email address.</summary>
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "New Email Address")]
    public string? Email { get; set; }

    /// <summary>New phone number.</summary>
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "New Phone Number")]
    public string? PhoneNumber { get; set; }

    /// <summary>Display name.</summary>
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Display name must be 2-100 characters")]
    [Sanitize(Trim = true)]
    [Display(Name = "Display Name")]
    public string? DisplayName { get; set; }

    /// <summary>Bio/About section.</summary>
    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
    [Display(Name = "Bio")]
    public string? Bio { get; set; }

    /// <summary>Website URL.</summary>
    [Url(ErrorMessage = "Please enter a valid website URL")]
    [Display(Name = "Website")]
    public string? Website { get; set; }
}

/// <summary>
/// Contact form - demonstrates anti-spam validation.
/// Features: Required, EmailAddress, StringLength, custom business rules
/// </summary>
public partial class ContactFormRequest
{
    /// <summary>Contact name.</summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2-100 characters")]
    [Sanitize(Trim = true)]
    [Display(Name = "Your Name")]
    public string? Name { get; set; }

    /// <summary>Contact email.</summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    /// <summary>Subject line.</summary>
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Subject must be 5-200 characters")]
    [Display(Name = "Subject")]
    public string? Subject { get; set; }

    /// <summary>Message content.</summary>
    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be 10-2000 characters")]
    [Display(Name = "Message")]
    public string? Message { get; set; }

    /// <summary>Anti-spam honeypot field (should be empty).</summary>
    [StringLength(0, ErrorMessage = "This field should be left empty")]
    [Display(Name = "Leave this field empty")]
    public string? Honeypot { get; set; }
}

/// <summary>
/// Newsletter subscription - demonstrates email validation.
/// Features: Required, EmailAddress, custom business rules
/// </summary>
public partial class NewsletterSubscriptionRequest
{
    /// <summary>Email address for subscription.</summary>
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    /// <summary>Subscription preferences.</summary>
    [Required(ErrorMessage = "Please select at least one subscription preference")]
    [Display(Name = "Subscription Preferences")]
    public SubscriptionPreferences? Preferences { get; set; }

    /// <summary>Marketing consent.</summary>
    [Display(Name = "I agree to receive marketing communications")]
    public bool MarketingConsent { get; set; }
}

/// <summary>
/// Subscription preferences.
/// </summary>
public partial class SubscriptionPreferences
{
    /// <summary>Receive product updates.</summary>
    [Display(Name = "Product Updates")]
    public bool ProductUpdates { get; set; }

    /// <summary>Receive promotional offers.</summary>
    [Display(Name = "Promotional Offers")]
    public bool PromotionalOffers { get; set; }

    /// <summary>Receive newsletter.</summary>
    [Display(Name = "Newsletter")]
    public bool Newsletter { get; set; }

    /// <summary>Receive event invitations.</summary>
    [Display(Name = "Event Invitations")]
    public bool EventInvitations { get; set; }
}