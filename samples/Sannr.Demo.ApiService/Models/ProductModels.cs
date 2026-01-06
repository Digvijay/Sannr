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
/// Product creation request - demonstrates file validation and business rules.
/// Features: Required, StringLength, Range, FileExtensions, CustomValidator
/// </summary>
public partial class ProductCreateRequest
{
    /// <summary>Product name with length constraints.</summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be 2-200 characters")]
    [Display(Name = "Product Name")]
    public string? Name { get; set; }

    /// <summary>Product description.</summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>Product price with decimal range.</summary>
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
    [Display(Name = "Price")]
    public decimal? Price { get; set; }

    /// <summary>Stock quantity.</summary>
    [Range(0, 10000, ErrorMessage = "Stock must be between 0 and 10,000")]
    [Display(Name = "Stock Quantity")]
    public int? StockQuantity { get; set; }

    /// <summary>Product category.</summary>
    [Required(ErrorMessage = "Category is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    [Display(Name = "Category")]
    public string? Category { get; set; }

    /// <summary>Product image file.</summary>
    [FileExtensions(Extensions = "jpg,jpeg,png,gif,webp", ErrorMessage = "Only image files are allowed")]
    [Display(Name = "Product Image")]
    public string? ImageFileName { get; set; }

    /// <summary>Is product featured.</summary>
    [Display(Name = "Featured Product")]
    public bool? IsFeatured { get; set; }
}

/// <summary>
/// Product search request - demonstrates query parameter validation.
/// Features: StringLength, Range validation on parameters
/// </summary>
public partial class ProductSearchRequest
{
    /// <summary>Search query.</summary>
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Search query must be 2-100 characters")]
    [Display(Name = "Search Query")]
    public string? Query { get; set; }

    /// <summary>Product category filter.</summary>
    [StringLength(50, ErrorMessage = "Category filter cannot exceed 50 characters")]
    [Display(Name = "Category")]
    public string? Category { get; set; }

    /// <summary>Minimum price filter.</summary>
    [Range(0, 999999.99, ErrorMessage = "Minimum price must be between $0 and $999,999.99")]
    [Display(Name = "Minimum Price")]
    public decimal? MinPrice { get; set; }

    /// <summary>Maximum price filter.</summary>
    [Range(0, 999999.99, ErrorMessage = "Maximum price must be between $0 and $999,999.99")]
    [Display(Name = "Maximum Price")]
    public decimal? MaxPrice { get; set; }

    /// <summary>Page number for pagination.</summary>
    [Range(1, 1000, ErrorMessage = "Page must be between 1 and 1000")]
    [Display(Name = "Page Number")]
    public int? Page { get; set; } = 1;

    /// <summary>Page size for pagination.</summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    [Display(Name = "Page Size")]
    public int? PageSize { get; set; } = 20;
}