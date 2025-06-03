# Quick Start Guide

This guide will help you get up and running with Facet in just a few steps.

## 1. Install the NuGet Package

```
dotnet add package Facet
```

For LINQ/EF Core helpers:
```
dotnet add package Facet.Extensions
```

## 2. Define Your Source Model

```csharp
public class Person
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}
```

## 3. Create a Facet (DTO/Projection)

```csharp
using Facet;

[Facet(typeof(Person), exclude: nameof(Person.Email))]
public partial class PersonDto { }
```

## 4. Use the Generated Type

```csharp
var person = new Person { Name = "Alice", Email = "a@b.com", Age = 30 };
var dto = new PersonDto(person); // Uses generated constructor
```

## 5. LINQ/EF Core Integration

```csharp
var query = dbContext.People.Select(PersonDto.Projection).ToList();
```

Or with Facet.Extensions:

```csharp
using Facet.Extensions;

var dtos = people.SelectFacets<Person, PersonDto>();
var efQuery = dbContext.People.SelectFacet<Person, PersonDto>();
```

---

See the [Attribute Reference](03_AttributeReference.md) and [Extension Methods](05_Extensions.md) for more details.
