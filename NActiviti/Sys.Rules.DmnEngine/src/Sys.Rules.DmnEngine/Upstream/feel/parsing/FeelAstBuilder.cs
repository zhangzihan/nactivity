using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Sys.Rules.DmnEngine.Feel.Ast;

namespace Sys.Rules.DmnEngine.Feel.Parsing
{
    /// <summary>
    /// ANTLR4 visitor that converts the parse tree into a FEEL AST.
    /// </summary>
    internal class FeelAstBuilder : FeelParserBaseVisitor<FeelAstNode>
    {
        // ==================== Entry Points ====================

        public override FeelAstNode VisitExpressionRoot(FeelParser.ExpressionRootContext context)
        {
            return Visit(context.expression());
        }

        public override FeelAstNode VisitUnaryTestsRoot(FeelParser.UnaryTestsRootContext context)
        {
            return Visit(context.simpleUnaryTests());
        }

        // ==================== Simple Unary Tests ====================

        public override FeelAstNode VisitPositiveUnaryTestDash(FeelParser.PositiveUnaryTestDashContext context)
        {
            return Dash.Instance;
        }

        public override FeelAstNode VisitNegatedUnaryTests(FeelParser.NegatedUnaryTestsContext context)
        {
            var inner = Visit(context.simpleUnaryTests());
            if (inner is UnaryTests ut)
                return new UnaryTests(ut.Tests, isNegated: true);
            return new UnaryTests(new[] { inner }, isNegated: true);
        }

        public override FeelAstNode VisitPositiveUnaryTests(FeelParser.PositiveUnaryTestsContext context)
        {
            var tests = context.simpleUnaryTest().Select(Visit).ToList();
            if (tests.Count == 1) return tests[0];
            return new UnaryTests(tests);
        }

        public override FeelAstNode VisitUnaryTestBracketedInterval(FeelParser.UnaryTestBracketedIntervalContext context)
        {
            var low = Visit(context.endpoint(0));
            var high = Visit(context.endpoint(1));

            // Determine inclusivity from bracket types
            var startBracket = context.intervalStartBracket();
            var endBracket = context.intervalEndBracket();

            // Start: [ = inclusive, ( = exclusive, ] = exclusive (European)
            var lowInclusive = startBracket.LBRACKET() != null;
            // End: ] = inclusive, ) = exclusive, [ = exclusive (European)
            var highInclusive = endBracket.RBRACKET() != null;

            return new RangeNode(low, lowInclusive, high, highInclusive);
        }

        public override FeelAstNode VisitUnaryTestInterval(FeelParser.UnaryTestIntervalContext context)
        {
            var low = Visit(context.endpoint(0));
            var high = Visit(context.endpoint(1));
            // Default closed interval [low..high]
            return new RangeNode(low, true, high, true);
        }

        public override FeelAstNode VisitUnaryTestOp(FeelParser.UnaryTestOpContext context)
        {
            // Standalone operator like "> 5" 鈥?implicit left operand (the input value)
            var op = GetComparisonOp(context.compOp());
            var right = Visit(context.endpoint());
            // Left is null to indicate the implicit input value 鈥?the evaluator fills it in
            return new Comparison(op, null, right);
        }

        public override FeelAstNode VisitUnaryTestComparison(FeelParser.UnaryTestComparisonContext context)
        {
            return Visit(context.comparison());
        }

        public override FeelAstNode VisitUnaryTestNull(FeelParser.UnaryTestNullContext context)
        {
            return NullLiteral.Instance;
        }

        public override FeelAstNode VisitEndpoint(FeelParser.EndpointContext context)
        {
            return Visit(context.additiveExpression());
        }

        // ==================== Expressions ====================

        public override FeelAstNode VisitExpression(FeelParser.ExpressionContext context)
        {
            if (context.textualExpression() != null)
                return Visit(context.textualExpression());
            return Visit(context.boxedExpression());
        }

        public override FeelAstNode VisitTextualExpression(FeelParser.TextualExpressionContext context)
        {
            if (context.functionDefinition() != null) return Visit(context.functionDefinition());
            if (context.forExpression() != null) return Visit(context.forExpression());
            if (context.ifExpression() != null) return Visit(context.ifExpression());
            if (context.quantifiedExpression() != null) return Visit(context.quantifiedExpression());
            return Visit(context.ternaryExpression());
        }

        public override FeelAstNode VisitBoxedExpression(FeelParser.BoxedExpressionContext context)
        {
            if (context.list() != null) return Visit(context.list());
            if (context.context() != null) return Visit(context.context());
            return Visit(context.functionDefinition());
        }

