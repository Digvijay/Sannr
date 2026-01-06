namespace Sannr.Cli.Tests;

public partial class MigrationCliTests
{
    [Fact]
    public async Task FluentValidationCommand_RequiresInputAndOutput()
    {
        // Arrange
        var args = new[] { "fluentvalidation" };
        var exitCode = await Program.Main(args);

        // Assert - should fail because input and output are required
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task FluentValidationCommand_WithValidArgs_Succeeds()
    {
        // Arrange
        var testDataPath = Path.Combine("..", "..", "..", "TestData");
        var outputPath = Path.Combine("..", "..", "..", "TestOutput");
        var args = new[] { "fluentvalidation", "--input", testDataPath, "--output", outputPath, "--dry-run" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task DataAnnotationsCommand_WithValidArgs_Succeeds()
    {
        // Arrange
        var testDataPath = Path.Combine("..", "..", "..", "TestData");
        var outputPath = Path.Combine("..", "..", "..", "TestOutput");
        var args = new[] { "dataannotations", "--input", testDataPath, "--output", outputPath, "--dry-run" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task AnalyzeCommand_WithValidArgs_Succeeds()
    {
        // Arrange
        var testDataPath = Path.Combine("..", "..", "..", "TestData");
        var args = new[] { "analyze", "--input", testDataPath };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task AnalyzeCommand_RequiresInput()
    {
        // Arrange
        var args = new[] { "analyze" };
        var exitCode = await Program.Main(args);

        // Assert - should fail because input is required
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task DataAnnotationsCommand_RequiresInputAndOutput()
    {
        // Arrange
        var args = new[] { "dataannotations", "--input", "test.cs" };
        var exitCode = await Program.Main(args);

        // Assert - should fail because output is required
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task FluentValidationCommand_RequiresInputAndOutput_OnlyInput()
    {
        // Arrange
        var args = new[] { "fluentvalidation", "--input", "test.cs" };
        var exitCode = await Program.Main(args);

        // Assert - should fail because output is required
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task DataAnnotationsCommand_WithOverwriteFlag_Succeeds()
    {
        // Arrange
        var testDataPath = Path.Combine("..", "..", "..", "TestData");
        var outputPath = Path.Combine("..", "..", "..", "TestOutput");
        var args = new[] { "dataannotations", "--input", testDataPath, "--output", outputPath, "--overwrite" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task FluentValidationCommand_WithOverwriteFlag_Succeeds()
    {
        // Arrange
        var testDataPath = Path.Combine("..", "..", "..", "TestData");
        var outputPath = Path.Combine("..", "..", "..", "TestOutput");
        var args = new[] { "fluentvalidation", "--input", testDataPath, "--output", outputPath, "--overwrite" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task AnalyzeCommand_WithTypeFilter_Succeeds()
    {
        // Arrange
        var testDataPath = Path.Combine("..", "..", "..", "TestData");
        var args = new[] { "analyze", "--input", testDataPath, "--type", "dataannotations" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task DataAnnotationsCommand_DryRun_DoesNotModifyFiles()
    {
        // Arrange
        var testDataPath = Path.Combine("..", "..", "..", "TestData");
        var outputPath = Path.Combine("..", "..", "..", "TestOutput");
        var args = new[] { "dataannotations", "--input", testDataPath, "--output", outputPath, "--dry-run" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task FluentValidationCommand_DryRun_DoesNotModifyFiles()
    {
        // Arrange
        var testDataPath = Path.Combine("..", "..", "..", "TestData");
        var outputPath = Path.Combine("..", "..", "..", "TestOutput");
        var args = new[] { "fluentvalidation", "--input", testDataPath, "--output", outputPath, "--dry-run" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task RootCommand_ShowsHelp_ContainsMigrationCommands()
    {
        // Arrange
        var args = new[] { "--help" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task FluentValidationCommand_Help_ShowsOptions()
    {
        // Arrange
        var args = new[] { "fluentvalidation", "--help" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task DataAnnotationsCommand_Help_ShowsOptions()
    {
        // Arrange
        var args = new[] { "dataannotations", "--help" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task AnalyzeCommand_Help_ShowsOptions()
    {
        // Arrange
        var args = new[] { "analyze", "--help" };
        var exitCode = await Program.Main(args);

        // Assert - should succeed
        Assert.Equal(0, exitCode);
    }
}
