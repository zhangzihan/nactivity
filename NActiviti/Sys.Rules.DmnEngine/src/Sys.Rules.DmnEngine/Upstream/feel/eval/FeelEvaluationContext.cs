using System.Collections.Generic;

namespace Sys.Rules.DmnEngine.Feel.Eval
{
    /// <summary>
    /// Evaluation context for FEEL expressions.
    /// Provides a scope chain for variable resolution and supports nested contexts
    /// (e.g. for/in iterations, context expressions, filter expressions).
    /// </summary>
    public class FeelEvaluationContext
    {
        private readonly Dictionary<string, object> variables = new();
        private readonly FeelEvaluationContext parent;

        /// <summary>
        /// The implicit input value used in unary tests and filter expressions.
        /// In unary tests, this is the decision table input value (the '?' variable).
        /// In filter expressions, this is the current list element ('item').
        /// </summary>
        public object InputValue { get; set; }

        public FeelEvaluationContext(FeelEvaluationContext parent = null)
        {
            this.parent = parent;
            InputValue = parent?.InputValue;
        }

        /// <summary>
        /// Gets the value of a variable, walking up the scope chain.
        /// Returns null if not found (FEEL semantics: unknown variable = null).
        /// </summary>
        public object GetVariable(string name)
        {
            if (variables.TryGetValue(name, out var value)) return value;
            return parent?.GetVariable(name);
        }

        /// <summary>
        /// Sets a variable in the current scope.
        /// </summary>
        public void SetVariable(string name, object value)
        {
            variables[name] = value;
        }

        /// <summary>
        /// Creates a child scope that inherits from this context.
        /// </summary>
        public FeelEvaluationContext CreateChildScope()
        {
            return new FeelEvaluationContext(this);
        }

        /// <summary>
        /// Checks if a variable exists in the current scope (not parents).
        /// </summary>
        public bool HasLocalVariable(string name) => variables.ContainsKey(name);
    }
}

