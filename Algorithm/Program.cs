namespace GraphVisualizer;

using Vertex;

class Program
{
    static void Main(string[] args)
    {
        //BaseVertex vertex1 = new BaseVertex();
        //BaseVertex vertex2 = new BaseVertex();

        //Console.WriteLine(vertex1);

        //List<BaseVertex> vertices = GenerateVertices(10);
        //foreach (BaseVertex vertex in vertices)
        // {
        //     Console.WriteLine(vertex);
        // }
        List<BaseVertex> sortedVertices = GenerateVertices(10).OrderBy(v => v.x).ThenBy(v => v.y).ToList();
        foreach (BaseVertex vertex in sortedVertices)
        {
            Console.WriteLine(vertex);
        }

        List<BaseEdge> edges = GenerateEdges(sortedVertices, 10);
        foreach (BaseEdge edge in edges)
        {
            Console.WriteLine(edge);
        }

        //Edge edge = new Edge(vertex1, vertex2);
        //Console.WriteLine(edge);
    }



    static List<BaseVertex> GenerateVertices(int count)
    {
        List<BaseVertex> vertices = new List<BaseVertex>();
        BaseVertex start = new BaseVertex(0, 0, "Start");
        vertices.Add(start);
        for (int i = 1; i < count; i++)
        {
            vertices.Add(new BaseVertex(i.ToString()));
        }
        BaseVertex end = new BaseVertex(100, 100, "End");
        vertices.Add(end);
        return vertices;
    }

    static List<BaseEdge> GenerateEdges(List<BaseVertex> sortedVertices, int count)
    {
        List<BaseEdge> edges = new List<BaseEdge>();
        Random rand = new Random();
        for (int i = 0; i < count; i++)
        {
            BaseVertex source = sortedVertices[i];
            BaseVertex target = sortedVertices[i + 1];
            if (source == target || source.name == "End" && target.name == "Start" || source.name == "Start" && target.name == "End")
            {
                i--;
                Console.WriteLine("Duplicate");
                continue;
            }
            double weight = Math.Sqrt(Math.Pow(source.x - target.x, 2) + Math.Pow(source.y - target.y, 2));
            edges.Add(new BaseEdge(source, target, $"Edge {i}", weight));
        }
        return edges;
    }

}
