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
            using (var context = new Context())
            {
                const int minNumber = 1;
                const int numberCount = 6;
                const int result = 35;


                var operatorType = context.MkEnumSort("operator", "Plus", "Minus", "Mult", "Div");
                var plus = operatorType.Consts[0];
                var minus = operatorType.Consts[1];
                var mult = operatorType.Consts[2];

                var operands = Enumerable.Range(minNumber, numberCount)
                    .Select(i => context.MkInt(i))
                    .ToList();
                var operators = Enumerable.Range(minNumber, numberCount - 1)
                    .Select(i => context.MkConst($"op{i}", operatorType))
                    .ToList();

                IntExpr applyOperator(IntExpr x, Expr @operator, IntExpr y) =>
                    (IntExpr)context.MkITE(context.MkEq(@operator, plus), context.MkAdd(x, y),
                        context.MkITE(context.MkEq(@operator, minus), context.MkSub(x, y),
                            context.MkITE(context.MkEq(@operator, mult), context.MkMul(x, y),
                                context.MkDiv(x, y))));

                var solver = context.MkSolver();

                var equation = applyOperator(operands[0], operators[0], operands[1]);
                for (var i = 2; i < operands.Count; ++i)
                {
                    equation = applyOperator(equation, operators[i - 1], operands[i]);
                }

                solver.Assert(context.MkEq(equation, context.MkInt(result)));

                var solutions = new List<Expr[]>();
                var step = 1;
                while (solver.Check() == Status.SATISFIABLE)
                {
                    Console.WriteLine($"## Step {step}");

                    var model = solver.Model;
                    Console.WriteLine($"Current model: {model}");

                    var values = operators.Select(o => (o, model.Eval(o, true))).ToList();
                    solutions.Add(values.Select(v => v.Item2).ToArray());

                    Console.WriteLine("Current operator values:");
                    values.ForEach(val => Console.WriteLine($"{val.Item1}: {val.Item2}"));

                    solver.Add(context.MkOr(
                        values.Select(val => context.MkNot(context.MkEq(val.Item1, val.Item2)))));

                    ++step;
                }

                Console.WriteLine("# Solutions");
                foreach (var solution in solutions)
                {
                    Console.WriteLine(string.Join(" ", solution.AsEnumerable()));
                }
            }
        }
    }
}
