using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Sys.Rules.DmnEngine.Feel.Ast;
using Sys.Rules.DmnEngine.Feel.Types;

namespace Sys.Rules.DmnEngine.Feel.Eval
{
    /// <summary>
    /// Tree-walking evaluator for FEEL AST nodes.
    /// Implements FEEL expression semantics including null propagation, three-valued logic,
    /// 1-based list indexing, and all FEEL operators.
    /// </summary>
    public class FeelEvaluator
    {
        private readonly FeelEvaluationContext context;
        private readonly Func<string, object[], object> functionResolver;

        /// <summary>
        /// Creates a new evaluator with the given evaluation context and function resolver.
        /// </summary>
        /// <param name="context">The evaluation context with variables</param>
        /// <param name="functionResolver">
        /// Callback to resolve function calls by name. Takes function name and arguments,
        /// returns the result. If null, only user-defined (lambda) functions work.
        /// </param>
        public FeelEvaluator(FeelEvaluationContext context, Func<string, object[], object> functionResolver = null)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.functionResolver = functionResolver;
        }

        /// <summary>
        /// Evaluates an AST node and returns the result.
        /// </summary>
        public object Evaluate(FeelAstNode node)
        {
            if (node == null) return null;

            return node switch
            {
                // Literals
                NumberLiteral n => n.Value,
                StringLiteral s => s.Value,
                BoolLiteral b => b.Value,
                NullLiteral => null,
                DateTimeLiteral dt => ParseDateTimeLiteral(dt.RawValue),
                Dash => true, // dash in unary test means "any value matches"

                // Names
                NameNode name => ResolveVariable(name.Name),
                QualifiedNameNode qn => ResolveQualifiedName(qn),

                // Operators
                BinaryOp bin => EvalBinaryOp(bin),
                UnaryMinus neg => EvalUnaryMinus(neg),
                UnaryNot not => EvalUnaryNot(not),
                Comparison cmp => EvalComparison(cmp),
                Between bet => EvalBetween(bet),
                InNode inNode => EvalIn(inNode),
                InstanceOf io => EvalInstanceOf(io),

                // Logic
                Conjunction conj => EvalConjunction(conj),
                Disjunction disj => EvalDisjunction(disj),

                // Control flow
                IfNode ifn => EvalIf(ifn),
                ForNode f => EvalFor(f),
                QuantifiedNode q => EvalQuantified(q),

                // Collections
                ListNode list => EvalList(list),
                ContextNode ctx => EvalContext(ctx),
                RangeNode range => EvalRange(range),
                FilterNode filter => EvalFilter(filter),
                PathNode path => EvalPath(path),

                // Functions
                FunctionDefinitionNode fd => EvalFunctionDefinition(fd),
                FunctionInvocation fi => EvalFunctionInvocation(fi),
                NamedFunctionInvocation nfi => EvalNamedFunctionInvocation(nfi),

                // Unary tests
                UnaryTests ut => EvalUnaryTests(ut),

                _ => throw new FeelEvaluationException($"Unknown AST node type: {node.GetType().Name}")
            };
        }

        /// <summary>
        /// Evaluates a FEEL expression AST and returns the result as a boolean.
        /// Used for unary tests evaluation. Non-boolean results are coerced:
        /// - null 鈫?false
        /// - A value equal to InputValue 鈫?true
        /// - A range containing InputValue 鈫?true
        /// - A list containing InputValue 鈫?true
        /// </summary>
        public bool EvaluateAsUnaryTest(FeelAstNode node)
        {
            var result = Evaluate(node);
            return CoerceUnaryTestResultForNode(node, result, context.InputValue);
        }

        private static bool CoerceUnaryTestResult(object result, object inputValue)
        {
            // null result from a failed comparison/operation means "no match"
            // (explicit null test handling is done via isNullLiteral parameter overload)
            if (result == null) return false;
            if (result is bool b) return b;
            if (result is FeelRange range) return range.Contains(inputValue) == true;
            if (result is List<object> list) return list.Any(item => FeelValueComparer.FeelEqual(inputValue, item) == true);
            // Direct equality comparison
            return FeelValueComparer.FeelEqual(inputValue, result) == true;
        }

