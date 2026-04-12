namespace Vertex;

public class StartVertex : BaseVertex
{
    public StartVertex(int x, int y, string name) : base(x, y, name)
    {
    }

    public override bool IsWalkable()
    {
        return true;
    }

    public override string GetVertexType()
    {
        return "Start";
    }
}