        // ==================== Logical Operators ====================

        public override FeelAstNode VisitDisjunction(FeelParser.DisjunctionContext context)
        {
            var conjunctions = context.conjunction();
            if (conjunctions.Length == 1) return Visit(conjunctions[0]);
            return new Disjunction(conjunctions.Select(Visit).ToList());
        }

        public override FeelAstNode VisitConjunction(FeelParser.ConjunctionContext context)
        {
            var comparisons = context.comparison();
            if (comparisons.Length == 1) return Visit(comparisons[0]);
            return new Conjunction(comparisons.Select(Visit).ToList());
        }

        // ==================== Comparison ====================

        public override FeelAstNode VisitComparisonOp(FeelParser.ComparisonOpContext context)
        {
            var left = Visit(context.additiveExpression(0));
            var right = Visit(context.additiveExpression(1));
            var op = GetComparisonOp(context.compOp());
            return new Comparison(op, left, right);
        }

        public override FeelAstNode VisitComparisonBetween(FeelParser.ComparisonBetweenContext context)
        {
            var value = Visit(context.additiveExpression(0));
            var low = Visit(context.additiveExpression(1));
            var high = Visit(context.additiveExpression(2));
            return new Between(value, low, high);
        }

        public override FeelAstNode VisitComparisonInList(FeelParser.ComparisonInListContext context)
        {
            var value = Visit(context.additiveExpression());
            var tests = Visit(context.simpleUnaryTests());
            return new InNode(value, tests);
        }

        public override FeelAstNode VisitComparisonInUnary(FeelParser.ComparisonInUnaryContext context)
        {
            var value = Visit(context.additiveExpression());
            var test = Visit(context.simpleUnaryTest());
            return new InNode(value, test);
        }

        public override FeelAstNode VisitComparisonInstanceOf(FeelParser.ComparisonInstanceOfContext context)
        {
            var value = Visit(context.additiveExpression());
            var typeName = context.feelType().GetText();
            return new InstanceOf(value, typeName);
        }

        public override FeelAstNode VisitComparisonBase(FeelParser.ComparisonBaseContext context)
        {
            return Visit(context.additiveExpression());
        }

        private static ComparisonOperator GetComparisonOp(FeelParser.CompOpContext ctx)
        {
            if (ctx.LT() != null) return ComparisonOperator.Lt;
            if (ctx.GT() != null) return ComparisonOperator.Gt;
            if (ctx.LTE() != null) return ComparisonOperator.Lte;
            if (ctx.GTE() != null) return ComparisonOperator.Gte;
            if (ctx.NEQ() != null) return ComparisonOperator.Neq;
            return ComparisonOperator.Eq;
        }

        // ==================== Arithmetic ====================

        public override FeelAstNode VisitAdditiveExpression(FeelParser.AdditiveExpressionContext context)
        {
            var terms = context.multiplicativeExpression();
            var result = Visit(terms[0]);
            for (var i = 1; i < terms.Length; i++)
            {
                var token = context.GetChild(2 * i - 1) as ITerminalNode;
                var op = token?.Symbol.Type == FeelLexer.PLUS ? BinaryOperator.Add : BinaryOperator.Sub;
                result = new BinaryOp(op, result, Visit(terms[i]));
            }
            return result;
        }

        public override FeelAstNode VisitMultiplicativeExpression(FeelParser.MultiplicativeExpressionContext context)
        {
            var factors = context.exponentiationExpression();
            var result = Visit(factors[0]);
            for (var i = 1; i < factors.Length; i++)
            {
                var token = context.GetChild(2 * i - 1) as ITerminalNode;
                BinaryOperator op;
                if (token?.Symbol.Type == FeelLexer.STAR) op = BinaryOperator.Mul;
                else if (token?.Symbol.Type == FeelLexer.PERCENT) op = BinaryOperator.Mod;
                else op = BinaryOperator.Div;
                result = new BinaryOp(op, result, Visit(factors[i]));
            }
            return result;
        }

        public override FeelAstNode VisitExponentiationExpression(FeelParser.ExponentiationExpressionContext context)
        {
            var bases = context.unaryExpression();
            if (bases.Length == 1) return Visit(bases[0]);
            // Right-associative: a ** b ** c = a ** (b ** c)
            var result = Visit(bases[^1]);
            for (var i = bases.Length - 2; i >= 0; i--)
            {
                result = new BinaryOp(BinaryOperator.Exp, Visit(bases[i]), result);
            }
            return result;
        }

        // ==================== Unary ====================

