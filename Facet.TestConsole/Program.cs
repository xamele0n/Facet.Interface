using Facet;

namespace Redacto.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Console.WriteLine("Redacted props:");
            foreach (var prop in typeof(PersonWithoutEmail).GetProperties())
            {
                Console.WriteLine($"- {prop.Name}");
            }

            Console.WriteLine("Redacted props only name:");
            foreach (var prop in typeof(PersonWithOnlyName).GetProperties())
            {
                Console.WriteLine($"- {prop.Name}");
            }

            Console.WriteLine("Redacted props extra property:");
            foreach (var prop in typeof(PersonWithOnlyNameAndExtraProperty).GetProperties())
            {
                Console.WriteLine($"- {prop.Name}");
            }
        }
    }

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

    [Facet(typeof(Person), new[] { nameof(Person.Email), nameof(Person.Age) })]
    public partial class PersonWithOnlyName
    {
    }

    [Facet(typeof(Person), new[] { nameof(Person.Email), nameof(Person.Age) })]
    public partial class PersonWithOnlyNameAndExtraProperty
    {
        public string ExtraProperty { get; set; }
    }
}
