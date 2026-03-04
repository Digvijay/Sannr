using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Sannr.Gen;
using Xunit;

namespace Sannr.Tests;

public class GeneratorLogicTests
{
    [Fact]
    public void Generator_Should_Detect_Library_Environment_Correctly()
    {
        // Setup a compilation without Sannr.AspNetCore types
        var compilation = CreateCompilation(@"
            namespace MyLib;
            using Sannr;

            public partial class MyModel {
                [Required]
                public string Name { get; set; }
            }
        ");

        var generator = new SannrGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        driver = driver.RunGenerators(compilation);
        var runResult = driver.GetRunResult();

        // Verify no SannrInitializer is generated (since AspNetCore is missing)
        var generatedFiles = runResult.GeneratedTrees.Select(t => t.FilePath).ToList();
        Assert.DoesNotContain(generatedFiles, f => f.EndsWith("SannrInitializer.g.cs"));
        
        // Verify validator IS generated
        Assert.Contains(generatedFiles, f => f.EndsWith("MyModel.SannrValidator.g.cs"));
    }

    [Fact]
    public void Generator_Should_Emit_SANN005_On_Version_Mismatch()
    {
        // Setup a compilation with v2 symbols but configured as v3 (SannrOpenApiVersion mismatch)
        var compilation = CreateCompilation(@"
            namespace Microsoft.OpenApi { public class OpenApiSchema {} }
            namespace MyApi;
            public class Dummy {}
        ");

        // Set MSBuild property to v3 (but we have v2 symbols in compilation)
        var optionsProvider = new TestOptionsProvider(new Dictionary<string, string> {
            { "build_property.SannrOpenApiVersion", "v3" }
        });

        var generator = new SannrGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            optionsProvider: optionsProvider);

        driver = driver.RunGenerators(compilation);
        var runResult = driver.GetRunResult();
        var diagnostics = runResult.Diagnostics;

        // Verify SANN005 is reported
        Assert.Contains(diagnostics, d => d.Id == "SANN005");
    }

    private static Compilation CreateCompilation(string source)
    {
        return CSharpCompilation.Create("TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[] { 
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Sannr.RequiredAttribute).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private sealed class TestOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly Dictionary<string, string> _options;

        public TestOptionsProvider(Dictionary<string, string> options)
        {
            _options = options;
        }

        public override AnalyzerConfigOptions GlobalOptions => new TestOptions(_options);

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;

        private sealed class TestOptions : AnalyzerConfigOptions
        {
            private readonly Dictionary<string, string> _options;
            public TestOptions(Dictionary<string, string> options) => _options = options;

            public override bool TryGetValue(string key, out string? value)
                => _options.TryGetValue(key, out value);
        }
    }
}
