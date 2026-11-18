using System;
using GraphCalculatorAvalonia.Models;
using MathNet.Symbolics;

namespace GraphCalculatorAvalonia.Services;

public static class ExpressionParser
{
    public static GraphFunction? Parse(string text)
    {
        try
        {
            var expression = SymbolicExpression.Parse(text);
            var evaluate = expression.Compile("x");

            return new GraphFunction(evaluate);
        }
        catch (Exception e)
        {
            return null;
        }
    }
}