        public override FeelAstNode VisitUnaryMinus(FeelParser.UnaryMinusContext context)
        {
            return new UnaryMinus(Visit(context.unaryExpression()));
        }

        public override FeelAstNode VisitUnaryNot(FeelParser.UnaryNotContext context)
        {
            return new UnaryNot(Visit(context.unaryExpression()));
        }

        public override FeelAstNode VisitUnaryPostfix(FeelParser.UnaryPostfixContext context)
        {
            return Visit(context.postfixExpression());
        }

        // ==================== Postfix ====================

        public override FeelAstNode VisitPostfixMemberAccess(FeelParser.PostfixMemberAccessContext context)
        {
            var source = Visit(context.postfixExpression());
            var member = context.simpleName().GetText();
            return new PathNode(source, member);
        }

        public override FeelAstNode VisitPostfixFilter(FeelParser.PostfixFilterContext context)
        {
            var source = Visit(context.postfixExpression());
            var filter = Visit(context.expression());
            return new FilterNode(source, filter);
        }

        public override FeelAstNode VisitPostfixInvocation(FeelParser.PostfixInvocationContext context)
        {
            var fn = Visit(context.postfixExpression());
            var args = context.namedOrPositionalArgs();
            return VisitInvocationArgs(fn, args);
        }

        public override FeelAstNode VisitPostfixEmptyInvocation(FeelParser.PostfixEmptyInvocationContext context)
        {
            var fn = Visit(context.postfixExpression());
            return new FunctionInvocation(fn, Array.Empty<FeelAstNode>());
        }

        public override FeelAstNode VisitPostfixPrimary(FeelParser.PostfixPrimaryContext context)
        {
            return Visit(context.primary());
        }

        private FeelAstNode VisitInvocationArgs(FeelAstNode fn, FeelParser.NamedOrPositionalArgsContext args)
        {
            if (args is FeelParser.NamedArgListContext namedCtx)
            {
                var namedArgs = namedCtx.namedArg().Select(a =>
                    (a.NAME().GetText(), Visit(a.expression()))).ToList();
                return new NamedFunctionInvocation(fn, namedArgs);
            }

            if (args is FeelParser.PositionalArgListContext posCtx)
            {
                var positionalArgs = posCtx.expression().Select(Visit).ToList();
                return new FunctionInvocation(fn, positionalArgs);
            }

            return new FunctionInvocation(fn, Array.Empty<FeelAstNode>());
        }

        // ==================== Primary ====================

        public override FeelAstNode VisitPrimaryLiteral(FeelParser.PrimaryLiteralContext context)
        {
            return Visit(context.literal());
        }

        public override FeelAstNode VisitPrimaryAtLiteral(FeelParser.PrimaryAtLiteralContext context)
        {
            var raw = context.StringLiteral().GetText();
            // Remove surrounding quotes
            return new DateTimeLiteral(raw.Substring(1, raw.Length - 2));
        }

        public override FeelAstNode VisitPrimaryAtLiteralToken(FeelParser.PrimaryAtLiteralTokenContext context)
        {
            var raw = context.AtLiteral().GetText();
            // Remove @" and trailing "
            return new DateTimeLiteral(raw.Substring(2, raw.Length - 3));
        }

        public override FeelAstNode VisitPrimaryName(FeelParser.PrimaryNameContext context)
        {
            return new NameNode(context.simpleName().GetText());
        }

        public override FeelAstNode VisitPrimaryParen(FeelParser.PrimaryParenContext context)
        {
            return Visit(context.expression());
        }

        public override FeelAstNode VisitPrimaryList(FeelParser.PrimaryListContext context)
        {
            return Visit(context.list());
        }

        public override FeelAstNode VisitPrimaryContext(FeelParser.PrimaryContextContext context)
        {
            return Visit(context.context());
        }

        // ==================== Literals ====================

        public override FeelAstNode VisitLiteralInteger(FeelParser.LiteralIntegerContext context)
        {
            return new NumberLiteral(decimal.Parse(context.IntegerLiteral().GetText(), CultureInfo.InvariantCulture));
        }

        public override FeelAstNode VisitLiteralFloat(FeelParser.LiteralFloatContext context)
        {
            return new NumberLiteral(decimal.Parse(context.FloatLiteral().GetText(), CultureInfo.InvariantCulture));
        }

        public override FeelAstNode VisitLiteralString(FeelParser.LiteralStringContext context)
        {
            var raw = context.StringLiteral().GetText();
            // Remove surrounding quotes and unescape
            var unescaped = UnescapeString(raw.Substring(1, raw.Length - 2));
            return new StringLiteral(unescaped);
        }

