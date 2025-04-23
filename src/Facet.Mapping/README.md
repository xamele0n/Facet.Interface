# Facet.Mapping

**Facet.Mapping** enables advanced static mapping logic for the [Facet](https://www.nuget.org/packages/Facet) source generator.

This package defines a strongly-typed interface that allows you to plug in custom mapping logic between source and generated Facet types — all at compile time, with zero runtime reflection.

---

## What is this for?

`Facet` lets you define slim, redacted, or projected versions of classes using just attributes.  
With **Facet.Mapping**, you can go further — and define custom logic like combining properties, renaming, transforming types, or applying conditions.

---

## How it works

1. Implement the `IFacetMapConfiguration<TSource, TTarget>` interface.
2. Define a static `Map` method.
3. Point the `[Facet(...)]` attribute to the config class using `Configuration = typeof(...)`.

---

## Install

```bash
dotnet add package Facet.Mapping

## Example

```csharp
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

[Facet(typeof(User), GenerateConstructor = true, Configuration = typeof(UserMapper))]
public partial class UserDto
{
    public string FullName { get; set; }
}
```

```csharp
public class UserMapper : IFacetMapConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}

```