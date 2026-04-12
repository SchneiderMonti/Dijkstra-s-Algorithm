namespace Vertex;

public class WallVertex : BaseVertex
{
    public WallVertex(int x, int y, string name) : base(x, y, name)
    {
    }

    public override bool IsWalkable()
    {
        return false;
    }

    public override string GetVertexType()
    {
        return "Wall";
    }
}