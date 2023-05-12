﻿using System.Linq.Expressions;

namespace Foundation.Linq.Expressions;

public static class ExpressionExtensions
{
    public static bool EqualsToExpression(this BinaryExpression lhs, BinaryExpression rhs)
    {
        if (null == lhs) return null == rhs;

        return lhs.NodeType == rhs.NodeType
            && EqualityComparer<object>.Default.Equals(lhs.Type, rhs.Type)
            && EqualityComparer<object>.Default.Equals(lhs.Left, rhs.Left)
            && EqualsToExpression(lhs, rhs);
    }

    public static bool EqualsToExpression(this ConstantExpression lhs, ConstantExpression rhs)
    {
        if (null == lhs) return null == rhs;
        return null != rhs && lhs.Value.EqualsNullable(rhs.Value);
    }

    public static bool EqualsToExpression(this Expression lhs, Expression rhs)
    {
        return lhs switch
        {
            BinaryExpression l => rhs is BinaryExpression r && EqualsToExpression(l, r),
            ConstantExpression l => rhs is ConstantExpression r && EqualsToExpression(l, r),
            LambdaExpression l => rhs is LambdaExpression r && EqualsToExpression(l, r),
            MemberExpression l => rhs is MemberExpression r && EqualsToExpression(l, r),
            ParameterExpression l => rhs is ParameterExpression r && EqualsToExpression(l, r),
            UnaryExpression l => rhs is UnaryExpression r && EqualsToExpression(l, r),
            _ => false
        };
    }

    public static bool EqualsToExpression(this LambdaExpression lhs, LambdaExpression rhs)
    {
        if(null == lhs) return null == rhs;
        return null != rhs
            && lhs.NodeType == rhs.NodeType
            && lhs.Type == rhs.Type
            && lhs.ReturnType == rhs.ReturnType
            && lhs.Parameters.Select(x => x.Type).SequenceEqual(rhs.Parameters.Select(x => x.Type))
            && EqualsToExpression(lhs.Body, rhs.Body);
    }

    public static bool EqualsToExpression(this MemberExpression lhs, MemberExpression rhs)
    {
        if (null == lhs) return null == rhs;

        return null != rhs
            && lhs.Member.Equals(rhs.Member)
            && lhs.Type.Equals(rhs.Type);
    }

    public static bool EqualsToExpression(this ParameterExpression lhs, ParameterExpression rhs, bool considerName = true)
    {
        if (null == lhs) return null == rhs;
        if(null == rhs) return false;

        if(considerName)
        {
            if (!lhs.Name.EqualsNullable(rhs.Name)) return false;
        }
        return lhs.Type == rhs.Type;
    }

    public static IEnumerable<Expression> Flatten(this Expression expression)
    {
        var flattener = new ExpressionTreeFlattener();
        return flattener.Flatten(expression);
    }

    public static int GetExpressionHashCode(this BinaryExpression expression)
    {
        expression.ThrowIfNull();
        return HashCode.FromOrderedHashCode(expression.NodeType.GetHashCode(), 
                                            expression.Type.GetHashCode(),
                                            expression.Left.GetExpressionHashCode(),
                                            expression.Right.GetExpressionHashCode());
    }

    public static int GetExpressionHashCode(this ConstantExpression expression)
    {
        return expression.ThrowIfNull().Value.GetNullableHashCode();
    }

    public static int GetExpressionHashCode(this Expression expression)
    {
        expression.ThrowIfNull();
        return expression switch
        {
            BinaryExpression e => e.GetExpressionHashCode(),
            ConstantExpression e => e.GetExpressionHashCode(),
            LambdaExpression e => e.GetExpressionHashCode(),
            MemberExpression e => e.GetExpressionHashCode(),
            ParameterExpression e => e.GetExpressionHashCode(false),
            UnaryExpression e => e.GetExpressionHashCode(),
            _ => 0
        };
    }

    public static int GetExpressionHashCode(this LambdaExpression expression)
    {
        expression.ThrowIfNull();

        var hashCodes = Enumerable.Empty<int>();
        hashCodes = hashCodes.Append(expression.NodeType.GetHashCode());
        hashCodes = hashCodes.Append(expression.Type.GetHashCode());
        hashCodes = hashCodes.Append(expression.ReturnType.GetHashCode());
        hashCodes = hashCodes.Concat(expression.Parameters.Select(x => x.GetExpressionHashCode(false)));
        hashCodes = hashCodes.Append(expression.Body.GetExpressionHashCode());

        return HashCode.FromOrderedHashCode(hashCodes.ToArray());
    }

    public static int GetExpressionHashCode(this MemberExpression expression)
    {
        expression.ThrowIfNull();
        return HashCode.FromOrderedHashCode(expression.Member.GetHashCode(),
                                            expression.Type.GetHashCode());
    }

    public static int GetExpressionHashCode(this ParameterExpression expression, bool considerName = true)
    {
        expression.ThrowIfNull();
        return considerName
            ? HashCode.FromOrderedHashCode(expression.Name.GetNullableHashCode(),
                                           expression.Type.GetHashCode())
            : expression.Type.GetHashCode();
    }

    public static int GetExpressionHashCode(this UnaryExpression expression)
    {
        expression.ThrowIfNull();
        return HashCode.FromOrderedHashCode(expression.NodeType.GetHashCode(),
                                            expression.Type.GetHashCode(),
                                            expression.Operand.GetExpressionHashCode());
    }

    //public static Type? GetDataType(this Expression expression)
    //{
    //    return expression switch
    //    {
    //        ConstantExpression constant => constant.Type,
    //        MemberExpression member => member.Member.ReflectedType,
    //        ParameterExpression parameter => parameter.Type,
    //        UnaryExpression unary => unary.Type,
    //        _ => expression.GetType(),
    //    };
    //}

    //public static ParameterExpression? GetParameter(this Expression expression)
    //{
    //    return expression switch
    //    {
    //        ParameterExpression parameter => parameter,
    //        MemberExpression member => member.Expression as ParameterExpression,
    //        _ => null,
    //    };
    //}

    //public static bool IsConvert(this Expression expression)
    //{
    //    return expression.NodeType == ExpressionType.Convert;
    //}

    //public static bool IsPredicate(this Expression expression)
    //{
    //    return expression is LambdaExpression lambda
    //        && lambda.Body is BinaryExpression binaryExpression
    //        && binaryExpression.IsPredicate();
    //}

    //public static bool IsTerminal(this Expression expression)
    //{
    //    return expression.NodeType == ExpressionType.Constant
    //        || expression.NodeType == ExpressionType.MemberAccess
    //        || expression.NodeType == ExpressionType.Parameter;
    //}
}