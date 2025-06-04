using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Facet.Extensions.EFCore;
/// <summary>
/// Provides EF Core async extension methods for mapping source entities or sequences
/// to Facet-generated types.
/// </summary>
public static class FacetEfCoreExtensions
{
    /// <summary>
    /// Asynchronously projects an IQueryable<TSource> to a List<TTarget>
    /// using the generated Projection expression and Entity Framework Core's ToListAsync.
    /// </summary>
    public static Task<List<TTarget>> ToFacetsAsync<TSource, TTarget>(
        this IQueryable<TSource> source,
        CancellationToken cancellationToken = default)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        return Facet.Extensions.FacetExtensions.SelectFacet<TSource, TTarget>(source)
                     .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously projects the first element of an IQueryable<TSource>
    /// to a facet, or returns null if none found, using Entity Framework Core's FirstOrDefaultAsync.
    /// </summary>
    public static Task<TTarget?> FirstFacetAsync<TSource, TTarget>(
        this IQueryable<TSource> source,
        CancellationToken cancellationToken = default)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        return Facet.Extensions.FacetExtensions.SelectFacet<TSource, TTarget>(source)
                     .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously projects a single element of an IQueryable<TSource>
    /// to a facet, throwing if not exactly one element exists, using Entity Framework Core's SingleAsync.
    /// </summary>
    public static Task<TTarget> SingleFacetAsync<TSource, TTarget>(
        this IQueryable<TSource> source,
        CancellationToken cancellationToken = default)
        where TTarget : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        return Facet.Extensions.FacetExtensions.SelectFacet<TSource, TTarget>(source)
                     .SingleAsync(cancellationToken);
    }
}