        private static bool CoerceUnaryTestResultForNode(FeelAstNode testNode, object result, object inputValue)
        {
            // For explicit null literal tests, check if inputValue is null
            if (testNode is NullLiteral) return inputValue == null;
            // For bool literals, compare with input value (not treat as test outcome)
            // A unary test of "true" means "input == true", not "this test passes"
            if (testNode is BoolLiteral) return FeelValueComparer.FeelEqual(inputValue, result) == true;
            return CoerceUnaryTestResult(result, inputValue);
        }

        // ==================== Variable Resolution ====================

        private object ResolveVariable(string name)
        {
            // Check special variables first
            if (name == "?") return context.InputValue;
            return context.GetVariable(name);
        }

        private object ResolveQualifiedName(QualifiedNameNode qn)
        {
            var value = ResolveVariable(qn.Parts[0]);
            for (var i = 1; i < qn.Parts.Count; i++)
            {
                value = AccessMember(value, qn.Parts[i]);
            }
            return value;
        }

        private static object AccessMember(object source, string member)
        {
            if (source == null) return null;

            // Context member access
            if (source is FeelContext ctx)
                return ctx[member];

            // List projection: list.member 鈫?[item.member for item in list]
            if (source is List<object> list)
                return list.Select(item => AccessMember(item, member)).ToList();

            // Try property access via reflection for CLR objects
            var prop = source.GetType().GetProperty(member);
            if (prop != null) return FeelTypeCoercion.CoerceToFeel(prop.GetValue(source));

            // Date/time component access
            return AccessDateTimeComponent(source, member);
        }

        private static object AccessDateTimeComponent(object source, string member)
        {
            return source switch
            {
                DateOnly d => member switch
                {
                    "year" => (decimal)d.Year,
                    "month" => (decimal)d.Month,
                    "day" => (decimal)d.Day,
                    _ => null
                },
                DateTimeOffset dt => member switch
                {
                    "year" => (decimal)dt.Year,
                    "month" => (decimal)dt.Month,
                    "day" => (decimal)dt.Day,
                    "hour" => (decimal)dt.Hour,
                    "minute" => (decimal)dt.Minute,
                    "second" => (decimal)dt.Second,
                    "offset" => dt.Offset,
                    "timezone" => dt.Offset,
                    _ => null
                },
                FeelTime t => member switch
                {
                    "hour" => (decimal)t.Time.Hour,
                    "minute" => (decimal)t.Time.Minute,
                    "second" => (decimal)t.Time.Second,
                    "offset" => t.Offset,
                    "timezone" => t.Offset,
                    _ => null
                },
                TimeSpan ts => member switch
                {
                    "days" => (decimal)ts.Days,
                    "hours" => (decimal)ts.Hours,
                    "minutes" => (decimal)ts.Minutes,
                    "seconds" => (decimal)ts.Seconds,
                    _ => null
                },
                FeelYmDuration ym => member switch
                {
                    "years" => (decimal)ym.Years,
                    "months" => (decimal)ym.Months,
                    _ => null
                },
                _ => null
            };
        }

        // ==================== Arithmetic ====================

        private object EvalBinaryOp(BinaryOp bin)
        {
            var left = Evaluate(bin.Left);
            var right = Evaluate(bin.Right);

            // For Add: skip null propagation 鈥?EvalAdd handles null for string concat compat
            if (bin.Op == BinaryOperator.Add)
                return EvalAdd(left, right);

            // Null propagation for all other operations
            if (left == null || right == null) return null;

            return bin.Op switch
            {
                BinaryOperator.Sub => EvalSub(left, right),
                BinaryOperator.Mul => EvalMul(left, right),
                BinaryOperator.Div => EvalDiv(left, right),
                BinaryOperator.Mod => EvalMod(left, right),
                BinaryOperator.Exp => EvalExp(left, right),
                _ => null
            };
        }

        private static object EvalAdd(object left, object right)
        {
            // Both null 鈫?null (FEEL semantics)
            if (left == null && right == null) return null;

            // Both numeric (neither null)
            if (left != null && right != null && FeelValueComparer.IsNumeric(left) && FeelValueComparer.IsNumeric(right))
                return FeelValueComparer.ToDecimal(left) + FeelValueComparer.ToDecimal(right);

            // String + string
            if (left is string ls && right is string rs)
                return ls + rs;

