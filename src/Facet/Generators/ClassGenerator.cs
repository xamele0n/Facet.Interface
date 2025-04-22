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
                var symbol = model.GetDeclaredSymbol(candidate) as INamedTypeSymbol;
                if (symbol == null)
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
                            .Cast<string>()
                    );

                    var sourceMembers = sourceTypeSymbol.GetMembers()
                        .OfType<IPropertySymbol>()
                        .Where(p => !excluded.Contains(p.Name))
                        .ToList();

                    var classSource = GenerateClass(symbol.Name, sourceMembers, symbol.ContainingNamespace.ToDisplayString());

                    context.AddSource($"{symbol.Name}.g.cs", SourceText.From(classSource, Encoding.UTF8));
                }
            }
        }

        private static string GenerateClass(string className, List<IPropertySymbol> properties, string ns)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"namespace {ns};");
            sb.AppendLine();
            sb.AppendLine($"public partial class {className}");
            sb.AppendLine("{");

            foreach (var prop in properties)
            {
                sb.AppendLine($"    public {prop.Type.ToDisplayString()} {prop.Name} {{ get; set; }}");
            }

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
