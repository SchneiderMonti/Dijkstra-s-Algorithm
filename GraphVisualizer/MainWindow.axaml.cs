using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Vertex;
using System.Threading.Tasks;

namespace GraphVisualizer;

// Main application window that controls graph generation, user interaction,
// and execution of Dijkstra's algorithm.
public partial class MainWindow : Window
{
    private readonly Random _random = new();
    private List<BaseVertex> _vertices = new();
    private List<BaseEdge> _edges = new();
    private BaseVertex? _startVertex;


    // Initializes the UI components and connects canvas events to handler methods.
    public MainWindow()
    {
        InitializeComponent();
        MyGraphCanvas.VertexClicked += OnVertexClicked;
        MyGraphCanvas.VertexRightClicked += OnVertexRightClicked;
    }


    // Generates a new graph based on user input (vertex count and neighbor count)
    // and updates the visualization.
    private void GenerateGraph_Click(object? sender, RoutedEventArgs e)
    {
        int vertexCount = ParseOrDefault(VertexCountBox.Text, 15);
        int neighborCount = ParseOrDefault(NeighborCountBox.Text, 3);

        _vertices = CreateVertices(vertexCount);
        _edges = CreateEdges(_vertices, neighborCount);

        // Find the start vertex (there is exactly one)
        _startVertex = _vertices.OfType<StartVertex>().FirstOrDefault();

        MyGraphCanvas.Vertices = _vertices;
        MyGraphCanvas.Edges = _edges;
        MyGraphCanvas.InvalidateVisual();

        InfoText.Text = $"Graph generated ({vertexCount} vertices, {neighborCount} neighbors).";
    }

    // Parses user input safely and falls back to a default value if parsing fails.
    private int ParseOrDefault(string? text, int defaultValue)
    {
        if (int.TryParse(text, out int result))
            return result;

        return defaultValue;
    }

    // Resets all visualization-related states (visited nodes, paths, highlights)
    // without modifying the graph structure itself.
    private void ResetGraphState()
    {
        foreach (var vertex in _vertices)
        {
            vertex.IsTarget = false;
            vertex.IsVisited = false;
            vertex.IsPath = false;
        }

        foreach (var edge in _edges)
        {
            edge.IsPath = false;
            edge.IsActive = false;
        }
    }

    // Returns all outgoing edges of a given vertex.
    private List<BaseEdge> GetOutgoingEdges(BaseVertex vertex)
    {
        return _edges
            .Where(e => e.source == vertex && e.target != null)
            .ToList();
    }

// Runs Dijkstra's Algorithm starting from the given start vertex.
// Returns two dictionaries:
// 1. dist → shortest known distance from start to each vertex
// 2. prev → previous vertex in the shortest path (used for path reconstruction)
private async Task<(Dictionary<BaseVertex, double> dist, Dictionary<BaseVertex, BaseVertex?> prev)>
    RunDijkstra(BaseVertex start)
{
    // Stores the shortest distance from the start to each vertex
    var dist = new Dictionary<BaseVertex, double>();

    // Stores the previous vertex for each vertex (to reconstruct the path later)
    var prev = new Dictionary<BaseVertex, BaseVertex?>();

    // List of vertices that have not been visited yet (only walkable ones)
    var unvisited = _vertices
        .Where(v => v.IsWalkable())
        .ToList();

    // Initialize all distances as infinity and previous nodes as null
    foreach (var vertex in _vertices)
    {
        dist[vertex] = double.PositiveInfinity;
        prev[vertex] = null;
    }

    // Distance to the start vertex is zero
    dist[start] = 0;

    // Main loop: continue until all reachable vertices are processed
    while (unvisited.Count > 0)
    {
        // Select the unvisited vertex with the smallest tentative distance
        var current = unvisited
            .OrderBy(v => dist[v])
            .First();

        // Remove it from the unvisited list (mark as visited)
        unvisited.Remove(current);

        // If the smallest distance is still infinity, remaining vertices are unreachable
        if (double.IsPositiveInfinity(dist[current]))
            break;

        // Mark vertex as visited for visualization purposes
        current.IsVisited = true;

        // Check all outgoing edges from the current vertex
        foreach (var edge in GetOutgoingEdges(current))
        {
            // Skip if there is no valid target vertex
            if (edge.target == null)
                continue;

            var neighbor = edge.target;

            // Skip if the neighbor is not walkable (e.g., a wall)
            if (!neighbor.IsWalkable())
                continue;

            // Skip if the neighbor has already been visited
            if (!unvisited.Contains(neighbor))
                continue;

            // Highlight the current edge (for visualization)
            edge.IsActive = true;
            MyGraphCanvas.InvalidateVisual();
            await Task.Delay(120);

            // Calculate a new possible distance through the current vertex
            double alternative = dist[current] + edge.weight;

            // If the new distance is shorter, update it
            if (alternative < dist[neighbor])
            {
                dist[neighbor] = alternative;
                prev[neighbor] = current;
            }

            // Remove highlight from the edge
            edge.IsActive = false;
            MyGraphCanvas.InvalidateVisual();
        }
    }

    // Return the computed distances and path information
    return (dist, prev);
}

