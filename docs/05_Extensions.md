# Extension Methods (LINQ, EF Core, etc.)

Facet.Extensions provides a set of provider-agnostic extension methods for mapping and projecting between your domain entities and generated facet types.
For async EF Core support, see the separate Facet.Extensions.EFCore package.

## Key Methods (Facet.Extensions)

| Method                              | Description                                                      |
|------------------------------------- |------------------------------------------------------------------|
| `ToFacet<TSource, TTarget>()`        | Map a single object using the generated constructor.              |
| `SelectFacets<TSource, TTarget>()`   | Map an `IEnumerable<TSource>` to `IEnumerable<TTarget>`.          |
| `SelectFacet<TSource, TTarget>()`    | Project an `IQueryable<TSource>` to `IQueryable<TTarget>`.        |

## Key Methods (Facet.Extensions.EFCore)

| Method                              | Description                                                      |
|------------------------------------- |------------------------------------------------------------------|
| `ToFacetsAsync<TSource, TTarget>()`  | Async projection to `List<TTarget>` (EF Core).                    |
| `FirstFacetAsync<TSource, TTarget>()`| Async projection to first or default (EF Core).                   |
| `SingleFacetAsync<TSource, TTarget>()`| Async projection to single (EF Core).                            |

## Usage Examples

### Extensions

```bash
dotnet add package Facet.Extensions
```

```csharp
using Facet.Extensions;

// provider-agnostic
// Single object
var dto = person.ToFacet<Person, PersonDto>();

// Enumerable
var dtos = people.SelectFacets<Person, PersonDto>();
```

### EF Core Extensions

```bash
dotnet add package Facet.Extensions.EFCore
```

```csharp
// IQueryable (LINQ/EF Core)

using Facet.Extensions.EFCore; 

var query = dbContext.People.SelectFacet<Person, PersonDto>();

// Async (EF Core)
var dtosAsync = await dbContext.People.ToFacetsAsync<Person, PersonDto>();
var firstDto = await dbContext.People.FirstFacetAsync<Person, PersonDto>();
var singleDto = await dbContext.People.SingleFacetAsync<Person, PersonDto>();
```

---

See [Quick Start](02_QuickStart.md) for setup and [Facet.Extensions.EFCore](https://www.nuget.org/packages/Facet.Extensions.EFCore) for async EF Core support.
