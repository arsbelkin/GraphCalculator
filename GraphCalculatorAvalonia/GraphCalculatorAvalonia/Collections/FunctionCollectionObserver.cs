using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using GraphCalculatorAvalonia.ViewModels;

namespace GraphCalculatorAvalonia.Collections;

public class FunctionCollectionObserver : IDisposable
{
    private ObservableCollection<FunctionViewModel>? _functions;
    private readonly Action _onChanged;
    private readonly HashSet<FunctionViewModel> _subFunctions = new HashSet<FunctionViewModel>();

    public FunctionCollectionObserver(
        ObservableCollection<FunctionViewModel>? functions,
        Action onChanged
    )
    {
        _onChanged = onChanged;
        ObserveFunctions(functions);
    }

    public void SetSource(ObservableCollection<FunctionViewModel>? functions)
    {
        ObserveFunctions(functions);
    }

    private void ObserveFunctions(ObservableCollection<FunctionViewModel>? functions)
    {
        _functions?.CollectionChanged -= OnFunctionsCollectionChanged;

        foreach (var item in _subFunctions)
        {
            item.PropertyChanged -= OnFunctionPropertyChanged;
        }
        _subFunctions.Clear();
        
        _functions = functions;
        if (_functions is null)
            return;

        _functions.CollectionChanged += OnFunctionsCollectionChanged;
        foreach (var item in _functions)
        {
            if (_subFunctions.Add(item))
                item.PropertyChanged += OnFunctionPropertyChanged;
        }
    }
    
    private void OnFunctionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ObserveFunctions(_functions);
        _onChanged();
    }
    
    private void OnFunctionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) ||
            e.PropertyName is nameof(FunctionViewModel.Function)
                or nameof(FunctionViewModel.GraphColor)
                or nameof(FunctionViewModel.IsVisible))
        {
            _onChanged();
        }
    }

    public void Dispose()
    {
        ObserveFunctions(null);
    }
}