    // Handles left-click on a vertex:
    // sets the target and runs Dijkstra's algorithm.
    private async void OnVertexClicked(BaseVertex clickedVertex)
    {
        if (_startVertex == null)
            return;

        // Prevent selecting blocked nodes as targets
        if (!clickedVertex.IsWalkable())
        {
            InfoText.Text = "You cannot select a blocked city as target.";
            return;
        }

        ResetGraphState();

        clickedVertex.IsTarget = true;

        var (dist, prev) = await RunDijkstra(_startVertex);

        // If target is unreachable, show message
        if (double.IsPositiveInfinity(dist[clickedVertex]))
        {
            InfoText.Text = $"No path from {_startVertex.Name} to {clickedVertex.Name}";
            MyGraphCanvas.InvalidateVisual();
            return;
        }

        var path = BuildPath(clickedVertex, prev);
        HighlightPath(path);

        InfoText.Text = $"Shortest route from {_startVertex.Name} to {clickedVertex.Name}: {dist[clickedVertex]:0.00}";
        MyGraphCanvas.InvalidateVisual();
    }

    // Reconstructs the shortest path by following the prev dictionary backwards.
    private List<BaseVertex> BuildPath(BaseVertex target, Dictionary<BaseVertex, BaseVertex?> prev)
    {
        var path = new List<BaseVertex>();
        BaseVertex? current = target;

        while (current != null)
        {
            path.Insert(0, current);
            current = prev[current];
        }

        return path;
    }

 // Highlights all vertices and edges that belong to the shortest path.
    private void HighlightPath(List<BaseVertex> path)
    {
        foreach (var vertex in path)
        {
            vertex.IsPath = true;
        }

        for (int i = 0; i < path.Count - 1; i++)
        {
            var from = path[i];
            var to = path[i + 1];

            var edge = _edges.FirstOrDefault(e => e.source == from && e.target == to);
            if (edge != null)
                edge.IsPath = true;

            var reverse = _edges.FirstOrDefault(e => e.source == to && e.target == from);
            if (reverse != null)
                reverse.IsPath = true;
        }
    }


