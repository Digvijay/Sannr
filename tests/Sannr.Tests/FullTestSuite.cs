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

using Xunit;
using Sannr.Tests.Models;
using System.Threading.Tasks;

namespace Sannr.Tests;

/// <summary>
/// Full test suite for Sannr validation features.
/// </summary>
public class FullTestSuite
{
    /// <summary>
    /// Helper to validate a model with optional group.
    /// </summary>
    private async Task<ValidationResult> Validate(object model, string? group = null)
    {
        SannrValidatorRegistry.TryGetValidator(model.GetType(), out var val);
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
        Assert.Contains("The field Years Old must be between 18 and 100.", res.Errors[0].Message);
    }

    [Fact]
    /// <summary>Tests that RequiredIf enforces conditional logic.</summary>
    public async Task RequiredIf_Should_Enforce_Conditional_Logic()
    {
        var model = new UserProfile { Username = "A", Email = "a@b.com", Age = 20, Country = "USA", ZipCode = null };
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.MemberName == "ZipCode");
    }

    [Fact]
    /// <summary>Tests that groups filter rules as expected.</summary>
    public async Task Groups_Should_Filter_Rules()
    {
        var model = new UserProfile { Username = "A", Email = "a@b.com", Age = 20, ReferralCode = null };
        var resDefault = await Validate(model);
        Assert.True(resDefault.IsValid);

        var resReg = await Validate(model, group: "Reg");
        Assert.False(resReg.IsValid);
    }
}
