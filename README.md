_This is a new project, any help, feedback and contributions are highy appreciated._

# Facet

```
"One part of a subject, situation, object that has many parts."
```

**Facet** is a source generator that instantly scaffolds DTOs, typed LINQ projections, and ViewModels without any runtime overhead.

---

- :white_check_mark: **Zero runtime cost**: all mapping logic happens at compile time
- :white_check_mark: **Boilerplate-free**: write only your domain and facet definitions
- :white_check_mark: **Configurable**: exclude members, include fields, add custom mapping
- :white_check_mark: **Queryable**: produce `Expression<Func<TSource,TTarget>>` for EF Core and other LINQ providers

---

| Feature |  Description    |
| ------- |------
| Partial classes & records    |   Generate partial class or record from your types   |
| Exclude members     |  Omit unwanted properties or fields via attribute parameters   |
| Include public fields    |  Opt‑in support for mapping public fields   |
| Custom mapping logic    |  Hook into `IFacetMapConfiguration<TSource, TTarget>` with `static Map()`   |
| Constructor & projection    |  Auto-generate ctor and/or `Expression<Func<…, …>>` for LINQ providers   |
| Records, classes, readonly, init-only | Works with all member kinds   |

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

[Facet(typeof(Person), Kind = FacetKind.Record]
public partial record PersonRecord
{
    // ..
}
```

The `PersonDto` will have a constructor that maps the source type properties.

`PersonWithNote` is generated and will not have the Email property, but will have the Note property.

`PersoneRecord` is generated as a **record** 

You can map source to target:

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

## Advanced mapping support

### 1. Constructor-based custom logic

Install mapping package:

```bash
dotnet add package Facet.Mapping
```
Define your configuration:

```csharp
using Facet;

public class UserMapConfig : IFacetMapConfiguration<User, UserDto>
{
    public static void Map(User src, UserDto dst)
    {
        dst.FullName = $"{src.FirstName} {src.LastName}";
        dst.RegisteredText = src.Registered.ToString("yyyy-MM-dd");
    }
}
```

Apply attribute with Configuration:

```csharp
[Facet(
    typeof(User),
    exclude: new[] { "Password" },
    Configuration = typeof(UserMapConfig)]
public partial class UserDto
{
    public string FullName { get; set; }
    public string RegisteredText { get; set; }
}
```

### 2. LINQ expression projection

Enable SQL-friendly projection:

```csharp
[Facet(
    typeof(PersonEntity),
    GenerateExpressionProjection = true)]
public partial class PersonDto { }
```

Use with EF Core or other `IQueryable<T>`:

```csharp
var dtos = dbContext.People
    .Where(p => p.IsActive)
    .Select(PersonDto.Projection)
    .ToList();
```
## 3. Chaining Facets

You can stack facets in-memory:

```csharp
// 1) Entity → DTO
[Facet(typeof(Person), GenerateExpressionProjection = true)]
public partial class PersonDto { }

// 2DTO → ViewModel
[Facet(typeof(PersonDto))]
public partial class PersonViewModel { /* add UI props */ }

// 1) SQL → DTO
var dtos = dbContext.People.Select(PersonDto.Projection).ToList();
// 2) In-memory → VM
var vms  = dtos.Select(PersonViewModel.Projection.Compile()).ToList();
```
