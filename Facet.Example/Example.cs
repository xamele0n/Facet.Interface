using Facet;

namespace FacetExample;

public interface IFoo
{
    string Name { get; set; }
}

public interface IBar
{
    Guid Name { get; set; }
    
    DateTime Created { get; }
}

public class Foo : IFoo
{
    public string Name { get; set; }
    
    public long Id { get; set; }
}

[Facet(typeof(IFoo), GenerateProjection = false)]
[Facet(typeof(IBar), GenerateProjection = false)]
public partial class MyFacet
{
    
}