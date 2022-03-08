using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mesawer.ApplicationLayer.Extensions
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Creates a selector expression using a property name e.g. c => c.Property
        /// </summary>
        /// <param name="type">Just a placeholder that means nothing</param>
        /// <param name="propertyName">It's the navigation path (e.g. Value.Length) to produce c => c.Value.Length</param>
        public static LambdaExpression CreateSelectorExpression<T>(this Type type, string propertyName)
        {
            var parameterExpression = Expression.Parameter(typeof(T));

            var members = propertyName.Split('-').ToList();

            if (!members.Any()) return null;

            try
            {
                var memberExpression = Expression.PropertyOrField(parameterExpression, members[0]);

                memberExpression = members.Skip(1)
                    .ToList()
                    .Aggregate(memberExpression, Expression.PropertyOrField);

                return Expression.Lambda(memberExpression, parameterExpression);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Invokes the passed in method using the passed in optional parameters
        /// </summary>
        public static T InvokeProjectToMethod<T>(
            this MethodInfo methodInfo,
            object source,
            params object[] optionalParameters)
        {
            var parameters = new List<object> { source };

            parameters.AddRange(optionalParameters);

            return (T) methodInfo.Invoke(null, parameters.ToArray());
        }

        /// <summary>
        /// Gets the project to method method which is the public static method that takes in the number of optional
        /// Parameters and the returns the target type and takes the source type as the first argument.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="optionalParametersCount"></param>
        /// <returns></returns>
        public static MethodInfo GetProjectToMethod(this Type target, Type source, int optionalParametersCount)
        {
            var method = target
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(m => new { Method = m, Parameters = m.GetParameters() })
                .FirstOrDefault(m => m.Parameters.Length == (optionalParametersCount + 1) &&
                                     m.Parameters.FirstOrDefault(p => p.ParameterType == source) is not null &&
                                     m.Method.ReturnType == target)
                ?.Method;

            if (method is null)
                throw new ArgumentException(
                    $"There isn't a static method in type {target} that takes type {source} as an argument and returns {target}");

            return method;
        }

        /// <summary>
        /// Checks if the <paramref name="testType"/> is the same/instance type of <paramref name="sourceType"/> even if it's nullable type
        /// </summary>
        public static bool IsOfType(this Type sourceType, Type testType)
        {
            if (!sourceType.IsGenericType) return sourceType == testType || sourceType.IsSubclassOf(testType);

            if (sourceType.GetGenericTypeDefinition() != typeof(Nullable<>)) return false;

            var innerType = sourceType.GetInnerTypes()[0];

            return testType == innerType || innerType.IsSubclassOf(sourceType);
        }

        /// <summary>
        /// Gets the generic arguments of a generic type
        /// </summary>
        /// <returns>A list of the generic arguments or null if the <paramref name="type"/> isn't generic</returns>
        public static Type[] GetInnerTypes(this Type type)
        {
            if (type is null) return null;

            if (!type.IsGenericType || type.GetGenericArguments().Length > 1) return null;

            return type.GetGenericArguments();
        }

        public static object GetDefault(this Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

        /// <summary>
        /// Aggregates the passed in array of expressions using the Or expression
        /// </summary>
        public static BinaryExpression Or(Expression[] comparisonExpressions)
        {
            if (!comparisonExpressions.Any())
                throw new ArgumentException("Binary expressions require at least two operands");

            if (comparisonExpressions.Length < 2)
                return Expression.OrElse(comparisonExpressions[0], Expression.Constant(false));

            var or = Expression.OrElse(comparisonExpressions[0], comparisonExpressions[1]);

            for (var i = 2; i < comparisonExpressions.Length; i++) or = Expression.OrElse(or, comparisonExpressions[i]);

            return or;
        }
    }
}
