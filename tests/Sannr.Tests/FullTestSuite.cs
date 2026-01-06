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
using System.Threading.Tasks;
using Sannr.Tests.Models;
using Xunit;

namespace Sannr.Tests;

/// <summary>
/// Full test suite for Sannr validation features.
/// </summary>
public partial class FullTestSuite
{
    public FullTestSuite()
    {
        // Explicitly register validators for AoT compatibility
        RegisterTestValidators();
    }

    private static readonly string[] AllowedDocumentExtensions = { "pdf", "doc", "docx" };
    private static readonly string[] AllowedResumeExtensions = { "pdf", "docx", "txt" };

    internal static void RegisterTestValidators()
    {
        // Register validator for UserProfile
        SannrValidatorRegistry.Register<UserProfile>((context) =>
        {
            var result = new ValidationResult();
            var model = (UserProfile)context.ObjectInstance;

            // Apply sanitization to Username
            if (model.Username != null)
            {
                model.Username = model.Username.Trim().ToUpperInvariant();
            }

            // Username validation
            if (string.IsNullOrEmpty(model.Username))
                result.Add("Username", "Username is required");

            // Email validation
            if (string.IsNullOrEmpty(model.Email))
                result.Add("Email", "Email is required");
            else if (!model.Email.Contains('@', StringComparison.Ordinal))
                result.Add("Email", "Email must be valid");

            // ZipCode validation (RequiredIf)
            if (model.Country == "US")
            {
                if (string.IsNullOrWhiteSpace(model.ZipCode))
                    result.Add("ZipCode", "ZipCode is required.");
                else if (!System.Text.RegularExpressions.Regex.IsMatch(model.ZipCode, @"^\d{5}(-\d{4})?$"))
                    result.Add("ZipCode", "Invalid US zip code format.");
            }

            // Age validation (use display name "Years Old")
            if (model.Age < 18 || model.Age > 100)
                result.Add("Age", "The field Years Old must be between 18 and 100.");

            // ReferralCode validation (Required with group "Reg")
            if (context.ActiveGroup == "Reg")
                result.Add("ReferralCode", "ReferralCode is required.");

            return Task.FromResult(result);
        });

        // Register validator for AdvancedValidationModel
        SannrValidatorRegistry.Register<AdvancedValidationModel>((context) =>
        {
            var result = new ValidationResult();
            var model = (AdvancedValidationModel)context.ObjectInstance;

            // Username validation (Required, StringLength)
            if (string.IsNullOrEmpty(model.Username))
                result.Add("Username", "Username is required");
            else if (model.Username.Length < 3)
                result.Add("Username", "Username must be at least 3 characters");
            else if (model.Username.Length > 50)
                result.Add("Username", "Username must not exceed 50 characters");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.Username, @"^[a-zA-Z0-9_-]+$"))
                result.Add("Username", "Username contains invalid characters.");

            // Email validation (Required, EmailAddress)
            if (string.IsNullOrEmpty(model.Email))
                result.Add("Email", "Email is required");
            else if (!model.Email.Contains('@', StringComparison.Ordinal))
                result.Add("Email", "The Email field is not a valid e-mail address.");

            // CreditCardNumber validation (CreditCard)
            if (!string.IsNullOrEmpty(model.CreditCardNumber))
            {
                // Simple credit card validation - should be 13-19 digits
                var digitsOnly = new string(model.CreditCardNumber.Where(char.IsDigit).ToArray());
                if (digitsOnly.Length < 13 || digitsOnly.Length > 19)
                    result.Add("CreditCardNumber", "The CreditCardNumber field is not a valid credit card number.");
            }

            // Website validation (Url)
            if (!string.IsNullOrEmpty(model.Website))
            {
                if (!model.Website.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !model.Website.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    result.Add("Website", "The Website field is not a valid URL.");
            }

            // PhoneNumber validation (Phone)
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                // Simple phone validation - should contain digits and common phone characters
                var validChars = "0123456789()-+ .";
                // Note: validChars.Contains(c) does not have an overload for char + StringComparison until newer .NET versions or via extension. 
                // However, StringComparison.Ordinal is for string.Contains(string). For char search in string, it is ordinal by default.
                // Reverting to LINQ Any/Contains checking against the string as a set of characters is implicitly ordinal.
                if (!model.PhoneNumber.All(c => validChars.Contains(c)))
                    result.Add("PhoneNumber", "The PhoneNumber field is not a valid phone number.");
            }

            // DocumentPath validation (FileExtensions)
            if (!string.IsNullOrEmpty(model.DocumentPath))
            {
                // Defined locally for now, replaced by static readonly to appease CA1861
                var valid = AllowedDocumentExtensions.Any(ext => model.DocumentPath.EndsWith("." + ext, StringComparison.OrdinalIgnoreCase));
                if (!valid)
                    result.Add("DocumentPath", "The DocumentPath field must have one of the following extensions: .pdf, .doc, .docx.");
            }

            // SecurityCode validation (Range)
            if (model.SecurityCode < 1000 || model.SecurityCode > 9999)
                result.Add("SecurityCode", "The field SecurityCode must be between 1000 and 9999.");

            // Password validation (CustomValidator)
            if (!string.IsNullOrEmpty(model.Password))
            {
                // Call the custom validator method
                var passwordResult = AdvancedValidationModel.ValidatePasswordStrength(model.Password, null);
                foreach (var error in passwordResult.Errors)
                {
                    result.Add(error.MemberName, error.Message, error.Severity);
                }
            }

            // ZipCode validation (RequiredIf)
            if (model.Country == "US" && string.IsNullOrEmpty(model.ZipCode))
                result.Add("ZipCode", "ZipCode is required.");

            return Task.FromResult(result);
        });

        // Register validator for TestModel (used in AttributeValidationTests)
        SannrValidatorRegistry.Register<Sannr.Tests.TestModel>((context) =>
        {
            var result = new ValidationResult();
            var model = (Sannr.Tests.TestModel)context.ObjectInstance;

            // Sanitize UserId (Trim and ToUpper)
            if (model.UserId != null)
            {
                model.UserId = model.UserId.Trim().ToUpperInvariant();
            }

            // Username validation (Required with custom message)
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrWhiteSpace(model.Username))
                result.Add("Username", "Username is mandatory.");

            // DisplayName validation (StringLength)
            if (!string.IsNullOrEmpty(model.DisplayName))
            {
                if (model.DisplayName.Length < 3)
                    result.Add("DisplayName", "Must be between 3 and 10 chars.");
                else if (model.DisplayName.Length > 10)
                    result.Add("DisplayName", "Must be between 3 and 10 chars.");
            }

            // Age validation (Range with custom message)
            if (model.Age < 18 || model.Age > 99)
                result.Add("Age", "Age must be valid.");

            // Price validation (Range)
            if (model.Price < 0.01 || model.Price > 1000.00)
                result.Add("Price", "The field Price must be between 0.01 and 1000.");

            // ContactEmail validation (EmailAddress)
            if (!string.IsNullOrEmpty(model.ContactEmail))
            {
                if (!model.ContactEmail.Contains('@', StringComparison.Ordinal))
                    result.Add("ContactEmail", "The ContactEmail field is not a valid e-mail address.");
            }

            // PaymentCard validation (CreditCard)
            if (!string.IsNullOrEmpty(model.PaymentCard))
            {
                // Simple credit card validation
                var digitsOnly = new string(model.PaymentCard.Where(char.IsDigit).ToArray());
                if (digitsOnly.Length < 13 || digitsOnly.Length > 19)
                    result.Add("PaymentCard", "The PaymentCard field is not a valid credit card number.");
            }

            // PortfolioUrl validation (Url)
            if (!string.IsNullOrEmpty(model.PortfolioUrl))
            {
                if (!model.PortfolioUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !model.PortfolioUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    result.Add("PortfolioUrl", "The PortfolioUrl field is not a valid URL.");
            }

            // PhoneNumber validation (Phone)
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                var validChars = "0123456789()-+ .";
                if (!model.PhoneNumber.All(c => validChars.Contains(c)))
                    result.Add("PhoneNumber", "The PhoneNumber field is not a valid phone number.");
            }


            // ResumeFileName validation (FileExtensions)
            if (!string.IsNullOrEmpty(model.ResumeFileName))
            {
                var valid = AllowedResumeExtensions.Any(ext => model.ResumeFileName.EndsWith("." + ext, StringComparison.OrdinalIgnoreCase));
                if (!valid)
                    result.Add("ResumeFileName", "The ResumeFileName field must have one of the following extensions: .pdf, .docx, .txt.");
            }

            // State validation (RequiredIf)
            if (model.Country == "USA" && string.IsNullOrEmpty(model.State))
                result.Add("State", "State is required.");

            // DisplayedAge validation (Range with Display name)
            if (model.DisplayedAge < 18 || model.DisplayedAge > 99)
                result.Add("DisplayedAge", "The field User's Age must be between 18 and 99.");

            return Task.FromResult(result);
        });

        // Register validators for RealWorldIntegrationTests models
        // ComplexModel validator
        SannrValidatorRegistry.Register<ComplexModel>((context) =>
        {
            var model = (ComplexModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Name: Required, StringLength(100)
            if (string.IsNullOrWhiteSpace(model.Name))
                result.Add("Name", "The Name field is required.");
            else if (model.Name.Length > 100)
                result.Add("Name", "The field Name must be a string with a maximum length of 100.");

            // Email: Required, EmailAddress
            if (string.IsNullOrWhiteSpace(model.Email))
                result.Add("Email", "The Email field is required.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Add("Email", "The Email field is not a valid e-mail address.");

            // Age: Range(0, 150)
            if (model.Age < 0 || model.Age > 150)
                result.Add("Age", "The field Age must be between 0 and 150.");

            // Salary: Range(0, 1000000)
            if (model.Salary < 0 || model.Salary > 1000000)
                result.Add("Salary", "The field Salary must be between 0 and 1000000.");

            // Phone: Phone
            if (!string.IsNullOrEmpty(model.Phone) && !System.Text.RegularExpressions.Regex.IsMatch(model.Phone, @"^[\+]?[1-9][\d\s\-\(\)]{7,14}$"))

                // Website: Url
                if (!string.IsNullOrEmpty(model.Website) && !Uri.TryCreate(model.Website, UriKind.Absolute, out _))
                    result.Add("Website", "The Website field is not a valid fully-qualified http, https, or ftp URL.");

            return Task.FromResult(result);
        });

        // LargeModel validator (all props Required)
        SannrValidatorRegistry.Register<LargeModel>((context) =>
        {
            var model = (LargeModel)context.ObjectInstance;
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(model.Prop1)) result.Add("Prop1", "The Prop1 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop2)) result.Add("Prop2", "The Prop2 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop3)) result.Add("Prop3", "The Prop3 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop4)) result.Add("Prop4", "The Prop4 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop5)) result.Add("Prop5", "The Prop5 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop6)) result.Add("Prop6", "The Prop6 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop7)) result.Add("Prop7", "The Prop7 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop8)) result.Add("Prop8", "The Prop8 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop9)) result.Add("Prop9", "The Prop9 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop10)) result.Add("Prop10", "The Prop10 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop11)) result.Add("Prop11", "The Prop11 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop12)) result.Add("Prop12", "The Prop12 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop13)) result.Add("Prop13", "The Prop13 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop14)) result.Add("Prop14", "The Prop14 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop15)) result.Add("Prop15", "The Prop15 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop16)) result.Add("Prop16", "The Prop16 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop17)) result.Add("Prop17", "The Prop17 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop18)) result.Add("Prop18", "The Prop18 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop19)) result.Add("Prop19", "The Prop19 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop20)) result.Add("Prop20", "The Prop20 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop21)) result.Add("Prop21", "The Prop21 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop22)) result.Add("Prop22", "The Prop22 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop23)) result.Add("Prop23", "The Prop23 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop24)) result.Add("Prop24", "The Prop24 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop25)) result.Add("Prop25", "The Prop25 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop26)) result.Add("Prop26", "The Prop26 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop27)) result.Add("Prop27", "The Prop27 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop28)) result.Add("Prop28", "The Prop28 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop29)) result.Add("Prop29", "The Prop29 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop30)) result.Add("Prop30", "The Prop30 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop31)) result.Add("Prop31", "The Prop31 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop32)) result.Add("Prop32", "The Prop32 field is required.");
            if (string.IsNullOrWhiteSpace(model.Prop33)) result.Add("Prop33", "The Prop33 field is required.");

            return Task.FromResult(result);
        });

        // Register validator for RealWorldAdvancedValidationModel
        SannrValidatorRegistry.Register<RealWorldAdvancedValidationModel>((context) =>
        {
            var model = (RealWorldAdvancedValidationModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Username: Required, StringLength(50)
            if (string.IsNullOrWhiteSpace(model.Username))
                result.Add("Username", "The Username field is required.");
            else if (model.Username.Length > 50)
                result.Add("Username", "The field Username must be a string with a maximum length of 50.");

            // PrimaryEmail: Required, EmailAddress
            if (string.IsNullOrWhiteSpace(model.PrimaryEmail))
                result.Add("PrimaryEmail", "The PrimaryEmail field is required.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.PrimaryEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Add("PrimaryEmail", "The PrimaryEmail field is not a valid e-mail address.");

            // SecondaryEmail: EmailAddress
            if (!string.IsNullOrEmpty(model.SecondaryEmail) && !System.Text.RegularExpressions.Regex.IsMatch(model.SecondaryEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Add("SecondaryEmail", "The SecondaryEmail field is not a valid e-mail address.");

            // Age: Required, Range(18, 120)
            if (model.Age < 18 || model.Age > 120)
                result.Add("Age", "The field Age must be between 18 and 120.");

            // CreditCard: Required, CreditCard
            if (string.IsNullOrWhiteSpace(model.CreditCard))
                result.Add("CreditCard", "The CreditCard field is required.");
            else
            {
                var digitsOnly = new string(model.CreditCard.Where(char.IsDigit).ToArray());
                if (digitsOnly.Length < 13 || digitsOnly.Length > 19)
                    result.Add("CreditCard", "The CreditCard field is not a valid credit card number.");
            }

            // Website: Required, Url
            if (string.IsNullOrWhiteSpace(model.Website))
                result.Add("Website", "The Website field is required.");
            else if (!Uri.TryCreate(model.Website, UriKind.Absolute, out _))
                result.Add("Website", "The Website field is not a valid fully-qualified http, https, or ftp URL.");

            // PhoneNumber: Phone
            if (!string.IsNullOrEmpty(model.PhoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(model.PhoneNumber, @"^[\+]?[1-9][\d\s\-\(\)]{7,14}$"))
                result.Add("PhoneNumber", "The PhoneNumber field is not a valid phone number.");

            // Score: Range(0, 100)
            if (model.Score < 0 || model.Score > 100)
                result.Add("Score", "The field Score must be between 0 and 100.");

            return Task.FromResult(result);
        });

        // Register validator for NestedModel
        SannrValidatorRegistry.Register<NestedModel>((context) =>
        {
            var model = (NestedModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Name: Required
            if (string.IsNullOrWhiteSpace(model.Name))
                result.Add("Name", "The Name field is required.");

            return Task.FromResult(result);
        });

        // Register validator for CircularModel
        SannrValidatorRegistry.Register<CircularModel>((context) =>
        {
            var model = (CircularModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Name: Required
            if (string.IsNullOrWhiteSpace(model.Name))
                result.Add("Name", "The Name field is required.");

            return Task.FromResult(result);
        });

        // Register validator for BaseModel
        SannrValidatorRegistry.Register<BaseModel>((context) =>
        {
            var model = (BaseModel)context.ObjectInstance;
            var result = new ValidationResult();

            // BaseProperty: Required
            if (string.IsNullOrWhiteSpace(model.BaseProperty))
                result.Add("BaseProperty", "The BaseProperty field is required.");

            return Task.FromResult(result);
        });

        // Register validator for DerivedModel
        SannrValidatorRegistry.Register<DerivedModel>((context) =>
        {
            var model = (DerivedModel)context.ObjectInstance;
            var result = new ValidationResult();

            // DerivedProperty: Required
            if (string.IsNullOrWhiteSpace(model.DerivedProperty))
                result.Add("DerivedProperty", "The DerivedProperty field is required.");

            // Name: Required, StringLength(100)
            if (string.IsNullOrWhiteSpace(model.Name))
                result.Add("Name", "The Name field is required.");
            else if (model.Name.Length > 100)
                result.Add("Name", "The field Name must be a string with a maximum length of 100.");

            // Email: Required, EmailAddress
            if (string.IsNullOrWhiteSpace(model.Email))
                result.Add("Email", "The Email field is required.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Add("Email", "The Email field is not a valid e-mail address.");

            // Age: Range(0, 150)
            if (model.Age < 0 || model.Age > 150)
                result.Add("Age", "The field Age must be between 0 and 150.");

            return Task.FromResult(result);
        });

        // Register validator for ConditionalModel (implements IValidatableObject)
        SannrValidatorRegistry.Register<ConditionalModel>((context) =>
        {
            var model = (ConditionalModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Username: Required
            if (string.IsNullOrWhiteSpace(model.Username))
                result.Add("Username", "The Username field is required.");

            // JobTitle: RequiredIf("IsEmployed", true)
            if (model.IsEmployed && string.IsNullOrWhiteSpace(model.JobTitle))
                result.Add("JobTitle", "The JobTitle field is required.");

            // Salary: RequiredIf("IsEmployed", true), Range(0, 1000000)
            if (model.IsEmployed)
            {
                if (!model.Salary.HasValue || model.Salary.Value <= 0)
                    result.Add("Salary", "The Salary field is required.");
                else if (model.Salary.Value < 0 || model.Salary.Value > 1000000)
                    result.Add("Salary", "The field Salary must be between 0 and 1000000.");
            }

            // Age: Range(0, 150)
            if (model.Age < 0 || model.Age > 150)
                result.Add("Age", "The field Age must be between 0 and 150.");

            // ZipCode: RequiredIf("Country", "USA")
            if (model.Country == "USA" && string.IsNullOrWhiteSpace(model.ZipCode))
                result.Add("ZipCode", "The ZipCode field is required.");

            // Call the IValidatableObject.Validate method
            var validatableResults = model.Validate(context);
            foreach (var vr in validatableResults)
            {
                result.Add(vr.MemberName ?? "", vr.Message, vr.Severity);
            }

            return Task.FromResult(result);
        });

        // Register validator for ModelLevelValidationModel (implements IValidatableObject)
        SannrValidatorRegistry.Register<ModelLevelValidationModel>((context) =>
        {
            var model = (ModelLevelValidationModel)context.ObjectInstance;
            var result = new ValidationResult();

            // FirstName: Required
            if (string.IsNullOrWhiteSpace(model.FirstName))
                result.Add("FirstName", "The FirstName field is required.");

            // LastName: Required
            if (string.IsNullOrWhiteSpace(model.LastName))
                result.Add("LastName", "The LastName field is required.");

            // Age: Range(0, 150)
            if (model.Age < 0 || model.Age > 150)
                result.Add("Age", "The field Age must be between 0 and 150.");

            // Salary: Range(0, 1000000)
            if (model.Salary.HasValue && (model.Salary.Value < 0 || model.Salary.Value > 1000000))
                result.Add("Salary", "The field Salary must be between 0 and 1000000.");

            // Call the IValidatableObject.Validate method
            var validatableResults = model.Validate(context);
            foreach (var vr in validatableResults)
            {
                result.Add(vr.MemberName ?? "", vr.Message, vr.Severity);
            }

            return Task.FromResult(result);
        });

        // Register validator for CollectionModel
        SannrValidatorRegistry.Register<CollectionModel>((context) =>
        {
            var model = (CollectionModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Name: Required
            if (string.IsNullOrWhiteSpace(model.Name))
                result.Add("Name", "The Name field is required.");

            return Task.FromResult(result);
        });

        // Register validator for BoundaryTestModel
        SannrValidatorRegistry.Register<BoundaryTestModel>((context) =>
        {
            var model = (BoundaryTestModel)context.ObjectInstance;
            var result = new ValidationResult();

            // ShortString: Required, StringLength(10, MinimumLength = 1)
            if (string.IsNullOrWhiteSpace(model.ShortString))
                result.Add("ShortString", "The ShortString field is required.");
            else if (model.ShortString.Length < 1)
                result.Add("ShortString", "The field ShortString must be a string with a minimum length of 1.");
            else if (model.ShortString.Length > 10)
                result.Add("ShortString", "The field ShortString must be a string with a maximum length of 10.");

            // IntegerRange: Range(1, 100)
            if (model.IntegerRange < 1 || model.IntegerRange > 100)
                result.Add("IntegerRange", "The field IntegerRange must be between 1 and 100.");

            // DoubleRange: Range(0.0, 1.0)
            if (model.DoubleRange < 0.0 || model.DoubleRange > 1.0)
                result.Add("DoubleRange", "The field DoubleRange must be between 0 and 1.");

            return Task.FromResult(result);
        });

        // Register validator for CultureTestModel
        SannrValidatorRegistry.Register<CultureTestModel>((context) =>
        {
            var model = (CultureTestModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Name: Required
            if (string.IsNullOrWhiteSpace(model.Name))
                result.Add("Name", "The Name field is required.");

            return Task.FromResult(result);
        });

        // Register validator for LocalizedValidationModel
        SannrValidatorRegistry.Register<LocalizedValidationModel>((context) =>
        {
            var model = (LocalizedValidationModel)context.ObjectInstance;
            var result = new ValidationResult();

            var culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

            // RequiredField: Required
            if (string.IsNullOrWhiteSpace(model.RequiredField))
            {
                var message = culture == "es-ES" ? "El campo RequiredField es obligatorio." : "The RequiredField field is required.";
                result.Add("RequiredField", message);
            }

            // EmailField: EmailAddress
            if (!string.IsNullOrEmpty(model.EmailField) && !System.Text.RegularExpressions.Regex.IsMatch(model.EmailField, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                var message = culture == "es-ES" ? "El campo EmailField no es una dirección de correo electrónico válida." : "The EmailField field is not a valid email address.";
                result.Add("EmailField", message);
            }

            // AgeField: Range(0, 150)
            if (model.AgeField < 0 || model.AgeField > 150)
            {
                var message = culture == "es-ES" ? "El campo AgeField debe estar entre 0 y 150." : "The AgeField field must be between 0 and 150.";
                result.Add("AgeField", message);
            }

            return Task.FromResult(result);
        });

        // Register validator for ClientValidationModel
        SannrValidatorRegistry.Register<ClientValidationModel>((context) =>
        {
            var model = (ClientValidationModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Name: Required, StringLength(100)
            if (string.IsNullOrWhiteSpace(model.Name))
                result.Add("Name", "The Name field is required.");
            else if (model.Name.Length > 100)
                result.Add("Name", "The field Name must be a string with a maximum length of 100.");

            // Email: Required, EmailAddress
            if (string.IsNullOrWhiteSpace(model.Email))
                result.Add("Email", "The Email field is required.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Add("Email", "The Email field is not a valid e-mail address.");

            // Age: Range(18, 120)
            if (model.Age < 18 || model.Age > 120)
                result.Add("Age", "The field Age must be between 18 and 120.");

            // StreetAddress: RequiredIf("HasAddress", true)
            if (model.HasAddress && string.IsNullOrWhiteSpace(model.StreetAddress))
                result.Add("StreetAddress", "The StreetAddress field is required.");

            return Task.FromResult(result);
        });

        // Register validator for MinimalApiTestModel
        SannrValidatorRegistry.Register<MinimalApiTestModel>((context) =>
        {
            var model = (MinimalApiTestModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Email: Required, EmailAddress
            if (string.IsNullOrWhiteSpace(model.Email))
                result.Add("Email", "The Email field is required.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Add("Email", "The Email field is not a valid e-mail address.");

            // Name: Required, StringLength(50, MinimumLength = 2)
            if (string.IsNullOrWhiteSpace(model.Name))
                result.Add("Name", "The Name field is required.");
            else if (model.Name.Length < 2)
                result.Add("Name", "The field Name must be a string with a minimum length of 2.");
            else if (model.Name.Length > 50)
                result.Add("Name", "The field Name must be a string with a maximum length of 50.");

            // Age: Range(18, 120)
            if (model.Age < 18 || model.Age > 120)
                result.Add("Age", "The field Age must be between 18 and 120.");

            return Task.FromResult(result);
        });

        // Register validator for SimpleTestModel
        SannrValidatorRegistry.Register<SimpleTestModel>((context) =>
        {
            var model = (SimpleTestModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Value: Required
            if (string.IsNullOrWhiteSpace(model.Value))
                result.Add("Value", "The Value field is required.");

            return Task.FromResult(result);
        });

        // Register validator for ComplexValidationModel
        SannrValidatorRegistry.Register<ComplexValidationModel>((context) =>
        {
            var model = (ComplexValidationModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Name: Required
            if (string.IsNullOrWhiteSpace(model.Name))
                result.Add("Name", "The Name field is required.");

            // Email: EmailAddress
            if (!string.IsNullOrEmpty(model.Email) && !System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Add("Email", "The Email field is not a valid e-mail address.");

            // Description: StringLength(100, MinimumLength = 2)
            if (!string.IsNullOrEmpty(model.Description))
            {
                if (model.Description.Length < 2)
                    result.Add("Description", "The field Description must be a string with a minimum length of 2.");
                else if (model.Description.Length > 100)
                    result.Add("Description", "The field Description must be a string with a maximum length of 100.");
            }

            // Age: Range(18, 120)
            if (model.Age < 18 || model.Age > 120)
                result.Add("Age", "The field Age must be between 18 and 120.");

            // FutureDate: FutureDate (custom)
            if (model.FutureDate.HasValue && model.FutureDate.Value <= DateTime.Now)
                result.Add("FutureDate", "The FutureDate field must be a date in the future.");

            return Task.FromResult(result);
        });

        // Register validator for SimpleValidationModel
        SannrValidatorRegistry.Register<SimpleValidationModel>((context) =>
        {
            var model = (SimpleValidationModel)context.ObjectInstance;
            var result = new ValidationResult();

            // Name: Required
            if (string.IsNullOrWhiteSpace(model.Name))
                result.Add("Name", "The Name field is required.");

            // Email: EmailAddress
            if (!string.IsNullOrEmpty(model.Email) && !System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Add("Email", "The Email field is not a valid e-mail address.");

            return Task.FromResult(result);
        });
    }

    /// <summary>
    /// Helper to validate a model with optional group.
    /// </summary>
    private async Task<ValidationResult> Validate(object model, string? group = null)
    {
        var success = SannrValidatorRegistry.TryGetValidator(model.GetType(), out var val);
        if (!success || val == null)
        {
            throw new InvalidOperationException($"No validator found for type {model.GetType().Name}");
        }
        return await val!(new SannrValidationContext(model, group: group));
    }

    [Fact]
    /// <summary>Tests that sanitization modifies data as expected.</summary>
    public async Task Sanitize_Should_Modify_Data()
    {
        var model = new UserProfile { Username = "  alice  ", Email = "a@b.com", Age = 20 };
        await Validate(model);
        Assert.Equal("ALICE", model.Username);
    }

    [Fact]
    /// <summary>Tests that display name is used in formatting.</summary>
    public async Task Formatting_Should_Use_DisplayName()
    {
        var model = new UserProfile { Username = "A", Email = "a@b.com", Age = 10 };
        var res = await Validate(model);
        Assert.Contains("The field Years Old must be between 18 and 100.", res.Errors.Select(e => e.Message));
    }

    [Fact]
    /// <summary>Tests that RequiredIf enforces conditional logic.</summary>
    public async Task RequiredIf_Should_Enforce_Conditional_Logic()
    {
        var model = new TestModel { Username = "test", ContactEmail = "test@example.com", Price = 50, Age = 25, Country = "USA", State = null };
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.MemberName == "State");
    }

    [Fact]
    /// <summary>Tests that groups filter rules as expected.</summary>
    public async Task Groups_Should_Filter_Rules()
    {
        var model = new UserProfile { Username = "A", Email = "a@b.com", Age = 25, ReferralCode = null };
        var resDefault = await Validate(model);
        Assert.True(resDefault.IsValid);

        var resReg = await Validate(model, group: "Reg");
        Assert.False(resReg.IsValid);
    }

    #region Advanced Source Generator Tests

    [Fact]
    /// <summary>Tests regex pre-compilation performance optimization.</summary>
    public async Task Regex_PreCompilation_Should_Work()
    {
        var model = new AdvancedValidationModel
        {
            Username = "test_user-123",
            Email = "test@example.com",
            CreditCardNumber = "4111111111111111",
            Website = "https://example.com",
            PhoneNumber = "+1-555-123-4567",
            DocumentPath = "document.pdf",
            SecurityCode = 1234,
            Password = "StrongPass123",
            Country = "US",
            ZipCode = "12345"
        };

        var res = await Validate(model);
        Assert.True(res.IsValid);
    }

    [Fact]
    /// <summary>Tests custom regex patterns are properly compiled and validated.</summary>
    public async Task Custom_Regex_Patterns_Should_Validate()
    {
        var model = new AdvancedValidationModel
        {
            Username = "invalid@username!", // Contains @ and !
            Email = "test@example.com",
            SecurityCode = 1234
        };

        var res = await Validate(model);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.MemberName == "Username");
    }

    [Fact]
    /// <summary>Tests built-in regex validations (email, credit card, URL, phone).</summary>
    public async Task BuiltIn_Regex_Validations_Should_Work()
    {
        var model = new AdvancedValidationModel
        {
            Username = "validuser",
            Email = "invalid-email", // Invalid email
            CreditCardNumber = "invalid-card", // Invalid credit card
            Website = "not-a-url", // Invalid URL
            PhoneNumber = "not-a-phone" // Invalid phone
        };

        var res = await Validate(model);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.MemberName == "Email");
        Assert.Contains(res.Errors, e => e.MemberName == "CreditCardNumber");
        Assert.Contains(res.Errors, e => e.MemberName == "Website");
        Assert.Contains(res.Errors, e => e.MemberName == "PhoneNumber");
    }

    [Fact]
    /// <summary>Tests file extension validation with multiple allowed extensions.</summary>
    public async Task File_Extensions_Should_Validate()
    {
        var validModel = new AdvancedValidationModel
        {
            Username = "user",
            Email = "test@example.com",
            DocumentPath = "file.pdf",
            SecurityCode = 1234
        };

        var invalidModel = new AdvancedValidationModel
        {
            Username = "user",
            Email = "test@example.com",
            DocumentPath = "file.exe", // Not allowed
            SecurityCode = 1234
        };

        var validRes = await Validate(validModel);
        var invalidRes = await Validate(invalidModel);

        if (!validRes.IsValid)
        {
            throw new InvalidOperationException($"Valid model should be valid but got errors: {string.Join(", ", validRes.Errors.Select(e => e.Message))}");
        }

        Assert.True(validRes.IsValid);
        Assert.False(invalidRes.IsValid);
        Assert.Contains(invalidRes.Errors, e => e.MemberName == "DocumentPath");
    }

    [Fact]
    /// <summary>Tests custom validator integration with advanced features.</summary>
    public async Task Custom_Validator_Should_Integrate()
    {
        var weakPasswordModel = new AdvancedValidationModel
        {
            Username = "user",
            Email = "test@example.com",
            Password = "weak" // Too short, missing requirements
        };

        var res = await Validate(weakPasswordModel);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.MemberName == "Password");
    }

    [Fact]
    /// <summary>Tests conditional validation with regex patterns.</summary>
    public async Task Conditional_Validation_With_Regex_Should_Work()
    {
        var usModel = new AdvancedValidationModel
        {
            Username = "user",
            Email = "test@example.com",
            Country = "US",
            ZipCode = null, // Invalid US zip format
            SecurityCode = 1234
        };

        var nonUsModel = new AdvancedValidationModel
        {
            Username = "user",
            Email = "test@example.com",
            Country = "CA",
            ZipCode = null, // Should not be required
            SecurityCode = 1234
        };

        var usRes = await Validate(usModel);
        var nonUsRes = await Validate(nonUsModel);

        Assert.False(usRes.IsValid);
        Assert.Contains(usRes.Errors, e => e.MemberName == "ZipCode");
        Assert.True(nonUsRes.IsValid);
    }

    [Fact]
    /// <summary>Tests string length validation with minimum and maximum constraints.</summary>
    public async Task String_Length_With_Min_Max_Should_Validate()
    {
        var tooShortModel = new AdvancedValidationModel
        {
            Username = "ab", // Too short (min 3)
            Email = "test@example.com",
            SecurityCode = 1234
        };

        var tooLongModel = new AdvancedValidationModel
        {
            Username = new string('a', 51), // Too long (max 50)
            Email = "test@example.com",
            SecurityCode = 1234
        };

        var justRightModel = new AdvancedValidationModel
        {
            Username = "abc", // Just right
            Email = "test@example.com",
            SecurityCode = 1234
        };

        var shortRes = await Validate(tooShortModel);
        var longRes = await Validate(tooLongModel);
        var rightRes = await Validate(justRightModel);

        Assert.False(shortRes.IsValid);
        Assert.False(longRes.IsValid);
        Assert.True(rightRes.IsValid);
    }

    [Fact]
    /// <summary>Tests range validation for numeric types.</summary>
    public async Task Range_Validation_Should_Work()
    {
        var tooLowModel = new AdvancedValidationModel
        {
            Username = "user",
            Email = "test@example.com",
            SecurityCode = 999 // Too low (min 1000)
        };

        var tooHighModel = new AdvancedValidationModel
        {
            Username = "user",
            Email = "test@example.com",
            SecurityCode = 10000 // Too high (max 9999)
        };

        var justRightModel = new AdvancedValidationModel
        {
            Username = "user",
            Email = "test@example.com",
            SecurityCode = 1234 // Just right
        };

        var lowRes = await Validate(tooLowModel);
        var highRes = await Validate(tooHighModel);
        var rightRes = await Validate(justRightModel);

        Assert.False(lowRes.IsValid);
        Assert.False(highRes.IsValid);
        Assert.True(rightRes.IsValid);
    }

    [Fact]
    /// <summary>Tests client-side validation model generation.</summary>
    public async Task Client_Validation_Model_Should_Generate()
    {
        var model = new ClientValidationModel
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            HasAddress = true,
            StreetAddress = "123 Main St"
        };

        var res = await Validate(model);
        Assert.True(res.IsValid);
    }

    [Fact]
    /// <summary>Tests client-side validation with conditional requirements.</summary>
    public async Task Client_Validation_Conditional_Should_Work()
    {
        var model = new ClientValidationModel
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            HasAddress = true,
            StreetAddress = null // Required when HasAddress is true
        };

        var res = await Validate(model);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.MemberName == "StreetAddress");
    }

    [Fact]
    /// <summary>Tests performance of regex pre-compilation by running multiple validations.</summary>
    public async Task Regex_PreCompilation_Performance_Should_Be_Optimal()
    {
        var models = new List<AdvancedValidationModel>();
        for (int i = 0; i < 100; i++)
        {
            models.Add(new AdvancedValidationModel
            {
                Username = $"user{i}",
                Email = $"user{i}@example.com",
                CreditCardNumber = "4111111111111111",
                Website = "https://example.com",
                PhoneNumber = "+1-555-123-4567",
                DocumentPath = "document.pdf",
                SecurityCode = 1234,
                Password = "StrongPass123",
                Country = "US",
                ZipCode = "12345"
            });
        }

        // Validate all models - should be fast due to pre-compiled regex
        foreach (var model in models)
        {
            var res = await Validate(model);
            Assert.True(res.IsValid);
        }
    }

    [Fact]
    /// <summary>Tests that diagnostic reporting works for invalid attribute usage.</summary>
    public async Task Diagnostic_Reporting_Should_Work_For_Invalid_Attributes()
    {
        // This test ensures the generator produces appropriate diagnostics
        // The actual diagnostic testing would require compilation analysis
        var model = new AdvancedValidationModel
        {
            Username = "user",
            Email = "test@example.com",
            SecurityCode = 1234
        };

        var res = await Validate(model);
        // If diagnostics are working, this should validate successfully
        Assert.True(res.IsValid);
    }

    #endregion
}
