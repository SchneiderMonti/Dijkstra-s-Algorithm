namespace Vertex;

// Represents a connection (edge) between two vertices in the graph.
public class BaseEdge
{
    // Source and target vertices of the edge
    public BaseVertex source { get; set; }
    public BaseVertex? target { get; set; }

    // Name and weight (distance) of the edge
    public string Name { get; set; }
    public double weight { get; set; }

    // Visualization flags
    public bool IsActive { get; set; }
    public bool IsPath { get; set; }

    public BaseEdge(BaseVertex source, BaseVertex? target, string name, double weight)
    {
        this.source = source;
        this.target = target;
        this.Name = name;
        this.weight = weight;
    }

    public override string ToString()
    {
        return $"{this.Name} from {this.source} to {this.target} with a length of {this.weight}";
    }
}