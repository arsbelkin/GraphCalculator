using Avalonia.Media;

using System;
using System.Collections.Generic;

namespace GraphCalculatorAvalonia.Styles;

public static class GraphPalette
{
    public static readonly Color Blue = Color.Parse("#2563EB");
    public static readonly Color Red = Color.Parse("#DC2626");
    public static readonly Color Green = Color.Parse("#15803D");
    public static readonly Color Purple = Color.Parse("#9333EA");
    public static readonly Color Black = Color.Parse("#000000");

    public static IReadOnlyList<Color> PaletteColors { get; } =
        Array.AsReadOnly(new[] { Blue, Red, Green, Purple, Black });
}
