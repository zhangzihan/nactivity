using System;
using System.Collections.Generic;
using System.Linq;

namespace Sys.Rules.DmnEngine.Feel.Types
{
    /// <summary>
    /// FEEL function wrapper that supports both positional and named parameter invocation.
    /// </summary>
    public sealed class FeelFunction
    {
        /// <summary>
        /// The ordered list of parameter names for this function
        /// </summary>
        public IReadOnlyList<string> ParameterNames { get; }

        /// <summary>
        /// The underlying callable that takes an array of positional arguments and returns a result
        /// </summary>
        private Func<object[], object> Body { get; }

        /// <summary>
        /// Display name of this function (for debugging/error messages)
        /// </summary>
        public string Name { get; }

        public FeelFunction(string name, IReadOnlyList<string> parameterNames, Func<object[], object> body)
        {
            Name = name ?? "anonymous";
            ParameterNames = parameterNames ?? Array.Empty<string>();
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        /// <summary>
        /// Invokes the function with positional arguments
        /// </summary>
        public object Invoke(params object[] args)
        {
            return Body(args ?? Array.Empty<object>());
        }

        /// <summary>
        /// Invokes the function with named arguments.
        /// Arguments are reordered to match the declared parameter order.
        /// Missing parameters are passed as null.
        /// </summary>
        public object InvokeNamed(IDictionary<string, object> namedArgs)
        {
            if (namedArgs == null) return Invoke();

            var positional = new object[ParameterNames.Count];
            for (var i = 0; i < ParameterNames.Count; i++)
            {
                positional[i] = namedArgs.TryGetValue(ParameterNames[i], out var val) ? val : null;
            }
            return Invoke(positional);
        }

        public override string ToString() => $"function({string.Join(", ", ParameterNames)})";
    }
}

