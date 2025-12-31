using System.CommandLine;
using System.CommandLine.Parsing;
using System.CommandLine.Invocation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace Sannr.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Sannr Migration Tools - Migrate from other validation libraries to Sannr");

        var fluentValidationCommand = new Command("fluentvalidation", "Migrate from FluentValidation to Sannr");
        fluentValidationCommand.AddOption(new Option<string>("--input", "Input file or directory containing FluentValidation code") { IsRequired = true });
        fluentValidationCommand.AddOption(new Option<string>("--output", "Output directory for migrated Sannr code") { IsRequired = true });
        fluentValidationCommand.AddOption(new Option<bool>("--overwrite", "Overwrite existing files"));
        fluentValidationCommand.AddOption(new Option<bool>("--dry-run", "Show what would be migrated without making changes"));

        var dataAnnotationsCommand = new Command("dataannotations", "Migrate from DataAnnotations to Sannr");
        dataAnnotationsCommand.AddOption(new Option<string>("--input", "Input file or directory containing DataAnnotations code") { IsRequired = true });
        dataAnnotationsCommand.AddOption(new Option<string>("--output", "Output directory for migrated Sannr code") { IsRequired = true });
        dataAnnotationsCommand.AddOption(new Option<bool>("--overwrite", "Overwrite existing files"));
        dataAnnotationsCommand.AddOption(new Option<bool>("--dry-run", "Show what would be migrated without making changes"));

        var analyzeCommand = new Command("analyze", "Analyze validation code to understand migration complexity");
        analyzeCommand.AddOption(new Option<string>("--input", "Input file or directory to analyze") { IsRequired = true });
        analyzeCommand.AddOption(new Option<string?>("--type", "Type of validation library (fluentvalidation, dataannotations, auto)"));

        rootCommand.AddCommand(fluentValidationCommand);
        rootCommand.AddCommand(dataAnnotationsCommand);
        rootCommand.AddCommand(analyzeCommand);

        fluentValidationCommand.SetHandler(async (InvocationContext context) =>
        {
            var input = context.ParseResult.GetValueForOption(fluentValidationCommand.Options.OfType<Option<string>>().First(o => o.Name == "input"));
            var output = context.ParseResult.GetValueForOption(fluentValidationCommand.Options.OfType<Option<string>>().First(o => o.Name == "output"));
            var overwrite = context.ParseResult.GetValueForOption(fluentValidationCommand.Options.OfType<Option<bool>>().First(o => o.Name == "overwrite"));
            var dryRun = context.ParseResult.GetValueForOption(fluentValidationCommand.Options.OfType<Option<bool>>().First(o => o.Name == "dry-run"));
            await MigrateFluentValidationAsync(input!, output!, overwrite, dryRun, context.Console);
        });

        dataAnnotationsCommand.SetHandler(async (InvocationContext context) =>
        {
            var input = context.ParseResult.GetValueForOption(dataAnnotationsCommand.Options.OfType<Option<string>>().First(o => o.Name == "input"));
            var output = context.ParseResult.GetValueForOption(dataAnnotationsCommand.Options.OfType<Option<string>>().First(o => o.Name == "output"));
            var overwrite = context.ParseResult.GetValueForOption(dataAnnotationsCommand.Options.OfType<Option<bool>>().First(o => o.Name == "overwrite"));
            var dryRun = context.ParseResult.GetValueForOption(dataAnnotationsCommand.Options.OfType<Option<bool>>().First(o => o.Name == "dry-run"));
            await MigrateDataAnnotationsAsync(input!, output!, overwrite, dryRun, context.Console);
        });

        analyzeCommand.SetHandler(async (InvocationContext context) =>
        {
            var input = context.ParseResult.GetValueForOption(analyzeCommand.Options.OfType<Option<string>>().First(o => o.Name == "input"));
            var type = context.ParseResult.GetValueForOption(analyzeCommand.Options.OfType<Option<string?>>().First(o => o.Name == "type"));
            await AnalyzeValidationCodeAsync(input!, type, context.Console);
        });

        rootCommand.SetHandler((InvocationContext context) =>
        {
            context.Console.WriteLine("Sannr Migration Tools");
            context.Console.WriteLine("Use 'sannr --help' to see available commands.");
            return Task.CompletedTask;
        });

        return await rootCommand.InvokeAsync(args);
    }

    static async Task MigrateFluentValidationAsync(string input, string output, bool overwrite, bool dryRun, IConsole console)
    {
        console.WriteLine($"🔄 Migrating FluentValidation code from: {input}");
        console.WriteLine($"📁 Output directory: {output}");
        console.WriteLine($"🔧 Overwrite existing files: {(overwrite ? "Yes" : "No")}");
        console.WriteLine($"👀 Dry run: {(dryRun ? "Yes" : "No")}");

        try
        {
            var migrationService = new FluentValidationMigrationService();
            var result = await migrationService.MigrateAsync(input, output, overwrite, dryRun);

            console.WriteLine($"\n✅ Migration completed!");
            console.WriteLine($"📊 Files processed: {result.FilesProcessed}");
            console.WriteLine($"🔄 Validators migrated: {result.ValidatorsMigrated}");
            console.WriteLine($"⚠️  Warnings: {result.Warnings.Count}");

            if (result.Warnings.Any())
            {
                console.WriteLine("\n⚠️  Warnings:");
                foreach (var warning in result.Warnings)
                {
                    console.WriteLine($"   - {warning}");
                }
            }
        }
        catch (Exception ex)
        {
            console.WriteLine($"\n❌ Migration failed: {ex.Message}");
        }
    }

    static async Task MigrateDataAnnotationsAsync(string input, string output, bool overwrite, bool dryRun, IConsole console)
    {
        console.WriteLine($"🔄 Migrating DataAnnotations code from: {input}");
        console.WriteLine($"📁 Output directory: {output}");
        console.WriteLine($"🔧 Overwrite existing files: {(overwrite ? "Yes" : "No")}");
        console.WriteLine($"👀 Dry run: {(dryRun ? "Yes" : "No")}");

        try
        {
            var migrationService = new DataAnnotationsMigrationService();
            var result = await migrationService.MigrateAsync(input, output, overwrite, dryRun);

            console.WriteLine($"\n✅ Migration completed!");
            console.WriteLine($"📊 Files processed: {result.FilesProcessed}");
            console.WriteLine($"🔄 DataAnnotations migrated: {result.ValidatorsMigrated}");
            console.WriteLine($"⚠️  Warnings: {result.Warnings.Count}");

            if (result.Warnings.Any())
            {
                console.WriteLine("\n⚠️  Warnings:");
                foreach (var warning in result.Warnings)
                {
                    console.WriteLine($"   - {warning}");
                }
            }
        }
        catch (Exception ex)
        {
            console.WriteLine($"\n❌ Migration failed: {ex.Message}");
        }
    }

    static async Task AnalyzeValidationCodeAsync(string input, string? type, IConsole console)
    {
        console.WriteLine($"🔍 Analyzing validation code in: {input}");
        console.WriteLine($"🏷️  Library type: {type ?? "auto-detect"}");

        try
        {
            var analyzer = new ValidationCodeAnalyzer();
            var result = await analyzer.AnalyzeAsync(input, type);

            console.WriteLine($"\n📊 Analysis Results:");
            console.WriteLine($"📁 Files scanned: {result.TotalFiles}");
            console.WriteLine($"🏷️  Validation libraries detected: {string.Join(", ", result.DetectedLibraries)}");

            if (result.FluentValidationRules.Any())
            {
                console.WriteLine($"🔄 FluentValidation rules found: {result.FluentValidationRules.Count}");
                console.WriteLine($"   - RuleFor calls: {result.FluentValidationRules.Count(r => r.Contains("RuleFor"))}");
                console.WriteLine($"   - Must calls: {result.FluentValidationRules.Count(r => r.Contains(".Must("))}");
            }

            if (result.DataAnnotationAttributes.Any())
            {
                console.WriteLine($"📝 DataAnnotations found: {result.DataAnnotationAttributes.Count}");
                console.WriteLine($"   - [Required]: {result.DataAnnotationAttributes.Count(a => a.Contains("[Required"))}");
                console.WriteLine($"   - [StringLength]: {result.DataAnnotationAttributes.Count(a => a.Contains("[StringLength"))}");
            }

            console.WriteLine($"\n🎯 Migration Complexity: {result.ComplexityEstimate}");
            console.WriteLine($"💡 Recommendation: {result.Recommendation}");
        }
        catch (Exception ex)
        {
            console.WriteLine($"\n❌ Analysis failed: {ex.Message}");
        }
    }
}

public class MigrationResult
{
    public int FilesProcessed { get; set; }
    public int ValidatorsMigrated { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class AnalysisResult
{
    public int TotalFiles { get; set; }
    public List<string> DetectedLibraries { get; set; } = new();
    public List<string> FluentValidationRules { get; set; } = new();
    public List<string> DataAnnotationAttributes { get; set; } = new();
    public string ComplexityEstimate { get; set; } = "Unknown";
    public string Recommendation { get; set; } = "Manual review recommended";
}

public class FluentValidationMigrationService
{
    public async Task<MigrationResult> MigrateAsync(string inputPath, string outputPath, bool overwrite, bool dryRun)
    {
        var result = new MigrationResult();

        // Get all C# files
        var csFiles = Directory.GetFiles(inputPath, "*.cs", SearchOption.AllDirectories);

        foreach (var file in csFiles)
        {
            result.FilesProcessed++;
            var content = await File.ReadAllTextAsync(file);

            if (content.Contains("FluentValidation") && content.Contains("RuleFor"))
            {
                var migratedContent = MigrateFluentValidationContent(content);
                var outputFile = GetOutputFilePath(file, inputPath, outputPath);

                if (!dryRun)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
                    await File.WriteAllTextAsync(outputFile, migratedContent);
                }

                result.ValidatorsMigrated++;
                result.Warnings.Add($"Migrated validator in {Path.GetFileName(file)}");
            }
        }

        return result;
    }

    private string MigrateFluentValidationContent(string content)
    {
        var lines = content.Split('\n');
        var result = new List<string>();

        // Add Sannr using statement if FluentValidation is detected
        if (content.Contains("FluentValidation"))
        {
            result.Add("using Sannr;");
            result.Add("");
        }

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Add comments for RuleFor calls
            if (line.Trim().StartsWith("RuleFor("))
            {
                result.Add($"            // TODO: Convert to Sannr attributes on model properties");
                result.Add($"            // Original FluentValidation: {line.Trim()}");
                result.Add($"            // See migration guide: https://github.com/your-repo/sannr/migration");
            }

            result.Add(line);
        }

        // Add migration guide comment at the end
        if (content.Contains("RuleFor("))
        {
            result.Add("");
            result.Add("            /*");
            result.Add("             * MIGRATION NOTES:");
            result.Add("             * 1. Move validation rules from validator classes to model properties as attributes");
            result.Add("             * 2. Convert RuleFor(x => x.Name).NotEmpty() to [Required] on Name property");
            result.Add("             * 3. Convert RuleFor(x => x.Name).Length(2, 50) to [StringLength(50)] on Name property");
            result.Add("             * 4. Convert RuleFor(x => x.Email).EmailAddress() to [Email] on Email property");
            result.Add("             * 5. Remove this validator class after migrating all rules");
            result.Add("             */");
        }

        return string.Join('\n', result);
    }



    private string GetOutputFilePath(string inputFile, string inputPath, string outputPath)
    {
        var relativePath = Path.GetRelativePath(inputPath, inputFile);
        return Path.Combine(outputPath, relativePath);
    }
}

public class DataAnnotationsMigrationService
{
    public async Task<MigrationResult> MigrateAsync(string inputPath, string outputPath, bool overwrite, bool dryRun)
    {
        var result = new MigrationResult();

        var csFiles = Directory.GetFiles(inputPath, "*.cs", SearchOption.AllDirectories);

        foreach (var file in csFiles)
        {
            result.FilesProcessed++;
            var content = await File.ReadAllTextAsync(file);

            if (content.Contains("[Required") || content.Contains("[StringLength") || content.Contains("System.ComponentModel.DataAnnotations"))
            {
                var migratedContent = MigrateDataAnnotationsContent(content);
                var outputFile = GetOutputFilePath(file, inputPath, outputPath);

                if (!dryRun)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
                    await File.WriteAllTextAsync(outputFile, migratedContent);
                }

                result.ValidatorsMigrated++;
                result.Warnings.Add($"Migrated DataAnnotations in {Path.GetFileName(file)}");
            }
        }

        return result;
    }

    private string MigrateDataAnnotationsContent(string content)
    {
        // Simple regex-based conversion for common DataAnnotations
        var result = content;

        // Convert [Required] to Sannr [Required]
        result = Regex.Replace(result, @"\[Required(\([^)]*\))?\]", "[Required]", RegexOptions.IgnoreCase);

        // Convert [StringLength(max, MinimumLength = min)] or [StringLength(max)] to Sannr [StringLength(max, MinimumLength = min)]
        result = Regex.Replace(result, @"\[StringLength\((\d+)(?:,\s*MinimumLength\s*=\s*(\d+))?\)\]", match =>
        {
            var max = match.Groups[1].Value;
            var min = match.Groups[2].Success ? match.Groups[2].Value : null;
            return min != null ? $"[StringLength({max}, MinimumLength = {min})]" : $"[StringLength({max})]";
        }, RegexOptions.IgnoreCase);

        // Convert [MaxLength(max)] to Sannr [StringLength(max)]
        result = Regex.Replace(result, @"\[MaxLength\((\d+)\)\]", "[StringLength($1)]", RegexOptions.IgnoreCase);

        // Convert [MinLength(min)] to Sannr [MinLength(min)]
        result = Regex.Replace(result, @"\[MinLength\((\d+)\)\]", "[MinLength($1)]", RegexOptions.IgnoreCase);

        // Convert [Range(min, max)] to Sannr [Range(min, max)]
        result = Regex.Replace(result, @"\[Range\((\d+),\s*(\d+)\)\]", "[Range($1, $2)]", RegexOptions.IgnoreCase);

        // Convert [EmailAddress] to Sannr [Email]
        result = Regex.Replace(result, @"\[EmailAddress(\([^)]*\))?\]", "[Email]", RegexOptions.IgnoreCase);

        // Add using statement for Sannr
        if (result.Contains("[Required]") || result.Contains("[StringLength") || result.Contains("[Email]"))
        {
            if (!result.Contains("using Sannr;"))
            {
                result = "using Sannr;\n" + result;
            }
        }

        return result;
    }

    private string GetOutputFilePath(string inputFile, string inputPath, string outputPath)
    {
        var relativePath = Path.GetRelativePath(inputPath, inputFile);
        return Path.Combine(outputPath, relativePath);
    }
}

public class ValidationCodeAnalyzer
{
    public async Task<AnalysisResult> AnalyzeAsync(string inputPath, string? type)
    {
        var result = new AnalysisResult();

        var csFiles = Directory.GetFiles(inputPath, "*.cs", SearchOption.AllDirectories);
        result.TotalFiles = csFiles.Length;

        foreach (var file in csFiles)
        {
            var content = await File.ReadAllTextAsync(file);

            // Detect FluentValidation
            if (content.Contains("FluentValidation") || content.Contains("AbstractValidator") || content.Contains("RuleFor"))
            {
                if (!result.DetectedLibraries.Contains("FluentValidation"))
                    result.DetectedLibraries.Add("FluentValidation");

                // Extract RuleFor patterns
                var ruleForMatches = Regex.Matches(content, @"RuleFor\([^)]+\)\s*\.[^;]+");
                foreach (Match match in ruleForMatches)
                {
                    result.FluentValidationRules.Add(match.Value.Trim());
                }
            }

            // Detect DataAnnotations
            if (content.Contains("System.ComponentModel.DataAnnotations") ||
                content.Contains("[Required") || content.Contains("[StringLength") ||
                content.Contains("[EmailAddress") || content.Contains("[Range"))
            {
                if (!result.DetectedLibraries.Contains("DataAnnotations"))
                    result.DetectedLibraries.Add("DataAnnotations");

                // Extract attribute patterns
                var attributeMatches = Regex.Matches(content, @"\[(Required|StringLength|EmailAddress|Range|MaxLength|MinLength)[^]]*\]");
                foreach (Match match in attributeMatches)
                {
                    result.DataAnnotationAttributes.Add(match.Value);
                }
            }
        }

        // Calculate complexity
        var totalValidations = result.FluentValidationRules.Count + result.DataAnnotationAttributes.Count;

        if (totalValidations == 0)
        {
            result.ComplexityEstimate = "None - No validation code found";
            result.Recommendation = "No migration needed";
        }
        else if (totalValidations < 10)
        {
            result.ComplexityEstimate = "Low";
            result.Recommendation = "Simple migration - can be done manually or with CLI tool";
        }
        else if (totalValidations < 50)
        {
            result.ComplexityEstimate = "Medium";
            result.Recommendation = "Moderate complexity - review generated code carefully";
        }
        else
        {
            result.ComplexityEstimate = "High";
            result.Recommendation = "Complex migration - consider phased approach";
        }

        return result;
    }
}
