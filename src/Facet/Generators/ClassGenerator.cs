using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Facet.Generators;

[Generator]
public sealed class ClassGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node
            )
            .Where(static m => m is not null);

        var compilation = context.CompilationProvider;
        var combined = classDeclarations.Combine(compilation);

        context.RegisterSourceOutput(combined, (spc, source) => Generate(source.Left, source.Right, spc));
    }

    private void Generate(ClassDeclarationSyntax classDecl, Compilation compilation, SourceProductionContext context)
    {
        var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
        if (model.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol targetSymbol)
            return;

        foreach (var attributeData in targetSymbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToDisplayString() == "Facet.FacetAttribute"))
        {
            var sourceTypeArg = attributeData.ConstructorArguments.FirstOrDefault();
            var excludedArg = attributeData.ConstructorArguments.ElementAtOrDefault(1);

            if (sourceTypeArg.Value is not INamedTypeSymbol sourceTypeSymbol)
                continue;

            var excluded = new HashSet<string>(excludedArg.Values
                .Select(v => v.Value?.ToString())
                .Where(v => v != null)!
                .Cast<string>());

            var namedArgs = attributeData.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            bool includeFields = namedArgs.TryGetValue("IncludeFields", out var includeFieldsValue)
                && includeFieldsValue.Value is bool f && f;

            bool generateConstructor = !namedArgs.TryGetValue("GenerateConstructor", out var generateCtorValue)
                || (generateCtorValue.Value is bool g && g);

            var configurationType = namedArgs.TryGetValue("Configuration", out var configValue)
                ? configValue.Value as INamedTypeSymbol
                : null;

            var props = sourceTypeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p =>
                    p.DeclaredAccessibility == Accessibility.Public &&
                    !excluded.Contains(p.Name))
                .Cast<ISymbol>();

            var fields = includeFields
                ? sourceTypeSymbol.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(f =>
                        f.DeclaredAccessibility == Accessibility.Public &&
                        !excluded.Contains(f.Name))
                    .Cast<ISymbol>()
                : Enumerable.Empty<ISymbol>();

            var sourceMembers = props.Concat(fields).ToList();

            var ns = targetSymbol.ContainingNamespace?.IsGlobalNamespace == false
                ? targetSymbol.ContainingNamespace.ToDisplayString()
                : null;

            var generatedSource = GenerateClass(
                targetSymbol.Name,
                ns,
                sourceMembers,
                sourceTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                generateConstructor,
                configurationType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );

            context.AddSource($"{targetSymbol.Name}.g.cs", SourceText.From(generatedSource, Encoding.UTF8));
        }
    }

    private static string GenerateClass(
        string className,
        string? ns,
        List<ISymbol> members,
        string sourceTypeName,
        bool generateConstructor,
        string? configurationTypeName)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(ns))
        {
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
        }

        sb.AppendLine($"public partial class {className}");
        sb.AppendLine("{");

        foreach (var member in members)
        {
            string type = (member as IPropertySymbol)?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                       ?? (member as IFieldSymbol)?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                       ?? "object";

            if (member is IPropertySymbol)
            {
                // always generate writable
                sb.AppendLine($"    public {type} {member.Name} {{ get; set; }}");
            }
            else if (member is IFieldSymbol)
            {
                sb.AppendLine($"    public {type} {member.Name};");
            }
        }

        if (generateConstructor)
        {
            sb.AppendLine();
            sb.AppendLine($"    public {className}({sourceTypeName} source)");
            sb.AppendLine("    {");

            foreach (var member in members)
            {
                sb.AppendLine($"        this.{member.Name} = source.{member.Name};");
            }

            if (!string.IsNullOrWhiteSpace(configurationTypeName))
                sb.AppendLine($"        {configurationTypeName}.Map(source, this);");

            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        if (!string.IsNullOrWhiteSpace(ns)) sb.AppendLine("}");

        return sb.ToString();
    }
}
