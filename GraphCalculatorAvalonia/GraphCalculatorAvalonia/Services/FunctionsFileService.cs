using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Media;
using GraphCalculatorAvalonia.Models;
using GraphCalculatorAvalonia.ViewModels;

namespace GraphCalculatorAvalonia.Services;

public static class FunctionsFileService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static string Serialize(IEnumerable<FunctionViewModel> functions)
    {
        var document = new FunctionsDocument
        {
            Functions = functions.Select(function => new SavedFunction
            {
                Expression = function.Expression,
                Color = function.GraphColor.ToString(),
                IsVisible = function.IsVisible,
                Parameters = function.Parameters.Select(parameter => new SavedParameter
                {
                    Name = parameter.Title,
                    Minimum = parameter.MinValue,
                    Maximum = parameter.MaxValue,
                    Value = parameter.Value,
                }).ToList(),
            }).ToList(),
        };
        return JsonSerializer.Serialize(document, Options);
    }

    public static IReadOnlyList<FunctionViewModel> Deserialize(string json)
    {
        var document = JsonSerializer.Deserialize<FunctionsDocument>(json, Options)
            ?? throw new InvalidDataException("Файл не содержит списка функций.");
        if (document.Version != 1)
            throw new InvalidDataException("Эта версия формата файла пока не поддерживается.");
        if (document.Functions is null)
            throw new InvalidDataException("В файле отсутствует список функций.");

        var restored = new List<FunctionViewModel>();
        foreach (var saved in document.Functions)
        {
            if (saved is null || saved.Expression is null || saved.Parameters is null ||
                !Color.TryParse(saved.Color, out var color))
                throw new InvalidDataException("Некорректные данные функции или её цвета.");

            var function = new FunctionViewModel
            {
                Expression = saved.Expression,
                GraphColor = color,
                IsVisible = saved.IsVisible,
            };

            
            foreach (var parameter in saved.Parameters)
            {
                if (parameter is null || string.IsNullOrWhiteSpace(parameter.Name) ||
                    !double.IsFinite(parameter.Minimum) || !double.IsFinite(parameter.Maximum) ||
                    !double.IsFinite(parameter.Value) || parameter.Minimum > parameter.Maximum ||
                    parameter.Value < parameter.Minimum || parameter.Value > parameter.Maximum)
                    throw new InvalidDataException("Некорректное значение или границы параметра.");
            }
            if (saved.Parameters.Select(parameter => parameter.Name).Distinct(StringComparer.Ordinal).Count()
                != saved.Parameters.Count || saved.Parameters.Count > ExpressionParser.MaxParameterCount)
                throw new InvalidDataException("Список параметров содержит повторы или превышает ограничение.");

            if (!function.HasError && !function.Parameters.Select(parameter => parameter.Title)
                    .ToHashSet(StringComparer.Ordinal)
                    .SetEquals(saved.Parameters.Select(parameter => parameter.Name)))
                throw new InvalidDataException("Параметры файла не соответствуют выражению функции.");

            function.RestoreParameters(saved.Parameters);
            restored.Add(function);
        }
        return restored;
    }
}