            // Date/time/duration operations (require both non-null)
            if (left != null && right != null)
            {
                // Date + duration
                if (left is DateOnly date && right is FeelYmDuration ymd)
                    return ymd.AddTo(date);
                if (left is DateOnly date2 && right is TimeSpan ts1)
                    return new DateTimeOffset(date2.ToDateTime(TimeOnly.MinValue)) + ts1;
                if (left is DateTimeOffset dto && right is FeelYmDuration ymd2)
                    return ymd2.AddTo(dto);
                if (left is DateTimeOffset dto2 && right is TimeSpan ts2)
                    return dto2 + ts2;

                // Time + duration
                if (left is FeelTime ft && right is TimeSpan ts3)
                    return ft + ts3;

                // Duration + date/datetime
                if (left is FeelYmDuration ymd3 && right is DateOnly date3)
                    return ymd3.AddTo(date3);
                if (left is FeelYmDuration ymd4 && right is DateTimeOffset dto3)
                    return ymd4.AddTo(dto3);
                if (left is TimeSpan ts4 && right is DateTimeOffset dto4)
                    return dto4 + ts4;
                if (left is TimeSpan ts5 && right is FeelTime ft2)
                    return ft2 + ts5;

                // Duration + duration
                if (left is TimeSpan tsl && right is TimeSpan tsr)
                    return tsl + tsr;
                if (left is FeelYmDuration yml && right is FeelYmDuration ymr)
                    return yml + ymr;
            }

            // C# compat: string + anything, anything + string 鈫?string concatenation
            // This covers: "str" + 5, null + "str", "str" + null, bool + "str", etc.
            if (left is string || right is string)
                return (left?.ToString() ?? "") + (right?.ToString() ?? "");

            // Null propagation for non-string operations
            if (left == null || right == null) return null;

            return null;
        }

        private static object EvalSub(object left, object right)
        {
            if (FeelValueComparer.IsNumeric(left) && FeelValueComparer.IsNumeric(right))
                return FeelValueComparer.ToDecimal(left) - FeelValueComparer.ToDecimal(right);

            // Date - date = duration
            if (left is DateOnly d1 && right is DateOnly d2)
                return d1.ToDateTime(TimeOnly.MinValue) - d2.ToDateTime(TimeOnly.MinValue);
            if (left is DateTimeOffset dto1 && right is DateTimeOffset dto2)
                return dto1 - dto2;
            if (left is FeelTime ft1 && right is FeelTime ft2)
                return ft1 - ft2;

            // Date - duration
            if (left is DateOnly date && right is FeelYmDuration ymd)
                return new FeelYmDuration(-ymd.TotalMonths).AddTo(date);
            if (left is DateOnly date2 && right is TimeSpan ts1)
                return new DateTimeOffset(date2.ToDateTime(TimeOnly.MinValue)) - ts1;
            if (left is DateTimeOffset dto && right is FeelYmDuration ymd2)
                return new FeelYmDuration(-ymd2.TotalMonths).AddTo(dto);
            if (left is DateTimeOffset dto3 && right is TimeSpan ts2)
                return dto3 - ts2;
            if (left is FeelTime ft3 && right is TimeSpan ts3)
                return ft3 - ts3;

            // Duration - duration
            if (left is TimeSpan tsl && right is TimeSpan tsr)
                return tsl - tsr;
            if (left is FeelYmDuration yml && right is FeelYmDuration ymr)
                return yml - ymr;

            return null;
        }

        private static object EvalMul(object left, object right)
        {
            if (FeelValueComparer.IsNumeric(left) && FeelValueComparer.IsNumeric(right))
                return FeelValueComparer.ToDecimal(left) * FeelValueComparer.ToDecimal(right);

            // Duration * number
            if (left is TimeSpan ts && FeelValueComparer.IsNumeric(right))
                return TimeSpan.FromTicks((long)(ts.Ticks * (double)FeelValueComparer.ToDecimal(right)));
            if (FeelValueComparer.IsNumeric(left) && right is TimeSpan ts2)
                return TimeSpan.FromTicks((long)(ts2.Ticks * (double)FeelValueComparer.ToDecimal(left)));
            if (left is FeelYmDuration ym && FeelValueComparer.IsNumeric(right))
                return ym * (int)FeelValueComparer.ToDecimal(right);
            if (FeelValueComparer.IsNumeric(left) && right is FeelYmDuration ym2)
                return ym2 * (int)FeelValueComparer.ToDecimal(left);

            return null;
        }

