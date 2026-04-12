using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Vertex;
using System.Threading.Tasks;

namespace GraphVisualizer;

public partial class MainWindow : Window
{
    private readonly Random _random = new();
    private List<BaseVertex> _vertices = new();
private List<BaseEdge> _edges = new();
private BaseVertex? _startVertex;

   public MainWindow()
{
    InitializeComponent();
    MyGraphCanvas.VertexClicked += OnVertexClicked;
    MyGraphCanvas.VertexRightClicked += OnVertexRightClicked;
}

private void GenerateGraph_Click(object? sender, RoutedEventArgs e)
{
    int vertexCount = ParseOrDefault(VertexCountBox.Text, 15);
    int neighborCount = ParseOrDefault(NeighborCountBox.Text, 3);

    _vertices = CreateVertices(vertexCount);
    _edges = CreateEdges(_vertices, neighborCount);

    _startVertex = _vertices.FirstOrDefault(v => v.IsStart);

    MyGraphCanvas.Vertices = _vertices;
    MyGraphCanvas.Edges = _edges;
    MyGraphCanvas.InvalidateVisual();

    InfoText.Text = $"Graph generated ({vertexCount} vertices, {neighborCount} neighbors).";
}

private int ParseOrDefault(string? text, int defaultValue)
{
    if (int.TryParse(text, out int result))
        return result;

    return defaultValue;
}

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

    if (_startVertex != null)
        _startVertex.IsStart = true;
}

private List<BaseEdge> GetOutgoingEdges(BaseVertex vertex)
{
    return _edges
        .Where(e => e.source == vertex && e.target != null)
        .ToList();
}

private async Task<(Dictionary<BaseVertex, double> dist, Dictionary<BaseVertex, BaseVertex?> prev)>
    RunDijkstra(BaseVertex start)
{
    var dist = new Dictionary<BaseVertex, double>();
    var prev = new Dictionary<BaseVertex, BaseVertex?>();
    var unvisited = _vertices
        .Where(v => !v.IsWall)
        .ToList();

    foreach (var vertex in _vertices)
    {
        dist[vertex] = double.PositiveInfinity;
        prev[vertex] = null;
    }

    dist[start] = 0;

    while (unvisited.Count > 0)
    {
        var current = unvisited
            .OrderBy(v => dist[v])
            .First();

        unvisited.Remove(current);

        if (double.IsPositiveInfinity(dist[current]))
            break;

        current.IsVisited = true;

        foreach (var edge in GetOutgoingEdges(current))
        {
            if (edge.target == null)
                continue;

            var neighbor = edge.target;

            if (neighbor.IsWall)
                continue;

            if (!unvisited.Contains(neighbor))
                continue;

            edge.IsActive = true;
            MyGraphCanvas.InvalidateVisual();
            await Task.Delay(120);

            double alternative = dist[current] + edge.weight;

            if (alternative < dist[neighbor])
            {
                dist[neighbor] = alternative;
                prev[neighbor] = current;
            }

            edge.IsActive = false;
            MyGraphCanvas.InvalidateVisual();
        }
    }

    return (dist, prev);
}

private async void OnVertexClicked(BaseVertex clickedVertex)
{
    if (_startVertex == null)
        return;

    if (clickedVertex.IsWall)
    {
        InfoText.Text = "You cannot select a wall as target.";
        return;
    }

    ResetGraphState();

    _startVertex.IsStart = true;
    clickedVertex.IsTarget = true;

    var (dist, prev) = await RunDijkstra(_startVertex);

    if (double.IsPositiveInfinity(dist[clickedVertex]))
    {
        InfoText.Text = $"No path from Eldoria to {clickedVertex.Name}";
        MyGraphCanvas.InvalidateVisual();
        return;
    }

    var path = BuildPath(clickedVertex, prev);
    HighlightPath(path);

    InfoText.Text = $"Target: {clickedVertex.Name} | Distance from Start: {dist[clickedVertex]:0.00}";
    MyGraphCanvas.InvalidateVisual();
}

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

