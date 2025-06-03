# Facet

[![NuGet](https://img.shields.io/nuget/v/Facet.svg)](https://www.nuget.org/packages/Facet)
[![Downloads](https://img.shields.io/nuget/dt/Facet.svg)](https://www.nuget.org/packages/Facet)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](/LICENSE)

> "One part of a subject, situation, object that has many parts."

**Facet** is a C# source generator that lets you define **lightweight projections** (DTOs, API models, etc.) directly from your domain models — without writing boilerplate.

It generates partial classes, records, structs, or record structs with constructors, optional LINQ projections, and even supports custom mappings — all at compile time, with zero runtime cost.

---

## What is Facetting?

Facetting is the process of defining **focused views** of a larger model at compile time.

Instead of manually writing separate DTOs, mappers, and projections, **Facet** allows you to declare what you want to keep — and generates everything else.

You can think of it like **carving out a specific facet** of a gem:

- The part you care about
- Leaving the rest behind.

## Why Facetting?

- Reduce duplication across DTOs, projections, and ViewModels
- Maintain strong typing with no runtime cost
- Stay DRY (Don't Repeat Yourself) without sacrificing performance
- Works seamlessly with LINQ providers like Entity Framework

## Key Features

- :white_check_mark: Generate classes, records, structs, or record structs from existing types
- :white_check_mark: Exclude fields/properties you don't want (create a Facetted view of your model)
- :white_check_mark: Include public fields (optional)
- :white_check_mark: Auto-generate constructors for fast mapping
- :white_check_mark: LINQ projection expressions `(Expression<Func<TSource,TTarget>>)`
-:white_check_mark: Custom mapping via `IFacetMapConfiguration`

## Documentation

- **[Documentation & Guides](docs/README.md)**
- [What is being generated?](docs/07_WhatIsBeingGenerated.md)

## Quick Start

```bash
dotnet add package Facetusing Facet;
```

```csharp
public class Person
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}

// Generate a DTO that excludes Email
[Facet(typeof(Person), exclude: nameof(Person.Email))]
public partial class PersonDto { }

var person = new Person { Name = "Alice", Email = "a@b.com", Age = 30 };
var dto = new PersonDto(person); // Uses generated constructor
```

LINQ projection support:

```csharp
var query = dbContext.People.Select(PersonDto.Projection).ToList();
```

## Advanced Scenarios

- Generate records, structs, or record structs
- Include/exclude fields and properties
- Custom mapping logic for derived or formatted values
- Seamless integration with LINQ and EF Core

See the [Advanced Scenarios](docs/06_AdvancedScenarios.md) guide for more.

## Facet.Extensions for LINQ/EF Core async

Install Facet.Extensions for one-line mapping helpers:

```bash
dotnet add package Facet.Extensionsusing Facet.Extensions;
```

```csharp
// Single object
var dto = person.ToFacet<Person, PersonDto>();

// Lists
var dtos = people.SelectFacets<Person, PersonDto>();

// IQueryable deferred projection
var query = dbContext.People.SelectFacet<Person, PersonDto>();

// EF Core async
var dtos = await dbContext.People.ToFacetsAsync<Person, PersonDto>();
var single = await dbContext.People.FirstFacetAsync<Person, PersonDto>();
```

---

**Facet** — Define less, project more.