        public override FeelAstNode VisitLiteralTrue(FeelParser.LiteralTrueContext context)
        {
            return new BoolLiteral(true);
        }

        public override FeelAstNode VisitLiteralFalse(FeelParser.LiteralFalseContext context)
        {
            return new BoolLiteral(false);
        }

        public override FeelAstNode VisitLiteralNull(FeelParser.LiteralNullContext context)
        {
            return NullLiteral.Instance;
        }

        // ==================== Control Flow ====================

        // ==================== Ternary (C# compat: cond ? a : b) ====================

        public override FeelAstNode VisitTernaryOp(FeelParser.TernaryOpContext context)
        {
            var condition = Visit(context.disjunction());
            var thenBranch = Visit(context.expression(0));
            var elseBranch = Visit(context.expression(1));
            return new IfNode(condition, thenBranch, elseBranch);
        }

        public override FeelAstNode VisitTernaryPassthrough(FeelParser.TernaryPassthroughContext context)
        {
            return Visit(context.disjunction());
        }

        public override FeelAstNode VisitIfExpression(FeelParser.IfExpressionContext context)
        {
            var condition = Visit(context.expression(0));
            var thenBranch = Visit(context.expression(1));
            var elseBranch = Visit(context.expression(2));
            return new IfNode(condition, thenBranch, elseBranch);
        }

        public override FeelAstNode VisitForExpression(FeelParser.ForExpressionContext context)
        {
            var iterations = context.iterationContext().Select(BuildIterationContext).ToList();
            var returnExpr = Visit(context.expression());
            return new ForNode(iterations, returnExpr);
        }

        public override FeelAstNode VisitQuantifiedExpression(FeelParser.QuantifiedExpressionContext context)
        {
            var quantifier = context.SOME() != null ? QuantifierType.Some : QuantifierType.Every;
            var iterations = context.iterationContext().Select(BuildIterationContext).ToList();
            var satisfies = Visit(context.expression());
            return new QuantifiedNode(quantifier, iterations, satisfies);
        }

        private IterationContext BuildIterationContext(FeelParser.IterationContextContext ctx)
        {
            var varName = ctx.NAME().GetText();
            var expressions = ctx.expression();
            var listExpr = Visit(expressions[0]);
            FeelAstNode rangeEnd = expressions.Length > 1 ? Visit(expressions[1]) : null;
            return new IterationContext(varName, listExpr, rangeEnd);
        }

        public override FeelAstNode VisitFunctionDefinition(FeelParser.FunctionDefinitionContext context)
        {
            var paramList = context.formalParameterList();
            var parameters = paramList?.formalParameter().Select(p => p.NAME().GetText()).ToList()
                ?? new List<string>();
            var isExternal = context.EXTERNAL() != null;
            var body = Visit(context.expression());
            return new FunctionDefinitionNode(parameters, body, isExternal);
        }

        // ==================== Collections ====================

        public override FeelAstNode VisitList(FeelParser.ListContext context)
        {
            var elements = context.expression()?.Select(Visit).ToList() ?? new List<FeelAstNode>();
            return new ListNode(elements);
        }

        public override FeelAstNode VisitContext(FeelParser.ContextContext context)
        {
            var entries = context.contextEntry()?.Select(e =>
            {
                var key = e.key().simpleName()?.GetText() ?? UnescapeString(
                    e.key().StringLiteral().GetText().Trim('"'));
                var value = Visit(e.expression());
                return new ContextEntryNode(key, value);
            }).ToList() ?? new List<ContextEntryNode>();
            return new ContextNode(entries);
        }

        // ==================== Names ====================

        public override FeelAstNode VisitQualifiedName(FeelParser.QualifiedNameContext context)
        {
            var parts = context.simpleName().Select(s => s.GetText()).ToList();
            if (parts.Count == 1) return new NameNode(parts[0]);
            return new QualifiedNameNode(parts);
        }

        // ==================== Type ====================

        public override FeelAstNode VisitTypeNamed(FeelParser.TypeNamedContext context)
        {
            return Visit(context.qualifiedName());
        }

        public override FeelAstNode VisitTypeNull(FeelParser.TypeNullContext context)
        {
            return new NameNode("null");
        }

        // ==================== Helpers ====================

        private static string UnescapeString(string s)
        {
            if (s == null) return null;
            return s.Replace("\\\"", "\"")
                    .Replace("\\\\", "\\")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t")
                    .Replace("\\/", "/")
                    .Replace("\\b", "\b")
                    .Replace("\\f", "\f");
        }
    }
}