   // Converts all wall vertices back into normal city vertices.
    private void ClearWalls_Click(object? sender, RoutedEventArgs e)
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            if (_vertices[i] is WallVertex wall)
            {
                var replacement = new CityVertex(wall.x, wall.y, wall.Name);
                _vertices[i] = replacement;

                // Update all edges that referenced the wall vertex
                foreach (var edge in _edges)
                {
                    if (edge.source == wall)
                        edge.source = replacement;

                    if (edge.target == wall)
                        edge.target = replacement;
                }
            }
        }

        ResetGraphState();
        InfoText.Text = "All blocked cities cleared.";

        MyGraphCanvas.Vertices = _vertices;
        MyGraphCanvas.Edges = _edges;
        MyGraphCanvas.InvalidateVisual();
    }

  // Creates vertices using fantasy city names.
    // If more vertices are required, suffixes are added to extend the list.
    private List<BaseVertex> CreateVertices(int count)
    {
        var vertices = new List<BaseVertex>();

        var baseCityNames = new List<string>
        {
            "Isengard", "Drakmoor", "Valencrest", "Mythrune", "Stormhaven",
            "Aetherfall", "Ravenrock", "Silverkeep", "Thornvale", "Frostgarde",
            "Duskmire", "Ironspire", "Mooncliff", "Emberfall", "Highreach",
            "Shadowfen", "Brighthelm", "Windermere", "Grimwatch", "Sunforge",
            "Blackwater", "Starhaven", "Oakenshire", "Crystalia", "Dragon’s Hollow"
        };

        var suffixes = new List<string>
        {
            "Lower", "Upper", "Center", "Outskirts"
        };

        var start = new StartVertex(0, 0, "Eldoria");
        vertices.Add(start);

        for (int i = 1; i < count; i++)
        {
            int baseIndex = (i - 1) % baseCityNames.Count;
            int suffixIndex = (i - 1) / baseCityNames.Count;

            string name = baseCityNames[baseIndex];

            if (suffixIndex > 0)
            {
                string suffix = suffixes[(suffixIndex - 1) % suffixes.Count];
                name = $"{suffix} {name}";
            }

            vertices.Add(new CityVertex(name));
        }

        return vertices;
    }

     // Creates edges between vertices.
    // First, a connected backbone is built so that all vertices are reachable.
    // Then, additional nearest-neighbor edges are added to make the graph denser.
    private List<BaseEdge> CreateEdges(List<BaseVertex> vertices, int neighborCount)
    {
        var edges = new List<BaseEdge>();

        // A graph with fewer than 2 vertices cannot have edges
        if (vertices.Count <= 1)
            return edges;

        // Build a connected base structure:
        // connected = vertices already in the graph
        // remaining = vertices not connected yet
        var connected = new List<BaseVertex>();
        var remaining = new List<BaseVertex>(vertices);

        // Start with the first vertex
        connected.Add(remaining[0]);
        remaining.RemoveAt(0);

        // Repeatedly connect the closest remaining vertex to the connected set
        while (remaining.Count > 0)
        {
            BaseVertex? bestFrom = null;
            BaseVertex? bestTo = null;
            double bestDistance = double.MaxValue;

            foreach (var c in connected)
            {
                foreach (var r in remaining)
                {
                    double d = Distance(c, r);
                    if (d < bestDistance)
                    {
                        bestDistance = d;
                        bestFrom = c;
                        bestTo = r;
                    }
                }
            }

            // Add the shortest found connection
            if (bestFrom != null && bestTo != null)
            {
                AddUndirectedEdge(edges, bestFrom, bestTo, bestDistance);

                connected.Add(bestTo);
                remaining.Remove(bestTo);
            }
        }

        // Add extra edges to the nearest neighbors of each vertex
        foreach (var v in vertices)
        {
            var nearest = vertices
                .Where(other => other != v)
                .OrderBy(other => Distance(v, other))
                .Take(neighborCount)
                .ToList();

            foreach (var n in nearest)
            {
                // Avoid duplicate undirected edges
                if (!HasUndirectedEdge(edges, v, n))
                {
                    double weight = Distance(v, n);
                    AddUndirectedEdge(edges, v, n, weight);
                }
            }
        }

        return edges;
    }

    // Adds an undirected connection by creating two directed edges:
    // one from a to b and one from b to a.
    private void AddUndirectedEdge(List<BaseEdge> edges, BaseVertex a, BaseVertex b, double weight)
    {
        edges.Add(new BaseEdge(a, b, $"{a.Name}->{b.Name}", weight));
        edges.Add(new BaseEdge(b, a, $"{b.Name}->{a.Name}", weight));
    }

    // Checks whether an undirected edge between two vertices already exists.
    private bool HasUndirectedEdge(List<BaseEdge> edges, BaseVertex a, BaseVertex b)
    {
        return edges.Any(e =>
            (e.source == a && e.target == b) ||
            (e.source == b && e.target == a));
    }

    // Handles right-clicks on vertices:
    // a normal city becomes a wall, and a wall becomes a normal city again.
    private void OnVertexRightClicked(BaseVertex clickedVertex)
    {
        // The start vertex cannot be blocked
        if (clickedVertex is StartVertex)
            return;

        int index = _vertices.IndexOf(clickedVertex);
        if (index == -1)
            return;

        BaseVertex replacement;

        // Replace a wall with a normal city
        if (clickedVertex is WallVertex)
        {
            replacement = new CityVertex(clickedVertex.x, clickedVertex.y, clickedVertex.Name)
            {
                IsVisited = false,
                IsPath = false,
                IsTarget = false
            };

            InfoText.Text = $"{clickedVertex.Name} is no longer blocked.";
        }
        else
        {
            // Replace a normal city with a wall
            replacement = new WallVertex(clickedVertex.x, clickedVertex.y, clickedVertex.Name);
            InfoText.Text = $"{clickedVertex.Name} is now blocked.";
        }

        // Replace the vertex in the main vertex list
        _vertices[index] = replacement;

        // Update all edges that referenced the old vertex object
        foreach (var edge in _edges)
        {
            if (edge.source == clickedVertex)
                edge.source = replacement;

            if (edge.target == clickedVertex)
                edge.target = replacement;

            edge.IsActive = false;
            edge.IsPath = false;
        }

        MyGraphCanvas.Vertices = _vertices;
        MyGraphCanvas.Edges = _edges;
        MyGraphCanvas.InvalidateVisual();
    }

    // Computes the Euclidean distance between two vertices.
    // This distance is used as the edge weight.
    private double Distance(BaseVertex a, BaseVertex b)
    {
        double dx = a.x - b.x;
        double dy = a.y - b.y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}