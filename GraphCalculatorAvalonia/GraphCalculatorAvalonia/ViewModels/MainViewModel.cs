using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using GraphCalculatorAvalonia.Styles;

namespace GraphCalculatorAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private int _nextColorIndex;

    public ObservableCollection<FunctionViewModel> Functions { get; } = new();

    public MainViewModel()
    {
        AddFunction();
    }

    [RelayCommand]
    private void AddFunction()
    {
        Functions.Add(new FunctionViewModel
        {
            GraphColor = GraphPalette.PaletteColors[_nextColorIndex],
        });
        _nextColorIndex = (_nextColorIndex + 1) % GraphPalette.PaletteColors.Count;
    }
    
    [RelayCommand]
    private void DeleteFunction(FunctionViewModel function)
    {
        Functions.Remove(function);
    }
}
