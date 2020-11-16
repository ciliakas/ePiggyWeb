using System;
using System.Linq.Expressions;

namespace ePiggyWeb.Utilities
{
    public static class ExpressionExtension
    {
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            return expression1.Compose(Expression.Or, expression2);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            return expression1.Compose(Expression.And, expression2);
        }

        private static Expression<Func<T, bool>> Compose<T>(this Expression<Func<T, bool>> expression1,
            Func<Expression, Expression, BinaryExpression> logicalOperation, Expression<Func<T, bool>> expression2)
        {
            var invokedExpression = Expression.Invoke(expression2, expression1.Parameters);
            return Expression.Lambda<Func<T, bool>>(logicalOperation.Invoke(expression1.Body, invokedExpression), expression1.Parameters);
        }
    }
}