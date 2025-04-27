# Facet.Extensions

Sync and EF Core async extension methods for the Facet library, enabling oneline mapping between your domain entities and generated facet types.

## Key Features

- Constructor-based mapping `(ToFacet<TSource,TTarget>)` for any object graph
- Enumerable mapping `(SelectFacets<TSource,TTarget>)` via LINQ
- IQueryable projection `(SelectFacet<TSource,TTarget>)` using the generated Projection expression
- EF Core async helpers (net6.0+ only):
- `ToFacetsAsync<TSource,TTarget>()`
- `FirstFacetAsync<TSource,TTarget>()`
- `SingleFacetAsync<TSource,TTarget>()`

All methods are zeroboilerplate and leverage your alreadygenerated ctor or Projection property.

## Getting Started

### 1. Install packages

# Core Facet generator + DTOs

```bash
dotnet add package Facet --version 1.5.1
```

# Sync + async mapping helpers

```bash
dotnet add package Facet.Extensions --version 1.0.0
```

> Note: EF Core async methods require your project to target .NET 6.0+.

### 2. Import namespaces

```csharp
using Facet;              // for [Facet] and generated types
using Facet.Extensions;    // for mapping extension methods
```

### 3. Define your facet types

```csharp
using Facet;

// emits ctor + Projection by default
[Facet(typeof(Person))]
public partial class PersonDto { }
```

### 4. Map entities to facets

```csharp
// Single-object mapping
var dto = person.ToFacet<Person, PersonDto>();

// Enumerable mapping (in-memory)
var dtos = people.SelectFacets<Person, PersonDto>().ToList();

// IQueryable projection (deferred)
var query = dbContext.People.SelectFacet<Person, PersonDto>();
var list  = query.ToList();
```

### 5. Async EF Core mapping (net6.0+)

```csharp
using Microsoft.EntityFrameworkCore;

// EF Core 6+ needed for ToListAsync, FirstOrDefaultAsync, SingleAsync
var dtosAsync = await dbContext.People
    .SelectFacet<Person, PersonDto>()
    .ToFacetsAsync(cancellationToken);

var firstDto = await dbContext.People
    .FirstFacetAsync<Person, PersonDto>(cancellationToken);

var singleDto = await dbContext.People
    .Where(p => p.Id == someId)
    .SingleFacetAsync<Person, PersonDto>(cancellationToken);
```

## API Reference

| Method |  Description    |
| ------- |------
| `ToFacet<TSource,TTarget>()`    |   Map one instance via generated constructor  |
| `SelectFacets<TSource,TTarget>()`     |  Map an `IEnumerable<TSource>` via constructor   |
| `SelectFacet<TSource,TTarget>()`    |  Project `IQueryable<TSource>` to `IQueryable<TTarget>`   |
| `ToFacetsAsync<TSource,TTarget>()`   |  Async `.ToListAsync()` on `IQueryable` (EF Core 6+)   |
| `FirstFacetAsync<TSource,TTarget>()`    |  Async `.FirstOrDefaultAsync()` (EF Core 6+)   |
| `SingleFacetAsync<TSource,TTarget>()` | Async `.SingleAsync()` (EF Core 6+)  |

## Requirements

- Facet v1.5.1+
- .NET Standard 2.0 (sync methods)
- .NET 6.0+ (async EF Core methods)
- Microsoft.EntityFrameworkCore 6.0+ (for async methods)