        private static object EvalDiv(object left, object right)
        {
            if (FeelValueComparer.IsNumeric(left) && FeelValueComparer.IsNumeric(right))
            {
                var divisor = FeelValueComparer.ToDecimal(right);
                if (divisor == 0m) return null; // FEEL: division by zero = null
                return FeelValueComparer.ToDecimal(left) / divisor;
            }

            // Duration / number
            if (left is TimeSpan ts && FeelValueComparer.IsNumeric(right))
            {
                var d = (double)FeelValueComparer.ToDecimal(right);
                if (d == 0) return null;
                return TimeSpan.FromTicks((long)(ts.Ticks / d));
            }
            if (left is FeelYmDuration ym && FeelValueComparer.IsNumeric(right))
            {
                var d = (int)FeelValueComparer.ToDecimal(right);
                if (d == 0) return null;
                return ym / d;
            }

            // Duration / duration = number
            if (left is TimeSpan tsl && right is TimeSpan tsr)
            {
                if (tsr.Ticks == 0) return null;
                return (decimal)((double)tsl.Ticks / tsr.Ticks);
            }
            if (left is FeelYmDuration yml && right is FeelYmDuration ymr)
            {
                if (ymr.TotalMonths == 0) return null;
                return (decimal)yml.TotalMonths / ymr.TotalMonths;
            }

            return null;
        }

        private static object EvalMod(object left, object right)
        {
            if (FeelValueComparer.IsNumeric(left) && FeelValueComparer.IsNumeric(right))
            {
                var divisor = FeelValueComparer.ToDecimal(right);
                if (divisor == 0m) return null;
                return FeelValueComparer.ToDecimal(left) % divisor;
            }
            return null;
        }

        private static object EvalExp(object left, object right)
        {
            if (FeelValueComparer.IsNumeric(left) && FeelValueComparer.IsNumeric(right))
            {
                var b = (double)FeelValueComparer.ToDecimal(left);
                var e = (double)FeelValueComparer.ToDecimal(right);
                return (decimal)Math.Pow(b, e);
            }
            return null;
        }

        private object EvalUnaryMinus(UnaryMinus neg)
        {
            var operand = Evaluate(neg.Operand);
            if (operand == null) return null;
            if (FeelValueComparer.IsNumeric(operand))
                return -FeelValueComparer.ToDecimal(operand);
            if (operand is TimeSpan ts)
                return -ts;
            if (operand is FeelYmDuration ym)
                return -ym;
            return null;
        }

        private object EvalUnaryNot(UnaryNot not)
        {
            var operand = Evaluate(not.Operand);
            var b = FeelValueComparer.ToBool(operand);
            return FeelValueComparer.FeelNot(b);
        }

        // ==================== Comparison ====================

        private object EvalComparison(Comparison cmp)
        {
            // If left is null, this is a unary test with implicit input value
            var left = cmp.Left != null ? Evaluate(cmp.Left) : context.InputValue;
            var right = Evaluate(cmp.Right);

            if (cmp.Op == ComparisonOperator.Eq)
                return FeelValueComparer.FeelEqual(left, right);
            if (cmp.Op == ComparisonOperator.Neq)
                return FeelValueComparer.FeelNot(FeelValueComparer.FeelEqual(left, right));

            var cmpResult = FeelValueComparer.Compare(left, right);
            if (cmpResult == null) return null;

            return cmp.Op switch
            {
                ComparisonOperator.Lt => cmpResult < 0,
                ComparisonOperator.Gt => cmpResult > 0,
                ComparisonOperator.Lte => cmpResult <= 0,
                ComparisonOperator.Gte => cmpResult >= 0,
                _ => null
            };
        }

        private object EvalBetween(Between bet)
        {
            var value = Evaluate(bet.Value);
            var low = Evaluate(bet.Low);
            var high = Evaluate(bet.High);

            if (value == null || low == null || high == null) return null;

            var cmpLow = FeelValueComparer.Compare(value, low);
            var cmpHigh = FeelValueComparer.Compare(value, high);
            if (cmpLow == null || cmpHigh == null) return null;

            return cmpLow >= 0 && cmpHigh <= 0;
        }

        private object EvalIn(InNode inNode)
        {
            var value = Evaluate(inNode.Value);
            var tests = Evaluate(inNode.Tests);

            // If tests is a list, check if value is in it
            if (tests is List<object> list)
                return list.Any(item => CoerceUnaryTestResult(item, value));

