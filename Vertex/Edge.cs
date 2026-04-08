namespace Vertex;

using Vertex;
public class BaseEdge
{
    public BaseVertex? source {get; set;}
    public BaseVertex? target {get; set;}
    public string edgename;
    public double weight {get; set;}

    public bool IsPath {get; set;} = false;
    public BaseEdge(BaseVertex Source, BaseVertex Target, string name, double weight)
    {
        this.source = Source;
        this.target = Target;
        this.edgename = name;
        this.weight = weight;
    }

    public override string ToString()
    {
        return $"{this.edgename} from {this.source} to {this.target} with a length of {this.weight}";
    }
}