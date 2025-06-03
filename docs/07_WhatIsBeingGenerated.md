# What is Being Generated?

This page shows concrete examples of what Facet generates for different scenarios. These examples help you understand the output you can expect in your own projects.

---

## 1. Class Exclude Example

**Input:**
```csharp
public class Person
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}

[Facet(typeof(Person), nameof(Person.Email))]
public partial class PersonWithoutEmail { }
```

**Generated:**
```csharp
public partial class PersonWithoutEmail
{
    public string Name { get; set; }
    public int Age { get; set; }
    public PersonWithoutEmail(Project.Namespace.Person source)
    {
        this.Name = source.Name;
        this.Age = source.Age;
    }
    public static Expression<Func<Project.Namespace.Person, PersonWithoutEmail>> Projection =>
        source => new PersonWithoutEmail(source);
}
```

---

## 2. Class With Additional Properties

**Input:**
```csharp
[Facet(typeof(Person), nameof(Person.Email), nameof(Person.Age))]
public partial class PersonWithNote
{
    public string Note { get; set; }
}
```

**Generated:**
```csharp
public partial class PersonWithNote
{
    public string Name { get; set; }
    public string Note { get; set; }
    public PersonWithNote(Project.Namespace.Person source)
    {
        this.Name = source.Name;
    }
    public static Expression<Func<Project.Namespace.Person, PersonWithNote>> Projection =>
        source => new PersonWithNote(source);
}
```

---

## 3. Field Facet Example

**Input:**
```csharp
public class PersonWithField
{
    public string Name;
    public int Age;
    public Guid Identifier;
    public string Email { get; set; }
}

[Facet(typeof(PersonWithField), IncludeFields = true)]
public partial class PersonWithFieldFacet { }
```

**Generated:**
```csharp
public partial class PersonWithFieldFacet
{
    public string Name;
    public int Age;
    public Guid Identifier;
    public string Email { get; set; }
    public PersonWithFieldFacet(Project.Namespace.PersonWithField source)
    {
        this.Name = source.Name;
        this.Age = source.Age;
        this.Identifier = source.Identifier;
        this.Email = source.Email;
    }
    public static Expression<Func<Project.Namespace.PersonWithField, PersonWithFieldFacet>> Projection =>
        source => new PersonWithFieldFacet(source);
}
```

---

## 4. Custom Mapping Example

**Input:**
```csharp
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Registered { get; set; }
}

public class UserMapper : IFacetMapConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.RegisteredText = source.Registered.ToString("yyyy-MM-dd");
    }
}

[Facet(typeof(User), nameof(User.FirstName), nameof(User.LastName), nameof(User.Registered), Configuration = typeof(UserMapper))]
public partial class UserDto
{
    public string FullName { get; set; }
    public string RegisteredText { get; set; }
}
```

**Generated:**
```csharp
public partial class UserDto
{
    public UserDto(Project.Namespace.User source)
    {
        Project.Namespace.UserMapper.Map(source, this);
    }
    public static Expression<Func<Project.Namespace.User, UserDto>> Projection =>
        source => new UserDto(source);
}
```

---

## 5. Readonly / Init-Only Example

**Input:**
```csharp
public class ReadonlySourceModel
{
    public string Id { get; }
    public DateTime CreatedAt { get; init; }
    public string Status { get; set; }
    public ReadonlySourceModel(string id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }
}

[Facet(typeof(ReadonlySourceModel))]
public partial class ReadonlySourceModelFacet { }
```

**Generated:**
```csharp
public partial class ReadonlySourceModelFacet
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
    public ReadonlySourceModelFacet(Project.Namespace.ReadonlySourceModel source)
    {
        this.Id = source.Id;
        this.CreatedAt = source.CreatedAt;
        this.Status = source.Status;
    }
    public static Expression<Func<Project.Namespace.ReadonlySourceModel, ReadonlySourceModelFacet>> Projection =>
        source => new ReadonlySourceModelFacet(source);
}
```

---

## 6. Record Facet Example

**Input:**
```csharp
public record UserRecord(string FirstName, string LastName, DateTime Registered);

public class UserRecordMapper : IFacetMapConfiguration<UserRecord, UserRecordDto>
{
    public static void Map(UserRecord source, UserRecordDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.RegisteredText = source.Registered.ToString("MMMM yyyy");
    }
}

[Facet(typeof(UserRecord), nameof(UserRecord.FirstName), nameof(UserRecord.LastName), nameof(UserRecord.Registered), Configuration = typeof(UserRecordMapper), Kind = FacetKind.Record)]
public partial record UserRecordDto
{
    public string FullName { get; set; }
    public string RegisteredText { get; set; }
}
```

**Generated:**
```csharp
public partial record UserRecordDto();
public partial record UserRecordDto
{
    public UserRecordDto(Project.Namespace.UserRecord source) : this()
    {
        Project.Namespace.UserRecordMapper.Map(source, this);
    }
    public static Expression<Func<Project.Namespace.UserRecord, UserRecordDto>> Projection =>
        source => new UserRecordDto(source);
}
```

---

## 7. Record Struct Facet Example

**Input:**
```csharp
public record struct DataRecordStruct(int Code, string Label);

[Facet(typeof(DataRecordStruct), Kind = FacetKind.RecordStruct)]
public partial record struct DataRecordStructDto { }
```

**Generated:**
```csharp
public partial record struct DataRecordStructDto(int Code, string Label);
public partial record struct DataRecordStructDto
{
    public DataRecordStructDto(Project.Namespace.DataRecordStruct source) : this(source.Code, source.Label) { }
    public static Expression<Func<Project.Namespace.DataRecordStruct, DataRecordStructDto>> Projection =>
        source => new DataRecordStructDto(source);
}
```

---

## 8. Plain Struct Facet Example

**Input:**
```csharp
public struct PersonStruct
{
    public string Name;
    public int Age;
    public string Email;
}

[Facet(typeof(PersonStruct), Kind = FacetKind.Struct, IncludeFields = true)]
public partial struct PersonStructDto { }
```

**Generated:**
```csharp
public partial struct PersonStructDto
{
    public string Name;
    public int Age;
    public string Email;
    public PersonStructDto(Project.Namespace.PersonStruct source)
    {
        this.Name = source.Name;
        this.Age = source.Age;
        this.Email = source.Email;
    }
    public static Expression<Func<Project.Namespace.PersonStruct, PersonStructDto>> Projection =>
        source => new PersonStructDto(source);
}
```

---

See the [Quick Start](02_QuickStart.md) and [Advanced Scenarios](06_AdvancedScenarios.md) for more details.