            // If tests is an UnaryTests result (already evaluated), check
            if (tests is FeelRange range)
                return range.Contains(value);

            // Single value equality
            return FeelValueComparer.FeelEqual(value, tests);
        }

        private object EvalInstanceOf(InstanceOf io)
        {
            var value = Evaluate(io.Value);
            var typeName = io.TypeName;

            if (value == null) return typeName == "null";

            return typeName switch
            {
                "number" => FeelValueComparer.IsNumeric(value),
                "string" => value is string,
                "boolean" => value is bool,
                "date" => value is DateOnly,
                "time" => value is FeelTime,
                "date and time" or "dateandtime" => value is DateTimeOffset,
                "years and months duration" or "yearsandmonthsduration" => value is FeelYmDuration,
                "days and time duration" or "daysandtimeduration" => value is TimeSpan,
                "list" => value is List<object>,
                "context" => value is FeelContext,
                "range" => value is FeelRange,
                "function" => value is FeelFunction,
                "null" => false, // value is not null if we got here
                "Any" or "any" => true,
                _ => false
            };
        }

        // ==================== Logic ====================

        private object EvalConjunction(Conjunction conj)
        {
            bool? result = true;
            foreach (var operand in conj.Operands)
            {
                var val = Evaluate(operand);
                var b = FeelValueComparer.ToBool(val);
                result = FeelValueComparer.FeelAnd(result, b);
                if (result == false) return false; // short circuit
            }
            return result;
        }

        private object EvalDisjunction(Disjunction disj)
        {
            bool? result = false;
            foreach (var operand in disj.Operands)
            {
                var val = Evaluate(operand);
                var b = FeelValueComparer.ToBool(val);
                result = FeelValueComparer.FeelOr(result, b);
                if (result == true) return true; // short circuit
            }
            return result;
        }

        // ==================== Control Flow ====================

        private object EvalIf(IfNode ifn)
        {
            var condition = Evaluate(ifn.Condition);
            var b = FeelValueComparer.ToBool(condition);
            if (b == true) return Evaluate(ifn.ThenBranch);
            return Evaluate(ifn.ElseBranch);
        }

        private object EvalFor(ForNode forNode)
        {
            var results = new List<object>();
            EvalForIterations(forNode.Iterations, 0, forNode.ReturnExpression, context, results);
            return results;
        }

        private void EvalForIterations(
            IReadOnlyList<IterationContext> iterations, int iterationIndex,
            FeelAstNode returnExpr, FeelEvaluationContext scope, List<object> results)
        {
            if (iterationIndex >= iterations.Count)
            {
                var childEvaluator = new FeelEvaluator(scope, functionResolver);
                results.Add(childEvaluator.Evaluate(returnExpr));
                return;
            }

            var iteration = iterations[iterationIndex];
            var childScope = scope.CreateChildScope();
            var childEval = new FeelEvaluator(childScope, functionResolver);

            if (iteration.RangeEnd != null)
            {
                // Range iteration: for x in 1..5
                var startVal = Evaluate(iteration.ListExpression);
                var endVal = Evaluate(iteration.RangeEnd);
                if (FeelValueComparer.IsNumeric(startVal) && FeelValueComparer.IsNumeric(endVal))
                {
                    var start = (int)FeelValueComparer.ToDecimal(startVal);
                    var end = (int)FeelValueComparer.ToDecimal(endVal);
                    var step = start <= end ? 1 : -1;
                    for (var i = start; step > 0 ? i <= end : i >= end; i += step)
                    {
                        childScope.SetVariable(iteration.VariableName, (decimal)i);
                        EvalForIterations(iterations, iterationIndex + 1, returnExpr, childScope, results);
                    }
                }
            }
            else
            {
                // List iteration: for x in [1, 2, 3]
                var listVal = Evaluate(iteration.ListExpression);
                var items = CoerceToList(listVal);
                foreach (var item in items)
                {
                    childScope.SetVariable(iteration.VariableName, item);
                    EvalForIterations(iterations, iterationIndex + 1, returnExpr, childScope, results);
                }
            }
        }

        private object EvalQuantified(QuantifiedNode q)
        {
            var allResults = new List<bool>();
            EvalQuantifiedIterations(q.Iterations, 0, q.Satisfies, context, allResults);

