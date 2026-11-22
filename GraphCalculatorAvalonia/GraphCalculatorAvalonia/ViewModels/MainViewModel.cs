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

    public void ReplaceFunctions(System.Collections.Generic.IEnumerable<FunctionViewModel> functions)
    {
        var restored = new System.Collections.Generic.List<FunctionViewModel>(functions);
        Functions.Clear();
        foreach (var function in restored)
            Functions.Add(function);
        _nextColorIndex = Functions.Count % GraphPalette.PaletteColors.Count;
    }
}
