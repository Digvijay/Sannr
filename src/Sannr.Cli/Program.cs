using System.CommandLine;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Sannr.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Sannr Migration Tools - Migrate from other validation libraries to Sannr");

        var fvInput = new Option<string>("--input") { Description = "Input file or directory containing FluentValidation code", Required = true };
        var fvOutput = new Option<string>("--output") { Description = "Output directory for migrated Sannr code", Required = true };
        var fvTarget = new Option<string>("--target") { Description = "Target Sannr migration style: 'attribute' or 'fluent'", DefaultValueFactory = _ => "fluent" };
        var fvOverwrite = new Option<bool>("--overwrite") { Description = "Overwrite existing files" };
        var fvDryRun = new Option<bool>("--dry-run") { Description = "Show what would be migrated without making changes" };

        var fluentValidationCommand = new Command("fluentvalidation", "Migrate from FluentValidation to Sannr")
        {
            fvInput, fvOutput, fvTarget, fvOverwrite, fvDryRun,
        };

        var daInput = new Option<string>("--input") { Description = "Input file or directory containing DataAnnotations code", Required = true };
        var daOutput = new Option<string>("--output") { Description = "Output directory for migrated Sannr code", Required = true };
        var daOverwrite = new Option<bool>("--overwrite") { Description = "Overwrite existing files" };
        var daDryRun = new Option<bool>("--dry-run") { Description = "Show what would be migrated without making changes" };

        var dataAnnotationsCommand = new Command("dataannotations", "Migrate from DataAnnotations to Sannr")
        {
            daInput, daOutput, daOverwrite, daDryRun,
        };

        var analyzeInput = new Option<string>("--input") { Description = "Input file or directory to analyze", Required = true };
        var analyzeType = new Option<string?>("--type") { Description = "Type of validation library (fluentvalidation, dataannotations, auto)" };

        var analyzeCommand = new Command("analyze", "Analyze validation code to understand migration complexity")
        {
            analyzeInput, analyzeType,
        };

        rootCommand.Subcommands.Add(fluentValidationCommand);
        rootCommand.Subcommands.Add(dataAnnotationsCommand);
        rootCommand.Subcommands.Add(analyzeCommand);

        fluentValidationCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var target = parseResult.GetValue(fvTarget);
            var targetEnum = string.Equals(target, "attribute", StringComparison.OrdinalIgnoreCase) ? MigrationTarget.Attribute : MigrationTarget.Fluent;
            await MigrateFluentValidationAsync(
                parseResult.GetValue(fvInput)!,
                parseResult.GetValue(fvOutput)!,
                targetEnum,
                parseResult.GetValue(fvOverwrite),
                parseResult.GetValue(fvDryRun),
                parseResult.InvocationConfiguration.Output).ConfigureAwait(false);
        });

        dataAnnotationsCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            await MigrateDataAnnotationsAsync(
                parseResult.GetValue(daInput)!,
                parseResult.GetValue(daOutput)!,
                parseResult.GetValue(daOverwrite),
                parseResult.GetValue(daDryRun),
                parseResult.InvocationConfiguration.Output).ConfigureAwait(false);
        });

        analyzeCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            await AnalyzeValidationCodeAsync(
                parseResult.GetValue(analyzeInput)!,
                parseResult.GetValue(analyzeType),
                parseResult.InvocationConfiguration.Output).ConfigureAwait(false);
        });

        rootCommand.SetAction(parseResult =>
        {
            parseResult.InvocationConfiguration.Output.WriteLine("Sannr Migration Tools");
            parseResult.InvocationConfiguration.Output.WriteLine("Use 'sannr --help' to see available commands.");
            return 0;
        });

        return await rootCommand.Parse(args).InvokeAsync().ConfigureAwait(false);
    }

    static async Task MigrateFluentValidationAsync(string input, string output, MigrationTarget target, bool overwrite, bool dryRun, TextWriter console)
    {
        console.WriteLine($"🔄 Migrating FluentValidation code from: {input}");
        console.WriteLine($"📁 Output directory: {output}");
        console.WriteLine($"🎯 Target style: {target}");
        console.WriteLine($"🔧 Overwrite existing files: {(overwrite ? "Yes" : "No")}");
        console.WriteLine($"👀 Dry run: {(dryRun ? "Yes" : "No")}");

        try
        {
            var migrationService = new FluentValidationMigrationService();
            var result = await migrationService.MigrateAsync(input, output, target, overwrite, dryRun);

            console.WriteLine($"\n✅ Migration completed!");
            console.WriteLine($"📊 Files processed: {result.FilesProcessed}");
            console.WriteLine($"🔄 Validators migrated: {result.ValidatorsMigrated}");
            console.WriteLine($"⚠️  Warnings: {result.Warnings.Count}");

            if (result.Warnings.Count > 0)
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

    static async Task MigrateDataAnnotationsAsync(string input, string output, bool overwrite, bool dryRun, TextWriter console)
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

            if (result.Warnings.Count > 0)
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

    static async Task AnalyzeValidationCodeAsync(string input, string? type, TextWriter console)
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

            if (result.FluentValidationRules.Count > 0)
            {
                console.WriteLine($"🔄 FluentValidation rules found: {result.FluentValidationRules.Count}");
                console.WriteLine($"   - RuleFor calls: {result.FluentValidationRules.Count(r => r.Contains("RuleFor"))}");
                console.WriteLine($"   - Must calls: {result.FluentValidationRules.Count(r => r.Contains(".Must("))}");
            }

            if (result.DataAnnotationAttributes.Count > 0)
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

public enum MigrationTarget
{
    Attribute,
    Fluent
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
    public async Task<MigrationResult> MigrateAsync(string inputPath, string outputPath, MigrationTarget target, bool overwrite, bool dryRun)
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
                var migratedContent = MigrateFluentValidationContent(content, target);
                var outputFile = GetOutputFilePath(file, inputPath, outputPath);

                if (!dryRun)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
                    await File.WriteAllTextAsync(outputFile, migratedContent);
                }

                result.ValidatorsMigrated++;
                result.Warnings.Add($"Migrated validator in {Path.GetFileName(file)} to {target} style");
            }
        }

        return result;
    }

    private string MigrateFluentValidationContent(string content, MigrationTarget target)
    {
        if (target == MigrationTarget.Attribute)
        {
            return MigrateToAttributes(content);
        }

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
            if (line.Trim().StartsWith("RuleFor(", StringComparison.Ordinal))
            {
                result.Add($"            // TODO: Convert to Sannr fluent rules if necessary, or move to attributes");
                result.Add($"            // Original FluentValidation: {line.Trim()}");
            }

            result.Add(line);
        }

        return string.Join('\n', result);
    }

    private string MigrateToAttributes(string content)
    {
        // Sophisticated (for a CLI) migration from FluentValidation classes to Attribute-based models
        // This handles simple RuleFor chains
        var lines = content.Split('\n');
        var result = new List<string>();
        var propertyRules = new Dictionary<string, List<string>>();

        // Basic detection of RuleFor(x => x.Prop).NotEmpty().Length(min, max)
        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"RuleFor\s*\(\s*x\s*=>\s*x\.(\w+)\s*\)\s*\.(.+)");
            if (match.Success)
            {
                var propName = match.Groups[1].Value;
                var ruleChain = match.Groups[2].Value;

                if (!propertyRules.ContainsKey(propName))
                    propertyRules[propName] = new List<string>();

                // Parse the chain
                if (ruleChain.Contains("NotEmpty")) propertyRules[propName].Add("[Required]");
                if (ruleChain.Contains("Email")) propertyRules[propName].Add("[EmailAddress]");

                var lengthMatch = Regex.Match(ruleChain, @"Length\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)");
                if (lengthMatch.Success)
                {
                    propertyRules[propName].Add($"[StringLength({lengthMatch.Groups[2].Value}, MinimumLength = {lengthMatch.Groups[1].Value})]");
                }
            }
        }

        bool inClass = false;
        foreach (var line in lines)
        {
            if (line.Contains("class") && !line.Contains("Validator"))
            {
                inClass = true;
                if (!content.Contains("using Sannr;"))
                    result.Add("using Sannr;");
            }

            var propMatch = Regex.Match(line, @"public\s+\w+\??\s+(\w+)\s+\{\s*get;\s*set;\s*\}");
            if (inClass && propMatch.Success)
            {
                var name = propMatch.Groups[1].Value;
                if (propertyRules.TryGetValue(name, out var attributes))
                {
                    foreach (var attr in attributes)
                    {
                        result.Add(new string(' ', line.TakeWhile(char.IsWhiteSpace).Count()) + attr);
                    }
                }
            }

            // Skip the validator class itself in attribute mode if possible, 
            // but for safety we'll just keep everything and add attributes to the models detected.
            result.Add(line);
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

        // Convert [EmailAddress] to Sannr [EmailAddress]
        result = Regex.Replace(result, @"\[EmailAddress(\([^)]*\))?\]", "[EmailAddress]", RegexOptions.IgnoreCase);

        // Add using statement for Sannr
        if (result.Contains("[Required]") || result.Contains("[StringLength") || result.Contains("[EmailAddress]"))
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
