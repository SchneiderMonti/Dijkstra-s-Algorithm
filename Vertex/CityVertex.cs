namespace Vertex;

public class CityVertex : BaseVertex
{
    public CityVertex(string name) : base(name)
    {
    }

    public CityVertex(int x, int y, string name) : base(x, y, name)
    {
    }

    public override bool IsWalkable()
    {
        return true;
    }

    public override string GetVertexType()
    {
        return "City";
    }
}