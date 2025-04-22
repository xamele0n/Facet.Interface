using System;

namespace Facet;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class FacetAttribute : Attribute
{
    public Type SourceType { get; }
    public string[] Exclude { get; }

    public FacetAttribute(Type sourceType, params string[] exclude)
    {
        SourceType = sourceType;
        Exclude = exclude ?? Array.Empty<string>();
    }
}