private void ClearWalls_Click(object? sender, RoutedEventArgs e)
{
    foreach (var vertex in _vertices)
    {
        vertex.IsWall = false;
    }

    ResetGraphState();
    InfoText.Text = "All walls cleared.";
    MyGraphCanvas.InvalidateVisual();
}
    // --- CREATE VERTICES ---
private List<BaseVertex> CreateVertices(int count)
{
    var vertices = new List<BaseVertex>();

    var baseCityNames = new List<string>
    {
        "Eldoria", "Drakmoor", "Valencrest", "Mythrune", "Stormhaven",
        "Aetherfall", "Ravenrock", "Silverkeep", "Thornvale", "Frostgarde",
        "Duskmire", "Ironspire", "Mooncliff", "Emberfall", "Highreach",
        "Shadowfen", "Brighthelm", "Windermere", "Grimwatch", "Sunforge",
        "Blackwater", "Starhaven", "Oakenshire", "Crystalia", "Dragon’s Hollow"
    };

    var suffixes = new List<string>
    {
        "Lower", "Upper", "Center", "Outskirts"
    };

    // Start city
    var start = new BaseVertex(0, 0, "Eldoria");
    start.IsStart = true;
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

        vertices.Add(new BaseVertex(name));
    }

    return vertices;
}

    // --- CREATE EDGES ---
private List<BaseEdge> CreateEdges(List<BaseVertex> vertices, int neighborCount)
{
    var edges = new List<BaseEdge>();

    if (vertices.Count <= 1)
        return edges;

    // -----------------------------
    // PHASE 1: build a connected backbone
    // -----------------------------
    var connected = new List<BaseVertex>();
    var remaining = new List<BaseVertex>(vertices);

    // start with the first vertex (your Start node is usually first)
    connected.Add(remaining[0]);
    remaining.RemoveAt(0);

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

        if (bestFrom != null && bestTo != null)
        {
            AddUndirectedEdge(edges, bestFrom, bestTo, bestDistance);

            connected.Add(bestTo);
            remaining.Remove(bestTo);
        }
    }

    // -----------------------------
    // PHASE 2: add extra nearest-neighbor edges
    // -----------------------------
    foreach (var v in vertices)
    {
        var nearest = vertices
            .Where(other => other != v)
            .OrderBy(other => Distance(v, other))
            .Take(neighborCount) // extra neighbors, can change to 3 if you want denser graphs
            .ToList();

        foreach (var n in nearest)
        {
            if (!HasUndirectedEdge(edges, v, n))
            {
                double weight = Distance(v, n);
                AddUndirectedEdge(edges, v, n, weight);
            }
        }
    }

    return edges;
}
private void AddUndirectedEdge(List<BaseEdge> edges, BaseVertex a, BaseVertex b, double weight)
{
    edges.Add(new BaseEdge(a, b, $"{a.Name}->{b.Name}", weight));
    edges.Add(new BaseEdge(b, a, $"{b.Name}->{a.Name}", weight));
}

    private bool HasUndirectedEdge(List<BaseEdge> edges, BaseVertex a, BaseVertex b)
    {
        return edges.Any(e =>
            (e.source == a && e.target == b) ||
            (e.source == b && e.target == a));
    }

private void OnVertexRightClicked(BaseVertex clickedVertex)
{
    if (clickedVertex.IsStart)
        return;

    clickedVertex.IsWall = !clickedVertex.IsWall;

    if (clickedVertex.IsWall)
    {
        clickedVertex.IsTarget = false;
        clickedVertex.IsVisited = false;
        clickedVertex.IsPath = false;
    }

    foreach (var edge in _edges)
    {
        edge.IsActive = false;
        edge.IsPath = false;
    }

    InfoText.Text = clickedVertex.IsWall
        ? $"{clickedVertex.Name} is now a wall."
        : $"{clickedVertex.Name} is no longer a wall.";

    MyGraphCanvas.InvalidateVisual();
}

    private double Distance(BaseVertex a, BaseVertex b)
    {
        double dx = a.x - b.x;
        double dy = a.y - b.y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}