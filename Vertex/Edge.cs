namespace Vertex;

public class BaseEdge
{
    public BaseVertex source { get; set; }
    public BaseVertex? target { get; set; }
    public string Name { get; set; }
    public double weight { get; set; }

    public bool IsActive { get; set; } = false;
    public bool IsPath { get; set; } = false;

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