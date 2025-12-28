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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Sannr.Gen;

/// <summary>
/// Source generator for Sannr validators. Generates validation logic for attributed classes.
/// </summary>
[Generator]
public class SannrGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the generator and registers the validator generation pipeline.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => s is PropertyDeclarationSyntax p && p.AttributeLists.Count > 0,
                transform: (ctx, _) => GetClassSemantic(ctx))
            .Where(m => m != null)
            .Collect()
            .SelectMany((c, _) => c.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>());

        context.RegisterSourceOutput(classes, GenerateValidator!);

        // Client-side validation generation pipeline
        var clientValidationClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => s is ClassDeclarationSyntax c && c.AttributeLists.Count > 0,
                transform: (ctx, _) => GetClientValidationClassSemantic(ctx))
            .Where(m => m != null)
            .Collect()
            .SelectMany((c, _) => c.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>());

        context.RegisterSourceOutput(clientValidationClasses, GenerateClientValidators!);
    }

    /// <summary>
    /// Gets the semantic symbol for a class marked with GenerateClientValidatorsAttribute.
    /// </summary>
    private static INamedTypeSymbol? GetClientValidationClassSemantic(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax classDecl)
        {
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
            if (classSymbol != null)
            {
                var hasAttribute = classSymbol.GetAttributes()
                    .Any(a => a.AttributeClass?.Name == "GenerateClientValidatorsAttribute");
                if (hasAttribute)
                {
                    return classSymbol;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the semantic symbol for a class containing a property with attributes.
    /// </summary>
    private static INamedTypeSymbol? GetClassSemantic(GeneratorSyntaxContext context)
    {
        if (context.Node is PropertyDeclarationSyntax prop &&
            prop.Parent is ClassDeclarationSyntax classDecl)
        {
             return context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
        }
        return null;
    }

    /// <summary>
    /// Formats an error message for a validation attribute.
    /// </summary>
    private static string GetFormattedError(AttributeData attr, string defaultFormat, string nameVar, params string[] args)
    {
        string formatExpr = $"\"{defaultFormat}\"";
        var resType = attr.NamedArguments.FirstOrDefault(kv => kv.Key == "ErrorMessageResourceType").Value.Value as INamedTypeSymbol;
        var resName = attr.NamedArguments.FirstOrDefault(kv => kv.Key == "ErrorMessageResourceName").Value.Value as string;
        var explicitMsg = attr.NamedArguments.FirstOrDefault(kv => kv.Key == "ErrorMessage").Value.Value as string;

        if (resType != null && !string.IsNullOrEmpty(resName))
            formatExpr = $"{resType.ToDisplayString()}.{resName}";
        else if (!string.IsNullOrEmpty(explicitMsg))
            formatExpr = $"\"{explicitMsg}\"";

        var sbArgs = new StringBuilder();
        sbArgs.Append(nameVar);
        foreach(var arg in args) sbArgs.Append(", " + arg);

        return $"string.Format({formatExpr}, {sbArgs.ToString()})";
    }

    /// <summary>
    /// Gets the severity value for a validation attribute.
    /// </summary>
    private static string GetSeverity(AttributeData attr)
    {
        var arg = attr.NamedArguments.FirstOrDefault(k => k.Key == "Severity");
        if (arg.Value.Value != null) return $"Severity.{(int)arg.Value.Value}";
        return "Severity.Error";
    }

    /// <summary>
    /// Generates the validator source code for a class symbol.
    /// </summary>
    private static void GenerateValidator(SourceProductionContext spc, INamedTypeSymbol classSymbol)
    {
        var className = classSymbol.Name;
        var ns = classSymbol.ContainingNamespace.ToDisplayString();
        var sb = new StringBuilder();

        sb.AppendLine($$"""
            // <auto-generated/>
            using System;
            using System.Threading.Tasks;
            using System.Text.RegularExpressions;
            using System.Runtime.CompilerServices;
            using Sannr;

            namespace {{ns}}
            {
                public static class {{className}}Validator
                {
                    private static readonly Regex _emailRgx = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
                    private static readonly Regex _ccRgx = new Regex(@"^[\d\- ]{13,19}$", RegexOptions.Compiled);
                    private static readonly Regex _urlRgx = new Regex(@"^https?://", RegexOptions.Compiled);
                    private static readonly Regex _phoneRgx = new Regex(@"^[\d\s\-\+\(\)]+$", RegexOptions.Compiled);

                    public static async Task<ValidationResult> ValidateAsync(SannrValidationContext context)
                    {
                        var model = ({{className}})context.ObjectInstance;
                        var result = new ValidationResult();
                        var activeGroup = context.ActiveGroup;
            """);

        foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            var attributes = member.GetAttributes();
            var prop = member.Name;

            var sanAttr = attributes.FirstOrDefault(a => a.AttributeClass?.Name == "SanitizeAttribute");
            if (sanAttr != null)
            {
                bool trim = (bool)(sanAttr.NamedArguments.FirstOrDefault(k => k.Key == "Trim").Value.Value ?? false);
                bool upper = (bool)(sanAttr.NamedArguments.FirstOrDefault(k => k.Key == "ToUpper").Value.Value ?? false);
                bool lower = (bool)(sanAttr.NamedArguments.FirstOrDefault(k => k.Key == "ToLower").Value.Value ?? false);
                
                if (trim) sb.AppendLine($$"""if (model.{{prop}} != null) model.{{prop}} = model.{{prop}}.Trim();""");
                if (upper) sb.AppendLine($$"""if (model.{{prop}} != null) model.{{prop}} = model.{{prop}}.ToUpper();""");
                if (lower) sb.AppendLine($$"""if (model.{{prop}} != null) model.{{prop}} = model.{{prop}}.ToLower();""");
            }

            var displayAttr = attributes.FirstOrDefault(a => a.AttributeClass?.Name == "DisplayAttribute");
            var explicitName = displayAttr?.NamedArguments.FirstOrDefault(k => k.Key == "Name").Value.Value?.ToString();
            string nameVar = explicitName != null ? $"\"{explicitName}\"" : $"\"{prop}\"";

            foreach(var attr in attributes)
            {
                var attrName = attr.AttributeClass?.Name;
                var severity = GetSeverity(attr);
                var groupArg = attr.NamedArguments.FirstOrDefault(k => k.Key == "Group").Value.Value as string;
                if (groupArg != null) sb.AppendLine($$"""if (activeGroup == "{{groupArg}}") {""");

                if (attrName == "RequiredAttribute") {
                    var msg = GetFormattedError(attr, "{0} is required.", nameVar);
                    var propType = member.Type;
                    bool isValueType = propType.IsValueType;
                    
                    if (isValueType) {
                        // For value types, Required always passes since they can't be null
                        // But we could add a check for default values if needed
                    } else {
                        // For reference types, check for null or empty strings
                        sb.AppendLine($$"""if (model.{{prop}} is null || (model.{{prop}} is string s_{{prop}} && string.IsNullOrWhiteSpace(s_{{prop}}))) result.Add("{{prop}}", {{msg}}, {{severity}});""");
                    }
                }
                else if (attrName == "StringLengthAttribute") {
                    int max = (int)(attr.ConstructorArguments[0].Value ?? 0);
                    int min = 0;
                    var minArg = attr.NamedArguments.FirstOrDefault(k => k.Key == "MinimumLength");
                    if (minArg.Value.Value != null) min = (int)minArg.Value.Value;
                    var msg = GetFormattedError(attr, "The field {0} must be a string with a maximum length of {1}.", nameVar, max.ToString());
                    if (min > 0) {
                        sb.AppendLine($$"""if (model.{{prop}} is string str_{{prop}} && (str_{{prop}}.Length < {{min}} || str_{{prop}}.Length > {{max}})) result.Add("{{prop}}", {{msg}}, {{severity}});""");
                    } else {
                        sb.AppendLine($$"""if (model.{{prop}} is string str_{{prop}} && str_{{prop}}.Length > {{max}}) result.Add("{{prop}}", {{msg}}, {{severity}});""");
                    }
                }
                else if (attrName == "RangeAttribute") {
                    var min = attr.ConstructorArguments[0].Value;
                    var max = attr.ConstructorArguments[1].Value;
                    var msg = GetFormattedError(attr, "The field {0} must be between {1} and {2}.", nameVar, min?.ToString() ?? "0", max?.ToString() ?? "0");
                    sb.AppendLine($$"""if (model.{{prop}} < (dynamic){{min}} || model.{{prop}} > (dynamic){{max}}) result.Add("{{prop}}", {{msg}}, {{severity}});""");
                }
                else if (attrName == "EmailAddressAttribute") {
                    var msg = GetFormattedError(attr, "The {0} field is not a valid e-mail address.", nameVar);
                    sb.AppendLine($$"""if (model.{{prop}} != null && !_emailRgx.IsMatch(model.{{prop}}.ToString())) result.Add("{{prop}}", {{msg}}, {{severity}});""");
                }
                else if (attrName == "CreditCardAttribute") {
                    var msg = GetFormattedError(attr, "The {0} field is not a valid credit card number.", nameVar);
                    sb.AppendLine($$"""if (model.{{prop}} != null && !_ccRgx.IsMatch(model.{{prop}}.ToString())) result.Add("{{prop}}", {{msg}}, {{severity}});""");
                }
                else if (attrName == "UrlAttribute") {
                    var msg = GetFormattedError(attr, "The {0} field is not a valid URL.", nameVar);
                    sb.AppendLine($$"""if (model.{{prop}} != null && !_urlRgx.IsMatch(model.{{prop}}.ToString())) result.Add("{{prop}}", {{msg}}, {{severity}});""");
                }
                else if (attrName == "PhoneAttribute") {
                    var msg = GetFormattedError(attr, "The {0} field is not a valid phone number.", nameVar);
                    sb.AppendLine($$"""if (model.{{prop}} != null && !_phoneRgx.IsMatch(model.{{prop}}.ToString())) result.Add("{{prop}}", {{msg}}, {{severity}});""");
                }
                else if (attrName == "FileExtensionsAttribute") {
                    var extensions = attr.NamedArguments.FirstOrDefault(k => k.Key == "Extensions").Value.Value as string ?? "png,jpg,jpeg,gif";
                    var extArray = extensions.Split(',').Select(e => e.Trim().ToLower()).Where(e => !string.IsNullOrEmpty(e)).ToArray();
                    var extList = string.Join(", ", extArray.Select(e => "."+e));
                    // Build all parts as separate strings
                    var msgText = "The {0} field must have one of the following extensions: " + extList + ".";
                    var msgPart = "string.Format(\"" + msgText + "\", " + nameVar + ")";
                    var condParts = new System.Collections.Generic.List<string>();
                    foreach (var ext in extArray) {
                        condParts.Add("!model." + prop + ".ToString().ToLower().EndsWith(\"." + ext + "\")");
                    }
                    var conditionStr = string.Join(" && ", condParts);
                    sb.AppendLine("if (model." + prop + " != null && (" + conditionStr + ")) result.Add(\"" + prop + "\", " + msgPart + ", " + severity + ");");
                }
                else if (attrName == "RequiredIfAttribute") {
                    var other = attr.ConstructorArguments[0].Value?.ToString();
                    var val = attr.ConstructorArguments[1].Value;
                    string valStr;
                    if (val is string s) {
                        valStr = $"\"{s}\"";
                    } else if (val == null) {
                        valStr = "null";
                    } else {
                        valStr = val.ToString();
                        if (val is bool) valStr = valStr.ToLower();
                    }
                    var msg = GetFormattedError(attr, "{0} is required.", nameVar);
                    
                    // Check if the property type is a string
                    var propType = member.Type;
                    bool isStringType = propType.SpecialType == SpecialType.System_String;
                    
                    if (isStringType) {
                        sb.AppendLine($$"""if (object.Equals(model.{{other}}, {{valStr}}) && (model.{{prop}} is null || string.IsNullOrWhiteSpace(model.{{prop}}))) result.Add("{{prop}}", {{msg}}, {{severity}});""");
                    } else {
                        // For non-string types, just check if the condition is met
                        sb.AppendLine($$"""if (object.Equals(model.{{other}}, {{valStr}}) && model.{{prop}} is null) result.Add("{{prop}}", {{msg}}, {{severity}});""");
                    }
                }
                else if (attrName == "CustomValidatorAttribute") {
                    var type = attr.ConstructorArguments[0].Value as INamedTypeSymbol;
                    var method = attr.ConstructorArguments[1].Value?.ToString() ?? "Check";
                    var isAsync = (bool)(attr.NamedArguments.FirstOrDefault(k => k.Key == "IsAsync").Value.Value ?? false);
                    if (type != null) {
                        string call = isAsync 
                            ? $"await {type.ToDisplayString()}.{method}(model.{prop}, context.ServiceProvider)"
                            : $"{type.ToDisplayString()}.{method}(model.{prop}, context.ServiceProvider)";
                        sb.AppendLine($$"""var customRes_{{prop}} = {{call}}; result.Merge(customRes_{{prop}}, "{{prop}}");""");
                    }
                }
                if (groupArg != null) sb.AppendLine("}");
            }
        }

        // Check if the class implements IValidatableObject
        var iValidatableObjectInterface = classSymbol.AllInterfaces.FirstOrDefault(i => i.Name == "IValidatableObject" && i.ContainingNamespace.Name == "Sannr");
        if (iValidatableObjectInterface != null)
        {
            sb.AppendLine($$"""
                        // Call IValidatableObject.Validate if implemented
                        if (model is Sannr.IValidatableObject validatable)
                        {
                            var modelResults = validatable.Validate(context);
                            foreach (var modelResult in modelResults)
                            {
                                result.Add(modelResult.MemberName ?? "", modelResult.Message, modelResult.Severity);
                            }
                        }
            """);
        }

        sb.AppendLine($$"""
                        return result;
                    }
                }
                internal static class {{className}}SannrLoader
                {
                    [ModuleInitializer]
                    internal static void Load()
                    {
                        SannrValidatorRegistry.Register(typeof({{className}}), async (ctx) => await {{className}}Validator.ValidateAsync(ctx));
                    }
                }
            }
            """);
        spc.AddSource($"{className}.SannrValidator.g.cs", sb.ToString());
    }

    /// <summary>
    /// <summary>
    /// Generates client-side validation code for a class marked with GenerateClientValidatorsAttribute.
    /// </summary>
    private static void GenerateClientValidators(SourceProductionContext spc, INamedTypeSymbol classSymbol)
    {
        var className = classSymbol.Name;
        var ns = classSymbol.ContainingNamespace.ToDisplayString();

        var sb = new StringBuilder();

        // Generate C# class with JSON validation rules
        sb.AppendLine($"// <auto-generated/>");
        sb.AppendLine($"// Generated from {ns}.{className}");
        sb.AppendLine();
        sb.AppendLine($"namespace {ns};");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Client-side validation rules for {className}.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public static class {className}Validators");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// JSON string containing validation rules for {className}.");
        sb.AppendLine($"    /// </summary>");
        var jsonBuilder = new StringBuilder();
        jsonBuilder.AppendLine("{");

        var firstProperty = true;
        foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            var propName = ToCamelCase(member.Name);
            var rules = GetClientValidationRulesJson(member);

            if (!string.IsNullOrEmpty(rules))
            {
                if (!firstProperty) jsonBuilder.AppendLine(",");
                jsonBuilder.Append($"  \"{propName}\": {rules}");
                firstProperty = false;
            }
        }

        jsonBuilder.AppendLine();
        jsonBuilder.Append("}");

        sb.AppendLine($"    public const string ValidationRulesJson =");
        sb.AppendLine($"        @\"{jsonBuilder.ToString().Replace("\"", "\"\"")}\";");
        sb.AppendLine("}");

        var fileName = $"{ns}.{className}.validators.cs";
        spc.AddSource(fileName, sb.ToString());
    }

    /// <summary>
    /// Converts a property name to camelCase for JavaScript/TypeScript.
    /// </summary>
    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name ?? "";
        return char.ToLower(name[0]) + name.Substring(1);
    }

    /// <summary>
    /// Gets the TypeScript type for a .NET type.
    /// </summary>
    private static string GetTypeScriptType(ITypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.System_String => "string",
            SpecialType.System_Int32 or SpecialType.System_Int64 or SpecialType.System_Int16 => "number",
            SpecialType.System_Decimal or SpecialType.System_Double or SpecialType.System_Single => "number",
            SpecialType.System_Boolean => "boolean",
            SpecialType.System_DateTime => "string", // ISO date string
            _ => type.Name.ToLower()
        };
    }

    /// <summary>
    /// Checks if a property is required based on its attributes.
    /// </summary>
    private static bool IsRequiredProperty(IPropertySymbol property)
    {
        return property.GetAttributes().Any(a => a.AttributeClass?.Name == "RequiredAttribute");
    }

    /// <summary>
    /// Gets client-side validation rules for a property based on its Sannr attributes.
    /// Returns JSON format for client-side consumption.
    /// </summary>
    private static string GetClientValidationRulesJson(IPropertySymbol property)
    {
        var rules = new List<string>();
        var attributes = property.GetAttributes();

        foreach (var attr in attributes)
        {
            var attrName = attr.AttributeClass?.Name;

            switch (attrName)
            {
                case "RequiredAttribute":
                    rules.Add("\"required\": true");
                    break;

                case "EmailAddressAttribute":
                    rules.Add("\"email\": true");
                    break;

                case "StringLengthAttribute":
                    var maxLength = attr.NamedArguments.FirstOrDefault(k => k.Key == "MaximumLength").Value.Value;
                    var minLength = attr.NamedArguments.FirstOrDefault(k => k.Key == "MinimumLength").Value.Value;
                    if (maxLength != null) rules.Add($"\"maxLength\": {maxLength}");
                    if (minLength != null) rules.Add($"\"minLength\": {minLength}");
                    break;

                case "RangeAttribute":
                    var min = attr.NamedArguments.FirstOrDefault(k => k.Key == "Minimum").Value.Value;
                    var max = attr.NamedArguments.FirstOrDefault(k => k.Key == "Maximum").Value.Value;
                    if (min != null) rules.Add($"\"min\": {min}");
                    if (max != null) rules.Add($"\"max\": {max}");
                    break;

                case "RegularExpressionAttribute":
                    var patternArg = attr.NamedArguments.FirstOrDefault(k => k.Key == "Pattern");
                    var pattern = patternArg.Value.Value as string;
                    if (pattern != null && !string.IsNullOrEmpty(pattern))
                    {
                        // Escape the pattern for JSON
                        var escapedPattern = pattern.Replace("\\", "\\\\").Replace("\"", "\\\"");
                        rules.Add($"\"pattern\": \"{escapedPattern}\"");
                    }
                    break;

                case "UrlAttribute":
                    rules.Add("\"url\": true");
                    break;

                case "PhoneAttribute":
                    rules.Add("\"phone\": true");
                    break;

                case "AllowedValuesAttribute":
                    var values = attr.ConstructorArguments.FirstOrDefault().Values;
                    if (values.Length > 0)
                    {
                        var valueStrings = values.Select(v => $"\"{v.Value}\"");
                        rules.Add($"\"allowedValues\": [{string.Join(", ", valueStrings)}]");
                    }
                    break;

                case "CompareAttribute":
                    var otherPropArg = attr.NamedArguments.FirstOrDefault(k => k.Key == "OtherProperty");
                    var otherProp = otherPropArg.Value.Value as string;
                    if (otherProp != null && !string.IsNullOrEmpty(otherProp))
                    {
                        rules.Add($"\"compare\": \"{ToCamelCase(otherProp)}\"");
                    }
                    break;

                case "FutureDateAttribute":
                    rules.Add("\"futureDate\": true");
                    break;

                case "ConditionalRangeAttribute":
                    var minRange = attr.NamedArguments.FirstOrDefault(k => k.Key == "Minimum").Value.Value;
                    var maxRange = attr.NamedArguments.FirstOrDefault(k => k.Key == "Maximum").Value.Value;
                    var condPropArg = attr.NamedArguments.FirstOrDefault(k => k.Key == "OtherProperty");
                    var condProp = condPropArg.Value.Value as string;
                    var condValueArg = attr.NamedArguments.FirstOrDefault(k => k.Key == "TargetValue");
                    var condValue = condValueArg.Value.Value;

                    if (minRange != null) rules.Add($"\"minRange\": {minRange}");
                    if (maxRange != null) rules.Add($"\"maxRange\": {maxRange}");
                    if (condProp != null && !string.IsNullOrEmpty(condProp))
                    {
                        rules.Add($"\"conditionProperty\": \"{ToCamelCase(condProp)}\"");
                    }
                    if (condValue != null) rules.Add($"\"conditionValue\": {condValue}");
                    break;
            }
        }

        return rules.Count > 0 ? $"{{ {string.Join(", ", rules)} }}" : "";
    }
}
