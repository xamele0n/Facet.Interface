using System;

namespace Facet;

/// <summary>
/// Indicates that this class should be generated based on a source type, optionally excluding properties or including fields.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class FacetAttribute : Attribute
{
    /// <summary>
    /// The type to project from.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// An array of property or field names to exclude from the generated class.
    /// </summary>
    public string[] Exclude { get; }

    /// <summary>
    /// Whether to include public fields from the source type (default: true).
    /// </summary>
    public bool IncludeFields { get; set; } = true;

    /// <summary>
    /// Whether to generate a constructor that accepts the source type and copies over matching members.
    /// </summary>
    public bool GenerateConstructor { get; set; } = false;

    /// <summary>
    /// Creates a new FacetAttribute that targets a given source type and excludes specified members.
    /// </summary>
    /// <param name="sourceType">The type to generate from.</param>
    /// <param name="exclude">The names of the properties or fields to exclude.</param>
    public FacetAttribute(Type sourceType, params string[] exclude)
    {
        SourceType = sourceType;
        Exclude = exclude ?? Array.Empty<string>();
    }
}