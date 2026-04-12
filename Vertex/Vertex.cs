namespace Vertex;

// Base class representing a vertex (node) in the graph.
// Stores position, state information for visualization,
// and provides default behavior for derived vertex types.
public class BaseVertex
{
    // Position of the vertex in the logical graph space
    public int x { get; set; }
    public int y { get; set; }

    // Visualization states used during Dijkstra execution
    public bool IsVisited { get; set; } = false;
    public bool IsPath { get; set; } = false;
    public bool IsTarget { get; set; } = false;

    // Display name of the vertex (e.g., city name)
    public string Name { get; set; } = "";

    // Random generator for assigning positions to vertices
    protected static Random rand = new Random();

    // Creates a vertex with a random position
    public BaseVertex(string name)
    {
        this.x = rand.Next(0, 100);
        this.y = rand.Next(0, 100);
        this.Name = name;
    }

    // Creates a vertex with a fixed position
    public BaseVertex(int x, int y, string name)
    {
        this.x = x;
        this.y = y;
        this.Name = name;
    }

    // Determines whether the vertex can be traversed.
    // Overridden in WallVertex to block movement.
    public virtual bool IsWalkable()
    {
        return true;
    }

    // Returns the type of the vertex (used for visualization/debugging).
    // Overridden in derived classes.
    public virtual string GetVertexType()
    {
        return "Base";
    }

    public override string ToString()
    {
        return $"{this.Name} = ({this.x}, {this.y})";
    }
}