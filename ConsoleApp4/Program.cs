using System;
using System.Linq;
using Microsoft.Z3;

namespace ConsoleApp4
{
    static class Program2
    {
        static void Main2(string[] args)
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
                    context.MkExists(
                        new[] { b1 },
                        context.MkEq(
                            myf(context.MkInt(2), b1, context.MkInt(2)),
                            context.MkInt(4))));

                while (solver.Check() == Status.SATISFIABLE)
                {
                    var model = solver.Model;
                    var decls = model.ConstDecls.Select(model.ConstInterp).First(); // srsly, wtf
                    //model.ConstDecls[0].

                    //Console.WriteLine(model);
                    Console.WriteLine(decls);

                    //solver.Add(context.MkNot(context.MkEq(decls., decls))); // forbid to show the same answer again
                }
                /* Output:
(define-fun b1!0 () operator
  Plus)
Plus
(define-fun b1!1 () operator
  Plus)
(define-fun b1 () operator
  Minus)
Minus
(define-fun b1!1 () operator
  Mult)
(define-fun b1 () operator
  Mult)
Mult
(define-fun b1!1 () operator
  Mult)
(define-fun b1 () operator
  Div)
Div
                 */
            }
        }
    }
}
