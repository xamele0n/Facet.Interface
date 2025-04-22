# Facet

**Facet** is a Roslyn-powered source generator that creates derived classes from existing types by excluding properties — and optionally extending them.


Use it to generate lean DTOs, slim views, or faceted projections of your models with a single attribute.

---

## Features

- Fast source generation at compile time
- Exclude properties with `nameof(...)` — no magic strings
- Supports adding extra properties manually
- No runtime reflection
- No base class or interface requirements

---

## Quick Start

### 1. Install

```bash
dotnet add package Facet
```

### 2. Use

```csharp
using Facet;

public class Person
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}

[Facet(typeof(Person), nameof(Person.Email))]
public partial class PersonWithoutEmail
{
}
```

### 3. What gets generated

```csharp
public partial class PersonWithoutEmail
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

### 4 Extending while redacting

```csharp
[Facet(typeof(Person), nameof(Person.Email), nameof(Person.Age))]
public partial class PersonNameWithNote
{
    public string Note { get; set; }
}
```

Becomes:

```csharp
public partial class PersonNameWithNote
{
    public string Name { get; set; }
    public string Note { get; set; }
}
```

## Package Info

- Target frameworks: netstandard2.0
- Supports: .NET Core 3.1+, .NET 5+, .NET 6+, .NET 7, .NET 8
- Analyzer delivery: Source generator is embedded in the NuGet package