// See https://aka.ms/new-console-template for more information
using Facet;
using Facet.Mapping;

Console.WriteLine("Hello, World!");

var user = new User
{
    FirstName = "Tim",
    LastName = "Maes",
    Registered = DateTime.UtcNow
};

var dto = new UserDto(user);

Console.WriteLine($"Name: {dto.FullName}");
Console.WriteLine($"Registered: {dto.RegisteredText}");

public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Registered { get; set; }
}

[Facet(typeof(User), Configuration = typeof(UserMapper))]
public partial class UserDto
{
    public string FullName { get; set; }
    public string RegisteredText { get; set; }
}

public class UserMapper : IFacetMapConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.RegisteredText = source.Registered.ToString("yyyy-MM-dd");
    }
}