            if (q.Quantifier == QuantifierType.Some)
                return allResults.Any(r => r);
            return allResults.All(r => r);
        }

        private void EvalQuantifiedIterations(
            IReadOnlyList<IterationContext> iterations, int iterationIndex,
            FeelAstNode satisfies, FeelEvaluationContext scope, List<bool> results)
        {
            if (iterationIndex >= iterations.Count)
            {
                var childEval = new FeelEvaluator(scope, functionResolver);
                var result = childEval.Evaluate(satisfies);
                var b = FeelValueComparer.ToBool(result);
                results.Add(b == true);
                return;
            }

            var iteration = iterations[iterationIndex];
            var childScope = scope.CreateChildScope();
            var childEval2 = new FeelEvaluator(childScope, functionResolver);

            var listVal = childEval2.Evaluate(iteration.ListExpression);
            var items = CoerceToList(listVal);
            foreach (var item in items)
            {
                childScope.SetVariable(iteration.VariableName, item);
                EvalQuantifiedIterations(iterations, iterationIndex + 1, satisfies, childScope, results);
            }
        }

        // ==================== Collections ====================

        private object EvalList(ListNode list)
        {
            return list.Elements.Select(Evaluate).ToList();
        }

        private object EvalContext(ContextNode ctx)
        {
            var result = new FeelContext();
            var childScope = context.CreateChildScope();
            var childEval = new FeelEvaluator(childScope, functionResolver);

            foreach (var entry in ctx.Entries)
            {
                var value = childEval.Evaluate(entry.Value);
                result.Put(entry.Key, value);
                // Context entries are visible to subsequent entries
                childScope.SetVariable(entry.Key, value);
            }
            return result;
        }

        private object EvalRange(RangeNode range)
        {
            var low = Evaluate(range.Low);
            var high = Evaluate(range.High);
            return new FeelRange(low, range.LowInclusive, high, range.HighInclusive);
        }

        private object EvalFilter(FilterNode filter)
        {
            var source = Evaluate(filter.Source);
            var items = CoerceToList(source);

            var filterExpr = filter.Filter;

            // Check if filter is a numeric index
            // Create a temp evaluator to check the filter value type
            var testScope = context.CreateChildScope();
            // Try evaluating the filter without item context to see if it's a simple numeric index
            if (filterExpr is NumberLiteral numLit)
            {
                var index = (int)numLit.Value;
                return GetListElement(items, index);
            }

            // Otherwise, filter the list
            var results = new List<object>();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var childScope = context.CreateChildScope();
                childScope.InputValue = item;
                childScope.SetVariable("item", item);
                // Set context variables if item is a context
                if (item is FeelContext ctx)
                {
                    foreach (var entry in ctx)
                        childScope.SetVariable(entry.Key, entry.Value);
                }
                var childEval = new FeelEvaluator(childScope, functionResolver);
                var filterVal = childEval.Evaluate(filterExpr);

                // Numeric filter result selects by index
                if (FeelValueComparer.IsNumeric(filterVal))
                {
                    var idx = (int)FeelValueComparer.ToDecimal(filterVal);
                    return GetListElement(items, idx);
                }

