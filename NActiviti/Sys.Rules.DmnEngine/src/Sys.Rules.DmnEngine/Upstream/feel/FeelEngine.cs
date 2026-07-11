using System;
using System.IO;
using Antlr4.Runtime;
using Sys.Rules.DmnEngine.Feel.Ast;
using Sys.Rules.DmnEngine.Feel.Eval;
using Sys.Rules.DmnEngine.Feel.Functions;
using Sys.Rules.DmnEngine.Feel.Parsing;

namespace Sys.Rules.DmnEngine.Feel
{
    /// <summary>
    /// Public facade for FEEL expression parsing and evaluation.
    /// </summary>
    public class FeelEngine
    {
        /// <summary>
        /// Parse a FEEL expression into an AST node.
        /// </summary>
        /// <param name="expression">The FEEL expression text</param>
        /// <param name="scope">Optional scope with known variable/function names for multi-word name resolution</param>
        /// <returns>The root AST node</returns>
        public FeelAstNode ParseExpression(string expression, FeelScope scope = null)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

            var (parser, _) = CreateParser(expression, scope);
            var tree = parser.expressionRoot();
            var builder = new FeelAstBuilder();
            return builder.Visit(tree);
        }

        /// <summary>
        /// Parse FEEL simple unary tests into an AST node.
        /// Used for decision table input entries.
        /// </summary>
        /// <param name="expression">The unary tests expression text</param>
        /// <param name="scope">Optional scope with known variable/function names</param>
        /// <returns>The root AST node</returns>
        public FeelAstNode ParseSimpleUnaryTests(string expression, FeelScope scope = null)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

            var (parser, _) = CreateParser(expression, scope);
            var tree = parser.unaryTestsRoot();
            var builder = new FeelAstBuilder();
            return builder.Visit(tree);
        }

        /// <summary>
        /// Evaluates a FEEL expression and returns the result.
        /// </summary>
        /// <param name="expression">The FEEL expression text</param>
        /// <param name="evalContext">The evaluation context with variable values</param>
        /// <param name="scope">Optional scope for multi-word name resolution</param>
        /// <returns>The evaluation result</returns>
        public object EvaluateExpression(string expression, FeelEvaluationContext evalContext, FeelScope scope = null)
        {
            var ast = ParseExpression(expression, scope);
            return Evaluate(ast, evalContext);
        }

        /// <summary>
        /// Evaluates FEEL simple unary tests against an input value.
        /// Returns true if the input value matches the tests.
        /// </summary>
        /// <param name="expression">The unary tests expression text</param>
        /// <param name="inputValue">The input value to test against</param>
        /// <param name="evalContext">The evaluation context with variable values</param>
        /// <param name="scope">Optional scope for multi-word name resolution</param>
        /// <returns>True if the input matches</returns>
        public bool EvaluateSimpleUnaryTests(string expression, object inputValue, FeelEvaluationContext evalContext, FeelScope scope = null)
        {
            var ast = ParseSimpleUnaryTests(expression, scope);
            evalContext.InputValue = inputValue;
            var evaluator = new FeelEvaluator(evalContext, ResolveFunctionCall);
            return evaluator.EvaluateAsUnaryTest(ast);
        }

        /// <summary>
        /// Evaluates a pre-parsed AST node with the given context.
        /// </summary>
        /// <param name="ast">The pre-parsed AST node</param>
        /// <param name="evalContext">The evaluation context</param>
        /// <returns>The evaluation result</returns>
        public object Evaluate(FeelAstNode ast, FeelEvaluationContext evalContext)
        {
            if (ast == null) throw new ArgumentNullException(nameof(ast));
            if (evalContext == null) throw new ArgumentNullException(nameof(evalContext));

            var evaluator = new FeelEvaluator(evalContext, ResolveFunctionCall);
            return evaluator.Evaluate(ast);
        }

        /// <summary>
        /// Function resolver that delegates to built-in functions.
        /// Can be overridden to add custom functions.
        /// </summary>
        protected virtual object ResolveFunctionCall(string name, object[] args)
        {
            return FeelBuiltInFunctions.Resolve(name, args);
        }

        /// <summary>
        /// Creates a configured ANTLR parser for the given FEEL expression.
        /// </summary>
        private static (FeelParser parser, CommonTokenStream tokenStream) CreateParser(string expression, FeelScope scope)
        {
            var inputStream = new AntlrInputStream(expression);
            var lexer = new FeelLexer(inputStream);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ThrowingErrorListener());

            var tokenStream = new CommonTokenStream(lexer);

            // If we have a scope with multi-word names, apply name resolution
            if (scope != null)
            {
                var resolver = new FeelNameResolver(tokenStream, scope);
                var resolvedStream = new CommonTokenStream(resolver);
                var parser = new FeelParser(resolvedStream);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new ThrowingErrorListener());
                return (parser, resolvedStream);
            }
            else
            {
                var parser = new FeelParser(tokenStream);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new ThrowingErrorListener());
                return (parser, tokenStream);
            }
        }

        /// <summary>
        /// ANTLR error listener that throws exceptions on syntax errors.
        /// </summary>
        private class ThrowingErrorListener : BaseErrorListener, IAntlrErrorListener<int>
        {
            public override void SyntaxError(
                TextWriter output, IRecognizer recognizer, IToken offendingSymbol,
                int line, int charPositionInLine,
                string msg, RecognitionException e)
            {
                throw new FeelParseException($"FEEL syntax error at line {line}:{charPositionInLine} - {msg}", e);
            }

            public void SyntaxError(
                TextWriter output, IRecognizer recognizer, int offendingSymbol,
                int line, int charPositionInLine,
                string msg, RecognitionException e)
            {
                throw new FeelParseException($"FEEL lexer error at line {line}:{charPositionInLine} - {msg}", e);
            }
        }
    }

    /// <summary>
    /// Exception thrown when a FEEL expression cannot be parsed.
    /// </summary>
    public class FeelParseException : Exception
    {
        public FeelParseException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }
}

