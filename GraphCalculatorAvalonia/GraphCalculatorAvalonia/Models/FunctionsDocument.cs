using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GraphCalculatorAvalonia.Models;

public sealed class FunctionsDocument
{
    [JsonRequired] public int Version { get; set; } = 1;
    [JsonRequired] public List<SavedFunction> Functions { get; set; } = new();
}

public sealed class SavedFunction
{
    [JsonRequired] public string Expression { get; set; } = string.Empty;
    [JsonRequired] public string Color { get; set; } = string.Empty;
    [JsonRequired] public bool IsVisible { get; set; } = true;
    [JsonRequired] public List<SavedParameter> Parameters { get; set; } = new();
}

public sealed class SavedParameter
{
    [JsonRequired] public string Name { get; set; } = string.Empty;
    [JsonRequired] public double Minimum { get; set; }
    [JsonRequired] public double Maximum { get; set; }
    [JsonRequired] public double Value { get; set; }
}
