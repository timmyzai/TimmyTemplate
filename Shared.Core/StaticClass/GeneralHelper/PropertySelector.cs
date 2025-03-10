using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace AwesomeProject
{
    public static class PropertySelector<T>
    {
        private static readonly ConcurrentDictionary<string, LambdaExpression> Selectors = new ConcurrentDictionary<string, LambdaExpression>();

        public static Expression<Func<T, object>> GetKeySelector(string propertyName)
        {
            var selector = Selectors.GetOrAdd(propertyName, prop =>
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(parameter, prop);
                var convert = Expression.Convert(property, typeof(object));
                return Expression.Lambda<Func<T, object>>(convert, parameter);
            });

            return (Expression<Func<T, object>>)selector;
        }
    }
}