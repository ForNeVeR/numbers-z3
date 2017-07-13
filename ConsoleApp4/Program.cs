using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Z3;

namespace ConsoleApp4
{
    static class Program2
    {
        static void Main(string[] args)
        {
            using (var context = new Context())
            {
                var @operator = context.MkEnumSort("operator", "Plus", "Minus", "Mult", "Div");
                var plus = @operator.Consts[0];
                var minus = @operator.Consts[1];
                var mult = @operator.Consts[2];

                IntExpr myf(IntExpr x, Expr z, IntExpr y) =>
                    (IntExpr)context.MkITE(context.MkEq(z, plus), context.MkAdd(x, y),
                        context.MkITE(context.MkEq(z, minus), context.MkSub(x, y),
                            context.MkITE(context.MkEq(z, mult), context.MkMul(x, y),
                                context.MkDiv(x, y))));

                var solver = context.MkSolver();

                var b1 = context.MkConst("b1", @operator);

                // Assert that 2 <operator> 2 = 4:
                solver.Assert(
                    context.MkEq(
                        myf(context.MkInt(2), b1, context.MkInt(2)),
                        context.MkInt(4)));

                var operators = new[] { b1 };
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
