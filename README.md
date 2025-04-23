_This is a new project, any help, feedback and contributions are highy appreciated._

# Facet

```
"One part of a subject, situation, etc. that has many parts."
```

**Facet** is a compile-time source generator that creates partial classes from existing types — by copying only the members you want.

Exclude properties, include public fields, add your own members, or generate a constructor — all with a single `[Facet]` attribute.

Use Facet to build lightweight DTOs, API shapes, or UI-bound view models **without writing boilerplate or mapping code.**

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

## Complex mapping support

If you want to map properties with custom logic, you can use the `Facet.Mapping` package.

### 1. Install the mapping package

```bash
dotnet add package Facet.Mapping
````

### 2. Define your models

Source model:
```csharp
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Registered { get; set; }
}
```
Target facet:
```csharp
[Facet(
    typeof(User),
    exclude: new[] { nameof(User.FirstName), nameof(User.LastName), nameof(User.Registered) },
    Configuration = typeof(UserMapper))]
public partial class UserDto
{
    public string FullName { get; set; }
    public string RegisteredText { get; set; }
}
```
### 3. Create and use map configuration

Mapping configuration from source to your facet:
```csharp
public class UserMapper : IFacetMapConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.RegisteredText = source.Registered.ToString("yyyy-MM-dd");
    }
}
```

```csharp
var user = new User
{
  FirstName = "Tim",
  LastName = "Maes",
  Registered = new DateTime(2020, 1, 15)
}

var dto = new UserDto(user);

Console.WriteLine(dto.FullName);        // Tim Maes
Console.WriteLine(dto.RegisteredText); // 2020-01-15

```
