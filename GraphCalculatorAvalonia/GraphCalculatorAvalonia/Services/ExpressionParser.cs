using System;
using GraphCalculatorAvalonia.Models;
using MathNet.Symbolics;
using System.Collections.Generic;
using System.Linq;
using LinqExpression = System.Linq.Expressions.Expression;

namespace GraphCalculatorAvalonia.Services;

public static class ExpressionParser
{
    public const int MaxParameterCount = 3;

    public static FunctionModel? Parse(string text, IReadOnlyDictionary<string, double>? parameterValues = null) =>
        Parse(text, parameterValues, out _);

    public static FunctionModel? Parse(string text, IReadOnlyDictionary<string, double>? parameterValues,
        out string? errorMessage)
    {
        errorMessage = null;
        try
        {
            var expression = SymbolicExpression.Parse(text);
            string[] parameterNames = expression.CollectVariables()
                .Select(variable => variable.VariableName)
                .Where(name => name != "x")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            if (parameterNames.Any(pn => pn.Length != 1 ||
                                         !(pn[0] is >= 'a' and <= 'z' or >= 'A' and <= 'Z')))
            {
                errorMessage = "Параметр должен быть одной латинской буквой.";
                return null;
            }

            if (parameterNames.Length > MaxParameterCount)
            {
                errorMessage = "Можно использовать не более трёх параметров, не считая x.";
                return null;
            }

            if (parameterNames.Length == 0)
                return new FunctionModel(expression.Compile("x"));

            string[] arguments = new[] { "x" }.Concat(parameterNames).ToArray();
            Delegate compiled = expression.Compile(arguments);
            var x = LinqExpression.Parameter(typeof(double), "x");
            var values = LinqExpression.Parameter(typeof(double[]), "values");
            var inputs = new List<System.Linq.Expressions.Expression> { x };
            for (int index = 0; index < parameterNames.Length; index++)
                inputs.Add(LinqExpression.ArrayIndex(values, LinqExpression.Constant(index)));

            var invoke = LinqExpression.Invoke(LinqExpression.Constant(compiled), inputs);
            var evaluate = LinqExpression.Lambda<Func<double, double[], double>>(invoke, x, values).Compile();
            return new FunctionModel(evaluate, parameterNames, parameterValues);
        }
        catch (Exception)
        {
            errorMessage = "Не удалось разобрать выражение. Проверьте запись формулы.";
            return null;
        }
    }
}
