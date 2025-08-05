namespace Facet.Mapping;

/// <summary>
/// Allows defining custom mapping logic between a source and target Facet-generated type.
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target Facet type</typeparam>
public interface IFacetMapConfiguration<TSource, TTarget>
{
    static abstract void Map(TSource source, TTarget target);
}

/// <summary>
/// Instance-based interface for defining custom mapping logic with dependency injection support.
/// Use this interface when you need to inject services into your mapper.
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TTarget">The target Facet type</typeparam>
public interface IFacetMapConfigurationInstance<TSource, TTarget>
{
    void Map(TSource source, TTarget target);
}
