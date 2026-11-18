using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;
using GraphCalculatorAvalonia.Models;
using GraphCalculatorAvalonia.Styles;

namespace GraphCalculatorAvalonia.ViewModels;

public partial class FunctionViewModel : ViewModelBase
{
    [ObservableProperty]
    private string expression = string.Empty;

    [ObservableProperty] 
    private bool isVisible = true;
    
    [ObservableProperty]
    private Color graphColor = GraphPalette.Blue;
    
    [ObservableProperty]
    private GraphFunction? function;

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
}
