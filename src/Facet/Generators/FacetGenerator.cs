using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Facet.Generators
{
    [Generator(LanguageNames.CSharp)]
    public sealed class FacetGenerator : IIncrementalGenerator
    {
        private const string FacetAttributeName = "Facet.FacetAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var facets = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    FacetAttributeName,
                    predicate: static (node, _) => node is TypeDeclarationSyntax,
                    transform: static (ctx, token) => GetTargetModel(ctx, token))
                .SelectMany(static (models, _) => models);

            context.RegisterSourceOutput(facets, static (spc, model) =>
            {
                spc.CancellationToken.ThrowIfCancellationRequested();
                var code = Generate(model);
                // Include source type name in the generated file name to ensure uniqueness
                var sourceTypeName = model.SourceTypeName.Split('.').Last();
                spc.AddSource($"{model.Name}_{sourceTypeName}.g.cs", SourceText.From(code, Encoding.UTF8));
            });
        }

        private static IEnumerable<FacetTargetModel> GetTargetModel(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (context.TargetSymbol is not INamedTypeSymbol targetSymbol) yield break;
            if (context.Attributes.Length == 0) yield break;

            var ns = targetSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : targetSymbol.ContainingNamespace.ToDisplayString();

            // Determine the kind based on the target type, not the source types
            var targetKind = InferFacetKind(targetSymbol);
            
            // Track property names and types across all attributes for this target
            var propertyTypes = new Dictionary<string, string>();
            var propertySourceTypes = new Dictionary<string, string>();
            var models = new List<FacetTargetModel>();

            // Process all attributes
            foreach (var attribute in context.Attributes)
            {
                token.ThrowIfCancellationRequested();

                var sourceType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
                if (sourceType == null) continue;

                var excluded = new HashSet<string>(
                    attribute.ConstructorArguments.ElementAtOrDefault(1).Values
                        .Select(v => v.Value?.ToString())
                        .Where(n => n != null)!);

                var includeFields = GetNamedArg(attribute.NamedArguments, "IncludeFields", false);
                var generateConstructor = GetNamedArg(attribute.NamedArguments, "GenerateConstructor", true);
                var generateProjection = GetNamedArg(attribute.NamedArguments, "GenerateProjection", true);
                
                var configurationTypeName = attribute.NamedArguments
                                                     .FirstOrDefault(kvp => kvp.Key == "Configuration")
                                                     .Value.Value?
                                                     .ToString();
                
                // Use the target kind for all models, regardless of source type
                var kind = targetKind;
                
                // For record types, default to preserving init-only and required modifiers
                // unless explicitly overridden by the user
                var preserveInitOnlyDefault = kind is FacetKind.Record or FacetKind.RecordStruct;
                var preserveRequiredDefault = kind is FacetKind.Record or FacetKind.RecordStruct;
                
                var preserveInitOnly = GetNamedArg(attribute.NamedArguments, "PreserveInitOnlyProperties", preserveInitOnlyDefault);
                var preserveRequired = GetNamedArg(attribute.NamedArguments, "PreserveRequiredProperties", preserveRequiredDefault);
                
                var members = new List<FacetMember>();
                var addedMembers = new HashSet<string>();

                var allMembersWithModifiers = GetAllMembersWithModifiers(sourceType);
                var sourceTypeName = sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var isInterfaceSource = sourceType.TypeKind == TypeKind.Interface;

                foreach (var (member, isInitOnly, isRequired) in allMembersWithModifiers)
                {
                    token.ThrowIfCancellationRequested();
                    if (excluded.Contains(member.Name)) continue;
                    if (addedMembers.Contains(member.Name)) continue;

                    if (member is IPropertySymbol { DeclaredAccessibility: Accessibility.Public } p)
                    {
                        var typeName = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        
                        // Check if this property name conflicts with an existing one with a different type
                        if (propertyTypes.TryGetValue(p.Name, out var existingType) && existingType != typeName && isInterfaceSource)
                        {
                            // Create a special member name for explicit implementation
                            // Format: "ExplicitImpl:{InterfaceName}:{PropertyName}"
                            var explicitName = $"ExplicitImpl:{sourceTypeName}:{p.Name}";
                            
                            members.Add(new FacetMember(
                                explicitName,
                                typeName,
                                FacetMemberKind.Property,
                                isInitOnly,
                                isRequired));
                        }
                        else
                        {
                            // Regular property
                            var shouldPreserveInitOnly = preserveInitOnly && isInitOnly;
                            var shouldPreserveRequired = preserveRequired && isRequired;
                            
                            members.Add(new FacetMember(
                                p.Name,
                                typeName,
                                FacetMemberKind.Property,
                                shouldPreserveInitOnly,
                                shouldPreserveRequired));
                            
                            // Track property type
                            propertyTypes[p.Name] = typeName;
                            propertySourceTypes[p.Name] = sourceTypeName;
                        }
                        
                        addedMembers.Add(p.Name);
                    }
                    else if (includeFields && member is IFieldSymbol { DeclaredAccessibility: Accessibility.Public } f)
                    {
                        var shouldPreserveRequired = preserveRequired && isRequired;
                        
                        var typeName = f.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        members.Add(new FacetMember(
                            f.Name,
                            typeName,
                            FacetMemberKind.Field,
                            false, // Fields don't have init-only
                            shouldPreserveRequired));
                        addedMembers.Add(f.Name);
                        
                        // Track property type
                        propertyTypes[f.Name] = typeName;
                        propertySourceTypes[f.Name] = sourceTypeName;
                    }
                }

                if (members.Count > 0)
                {
                    var model = new FacetTargetModel(
                        targetSymbol.Name,
                        ns,
                        kind,
                        generateConstructor,
                        generateProjection,
                        sourceTypeName,
                        configurationTypeName,
                        members.ToImmutableArray());
                    
                    models.Add(model);
                    yield return model;
                }
            }
        }

        /// <summary>
        /// Gets all members from the inheritance hierarchy, starting from the most derived type
        /// and walking up to the base types. This ensures that overridden members are preferred.
        /// </summary>
        private static IEnumerable<(ISymbol Symbol, bool IsInitOnly, bool IsRequired)> GetAllMembersWithModifiers(INamedTypeSymbol type)
        {
            var visited = new HashSet<string>();
            var current = type;

            while (current != null)
            {
                foreach (var member in current.GetMembers())
                {
                    if (member.DeclaredAccessibility == Accessibility.Public &&
                        !visited.Contains(member.Name))
                    {
                        if (member is IPropertySymbol prop)
                        {
                            visited.Add(member.Name);
                            var isInitOnly = prop.SetMethod?.IsInitOnly == true;
                            var isRequired = prop.IsRequired;
                            yield return (prop, isInitOnly, isRequired);
                        }
                        else if (member is IFieldSymbol field)
                        {
                            visited.Add(member.Name);
                            var isRequired = field.IsRequired;
                            yield return (field, false, isRequired);
                        }
                    }
                }

                current = current.BaseType;

                if (current?.SpecialType == SpecialType.System_Object)
                    break;
            }
        }

        /// <summary>
        /// Gets all members from the inheritance hierarchy, starting from the most derived type
        /// and walking up to the base types. This ensures that overridden members are preferred.
        /// </summary>
        private static IEnumerable<ISymbol> GetAllMembers(INamedTypeSymbol type)
        {
            return GetAllMembersWithModifiers(type).Select(x => x.Symbol);
        }

        /// <summary>
        /// Attempts to determine the FacetKind from the target symbol's declaration.
        /// </summary>
        private static FacetKind InferFacetKind(INamedTypeSymbol targetSymbol)
        {
            if (targetSymbol.TypeKind == TypeKind.Interface)
            {
                return FacetKind.Interface;
            }
            
            if (targetSymbol.TypeKind == TypeKind.Struct)
            {
                var syntax = targetSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                if (syntax != null && syntax.ToString().Contains("record struct"))
                {
                    return FacetKind.RecordStruct;
                }
                return FacetKind.Struct;
            }

            if (targetSymbol.TypeKind == TypeKind.Class)
            {
                var syntax = targetSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                if (syntax != null && syntax.ToString().Contains("record "))
                {
                    return FacetKind.Record;
                }
                
                if (targetSymbol.GetMembers().Any(m => m.Name.Contains("Clone") && m.IsImplicitlyDeclared))
                {
                    return FacetKind.Record;
                }
                
                return FacetKind.Class;
            }

            return FacetKind.Class;
        }

        private static T GetNamedArg<T>(
            ImmutableArray<KeyValuePair<string, TypedConstant>> args,
            string name,
            T defaultValue)
            => args.FirstOrDefault(kv => kv.Key == name)
                   .Value.Value is T t ? t : defaultValue;

        private static string Generate(FacetTargetModel model)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq.Expressions;");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(model.Namespace))
            {
                sb.AppendLine($"namespace {model.Namespace}");
                sb.AppendLine("{");
            }

            var keyword = model.Kind switch
            {
                FacetKind.Class => "class",
                FacetKind.Record => "record",
                FacetKind.RecordStruct => "record struct",
                FacetKind.Struct => "struct",
                FacetKind.Interface => "interface",
                _ => "class",
            };

            var isPositional = model.Kind is FacetKind.Record or FacetKind.RecordStruct;
            var isInterface = model.Kind is FacetKind.Interface;
            var hasInitOnlyProperties = model.Members.Any(m => m.IsInitOnly);
            var hasCustomMapping = !string.IsNullOrWhiteSpace(model.ConfigurationTypeName);

            if (isPositional)
            {
                var parameters = string.Join(", ",
                    model.Members.Select(m =>
                    {
                        var param = $"{m.TypeName} {m.Name}";
                        // Add required modifier for positional parameters if needed
                        if (m.IsRequired && model.Kind == FacetKind.RecordStruct)
                        {
                            param = $"required {param}";
                        }
                        return param;
                    }));
                sb.AppendLine($"public partial {keyword} {model.Name}({parameters});");
            }

            sb.AppendLine($"public partial {keyword} {model.Name}");
            sb.AppendLine("{");

            if (!isPositional && !isInterface)
            {
                foreach (var m in model.Members)
                {
                    if (m.Name.StartsWith("ExplicitImpl:"))
                    {
                        // Handle explicit interface implementation
                        var parts = m.Name.Split(':');
                        if (parts.Length == 3)
                        {
                            var interfaceType = parts[1];
                            var propertyName = parts[2];
                            
                            if (m.Kind == FacetMemberKind.Property)
                            {
                                // Format: {ReturnType} {InterfaceName}.{PropertyName} { get; }
                                sb.AppendLine($"    {m.TypeName} {interfaceType}.{propertyName} {{ get; }}");
                            }
                        }
                    }
                    else if (m.Kind == FacetMemberKind.Property)
                    {
                        var propDef = $"public {m.TypeName} {m.Name}";
                        
                        if (m.IsInitOnly)
                        {
                            propDef += " { get; init; }";
                        }
                        else
                        {
                            propDef += " { get; set; }";
                        }

                        if (m.IsRequired)
                        {
                            propDef = $"required {propDef}";
                        }

                        sb.AppendLine($"    {propDef}");
                    }
                    else
                    {
                        var fieldDef = $"public {m.TypeName} {m.Name};";
                        if (m.IsRequired)
                        {
                            fieldDef = $"required {fieldDef}";
                        }
                        sb.AppendLine($"    {fieldDef}");
                    }
                }
            }

            // Generate constructor (interfaces don't have constructors)
            if (model.GenerateConstructor && !isInterface)
            {
                GenerateConstructor(sb, model, isPositional, hasInitOnlyProperties, hasCustomMapping);
            }

            // Generate projection (not applicable for interfaces)
            if (model.GenerateExpressionProjection && !isInterface)
            {
                sb.AppendLine();
                sb.AppendLine($"    public static Expression<Func<{model.SourceTypeName}, {model.Name}>> Projection =>");
                sb.AppendLine($"        source => new {model.Name}(source);");
            }

            sb.AppendLine("}");

            if (!string.IsNullOrWhiteSpace(model.Namespace))
                sb.AppendLine("}");

            return sb.ToString();
        }

        private static void GenerateConstructor(StringBuilder sb, FacetTargetModel model, bool isPositional, bool hasInitOnlyProperties, bool hasCustomMapping)
        {
            var ctorSig = $"public {model.Name}({model.SourceTypeName} source)";
            
            if (isPositional)
            {
                var args = string.Join(", ",
                    model.Members.Select(m => GetSourcePropertyAccess(m)));
                ctorSig += $" : this({args})";
            }
            
            sb.AppendLine($"    {ctorSig}");
            sb.AppendLine("    {");
            
            if (!isPositional)
            {
                if (hasCustomMapping && hasInitOnlyProperties)
                {
                    // For types with init-only properties and custom mapping,
                    // we can't assign after construction
                    sb.AppendLine($"        // This constructor should not be used for types with init-only properties and custom mapping");
                    sb.AppendLine($"        // Use FromSource factory method instead");
                    sb.AppendLine($"        throw new InvalidOperationException(\"Use {model.Name}.FromSource(source) for types with init-only properties\");");
                }
                else if (hasCustomMapping)
                {
                    // Regular mutable properties - copy first, then apply custom mapping
                    foreach (var m in model.Members)
                    {
                        if (m.Name.StartsWith("ExplicitImpl:"))
                        {
                            var parts = m.Name.Split(':');
                            if (parts.Length == 3)
                            {
                                var interfaceType = parts[1];
                                var propertyName = parts[2];
                                sb.AppendLine($"        (({interfaceType})this).{propertyName} = source.{propertyName};");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"        this.{m.Name} = source.{m.Name};");
                        }
                    }
                    sb.AppendLine($"        {model.ConfigurationTypeName}.Map(source, this);");
                }
                else
                {
                    // No custom mapping - copy properties directly
                    foreach (var m in model.Members.Where(x => !x.IsInitOnly))
                    {
                        if (m.Name.StartsWith("ExplicitImpl:"))
                        {
                            var parts = m.Name.Split(':');
                            if (parts.Length == 3)
                            {
                                var interfaceType = parts[1];
                                var propertyName = parts[2];
                                sb.AppendLine($"        (({interfaceType})this).{propertyName} = source.{propertyName};");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"        this.{m.Name} = source.{m.Name};");
                        }
                    }
                }
            }
            else if (hasCustomMapping)
            {
                // For positional records/record structs with custom mapping
                sb.AppendLine($"        {model.ConfigurationTypeName}.Map(source, this);");
            }
            
            sb.AppendLine("    }");

            // Add static factory method for types with init-only properties
            if (!isPositional && hasInitOnlyProperties)
            {
                sb.AppendLine();
                sb.AppendLine($"    public static {model.Name} FromSource({model.SourceTypeName} source)");
                sb.AppendLine("    {");
                
                if (hasCustomMapping)
                {
                    // For custom mapping with init-only properties, the mapper should create the instance
                    sb.AppendLine($"        // Custom mapper creates and returns the instance with init-only properties set");
                    sb.AppendLine($"        return {model.ConfigurationTypeName}.Map(source, null);");
                }
                else
                {
                    sb.AppendLine($"        return new {model.Name}");
                    sb.AppendLine("        {");
                    foreach (var m in model.Members)
                    {
                        var comma = m == model.Members.Last() ? "" : ",";
                        
                        if (m.Name.StartsWith("ExplicitImpl:"))
                        {
                            var parts = m.Name.Split(':');
                            if (parts.Length == 3)
                            {
                                var propertyName = parts[2];
                                sb.AppendLine($"            {propertyName} = source.{propertyName}{comma}");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"            {m.Name} = source.{m.Name}{comma}");
                        }
                    }
                    sb.AppendLine("        };");
                }
                
                sb.AppendLine("    }");
            }
        }
        
        /// <summary>
        /// Gets the source property access expression for positional record constructor arguments,
        /// handling explicit interface implementations.
        /// </summary>
        private static string GetSourcePropertyAccess(FacetMember member)
        {
            if (member.Name.StartsWith("ExplicitImpl:"))
            {
                var parts = member.Name.Split(':');
                if (parts.Length == 3)
                {
                    return $"source.{parts[2]}"; // Access the property directly
                }
            }
            return $"source.{member.Name}";
        }
    }
}
