using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Z3;

namespace ConsoleApp4
{
    static class Program
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
        var b2 = context.MkConst("b2", @operator);
        var b3 = context.MkConst("b3", @operator);
        var b4 = context.MkConst("b4", @operator);
        var b5 = context.MkConst("b5", @operator);

        solver.Assert(
            context.MkExists(
                new[] { b1, b2, b3, b4, b5 },
                context.MkEq(
                    myf(
                        myf(
                            myf(
                                myf(
                                    myf(
                                        context.MkInt(1),
                                        b1,
                                        context.MkInt(2)),
                                    b2,
                                    context.MkInt(3)),
                                b3,
                                context.MkInt(4)),
                            b4,
                            context.MkInt(5)),
                        b5,
                        context.MkInt(6)),
                    context.MkInt(35))));

        while (Status.SATISFIABLE == solver.Check())
        {
            var operators = new[] { b1, b2, b3, b4, b5 };
            var model = solver.Model;
            var values = operators.Select(o => model.Eval(o, true)); // That doesn't return the right values

            Console.WriteLine(model);
            Console.WriteLine(string.Join(" ", values));

            solver.Add(context.MkOr(
                operators.Select(o => context.MkNot(context.MkEq(o, model.Eval(o, true)))))); // That's supposed to work, but it doesn't
        }
            }
        }
    }
}
