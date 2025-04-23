# Facet

**Facet** is a compile-time source generator that creates partial classes from existing types — by copying only the members you want.

Exclude properties, include public fields, add your own members, or generate a constructor — all with a single `[Facet]` attribute.

Use Facet to build lightweight DTOs, API shapes, or UI-bound view models **without writing boilerplate or mapping code.**

No base classes. No reflection. No runtime cost.

---

## Features

- :white_check_mark: Exclude properties and fields with `nameof(...)`
- :white_check_mark: Include public fields (optional)
- :white_check_mark: Generate constructors that copy from the source type
- :white_check_mark: Add your own extra properties in the generated type
- :white_check_mark: No base class or interface required
- :white_check_mark: No runtime reflection — compile-time only
- :white_check_mark: Works with or without namespaces

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

### 4 Extending

```csharp
[Facet(typeof(Person),
 excludes: new [] { nameof(Person.Email), nameof(Person.Age) } )]
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

### 5 Constructors

```csharp
[Facet(typeof(Person), nameof(Person.Email), GenerateConstructor = true)]
public partial class PersonDto { }
```

Now you can do:

```csharp
var person = new Person();
var dto = new PersonDto(person);
```

Which results in:

```csharp
public partial class PersonDto
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Guid RawId;

    public PersonDto(Person source)
    {
        this.Name = source.Name;
        this.Age = source.Age;
        this.RawId = source.RawId;
    }
}
```

## Why Facet?

Why Facet?
No mappings. No reflection. No bloated libraries.

Just clean, compile-time class shaping — with minimal syntax and full control.

## Package Info

- Target frameworks: netstandard2.0
- Supports: .NET Core 3.1+, .NET 5+, .NET 6+, .NET 7, .NET 8
- Analyzer delivery: Source generator is embedded in the NuGet package
