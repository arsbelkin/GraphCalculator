using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using GraphCalculatorAvalonia.Styles;
using Avalonia.Threading;
using System.Diagnostics;
using System.Linq;

namespace GraphCalculatorAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private int _nextColorIndex;

    public ObservableCollection<FunctionViewModel> Functions { get; } = new();

    private DispatcherTimer? _timer;
    private readonly HashSet<FunctionViewModel> _observedFunctions = new();

    public MainViewModel()
    {
        Functions.CollectionChanged += OnFunctionsChanged;
        AddFunction();
    }

    private void OnFunctionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var function in _observedFunctions)
            function.Parameters.CollectionChanged -= OnParametersChanged;
        _observedFunctions.Clear();

        foreach (var function in Functions)
        {
            if (_observedFunctions.Add(function))
                function.Parameters.CollectionChanged += OnParametersChanged;
        }
        NotifyAnimationAvailabilityChanged();
    }

    private void OnParametersChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        NotifyAnimationAvailabilityChanged();

    private void NotifyAnimationAvailabilityChanged()
    {
        OnPropertyChanged(nameof(CanAnimate));
        ProcessAnimationCommand.NotifyCanExecuteChanged();
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

    public void ReplaceFunctions(IEnumerable<FunctionViewModel> functions)
    {
        var restored = new List<FunctionViewModel>(functions);
        Functions.Clear();
        foreach (var function in restored)
            Functions.Add(function);
        _nextColorIndex = Functions.Count % GraphPalette.PaletteColors.Count;
    }

    private void UpdateAnimationProgress(double progress, bool saveValue = false)
    {
        foreach (var function in Functions)
        {
            foreach (var parameter in function.Parameters)
            {
                if (saveValue)
                    parameter.SaveValue();
                
                parameter.OnProgressChanged(progress);
            }
        }
    }
    
    private void RestoreOldParameterValue()
    {
        foreach (var function in Functions)
        {
            foreach (var parameter in function.Parameters)
            {
                parameter.RestoreValue();
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanProcessAnimation))]
    private void ProcessAnimation()
    {
        if (IsAnimating)
        {
            StopAnimation();
            return;
        }

        if (!CanAnimate)
            return;

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(20)
        };
        _timer = timer;
        OnPropertyChanged(nameof(IsAnimating));
        ProcessAnimationCommand.NotifyCanExecuteChanged();
        
        UpdateAnimationProgress(0.0, true);
        var stopWatch = Stopwatch.StartNew();
        TimeSpan? maxTime = null;

        timer.Tick += (_, _) =>
        {
            var elapsed = stopWatch.Elapsed;
            
            double progress = Math.Min(1.0,
                elapsed.TotalSeconds / 5
            );
            UpdateAnimationProgress(progress);

            if (progress < 1)
                return;
            
            maxTime ??= elapsed;

            if (elapsed - maxTime.Value >= TimeSpan.FromMilliseconds(200))
            {
                stopWatch.Stop();
                StopAnimation();
            }
        };

        timer.Start();
    }

    public void StopAnimation()
    {
        if (_timer is null)
            return;
        
        _timer?.Stop();
        _timer = null;
        OnPropertyChanged(nameof(IsAnimating));
        ProcessAnimationCommand.NotifyCanExecuteChanged();
        
        RestoreOldParameterValue();
    }

    public bool IsAnimating => _timer is not null;
    public bool CanAnimate => Functions.Any(f => f.HasParameters);
    private bool CanProcessAnimation() => IsAnimating || CanAnimate;
}
