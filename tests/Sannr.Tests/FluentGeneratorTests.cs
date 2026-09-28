using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Sannr.Gen;
using Xunit;

namespace Sannr.Tests;

/// <summary>
/// Pins the behaviour of the fluent (<c>ValidatorConfig&lt;T&gt;</c>) pipeline and the generator's
/// output hygiene. Every test here failed before the fix it describes.
/// </summary>
public class FluentGeneratorTests
{
    private const string FluentSource = @"
        namespace MyLib;
        using Sannr;

        public partial class Customer { public string? Name { get; set; } }

        public partial class CustomerValidator : ValidatorConfig<Customer>
        {
            public override void Configure()
            {
                RuleFor(x => x.Name).NotEmpty().WithMessage(""Name is required"");
            }
        }
    ";

    [Fact]
    public void Fluent_validator_is_generated_without_opting_in_to_schema_generation()
    {
        // A library, worker or test project has no AddSannr() call and no reason to set
        // EnableSannrSchemaGen, which is an OpenAPI switch. Its validators must still exist.
        var trees = Run(CreateCompilation(FluentSource));

        Assert.Contains(trees, t => t.GetText().ToString().Contains("public static class CustomerFluentValidator", StringComparison.Ordinal));
    }

    [Fact]
    public void Fluent_validator_is_generated_on_every_run_in_the_same_process()
    {
        // The IDE and the compiler server keep the generator loaded across compilations. A
        // validator must not disappear the second time the generator runs.
        var compilation = CreateCompilation(FluentSource);

        var first = Run(compilation);
        var second = Run(compilation);

        Assert.Contains(first, t => t.GetText().ToString().Contains("CustomerFluentValidator", StringComparison.Ordinal));
        Assert.Contains(second, t => t.GetText().ToString().Contains("CustomerFluentValidator", StringComparison.Ordinal));
    }

    [Fact]
    public void Generator_output_is_deterministic()
    {
        var compilation = CreateCompilation(FluentSource + @"
            public partial class Order { [Required] public string? Id { get; set; } }
        ");

        var first = Run(compilation, optIn: true).Select(t => (Path: System.IO.Path.GetFileName(t.FilePath), Text: t.GetText().ToString())).OrderBy(x => x.Path, StringComparer.Ordinal).ToArray();
        var second = Run(compilation, optIn: true).Select(t => (Path: System.IO.Path.GetFileName(t.FilePath), Text: t.GetText().ToString())).OrderBy(x => x.Path, StringComparer.Ordinal).ToArray();

        Assert.Equal(first.Select(x => x.Path), second.Select(x => x.Path));
        Assert.Equal(first.Select(x => x.Text), second.Select(x => x.Text));
    }

    [Fact]
    public void Generator_emits_no_diagnostic_scaffolding_into_consumer_compilations()
    {
        var trees = Run(CreateCompilation(FluentSource + @"
            public partial class Order { [Required] public string? Id { get; set; } }
        "));

        Assert.DoesNotContain(trees, t => t.FilePath.Contains("Debug", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(trees, t => t.FilePath.EndsWith("TestGenerator.g.cs", StringComparison.Ordinal));
        Assert.DoesNotContain(trees, t => t.GetText().ToString().Contains("// DEBUG", StringComparison.Ordinal));

        // A timestamp makes every build's output differ, which defeats deterministic builds and
        // incremental caching. Checked directly: two runs in the same second would not reveal it.
        Assert.DoesNotContain(trees, t => System.Text.RegularExpressions.Regex.IsMatch(t.GetText().ToString(), @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}"));
    }

    private static SyntaxTree[] Run(Compilation compilation, bool optIn = false)
    {
        var options = new OptionsProvider(optIn ? "true" : null);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { new SannrGenerator().AsSourceGenerator() }, optionsProvider: options);
        return driver.RunGenerators(compilation).GetRunResult().GeneratedTrees.ToArray();
    }

    private sealed class OptionsProvider(string? enableSchemaGen) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(enableSchemaGen);
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;

        private sealed class Options(string? enableSchemaGen) : AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
            {
                value = key == "build_property.EnableSannrSchemaGen" ? enableSchemaGen : null;
                return value is not null;
            }
        }
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(System.IO.Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p))
            .Append(MetadataReference.CreateFromFile(typeof(Sannr.RequiredAttribute).Assembly.Location));

        return CSharpCompilation.Create("TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }
}
