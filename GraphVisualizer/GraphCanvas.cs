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

public class GraphCanvas : Control
{
    public List<BaseVertex> Vertices { get; set; } = new();
    public List<BaseEdge> Edges { get; set; } = new();

    public const double VertexRadius = 10;
    private const double Padding = 60;

    public event Action<BaseVertex>? VertexClicked;
    public event Action<BaseVertex>? VertexRightClicked;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Vertices == null || Vertices.Count == 0)
            return;

        var layout = GetLayoutValues();

        // draw edges first
        foreach (var edge in Edges)
        {
            if (edge.source == null || edge.target == null)
                continue;

            var start = Transform(edge.source, layout);
            var end = Transform(edge.target, layout);

            Pen pen = new Pen(Brushes.LightGray, 1.5);

            if (edge.IsActive)
                pen = new Pen(Brushes.Yellow, 4);
            else if (edge.IsPath)
                pen = new Pen(Brushes.Gold, 3);

            context.DrawLine(pen, start, end);
        }

        // draw vertices on top
        foreach (var vertex in Vertices)
        {
            IBrush fill = Brushes.SteelBlue;

            if (vertex.IsWall)
                fill = Brushes.Red;
            else if (vertex.IsStart)
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

            var text = new FormattedText(
                vertex.Name,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.Gold
            );

            context.DrawText(text, new Point(center.X + 12, center.Y - 8));
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (Vertices == null || Vertices.Count == 0)
            return;

        var point = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;
        var layout = GetLayoutValues();

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

    private Point Transform(BaseVertex vertex, GraphLayout layout)
    {
        double x = (vertex.x - layout.MinX) * layout.Scale + layout.OffsetX;
        double y = (vertex.y - layout.MinY) * layout.Scale + layout.OffsetY;
        return new Point(x, y);
    }

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
        double scale = Math.Min(scaleX, scaleY);

        double drawnWidth = graphWidth * scale;
        double drawnHeight = graphHeight * scale;

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

    private class GraphLayout
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double Scale { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
    }
}