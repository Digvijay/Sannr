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

using System.Globalization;
using System.Resources;

namespace Sannr.Tests.Resources;

/// <summary>
/// Strongly-typed resource class for validation messages.
/// This would normally be auto-generated from ValidationMessages.resx
/// </summary>
public class ValidationMessages
{
    private static readonly ResourceManager ResourceManager =
        new ResourceManager("Sannr.Tests.Resources.ValidationMessages",
                           typeof(ValidationMessages).Assembly);

    /// <summary>
    /// Gets the localized string for RequiredFieldRequired.
    /// English: "The {0} field is required."
    /// Spanish: "El campo {0} es obligatorio."
    /// </summary>
    public static string RequiredFieldRequired =>
        ResourceManager.GetString("RequiredFieldRequired", CultureInfo.CurrentCulture)
        ?? "The {0} field is required."; // Fallback

    /// <summary>
    /// Gets the localized string for EmailFieldInvalid.
    /// English: "The {0} field is not a valid email address."
    /// Spanish: "El campo {0} no es una dirección de correo electrónico válida."
    /// </summary>
    public static string EmailFieldInvalid =>
        ResourceManager.GetString("EmailFieldInvalid", CultureInfo.CurrentCulture)
        ?? "The {0} field is not a valid email address."; // Fallback

    /// <summary>
    /// Gets the localized string for AgeFieldOutOfRange.
    /// English: "The {0} field must be between {1} and {2}."
    /// Spanish: "El campo {0} debe estar entre {1} y {2}."
    /// </summary>
    public static string AgeFieldOutOfRange =>
        ResourceManager.GetString("AgeFieldOutOfRange", CultureInfo.CurrentCulture)
        ?? "The {0} field must be between {1} and {2}."; // Fallback

    /// <summary>
    /// Gets the localized string for NameRequired.
    /// English: "Name is required and cannot be empty."
    /// Spanish: "El nombre es obligatorio y no puede estar vacío."
    /// </summary>
    public static string NameRequired =>
        ResourceManager.GetString("NameRequired", CultureInfo.CurrentCulture)
        ?? "Name is required and cannot be empty."; // Fallback

    /// <summary>
    /// Gets the localized string for EmailRequired.
    /// English: "Email address is required."
    /// Spanish: "La dirección de correo electrónico es obligatoria."
    /// </summary>
    public static string EmailRequired =>
        ResourceManager.GetString("EmailRequired", CultureInfo.CurrentCulture)
        ?? "Email address is required."; // Fallback

    /// <summary>
    /// Gets the localized string for AgeRange.
    /// English: "Age must be between {0} and {1} years."
    /// Spanish: "La edad debe estar entre {0} y {1} años."
    /// </summary>
    public static string AgeRange =>
        ResourceManager.GetString("AgeRange", CultureInfo.CurrentCulture)
        ?? "Age must be between {0} and {1} years."; // Fallback
}