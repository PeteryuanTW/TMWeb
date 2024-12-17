using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG.Extention
{
    public static class ExpressionHelper
    {
        private static readonly DataTable dt = new DataTable();
        private static readonly Dictionary<string, string> expressionCache = new();
        private static readonly Dictionary<string, object> resultCache = new();

        private static readonly (string symbol, string str)[] tokens = new[] { ("&&", "AND"), ("||", "OR") };

        public static T Compute<T>(this string expression, params (string name, object value)[] arguments) =>
        (T)Convert.ChangeType(expression.Transform().GetResult(arguments), typeof(T));


        private static object GetResult(this string expression, params (string name, object value)[] arguments)
        {
            foreach (var arg in arguments)
                expression = expression.Replace(arg.name, arg.value.ToString());

            if (resultCache.TryGetValue(expression, out var result))
                return result;

            return resultCache[expression] = dt.Compute(expression, string.Empty);
        }


        private static string Transform(this string expression)
        {
            if (expressionCache.TryGetValue(expression, out var result))
                return result;

            result = expression;
            foreach (var t in tokens)
                result = result.Replace(t.symbol, t.str);

            return expressionCache[expression] = result;
        }
    }
}
