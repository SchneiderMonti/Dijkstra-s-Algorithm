namespace Vertex;

public class BaseVertex
{
    public int x { get; set; }
    public int y { get; set; }

    public bool IsVisited { get; set; } = false;
    public bool IsPath { get; set; } = false;
    public bool IsTarget { get; set; } = false;

    public string Name { get; set; } = "";

    protected static Random rand = new Random();

    public BaseVertex(string name)
    {
        this.x = rand.Next(0, 100);
        this.y = rand.Next(0, 100);
        this.Name = name;
    }

    public BaseVertex(int x, int y, string name)
    {
        this.x = x;
        this.y = y;
        this.Name = name;
    }

    public virtual bool IsWalkable()
    {
        return true;
    }

    public virtual string GetVertexType()
    {
        return "Base";
    }

    public override string ToString()
    {
        return $"{this.Name} = ({this.x}, {this.y})";
    }
}