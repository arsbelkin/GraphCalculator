using CommunityToolkit.Mvvm.ComponentModel;

namespace GraphCalculatorAvalonia.ViewModels;

public class FunctionParameterModelView : ViewModelBase
{
    public string Title { get; init; } = string.Empty;

    private double minValue = -10;
    private double maxValue = 10;
    private double value = 1;

    public double MinValue
    {
        get => minValue;
        set
        {
            if (double.IsFinite(value) && SetProperty(ref minValue, System.Math.Min(value, MaxValue)))
                Value = System.Math.Clamp(Value, MinValue, MaxValue);
        }
    }

    public double MaxValue
    {
        get => maxValue;
        set
        {
            if (double.IsFinite(value) && SetProperty(ref maxValue, System.Math.Max(value, MinValue)))
                Value = System.Math.Clamp(Value, MinValue, MaxValue);
        }
    }

    public double Value
    {
        get => value;
        set
        {
            if (double.IsFinite(value))
                SetProperty(ref this.value, System.Math.Clamp(value, MinValue, MaxValue));
        }
    }
}
