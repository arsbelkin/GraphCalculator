namespace GraphCalculatorAvalonia.ViewModels;

public class FunctionParameterModelView : ViewModelBase
{
    public string Title { get; init; } = string.Empty;

    private double _minValue = -10;
    private double _maxValue = 10;
    private double _value = 1;

    public double MinValue
    {
        get => _minValue;
        set
        {
            if (double.IsFinite(value) && SetProperty(ref _minValue, System.Math.Min(value, MaxValue)))
                Value = System.Math.Clamp(Value, MinValue, MaxValue);
        }
    }

    public double MaxValue
    {
        get => _maxValue;
        set
        {
            if (double.IsFinite(value) && SetProperty(ref _maxValue, System.Math.Max(value, MinValue)))
                Value = System.Math.Clamp(Value, MinValue, MaxValue);
        }
    }

    public double Value
    {
        get => _value;
        set
        {
            if (double.IsFinite(value))
                SetProperty(ref this._value, System.Math.Clamp(value, MinValue, MaxValue));
        }
    }
}
