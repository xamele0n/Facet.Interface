# Facet.Extensions

Provider-agnostic extension methods for the Facet library, enabling one-line mapping between your domain entities and generated facet types.

## Key Features

- Constructor-based mapping `(ToFacet<TSource,TTarget>)` for any object graph
- Enumerable mapping `(SelectFacets<TSource,TTarget>)` via LINQ
- IQueryable projection `(SelectFacet<TSource,TTarget>)` using the generated Projection expression

All methods are zero-boilerplate and leverage your already generated ctor or Projection property.

## Getting Started

### 1. Install packages

# Core Facet generator + DTOs

```bash
dotnet add package Facet
```

# Provider-agnostic mapping helpers

```bash
dotnet add package Facet.Extensions
```

> Note: For EF Core async methods, see [Facet.Extensions.EFCore](https://www.nuget.org/packages/Facet.Extensions.EFCore).

### 2. Import namespaces

```csharp
using Facet;              // for [Facet] and generated types
using Facet.Extensions;   // for mapping extension methods
```

### 3. Define your facet types

```csharp
using Facet;

// emits ctor + Projection by default
[Facet(typeof(Person))]
public partial class PersonDto { }
```

### 4. Map to facets

```csharp
// Single-object mapping
var dto = person.ToFacet<Person, PersonDto>();

// Enumerable mapping (in-memory)
var dtos = people.SelectFacets<Person, PersonDto>().ToList();

// IQueryable projection (deferred)
var query = dbContext.People.SelectFacet<Person, PersonDto>();
var list  = query.ToList();
```

## API Reference

| Method |  Description    |
| ------- |------
| `ToFacet<TSource,TTarget>()`    |   Map one instance via generated constructor  |
| `SelectFacets<TSource,TTarget>()`     |  Map an `IEnumerable<TSource>` via constructor   |
| `SelectFacet<TSource,TTarget>()`    |  Project `IQueryable<TSource>` to `IQueryable<TTarget>`   |

## Requirements

- Facet v1.6.0+
- .NET Standard 2.0+ (sync methods)

---

For EF Core async support, see [Facet.Extensions.EFCore](https://www.nuget.org/packages/Facet.Extensions.EFCore).
