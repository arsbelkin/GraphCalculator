using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;
using GraphCalculatorAvalonia.Models;
using GraphCalculatorAvalonia.Services;
using GraphCalculatorAvalonia.Styles;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace GraphCalculatorAvalonia.ViewModels;

public partial class FunctionViewModel : ViewModelBase
{
    [ObservableProperty] private string expression = string.Empty;

    [ObservableProperty] private bool isVisible = true;

    [ObservableProperty] private Color graphColor = GraphPalette.Blue;

    [ObservableProperty] private FunctionModel? function;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ObservableCollection<FunctionParameterModelView> Parameters { get; } = new();
    public bool HasParameters => Parameters.Count > 0;

    [RelayCommand]
    private void SetColor(Color color)
    {
        GraphColor = color;
    }

    [RelayCommand]
    private void ChangeVisibility()
    {
        IsVisible = !IsVisible;
    }

    partial void OnExpressionChanged(string value)
    {
        var values = Parameters.ToDictionary(parameter => parameter.Title, parameter => parameter.Value);
        string? error = null;
        var parsed = string.IsNullOrWhiteSpace(value) ? null : ExpressionParser.Parse(value, values, out error);
        ErrorMessage = error;

        if (parsed is not null || string.IsNullOrWhiteSpace(value))
        {
            var names = parsed?.ParameterNames ?? System.Array.Empty<string>();
            foreach (var parameter in Parameters.ToArray())
            {
                if (names.Contains(parameter.Title))
                    continue;
                parameter.PropertyChanged -= OnParameterChanged;
                Parameters.Remove(parameter);
            }

            foreach (string name in names)
            {
                if (Parameters.Any(parameter => parameter.Title == name))
                    continue;
                var parameter = new FunctionParameterModelView { Title = name };
                parameter.PropertyChanged += OnParameterChanged;
                Parameters.Add(parameter);
            }
            OnPropertyChanged(nameof(HasParameters));
        }

        Function = parsed;
    }

    private void OnParameterChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FunctionParameterModelView.Value))
        {
            Function = Function?.WithParameters(Parameters.ToDictionary(
                parameter => parameter.Title, parameter => parameter.Value));
        }
    }
}
