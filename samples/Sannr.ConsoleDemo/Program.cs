// ============================================================================
// Sannr Console Demo - The "Wow" Factor
// ============================================================================
// This demo showcases Sannr's compile-time code generation with ZERO manual code!
// Everything is source-generated from attributes - no reflection, 100% AoT!
// ============================================================================

using System.Linq;
using Sannr.ConsoleDemo;

Console.Clear();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                                                                   ║");
Console.WriteLine("║           🔍 SANNR - Compile-Time Validation Framework           ║");
Console.WriteLine("║                                                                   ║");
Console.WriteLine("║  Zero Manual Code • 100% AoT Compatible • TypeScript Generation  ║");
Console.WriteLine("║                                                                   ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine();

// ============================================================================
// DEMO: Generated Code - The "Wow" Factor
// ============================================================================

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine(" Automatic Code Generation");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.ResetColor();
Console.WriteLine();

Console.WriteLine("📝 You wrote this simple model class:");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("────────────────────────────────────────────────────────────────────");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("public partial class UserRegistration");
Console.WriteLine("{");
Console.WriteLine("    [Required]");
Console.WriteLine("    [StringLength(50, MinimumLength = 3)]");
Console.WriteLine("    public string? Username { get; set; }");
Console.WriteLine("");
Console.WriteLine("    [EmailAddress]");
Console.WriteLine("    public string? Email { get; set; }");
Console.WriteLine("");
Console.WriteLine("    [Range(18, 120)]");
Console.WriteLine("    public int Age { get; set; }");
Console.WriteLine("    // ... more properties");
Console.WriteLine("}");
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("────────────────────────────────────────────────────────────────────");
Console.ResetColor();
Console.WriteLine();

Console.WriteLine("Press any key to see what Sannr generated...");
Console.ReadKey(true);
Console.WriteLine();

// ============================================================================
// Show Generated TypeScript
// ============================================================================

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine(" Generated TypeScript Client Validation (Automatic!)");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.ResetColor();
Console.WriteLine();

Console.WriteLine("✨ Sannr automatically generated this TypeScript code:");
Console.WriteLine("   Access it via: UserRegistration.ValidationRulesTypeScript");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("────────────────────────────────────────────────────────────────────");
Console.ResetColor();

// Display the generated TypeScript code
var tsCode = UserRegistration.ValidationRulesTypeScript;
var lines = tsCode.Split('\n');
foreach (var line in lines)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(line);
}
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("────────────────────────────────────────────────────────────────────");
Console.ResetColor();
Console.WriteLine();

Console.WriteLine("Press any key to see JavaScript validation...");
Console.ReadKey(true);
Console.WriteLine();

// ============================================================================
// Show Generated JavaScript
// ============================================================================

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine(" Generated JavaScript Client Validation (Also Automatic!)");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.ResetColor();
Console.WriteLine();

Console.WriteLine("✨ And this JavaScript code too:");
Console.WriteLine("   Access it via: UserRegistration.ValidationRulesJavaScript");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("────────────────────────────────────────────────────────────────────");
Console.ResetColor();

var jsCode = UserRegistration.ValidationRulesJavaScript;
var jsLines = jsCode.Split('\n');
foreach (var line in jsLines)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine(line);
}
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("────────────────────────────────────────────────────────────────────");
Console.ResetColor();
Console.WriteLine();

Console.WriteLine("Press any key to see JSON validation rules...");
Console.ReadKey(true);
Console.WriteLine();

// ============================================================================
// Show Generated JSON
// ============================================================================

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.WriteLine(" Generated JSON Validation Rules (Yep, Automatic Too!)");
Console.WriteLine("═══════════════════════════════════════════════════════════════════");
Console.ResetColor();
Console.WriteLine();

Console.WriteLine("✨ And validation rules in JSON format:");
Console.WriteLine("   Access it via: UserRegistration.ValidationRulesJson");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("────────────────────────────────────────────────────────────────────");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine(UserRegistration.ValidationRulesJson);
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("────────────────────────────────────────────────────────────────────");
Console.ResetColor();
Console.WriteLine();

// ============================================================================
// THE WOW FACTOR REVEAL
// ============================================================================

Console.WriteLine("Press any key to see THE WOW FACTOR...");
Console.ReadKey(true);
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                        🎉 Demo Completed 🎉                       ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("✅ ZERO manual validation code written!");
Console.WriteLine("✅ ALL code generated at COMPILE-TIME from attributes!");
Console.WriteLine("✅ NO reflection - 100% Native AOT compatible!");
Console.WriteLine("✅ TypeScript/JavaScript/JSON auto-generated!");
Console.WriteLine("✅ Server validator also generated!");
Console.WriteLine("✅ OpenAPI schema filter generated!");
Console.WriteLine("✅ Server + Client validation from ONE source!");
Console.ResetColor();
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("📊 What You Get From ONE Model Class:");
Console.WriteLine();
Console.WriteLine("   1️⃣  Server-side validator (C#)");
Console.WriteLine("   2️⃣  TypeScript interface + validators");
Console.WriteLine("   3️⃣  JavaScript validators");
Console.WriteLine("   4️⃣  JSON validation rules");
Console.WriteLine("   5️⃣  OpenAPI schema extensions");
Console.WriteLine();
Console.WriteLine("   ALL generated at compile-time!");
Console.WriteLine("   ALL from the same attributes!");
Console.ResetColor();
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("📈 Lines of Code Comparison:");
Console.WriteLine();
Console.WriteLine("   Traditional Approach:     ~500+ lines of validation code");
Console.WriteLine("   With FluentValidation:    ~200+ lines of validators");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("   With Sannr:               ~15 lines of attributes");
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine();
Console.WriteLine("   💡 97% LESS CODE TO WRITE AND MAINTAIN!");
Console.ResetColor();
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("🚀 Try it yourself:");
Console.WriteLine();
Console.WriteLine("   1. Add Sannr attributes to your models");
Console.WriteLine("   2. Build your project");
Console.WriteLine("   3. Everything is generated automatically!");
Console.WriteLine();
Console.WriteLine("   No setup, no configuration, just add attributes!");
Console.ResetColor();
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);
Console.ResetColor();