                // Boolean filter
                var b = FeelValueComparer.ToBool(filterVal);
                if (b == true) results.Add(item);
            }
            return results;
        }

        /// <summary>
        /// Gets element from list using FEEL 1-based indexing.
        /// Negative indices count from the end (-1 = last).
        /// Returns null for out-of-bounds.
        /// </summary>
        private static object GetListElement(List<object> list, int feelIndex)
        {
            int clrIndex;
            if (feelIndex > 0)
                clrIndex = feelIndex - 1; // 1-based to 0-based
            else if (feelIndex < 0)
                clrIndex = list.Count + feelIndex; // -1 = last
            else
                return null; // 0 is not a valid FEEL index

            if (clrIndex < 0 || clrIndex >= list.Count) return null;
            return list[clrIndex];
        }

        private object EvalPath(PathNode path)
        {
            var source = Evaluate(path.Source);
            return AccessMember(source, path.Member);
        }

        // ==================== Functions ====================

        private object EvalFunctionDefinition(FunctionDefinitionNode fd)
        {
            // Capture the current scope for closure
            var closureScope = context;
            var closureResolver = functionResolver;

            return new FeelFunction(
                "anonymous",
                fd.Parameters.ToList(),
                args =>
                {
                    var childScope = closureScope.CreateChildScope();
                    for (var i = 0; i < fd.Parameters.Count; i++)
                    {
                        childScope.SetVariable(fd.Parameters[i], i < args.Length ? args[i] : null);
                    }
                    var childEval = new FeelEvaluator(childScope, closureResolver);
                    return childEval.Evaluate(fd.Body);
                });
        }

        // Common CLR type aliases for static method resolution (C# compat)
        private static readonly Dictionary<string, Type> ClrTypeAliases = new()
        {
            { "double", typeof(double) },
            { "float", typeof(float) },
            { "int", typeof(int) },
            { "long", typeof(long) },
            { "decimal", typeof(decimal) },
            { "string", typeof(string) },
            { "bool", typeof(bool) },
            { "Math", typeof(Math) },
            { "Convert", typeof(Convert) },
            { "Int32", typeof(int) },
            { "Int64", typeof(long) },
            { "Double", typeof(double) },
            { "Single", typeof(float) },
            { "Decimal", typeof(decimal) },
            { "String", typeof(string) },
            { "Boolean", typeof(bool) },
            { "DateTime", typeof(DateTime) },
            { "DateTimeOffset", typeof(DateTimeOffset) },
        };

        private object EvalFunctionInvocation(FunctionInvocation fi)
        {
            // Handle CLR method calls: obj.Method(args) or Type.StaticMethod(args)
            if (fi.Function is PathNode pathNode)
            {
                var source = Evaluate(pathNode.Source);
                if (source != null)
                {
                    var args = fi.Arguments.Select(Evaluate).ToArray();
                    var method = source.GetType().GetMethod(pathNode.Member,
                        args.Length == 0 ? Type.EmptyTypes :
                        args.Select(a => a?.GetType() ?? typeof(object)).ToArray());
                    // Try parameterless overload if exact match failed
                    method ??= source.GetType().GetMethod(pathNode.Member, Type.EmptyTypes);
                    if (method != null)
                        return FeelTypeCoercion.CoerceToFeel(method.Invoke(source, method.GetParameters().Length == 0 ? Array.Empty<object>() : args));
                }
                // CLR static method calls: double.Parse(x), Math.Abs(x), etc.
                else if (pathNode.Source is NameNode clrTypeName &&
                         ClrTypeAliases.TryGetValue(clrTypeName.Name, out var clrType))
                {
                    var args = fi.Arguments.Select(Evaluate).ToArray();
                    var argTypes = args.Select(a => a?.GetType() ?? typeof(object)).ToArray();
                    var method = clrType.GetMethod(pathNode.Member, argTypes);
                    if (method != null && method.IsStatic)
                        return FeelTypeCoercion.CoerceToFeel(method.Invoke(null, args));
                }
            }

            var fnValue = Evaluate(fi.Function);
            var evalArgs = fi.Arguments.Select(Evaluate).ToArray();

            // If it resolved to a FeelFunction, invoke directly
            if (fnValue is FeelFunction fn)
                return fn.Invoke(evalArgs);

            // If the function node is a name, try the function resolver
            if (fi.Function is NameNode nameNode && functionResolver != null)
                return functionResolver(nameNode.Name, evalArgs);

            // If fnValue is null and function is a name, try the resolver
            if (fnValue == null && fi.Function is NameNode name2 && functionResolver != null)
                return functionResolver(name2.Name, evalArgs);

            return null;
        }

        private object EvalNamedFunctionInvocation(NamedFunctionInvocation nfi)
        {
            var fnValue = Evaluate(nfi.Function);
            var namedArgs = new Dictionary<string, object>();
            foreach (var (name, valueNode) in nfi.Arguments)
            {
                namedArgs[name] = Evaluate(valueNode);
            }

            if (fnValue is FeelFunction fn)
                return fn.InvokeNamed(namedArgs);

            // Named args with function resolver 鈥?pass as positional
            if (nfi.Function is NameNode nameNode && functionResolver != null)
            {
                var positionalArgs = namedArgs.Values.ToArray();
                return functionResolver(nameNode.Name, positionalArgs);
            }

            return null;
        }

        // ==================== Unary Tests ====================

        private object EvalUnaryTests(UnaryTests ut)
        {
            var inputValue = context.InputValue;
            var anyMatch = false;

            foreach (var test in ut.Tests)
            {
                if (CoerceUnaryTestResultForNode(test, Evaluate(test), inputValue))
                {
                    anyMatch = true;
                    break;
                }
            }

            return ut.IsNegated ? !anyMatch : anyMatch;
        }

        // ==================== Date/Time Parsing ====================

        private static object ParseDateTimeLiteral(string raw)
        {
            // Try ISO date: 2024-01-15
            if (DateOnly.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            // Try ISO time: 10:30:00, 10:30:00+05:00, 10:30:00Z
            if (TryParseFeelTime(raw, out var time))
                return time;

            // Try ISO date-time: 2024-01-15T10:30:00, etc.
            if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
                return dto;

            // Try duration: P1Y2M, P3DT4H, PT1H30M, etc.
            if (TryParseDuration(raw, out var duration))
                return duration;

            return null; // Unknown format
        }

        private static bool TryParseFeelTime(string raw, out FeelTime result)
        {
            result = default;

            // Match time patterns: HH:mm, HH:mm:ss, HH:mm:ss.fff, with optional offset
            var match = Regex.Match(raw, @"^(\d{1,2}):(\d{2})(?::(\d{2})(?:\.(\d+))?)?(?:(Z)|([+-]\d{2}:\d{2}))?$");
            if (!match.Success) return false;

            var hour = int.Parse(match.Groups[1].Value);
            var minute = int.Parse(match.Groups[2].Value);
            var second = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
            var ms = match.Groups[4].Success ? int.Parse(match.Groups[4].Value.PadRight(3, '0').Substring(0, 3)) : 0;

            TimeSpan? offset = null;
            if (match.Groups[5].Success) // Z
                offset = TimeSpan.Zero;
            else if (match.Groups[6].Success) // +/-HH:mm
                offset = TimeSpan.Parse(match.Groups[6].Value);

            result = new FeelTime(hour, minute, second, ms, offset);
            return true;
        }

        private static bool TryParseDuration(string raw, out object duration)
        {
            duration = null;
            if (string.IsNullOrEmpty(raw)) return false;

            var negative = raw.StartsWith("-");
            var s = negative ? raw.Substring(1) : raw;
            if (!s.StartsWith("P")) return false;
            s = s.Substring(1); // Remove P

            // Check if it's a years-and-months duration (no T, only Y and M)
            if (!s.Contains('T') && !s.Contains('D') && !s.Contains('H') && !s.Contains('S'))
            {
                var years = 0;
                var months = 0;
                var yMatch = Regex.Match(s, @"(\d+)Y");
                if (yMatch.Success) years = int.Parse(yMatch.Groups[1].Value);
                var mMatch = Regex.Match(s, @"(\d+)M");
                if (mMatch.Success) months = int.Parse(mMatch.Groups[1].Value);
                var total = years * 12 + months;
                if (negative) total = -total;
                duration = new FeelYmDuration(total);
                return true;
            }

            // Days-and-time duration
            var days = 0;
            var hours = 0;
            var minutes = 0;
            var seconds = 0;

            var dMatch = Regex.Match(s, @"(\d+)D");
            if (dMatch.Success) days = int.Parse(dMatch.Groups[1].Value);

            var tIdx = s.IndexOf('T');
            if (tIdx >= 0)
            {
                var timePart = s.Substring(tIdx + 1);
                var hMatch = Regex.Match(timePart, @"(\d+)H");
                if (hMatch.Success) hours = int.Parse(hMatch.Groups[1].Value);
                var minMatch = Regex.Match(timePart, @"(\d+)M");
                if (minMatch.Success) minutes = int.Parse(minMatch.Groups[1].Value);
                var sMatch = Regex.Match(timePart, @"(\d+)S");
                if (sMatch.Success) seconds = int.Parse(sMatch.Groups[1].Value);
            }

            var ts = new TimeSpan(days, hours, minutes, seconds);
            if (negative) ts = -ts;
            duration = ts;
            return true;
        }

        // ==================== Helpers ====================

        private static List<object> CoerceToList(object value)
        {
            if (value == null) return new List<object>();
            if (value is List<object> list) return list;
            // Singleton list coercion
            return new List<object> { value };
        }
    }

    /// <summary>
    /// Exception thrown when FEEL evaluation fails.
    /// </summary>
    public class FeelEvaluationException : Exception
    {
        public FeelEvaluationException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }
}

