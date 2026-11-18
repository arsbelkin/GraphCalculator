using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GraphCalculatorAvalonia.Models;
using GraphCalculatorAvalonia.Styles;

namespace GraphCalculatorAvalonia.Controls;

public class GraphView : Control
{
    private const double DefaultScale = 60;
    private const double MinScale = 0.01;
    private const double MaxScale = 10000;

    private static readonly Pen MinorGridPen = new(Brush.Parse("#EFF3F8"));
    private static readonly Pen MajorGridPen = new(Brush.Parse("#DCE4EF"));
    private static readonly Pen AxisPen = new(Brush.Parse("#64748B"), 1.5);
    private static readonly IBrush LabelBrush = Brush.Parse("#64748B");
    private static readonly Typeface LabelTypeface = new("Inter");

    private Point center;
    private double pixelsPerUnit = DefaultScale;
    private Point lastPointerPosition;
    private IPointer? dragPointer;

    private readonly List<(GraphFunction Function, Color Color)> _graphData = new()
    {
        (new GraphFunction(x => x * x), GraphPalette.Blue),
        (new GraphFunction(x => Math.Sin(x)), GraphPalette.Red),
        (new GraphFunction(x => x), GraphPalette.Green),
        (new GraphFunction(x => 1 / x), GraphPalette.Purple)
    };

    static GraphView()
    {
        AffectsRender<GraphView>(BoundsProperty);
    }

    public GraphView()
    {
        ClipToBounds = true;
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Cross);
    }

    public Point WorldToScreen(Point point) => new(
        Bounds.Width / 2 + (point.X - center.X) * pixelsPerUnit,
        Bounds.Height / 2 - (point.Y - center.Y) * pixelsPerUnit);

    public Point ScreenToWorld(Point point) => new(
        center.X + (point.X - Bounds.Width / 2) / pixelsPerUnit,
        center.Y - (point.Y - Bounds.Height / 2) / pixelsPerUnit);

    public void ResetView()
    {
        center = default;
        pixelsPerUnit = DefaultScale;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var area = new Rect(Bounds.Size);
        context.DrawRectangle(Brushes.White, null, area);
        if (area.Width <= 0 || area.Height <= 0)
            return;

        double majorStep = GetGridStep(80 / pixelsPerUnit);
        DrawGrid(context, majorStep / 5, MinorGridPen, false);
        DrawGrid(context, majorStep, MajorGridPen, true);

        Point origin = WorldToScreen(default);
        if (origin.Y >= 0 && origin.Y <= area.Height)
            context.DrawLine(AxisPen, new Point(0, origin.Y), new Point(area.Width, origin.Y));
        if (origin.X >= 0 && origin.X <= area.Width)
            context.DrawLine(AxisPen, new Point(origin.X, 0), new Point(origin.X, area.Height));

        if (area.Contains(origin))
            DrawLabel(context, "0", origin.X + 6, origin.Y + 6);

        foreach (var (function, color) in _graphData)
        {
            var pen = new Pen(new SolidColorBrush(color), 2);
            DrawFunc(context, 1 / pixelsPerUnit, pen, function);
        }
    }

    private void DrawGrid(DrawingContext context, double step, Pen pen, bool showLabels)
    {
        Point topLeft = ScreenToWorld(default);
        Point bottomRight = ScreenToWorld(new Point(Bounds.Width, Bounds.Height));
        Point origin = WorldToScreen(default);

        double firstX = Math.Ceiling(topLeft.X / step);
        double lastX = Math.Floor(bottomRight.X / step);
        for (double index = firstX; index <= lastX; index++)
        {
            double value = index * step;
            double x = WorldToScreen(new Point(value, 0)).X;
            context.DrawLine(pen, new Point(x, 0), new Point(x, Bounds.Height));
            if (showLabels && index != 0)
                DrawLabel(context, FormatTick(value), x + 4, origin.Y + 6);
        }

        double firstY = Math.Ceiling(bottomRight.Y / step);
        double lastY = Math.Floor(topLeft.Y / step);
        for (double index = firstY; index <= lastY; index++)
        {
            double value = index * step;
            double y = WorldToScreen(new Point(0, value)).Y;
            context.DrawLine(pen, new Point(0, y), new Point(Bounds.Width, y));
            if (showLabels && index != 0)
                DrawLabel(context, FormatTick(value), origin.X + 6, y + 4);
        }
    }

    private void DrawLabel(DrawingContext context, string value, double x, double y)
    {
        var text = new FormattedText(value, CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight, LabelTypeface, 11, LabelBrush);
        x = Math.Clamp(x, 4, Math.Max(4, Bounds.Width - text.Width - 4));
        y = Math.Clamp(y, 4, Math.Max(4, Bounds.Height - text.Height - 4));
        var background = new Rect(x - 2, y, text.Width + 4, text.Height);
        context.DrawRectangle(Brushes.White, null, background);
        context.DrawText(text, new Point(x, y));
    }

    private static double GetGridStep(double desiredStep)
    {
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(desiredStep)));
        double fraction = desiredStep / magnitude;
        double multiplier = fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10;
        return multiplier * magnitude;
    }

    private static string FormatTick(double value) =>
        value.ToString("G6", CultureInfo.CurrentCulture);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        Focus();
        if (e.ClickCount == 2)
        {
            ResetView();
        }
        else
        {
            dragPointer = e.Pointer;
            lastPointerPosition = e.GetPosition(this);
            e.Pointer.Capture(this);
            Cursor = new Cursor(StandardCursorType.SizeAll);
        }

        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (dragPointer != e.Pointer)
            return;

        Point position = e.GetPosition(this);
        Vector delta = position - lastPointerPosition;
        center = new Point(center.X - delta.X / pixelsPerUnit, center.Y + delta.Y / pixelsPerUnit);
        lastPointerPosition = position;
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (dragPointer == e.Pointer && e.InitialPressMouseButton == MouseButton.Left)
        {
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        dragPointer = null;
        Cursor = new Cursor(StandardCursorType.Cross);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        Point pointer = e.GetPosition(this);
        Point anchor = ScreenToWorld(pointer);
        pixelsPerUnit = Math.Clamp(pixelsPerUnit * Math.Pow(1.15, e.Delta.Y), MinScale, MaxScale);
        Point afterZoom = ScreenToWorld(pointer);
        center = new Point(center.X + anchor.X - afterZoom.X, center.Y + anchor.Y - afterZoom.Y);
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Home)
        {
            ResetView();
            e.Handled = true;
        }
    }

    private void DrawFunc(DrawingContext context, double step, Pen pen, GraphFunction function)
    {
        Point topLeft = ScreenToWorld(default);
        Point bottomRight = ScreenToWorld(new Point(Bounds.Width, Bounds.Height));

        double firstX = Math.Floor(topLeft.X / step);
        double lastX = Math.Ceiling(bottomRight.X / step);

        for (double index = firstX; index < lastX; index++)
        {
            double x = index * step;
            double nextX = (index + 1) * step;
            
            double y = function.Evaluate(x);
            double nextY = function.Evaluate(nextX);
            if (!double.IsFinite(y) || !double.IsFinite(nextY))
                continue;

            Point current = WorldToScreen(new Point(x, y));
            Point next = WorldToScreen(new Point(nextX, nextY));
            if (!double.IsFinite(current.X) || !double.IsFinite(current.Y) ||
                !double.IsFinite(next.X) || !double.IsFinite(next.Y))
                continue;
            
            context.DrawLine(pen, current, next);
        }
    }
}
