using System;
using System.Collections.Generic;
using Avalonia;

namespace GraphCalculatorAvalonia.Services;

public readonly record struct CurveSegment(Point Start, Point End);


public static class CurveSampler
{
    private const double PixelTolerance = 0.5;
    private const int MaxDepth = 40;
    private const int MaxEvaluationsPerInterval = 512;

    public static IReadOnlyList<CurveSegment> Sample(Func<double, double> evaluate,
        Size size, Point center, double pixelsPerUnit)
    {
        var segments = new List<CurveSegment>();
        if (!double.IsFinite(size.Width) || !double.IsFinite(size.Height) ||
            size.Width <= 0 || size.Height <= 0 ||
            !double.IsFinite(pixelsPerUnit) || pixelsPerUnit <= 0 ||
            !double.IsFinite(center.X) || !double.IsFinite(center.Y))
            return segments;

        Point Evaluate(double screenX)
        {
            double x = center.X + (screenX - size.Width / 2) / pixelsPerUnit;
            return new Point(screenX, size.Height / 2 - (evaluate(x) - center.Y) * pixelsPerUnit);
        }

        var stack = new Stack<(Point Left, Point Right, int Depth)>();
        Point left = Evaluate(0);
        for (double screenX = 0; screenX < size.Width; screenX += 1)
        {
            Point right = Evaluate(Math.Min(screenX + 1, size.Width));
            stack.Push((left, right, 0));
            int evaluations = 0;
            while (stack.Count > 0 && evaluations < MaxEvaluationsPerInterval)
            {
                var (start, end, depth) = stack.Pop();
                double middleX = start.X + (end.X - start.X) / 2;
                
                double worldStart = center.X + (start.X - size.Width / 2) / pixelsPerUnit;
                double worldMiddle = center.X + (middleX - size.Width / 2) / pixelsPerUnit;
                double worldEnd = center.X + (end.X - size.Width / 2) / pixelsPerUnit;
                Point middle = Evaluate(middleX);
                evaluations++;

                bool startFinite = double.IsFinite(start.Y);
                bool middleFinite = double.IsFinite(middle.Y);
                bool endFinite = double.IsFinite(end.Y);
                if (!startFinite && !middleFinite && !endFinite)
                    continue;

                if (startFinite && middleFinite && endFinite)
                {
                    if ((start.Y < 0 && middle.Y < 0 && end.Y < 0) ||
                        (start.Y > size.Height && middle.Y > size.Height && end.Y > size.Height))
                        continue;

                    double expectedY = start.Y / 2 + end.Y / 2;
                    bool middleBetweenEndpoints = middle.Y >= Math.Min(start.Y, end.Y) &&
                                                  middle.Y <= Math.Max(start.Y, end.Y);
                    if (middleBetweenEndpoints && Math.Abs(middle.Y - expectedY) <= PixelTolerance)
                    {
                        segments.Add(new CurveSegment(ClipEndpoint(start, end, size.Height),
                            ClipEndpoint(end, start, size.Height)));
                        continue;
                    }
                }

                if (depth >= MaxDepth ||
                    middleX == start.X || middleX == end.X ||
                    worldMiddle == worldStart || worldMiddle == worldEnd)
                    continue;

                stack.Push((middle, end, depth + 1));
                stack.Push((start, middle, depth + 1));
            }
            stack.Clear();
            left = right;
        }
        return segments;
    }

    private static Point ClipEndpoint(Point point, Point other, double height)
    {
        if (point.Y >= 0 && point.Y <= height)
            return point;

        double boundary = point.Y < 0 ? 0 : height;
        double scale = Math.Max(1, Math.Max(Math.Abs(point.Y), Math.Abs(other.Y)));
        double fraction = (boundary / scale - point.Y / scale) /
                          (other.Y / scale - point.Y / scale);
        return new Point(point.X + (other.X - point.X) * fraction, boundary);
    }
}
