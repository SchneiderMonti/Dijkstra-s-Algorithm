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

        _startVertex = _vertices.OfType<StartVertex>().FirstOrDefault();

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
            .Where(v => v.IsWalkable())
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

                if (!neighbor.IsWalkable())
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

        if (!clickedVertex.IsWalkable())
        {
            InfoText.Text = "You cannot select a blocked city as target.";
            return;
        }

        ResetGraphState();

        clickedVertex.IsTarget = true;

        var (dist, prev) = await RunDijkstra(_startVertex);

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
        for (int i = 0; i < _vertices.Count; i++)
        {
            if (_vertices[i] is WallVertex wall)
            {
                var replacement = new CityVertex(wall.x, wall.y, wall.Name);
                _vertices[i] = replacement;

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

    private List<BaseEdge> CreateEdges(List<BaseVertex> vertices, int neighborCount)
    {
        var edges = new List<BaseEdge>();

        if (vertices.Count <= 1)
            return edges;

        var connected = new List<BaseVertex>();
        var remaining = new List<BaseVertex>(vertices);

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

        foreach (var v in vertices)
        {
            var nearest = vertices
                .Where(other => other != v)
                .OrderBy(other => Distance(v, other))
                .Take(neighborCount)
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
        if (clickedVertex is StartVertex)
            return;

        int index = _vertices.IndexOf(clickedVertex);
        if (index == -1)
            return;

        BaseVertex replacement;

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
            replacement = new WallVertex(clickedVertex.x, clickedVertex.y, clickedVertex.Name);
            InfoText.Text = $"{clickedVertex.Name} is now blocked.";
        }

        _vertices[index] = replacement;

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

    private double Distance(BaseVertex a, BaseVertex b)
    {
        double dx = a.x - b.x;
        double dy = a.y - b.y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}