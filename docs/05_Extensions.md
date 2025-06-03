# Extension Methods (LINQ, EF Core, etc.)

Facet.Extensions provides a set of extension methods for mapping and projecting between your domain entities and generated facet types.

## Key Methods

| Method                              | Description                                                      |
|------------------------------------- |------------------------------------------------------------------|
| `ToFacet<TSource, TTarget>()`        | Map a single object using the generated constructor.              |
| `SelectFacets<TSource, TTarget>()`   | Map an `IEnumerable<TSource>` to `IEnumerable<TTarget>`.          |
| `SelectFacet<TSource, TTarget>()`    | Project an `IQueryable<TSource>` to `IQueryable<TTarget>`.        |
| `ToFacetsAsync<TSource, TTarget>()`  | Async projection to `List<TTarget>` (EF Core).                    |
| `FirstFacetAsync<TSource, TTarget>()`| Async projection to first or default (EF Core).                   |
| `SingleFacetAsync<TSource, TTarget>()`| Async projection to single (EF Core).                            |

## Usage Examples

```csharp
using Facet.Extensions;

// Single object
var dto = person.ToFacet<Person, PersonDto>();

// Enumerable
var dtos = people.SelectFacets<Person, PersonDto>();

// IQueryable (LINQ/EF Core)
var query = dbContext.People.SelectFacet<Person, PersonDto>();

// Async (EF Core)
var dtosAsync = await dbContext.People.ToFacetsAsync<Person, PersonDto>();
```

## Requirements
- Sync methods: .NET Standard 2.0+
- Async methods: .NET 6.0+ and Microsoft.EntityFrameworkCore 6.0+

---

See [Quick Start](02_QuickStart.md) for setup
