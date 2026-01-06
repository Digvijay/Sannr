using Xunit;
using Sannr.Cli;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Sannr.Cli.Tests;

public class MigrationServiceTests
{
    [Fact]
    public async Task FluentValidation_MigrateToAttribute_AddsAttributesToModel()
    {
        // Arrange
        var service = new FluentValidationMigrationService();
        var content = @"
using FluentValidation;

public partial class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 50);
        RuleFor(x => x.Email).EmailAddress();
    }
}

public partial class User
{
    public string Name { get; set; }
    public string Email { get; set; }
}";
        // Create temporary directory
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var inputFile = Path.Combine(tempDir, "User.cs");
        await File.WriteAllTextAsync(inputFile, content);
        
        var outputDir = Path.Combine(tempDir, "Output");

        // Act
        var result = await service.MigrateAsync(tempDir, outputDir, MigrationTarget.Attribute, true, false);

        // Assert
        Assert.Equal(1, result.ValidatorsMigrated);
        var outputFile = Path.Combine(outputDir, "User.cs");
        var outputContent = await File.ReadAllTextAsync(outputFile);

        Assert.Contains("[Required]", outputContent);
        Assert.Contains("[StringLength(50, MinimumLength = 2)]", outputContent);
        Assert.Contains("[EmailAddress]", outputContent);
        Assert.Contains("using Sannr;", outputContent);
        
        // Clean up
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task FluentValidation_MigrateToFluent_AddsCommentsAndUsing()
    {
        // Arrange
        var service = new FluentValidationMigrationService();
        var content = @"
using FluentValidation;

public partial class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}";
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var inputFile = Path.Combine(tempDir, "UserValidator.cs");
        await File.WriteAllTextAsync(inputFile, content);
        
        var outputDir = Path.Combine(tempDir, "Output");

        // Act
        var result = await service.MigrateAsync(tempDir, outputDir, MigrationTarget.Fluent, true, false);

        // Assert
        Assert.Equal(1, result.ValidatorsMigrated);
        var outputFile = Path.Combine(outputDir, "UserValidator.cs");
        var outputContent = await File.ReadAllTextAsync(outputFile);

        Assert.Contains("// TODO: Convert to Sannr fluent rules", outputContent);
        Assert.Contains("using Sannr;", outputContent);
        
        // Clean up
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task DataAnnotations_Migrate_ConvertsAttributes()
    {
        // Arrange
        var service = new DataAnnotationsMigrationService();
        var content = @"
using System.ComponentModel.DataAnnotations;

public class User
{
    [Required]
    [StringLength(100, MinimumLength = 10)]
    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }
}";
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var inputFile = Path.Combine(tempDir, "User.cs");
        await File.WriteAllTextAsync(inputFile, content);
        
        var outputDir = Path.Combine(tempDir, "Output");

        // Act
        var result = await service.MigrateAsync(tempDir, outputDir, true, false);

        // Assert
        Assert.Equal(1, result.ValidatorsMigrated);
        var outputFile = Path.Combine(outputDir, "User.cs");
        var outputContent = await File.ReadAllTextAsync(outputFile);

        Assert.Contains("[Required]", outputContent);
        Assert.Contains("[StringLength(100, MinimumLength = 10)]", outputContent);
        Assert.Contains("[EmailAddress]", outputContent); // Note: I fixed this to match Sannr [EmailAddress] in previous step
        Assert.Contains("using Sannr;", outputContent);
        
        // Clean up
        Directory.Delete(tempDir, true);
    }
}
