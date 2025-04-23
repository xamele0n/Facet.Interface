_This is a new project, any help, feedback and contributions are highy appreciated._

# Facet

```
"One part of a subject, situation, etc. that has many parts."
```

**Facet** is a compile-time source generator that creates partial classes from existing types — by copying only the members you want.

Exclude properties, include public fields, add your own members, or generate a constructor — all with a single `[Facet]` attribute.

Use Facet to build lightweight DTOs, API shapes, or UI-bound view models **without writing boilerplate or mapping code.**

No base classes. No mapppings. No reflection. No runtime cost.

## Features

- :white_check_mark: Exclude properties and fields with `nameof(...)`
- :white_check_mark: Include public fields (optional)
- :white_check_mark: Generate constructors that copy from the source type
- :white_check_mark: Add your own extra properties in the generated type
- :white_check_mark: No base class or interface required
- :white_check_mark: No runtime reflection — compile-time only
- :white_check_mark: Works with or without namespaces
- :white_check_mark: Don't worry about usings

---

## Quick Start

### 1. Install

```bash
dotnet add package Facet
```

### 2. Create your facet

Define a `partial class`, attach the `Facet` attribute and you're done.
```csharp
using Facet;

public class Person
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public Guid RawId;
}

// Generate class that can be mapped from source model

[Facet(typeof(Person), GenerateConstructor = true)]
public partial class PersonDto { }

// Generate class while adding and removing properties
// You can pass an array of properties to exclude

[Facet(typeof(Person), exclude: nameof(Person.Email)]
public partial class PersonWithNote 
{
    public string Note { get; set; }
}
```

The `PersonDto` will have a constructor that maps the source type properties.

`PersonWithNote` is generated and will not have the Email property, but will have the Note property.

### 3. Usage

```csharp
var person = new Person
{
    Name = "Tim",
    Email = "hidden@example.com",
    Age = 33,
    RawId = Guid.NewGuid()
};

var dto = new PersonDto(person);
```

## Why Facet?

Why Facet?
No mappings. No reflection. No bloated libraries.

Just clean, compile-time class shaping — with minimal syntax and full control.

## Package Info

- Target frameworks: netstandard2.0
- Supports: .NET Core 3.1+, .NET 5+, .NET 6+, .NET 7, .NET 8
- Analyzer delivery: Source generator is embedded in the NuGet package
