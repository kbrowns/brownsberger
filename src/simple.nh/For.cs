using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Simple.NH
{
    public static class For
    {
        public static ForType<T> Type<T>()
        {
            return new ForType<T>();
        }

        public class ForType<T>
        {
            public MethodInfo GetMethod(Expression<Func<T, object>> expression)
            {
                return GetMethodInfo(expression);
            }

            public MethodInfo GetMethod(Expression<Action<T>> expression)
            {
                return GetMethodInfo(expression);
            }

            public MethodInfo GetMethod(Expression<Func<object>> expression)
            {
                return GetMethodInfo(expression);
            }

            public MethodInfo GetMethod(Expression<Action> expression)
            {
                return GetMethodInfo(expression);
            }

            public ConstructorInfo GetConstructor(Expression<Func<object>> expression)
            {
                return GetExpression<NewExpression>(expression, "Invalid constructor expression").Constructor;
            }

            public MemberInfo GetMember(Expression<Func<T, object>> expression)
            {
                return GetMemberInfo<PropertyInfo>(expression, "Invalid member expression");
            }

            public MemberInfo GetMember(Expression<Func<object>> expression)
            {
                return GetMemberInfo<PropertyInfo>(expression, "Invalid member expression");
            }

            public PropertyInfo GetProperty(Expression<Func<T, object>> expression)
            {
                return GetMemberInfo<PropertyInfo>(expression, "Invalid property expression");
            }

            public string GetPropertyName(Expression<Func<T, object>> expression)
            {
                return GetMemberInfo<PropertyInfo>(expression, "Invalid property expression").Name;
            }

            public PropertyInfo GetProperty<TProperty>(Expression<Func<T, TProperty>> expression)
            {
                return GetMemberInfo<PropertyInfo>(expression, "Invalid property expression");
            }

            public PropertyInfo GetProperty(Expression<Func<object>> expression)
            {
                return GetMemberInfo<PropertyInfo>(expression, "Invalid property expression");
            }

            public FieldInfo GetField(Expression<Func<T, object>> expression)
            {
                return GetMemberInfo<FieldInfo>(expression, "Invalid field expression");
            }

            public FieldInfo GetField(Expression<Func<object>> expression)
            {
                return GetMemberInfo<FieldInfo>(expression, "Invalid field expression");
            }

            private MethodInfo GetMethodInfo(LambdaExpression expression)
            {
                return GetExpression<MethodCallExpression>(expression, "Invalid method call expression").Method;
            }

            private TMemberInfo GetMemberInfo<TMemberInfo>(LambdaExpression expression, string message) where TMemberInfo : MemberInfo
            {
                var memberExpression = GetExpression<MemberExpression>(expression, message);

                var member = memberExpression.Member as TMemberInfo;
 
                if (member == null)
                    throw new ArgumentException(message, "expression");

                return member;
            }

            private TExpression GetExpression<TExpression>(LambdaExpression expression, string context) where TExpression : Expression
            {
                var body = expression.Body;
                var unaryExpression = body as UnaryExpression;
                var result = (unaryExpression == null ? body : unaryExpression.Operand) as TExpression;

                if (result == null)
                    throw new ArgumentException(context, "expression");


                return result;
            }
        }
    }
}
