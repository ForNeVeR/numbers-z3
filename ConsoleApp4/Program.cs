using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Z3;

namespace ConsoleApp4
{
    static class Program
    {
        static void Main()
        {
            var verbose = true;
            using (var context = new Context())
            {
                const int minNumber = 1;
                const int numberCount = 9;
                const int result = 100;

                var operatorType = context.MkEnumSort("operator", "+", "-", "*"); // TODO: Add concat!
                var plus = operatorType.Consts[0];
                var minus = operatorType.Consts[1];
                var mult = operatorType.Consts[2];

                var operands = Enumerable.Range(minNumber, numberCount)
                    .Select(i => context.MkIntConst($"n{i}"))
                    .ToList();
                var operators = Enumerable.Range(minNumber, numberCount - 1)
                    .Select(i => context.MkConst($"op{i}", operatorType))
                    .ToList();

                var ten = context.MkInt(10);
                IntExpr ApplyOperator(IntExpr x, Expr @operator, IntExpr y) =>
                    (IntExpr)context.MkITE(context.MkEq(@operator, plus), context.MkAdd(x, y),
                        context.MkITE(context.MkEq(@operator, minus), context.MkSub(x, y),
                            context.MkMul(x, y))); // TODO: Add concat!

                var solver = context.MkSolver();

                foreach (var operand in operands)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"Assertion.1: {minNumber} <= {operand} < {minNumber + numberCount}");
                    }

                    solver.Assert(context.MkGe(operand, context.MkInt(minNumber)));
                    solver.Assert(context.MkLt(operand, context.MkInt(minNumber + numberCount)));
                }

                for (int i = 0; i < operands.Count; ++i)
                {
                    var operand1 = operands[i];
                    for (int j = 0; j < i; ++j)
                    {
                        var operand2 = operands[j];
                        if (verbose)
                        {
                            Console.WriteLine($"Assertion.2: {operand1} != {operand2}");
                        }

                        solver.Assert(context.MkNot(context.MkEq(operand1, operand2)));
                    }
                }

                var equation = ApplyOperator(operands[0], operators[0], operands[1]);
                for (var i = 2; i < operands.Count; ++i)
                {
                    equation = ApplyOperator(equation, operators[i - 1], operands[i]);
                }

                if (verbose)
                {
                    Console.WriteLine($"Assertion.3: {equation} = {result}");
                }

                solver.Assert(context.MkEq(equation, context.MkInt(result)));

                if (verbose)
                {
                    Console.WriteLine("Hit key when ready");
                    Console.ReadKey();
                }

                var step = 1;
                while (solver.Check() == Status.SATISFIABLE)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"## Step {step}");
                    }

                    var model = solver.Model;
                    if (verbose)
                    {
                        Console.WriteLine($"Current model: {model}");
                    }

                    var values = operators.Concat(operands).Select(o => (o, model.Eval(o, true))).ToList();

                    if (verbose)
                    {
                        Console.WriteLine("Current parameter values:");
                        values.ForEach(val => Console.WriteLine($"{val.Item1}: {val.Item2}"));
                    }

                    if (verbose)
                    {
                        Console.Write("Equation: ");
                    }

                    var tree = values.ToDictionary(v => v.Item1, v => v.Item2);
                    string Traverse(int i)
                    {
                        if (i == 0)
                        {
                            return tree[operands[0]].ToString();
                        }

                        var operand = operands[i];
                        var @operator = operators[i - 1];
                        var operatorName = tree[@operator].ToString();
                        return $"({Traverse(i - 1)}{operatorName}{tree[operand]})";
                    }

                    Console.WriteLine($"Solution {step}: {Traverse(numberCount - 1)}");

                    solver.Add(context.MkOr(
                        values.Select(val => context.MkNot(context.MkEq(val.Item1, val.Item2)))));

                    ++step;
                }
            }
        }
    }
}
