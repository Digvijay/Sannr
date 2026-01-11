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
/// Order placement request - demonstrates complex nested validation.
/// Features: Required, RequiredIf, Range, nested object validation
/// </summary>
public partial class OrderCreateRequest
{
    /// <summary>Customer ID.</summary>
    [Required(ErrorMessage = "Customer ID is required")]
    [Display(Name = "Customer ID")]
    public Guid? CustomerId { get; set; }

    /// <summary>Order items.</summary>
    [Required(ErrorMessage = "At least one item is required")]
    [Display(Name = "Order Items")]
    public List<OrderItemRequest>? Items { get; set; }

    /// <summary>Shipping address.</summary>
    [Required(ErrorMessage = "Shipping address is required")]
    [Display(Name = "Shipping Address")]
    public AddressRequest? ShippingAddress { get; set; }

    /// <summary>Billing address (optional, defaults to shipping).</summary>
    [Display(Name = "Billing Address")]
    public AddressRequest? BillingAddress { get; set; }

    /// <summary>Payment method.</summary>
    [Required(ErrorMessage = "Payment method is required")]
    [Display(Name = "Payment Method")]
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>Credit card details (required if payment method is credit card).</summary>
    [RequiredIf("PaymentMethod", Sannr.Demo.ApiService.Models.PaymentMethod.CreditCard, ErrorMessage = "Credit card details are required for credit card payments")]
    [Display(Name = "Credit Card Details")]
    public CreditCardDetails? CreditCardDetails { get; set; }

    /// <summary>Order notes.</summary>
    [StringLength(500, ErrorMessage = "Order notes cannot exceed 500 characters")]
    [Display(Name = "Order Notes")]
    public string? Notes { get; set; }
}

/// <summary>
/// Order item request.
/// </summary>
public partial class OrderItemRequest
{
    /// <summary>Product ID.</summary>
    [Required(ErrorMessage = "Product ID is required")]
    [Display(Name = "Product ID")]
    public Guid ProductId { get; set; }

    /// <summary>Quantity.</summary>
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    [Display(Name = "Quantity")]
    public int? Quantity { get; set; }

    /// <summary>Unit price.</summary>
    [Required(ErrorMessage = "Unit price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Unit price must be between $0.01 and $999,999.99")]
    [Display(Name = "Unit Price")]
    public decimal? UnitPrice { get; set; }
}

/// <summary>
/// Address request with comprehensive validation.
/// </summary>
public partial class AddressRequest
{
    /// <summary>Street address.</summary>
    [Required(ErrorMessage = "Street address is required")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Street address must be 5-200 characters")]
    [Display(Name = "Street Address")]
    public string? StreetAddress { get; set; }

    /// <summary>City.</summary>
    [Required(ErrorMessage = "City is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be 2-100 characters")]
    [Display(Name = "City")]
    public string? City { get; set; }

    /// <summary>State/Province.</summary>
    [Required(ErrorMessage = "State/Province is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "State/Province must be 2-100 characters")]
    [Display(Name = "State/Province")]
    public string? State { get; set; }

    /// <summary>Postal code.</summary>
    [Required(ErrorMessage = "Postal code is required")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Postal code must be 3-20 characters")]
    [Display(Name = "Postal Code")]
    public string? PostalCode { get; set; }

    /// <summary>Country.</summary>
    [Required(ErrorMessage = "Country is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Country must be 2-100 characters")]
    [Display(Name = "Country")]
    public string? Country { get; set; }
}

/// <summary>
/// Credit card details.
/// </summary>
public partial class CreditCardDetails
{
    /// <summary>Credit card number.</summary>
    [Required(ErrorMessage = "Credit card number is required")]
    [CreditCard(ErrorMessage = "Please enter a valid credit card number")]
    [Display(Name = "Card Number")]
    public string? CardNumber { get; set; }

    /// <summary>Expiration month.</summary>
    [Required(ErrorMessage = "Expiration month is required")]
    [Range(1, 12, ErrorMessage = "Expiration month must be between 1 and 12")]
    [Display(Name = "Expiration Month")]
    public int? ExpirationMonth { get; set; }

    /// <summary>Expiration year.</summary>
    [Required(ErrorMessage = "Expiration year is required")]
    [Range(2024, 2050, ErrorMessage = "Expiration year must be between 2024 and 2050")]
    [Display(Name = "Expiration Year")]
    public int? ExpirationYear { get; set; }

    /// <summary>CVV security code.</summary>
    [Required(ErrorMessage = "CVV is required")]
    [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3-4 digits")]
    [Display(Name = "CVV")]
    public string? Cvv { get; set; }

    /// <summary>Cardholder name.</summary>
    [Required(ErrorMessage = "Cardholder name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Cardholder name must be 2-100 characters")]
    [Display(Name = "Cardholder Name")]
    public string? CardholderName { get; set; }
}

/// <summary>
/// Payment method enumeration.
/// </summary>
public enum PaymentMethod
{
    CreditCard,
    PayPal,
    BankTransfer,
    CashOnDelivery
}