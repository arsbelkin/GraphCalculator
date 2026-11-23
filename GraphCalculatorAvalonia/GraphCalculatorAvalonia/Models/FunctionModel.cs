using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphCalculatorAvalonia.Models;

public class FunctionModel
{
    private readonly Func<double, double> _function;
    private readonly Func<double, double[], double>? _parameterizedFunction;

    public IReadOnlyList<string> ParameterNames { get; } = Array.Empty<string>();

    public FunctionModel(Func<double, double> function)
    {
        _function = function;
    }

    public FunctionModel(Func<double, double[], double> function, string[] parameterNames,
        IReadOnlyDictionary<string, double>? parameterValues = null)
    {
        _parameterizedFunction = function;
        ParameterNames = Array.AsReadOnly((string[])parameterNames.Clone());

        double[] values = ParameterNames.Select(name =>
            parameterValues is not null 
            && parameterValues.TryGetValue(name, out var value) ? value : 1).ToArray();
        
        _function = x => function(x, values);
    }

    public FunctionModel WithParameters(IReadOnlyDictionary<string, double> values) =>
        _parameterizedFunction is null
            ? this
            : new FunctionModel(_parameterizedFunction, ParameterNames.ToArray(), values);

    public double Evaluate(double x) => _function(x);
}