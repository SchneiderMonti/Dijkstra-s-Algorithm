using System;
using System.Collections.Generic;
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
    public const double Scale = 6;

    public event Action<BaseVertex>? VertexClicked;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // draw edges first
        foreach (var edge in Edges)
        {
            if (edge.source == null || edge.target == null)
                continue;

            var start = new Point(edge.source.x * Scale + 40, edge.source.y * Scale + 40);
            var end = new Point(edge.target.x * Scale + 40, edge.target.y * Scale + 40);

            Pen pen = new Pen(Brushes.LightGray, 1.5);

            if (edge.IsActive)
            {
                pen = new Pen(Brushes.Yellow, 4);
            }
            else if (edge.IsPath)
            {
                pen = new Pen(Brushes.Red, 3);
            }

            context.DrawLine(pen, start, end);
        }

        // draw vertices on top
        foreach (var vertex in Vertices)
        {
            IBrush fill = Brushes.SteelBlue;

            if (vertex.IsStart)
                fill = Brushes.OrangeRed;
            else if (vertex.IsTarget)
                fill = Brushes.LimeGreen;
            else if (vertex.IsPath)
                fill = Brushes.Gold;
            else if (vertex.IsVisited)
                fill = Brushes.LightBlue;

            var centerX = vertex.x * Scale + 40;
            var centerY = vertex.y * Scale + 40;

            context.DrawEllipse(
                fill,
                new Pen(Brushes.Black, 1.5),
                new Point(centerX, centerY),
                VertexRadius,
                VertexRadius
            );

            var text = new FormattedText(
                vertex.Name,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.White
            );

            context.DrawText(text, new Point(centerX - 10, centerY - 25));
        }
    }
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetPosition(this);

        foreach (var vertex in Vertices)
        {
            double centerX = vertex.x * Scale + 40;
            double centerY = vertex.y * Scale + 40;

            double dx = point.X - centerX;
            double dy = point.Y - centerY;

            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= VertexRadius)
            {
                VertexClicked?.Invoke(vertex);
                break;
            }
        }
    }
}