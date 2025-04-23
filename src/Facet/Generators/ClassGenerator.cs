using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Facet.Generators
{
    [Generator]
    public sealed class ClassGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new RedactReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not RedactReceiver receiver)
                return;

            foreach (var candidate in receiver.Candidates)
            {
                var model = context.Compilation.GetSemanticModel(candidate.SyntaxTree);

                if (model.GetDeclaredSymbol(candidate) is not INamedTypeSymbol symbol)
                    continue;

                foreach (var attributeData in symbol.GetAttributes()
                    .Where(a => a.AttributeClass?.ToDisplayString() == "Facet.FacetAttribute"))
                {
                    var sourceTypeArg = attributeData.ConstructorArguments.FirstOrDefault();
                    var excludedArg = attributeData.ConstructorArguments.ElementAtOrDefault(1);

                    if (sourceTypeArg.Value is not INamedTypeSymbol sourceTypeSymbol)
                        continue;

                    var excluded = new HashSet<string>(
                        excludedArg.Values
                            .Select(v => v.Value?.ToString())
                            .Where(v => v != null)!
                            .Cast<string>());

                    var namedArgs = attributeData.NamedArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    bool includeFields = !namedArgs.TryGetValue("IncludeFields", out var includeFieldsValue)
                        || (includeFieldsValue.Value is bool f && f);

                    bool generateConstructor = namedArgs.TryGetValue("GenerateConstructor", out var generateCtorValue)
                        && generateCtorValue.Value is bool g && g;

                    var props = sourceTypeSymbol.GetMembers()
                        .OfType<IPropertySymbol>()
                        .Where(p => p.DeclaredAccessibility == Accessibility.Public && !excluded.Contains(p.Name));

                    var fields = includeFields
                        ? sourceTypeSymbol.GetMembers()
                            .OfType<IFieldSymbol>()
                            .Where(f => f.DeclaredAccessibility == Accessibility.Public && !excluded.Contains(f.Name))
                        : Enumerable.Empty<IFieldSymbol>();

                    var sourceMembers = props.Cast<ISymbol>().Concat(fields).ToList();

                    var namespaceName = symbol.ContainingNamespace?.IsGlobalNamespace == false
                        ? symbol.ContainingNamespace.ToDisplayString()
                        : null;

                    var classSource = GenerateClass(
                        className: symbol.Name,
                        ns: namespaceName,
                        members: sourceMembers,
                        sourceTypeName: sourceTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        generateConstructor: generateConstructor
                    );

                    context.AddSource($"{symbol.Name}.g.cs", SourceText.From(classSource, Encoding.UTF8));
                }
            }
        }

        private static string GenerateClass(string className, string? ns, List<ISymbol> members, string sourceTypeName, bool generateConstructor)
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
                    sb.AppendLine($"    public {type} {member.Name} {{ get; set; }}");
                else if (member is IFieldSymbol)
                    sb.AppendLine($"    public {type} {member.Name};");
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

                sb.AppendLine("    }");
            }

            sb.AppendLine("}");

            if (!string.IsNullOrWhiteSpace(ns))
                sb.AppendLine("}");

            return sb.ToString();
        }

        private sealed class RedactReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> Candidates { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDecl &&
                    classDecl.AttributeLists.Count > 0)
                {
                    Candidates.Add(classDecl);
                }
            }
        }
    }
}
