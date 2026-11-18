using System;

namespace GraphCalculatorAvalonia.Models;

public class GraphFunction
{
    private readonly Func<double, double> _function;

    public GraphFunction(Func<double, double> function)
    {
        _function = function;
    }
    
    public double Evaluate(double x) => _function(x);
}