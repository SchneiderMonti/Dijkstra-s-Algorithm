using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Vertex;

namespace GraphVisualizer;

// Custom canvas control responsible for drawing the graph,
// scaling it to the window size, and handling mouse interaction.
public class GraphCanvas : Control
{
    // Vertices and edges currently displayed on the canvas
    public List<BaseVertex> Vertices { get; set; } = new();
    public List<BaseEdge> Edges { get; set; } = new();

    // Radius of each drawn vertex circle
    public const double VertexRadius = 10;

    // Padding between the graph and the canvas borders
    private const double Padding = 60;

    // Events raised when a vertex is left-clicked or right-clicked
    public event Action<BaseVertex>? VertexClicked;
    public event Action<BaseVertex>? VertexRightClicked;

    // Renders the complete graph:
    // first edges, then vertices, then labels.
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Vertices == null || Vertices.Count == 0)
            return;

        // Compute scaling and positioning so the graph fits inside the window
        var layout = GetLayoutValues();

        // Draw edges first so vertices appear on top
        foreach (var edge in Edges)
        {
            if (edge.source == null || edge.target == null)
                continue;

            var start = Transform(edge.source, layout);
            var end = Transform(edge.target, layout);

            Pen pen = new Pen(Brushes.LightGray, 1.5);

            // Highlight active edges during algorithm execution
            if (edge.IsActive)
                pen = new Pen(Brushes.Yellow, 4);
            else if (edge.IsPath)
                pen = new Pen(Brushes.Gold, 3);

            context.DrawLine(pen, start, end);
        }

        // Draw vertices on top of the edges
        foreach (var vertex in Vertices)
        {
            IBrush fill = Brushes.SteelBlue;

            // Choose vertex color depending on type or current state
            if (vertex is WallVertex)
                fill = Brushes.Red;
            else if (vertex is StartVertex)
                fill = Brushes.Gold;
            else if (vertex.IsTarget)
                fill = Brushes.LimeGreen;
            else if (vertex.IsPath)
                fill = Brushes.Gold;
            else if (vertex.IsVisited)
                fill = Brushes.LightBlue;

            var center = Transform(vertex, layout);

            context.DrawEllipse(
                fill,
                new Pen(Brushes.Black, 1.5),
                center,
                VertexRadius,
                VertexRadius
            );

            // Draw the city/vertex name next to the node
            var text = new FormattedText(
                vertex.Name,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.White
            );

            context.DrawText(text, new Point(center.X + 12, center.Y - 8));
        }
    }

    // Detects mouse clicks on vertices and raises the corresponding event.
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (Vertices == null || Vertices.Count == 0)
            return;

        var point = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;
        var layout = GetLayoutValues();

        // Check whether the mouse click lies inside a vertex circle
        foreach (var vertex in Vertices)
        {
            var center = Transform(vertex, layout);

            double dx = point.X - center.X;
            double dy = point.Y - center.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= VertexRadius)
            {
                if (properties.IsRightButtonPressed)
                    VertexRightClicked?.Invoke(vertex);
                else if (properties.IsLeftButtonPressed)
                    VertexClicked?.Invoke(vertex);

                break;
            }
        }
    }

    // Converts logical graph coordinates into screen coordinates.
    // This keeps the graph centered and scaled inside the canvas.
    private Point Transform(BaseVertex vertex, GraphLayout layout)
    {
        double x = (vertex.x - layout.MinX) * layout.Scale + layout.OffsetX;
        double y = (vertex.y - layout.MinY) * layout.Scale + layout.OffsetY;
        return new Point(x, y);
    }

    // Calculates scaling and offsets so that the complete graph fits
    // into the available drawing area while preserving proportions.
    private GraphLayout GetLayoutValues()
    {
        double minX = Vertices.Min(v => (double)v.x);
        double maxX = Vertices.Max(v => (double)v.x);
        double minY = Vertices.Min(v => (double)v.y);
        double maxY = Vertices.Max(v => (double)v.y);

        double graphWidth = Math.Max(1, maxX - minX);
        double graphHeight = Math.Max(1, maxY - minY);

        double availableWidth = Math.Max(1, Bounds.Width - 2 * Padding);
        double availableHeight = Math.Max(1, Bounds.Height - 2 * Padding);

        double scaleX = availableWidth / graphWidth;
        double scaleY = availableHeight / graphHeight;

        // Use the smaller scale so the graph fits fully inside the canvas
        double scale = Math.Min(scaleX, scaleY);

        double drawnWidth = graphWidth * scale;
        double drawnHeight = graphHeight * scale;

        // Center the scaled graph inside the control
        double offsetX = (Bounds.Width - drawnWidth) / 2;
        double offsetY = (Bounds.Height - drawnHeight) / 2;

        return new GraphLayout
        {
            MinX = minX,
            MinY = minY,
            Scale = scale,
            OffsetX = offsetX,
            OffsetY = offsetY
        };
    }

    // Helper class storing layout information for transforming graph coordinates.
    private class GraphLayout
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double Scale { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
    }
}