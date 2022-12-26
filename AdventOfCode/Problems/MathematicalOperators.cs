using System.Numerics;

namespace AdventOfCode.Problems;

public static class MathematicalOperators
{
    public static MathematicalOperator Parse(char c)
    {
        return c switch
        {
            '+' => MathematicalOperator.Addition,
            '-' => MathematicalOperator.Subtraction,
            '*' => MathematicalOperator.Multiplication,
            '/' => MathematicalOperator.Division,

            '%' => MathematicalOperator.Modulo,
            '^' => MathematicalOperator.Exponentation,

            // just ignore other possibilities
            _ => default,
        };
    }

    public static T Operate<T>(this MathematicalOperator @operator, T left, T right)
        where T : IBinaryInteger<T>
    {
        return @operator switch
        {
            // TODO: Extend for floating points
            MathematicalOperator.Addition => left + right,
            MathematicalOperator.Subtraction => left - right,
            MathematicalOperator.Multiplication => left * right,
            MathematicalOperator.Division => left / right,
            MathematicalOperator.Modulo => left % right,

            _ => default,
        };
    }
}
