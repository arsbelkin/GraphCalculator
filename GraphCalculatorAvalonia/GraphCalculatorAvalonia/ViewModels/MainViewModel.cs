using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using GraphCalculatorAvalonia.Models;
using GraphCalculatorAvalonia.Styles;

namespace GraphCalculatorAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private int nextColorIndex;

    public ObservableCollection<FunctionViewModel> Functions { get; } = new();

    public MainViewModel()
    {
        Functions.Add(new FunctionViewModel
        {
            Expression = "x^2",
            Function = new GraphFunction(x => x * x),
            GraphColor = GraphPalette.Blue,
        });

        Functions.Add(new FunctionViewModel
        {
            Expression = "sin(x)",
            Function = new GraphFunction(Math.Sin),
            GraphColor = GraphPalette.Red,
        });

        nextColorIndex = Functions.Count % GraphPalette.PaletteColors.Count;
    }

    [RelayCommand]
    private void AddFunction()
    {
        Functions.Add(new FunctionViewModel
        {
            GraphColor = GraphPalette.PaletteColors[nextColorIndex],
        });
        nextColorIndex = (nextColorIndex + 1) % GraphPalette.PaletteColors.Count;
    }
    
    [RelayCommand]
    private void DeleteFunction(FunctionViewModel function)
    {
        Functions.Remove(function);
    }
}
