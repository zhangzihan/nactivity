using System.Collections.Generic;

namespace Sys.Rules.DmnEngine.Feel.Ast
{
    /// <summary>
    /// Base class for all FEEL AST nodes
    /// </summary>
    public abstract class FeelAstNode { }

    // ==================== Literals ====================

    public sealed class NumberLiteral : FeelAstNode
    {
        public decimal Value { get; }
        public NumberLiteral(decimal value) => Value = value;
        public override string ToString() => Value.ToString();
    }

    public sealed class StringLiteral : FeelAstNode
    {
        public string Value { get; }
        public StringLiteral(string value) => Value = value;
        public override string ToString() => $"\"{Value}\"";
    }

    public sealed class BoolLiteral : FeelAstNode
    {
        public bool Value { get; }
        public BoolLiteral(bool value) => Value = value;
        public override string ToString() => Value ? "true" : "false";
    }

    public sealed class NullLiteral : FeelAstNode
    {
        public static readonly NullLiteral Instance = new();
        public override string ToString() => "null";
    }

    /// <summary>
    /// Date/time/duration literal from @"..." or date(), time(), etc.
    /// The raw string is stored; interpretation happens at evaluation time.
    /// </summary>
    public sealed class DateTimeLiteral : FeelAstNode
    {
        public string RawValue { get; }
        public DateTimeLiteral(string rawValue) => RawValue = rawValue;
        public override string ToString() => $"@\"{RawValue}\"";
    }

    // ==================== Names ====================

    public sealed class NameNode : FeelAstNode
    {
        public string Name { get; }
        public NameNode(string name) => Name = name;
        public override string ToString() => Name;
    }

    public sealed class QualifiedNameNode : FeelAstNode
    {
        public IReadOnlyList<string> Parts { get; }
        public QualifiedNameNode(IReadOnlyList<string> parts) => Parts = parts;
        public override string ToString() => string.Join(".", Parts);
    }

    // ==================== Operators ====================

    public enum BinaryOperator
    {
        Add, Sub, Mul, Div, Exp, Mod
    }

    public sealed class BinaryOp : FeelAstNode
    {
        public BinaryOperator Op { get; }
        public FeelAstNode Left { get; }
        public FeelAstNode Right { get; }
        public BinaryOp(BinaryOperator op, FeelAstNode left, FeelAstNode right)
        {
            Op = op; Left = left; Right = right;
        }
    }

    public sealed class UnaryMinus : FeelAstNode
    {
        public FeelAstNode Operand { get; }
        public UnaryMinus(FeelAstNode operand) => Operand = operand;
    }

    public sealed class UnaryNot : FeelAstNode
    {
        public FeelAstNode Operand { get; }
        public UnaryNot(FeelAstNode operand) => Operand = operand;
    }

    public enum ComparisonOperator
    {
        Lt, Gt, Lte, Gte, Eq, Neq
    }

    public sealed class Comparison : FeelAstNode
    {
        public ComparisonOperator Op { get; }
        public FeelAstNode Left { get; }
        public FeelAstNode Right { get; }
        public Comparison(ComparisonOperator op, FeelAstNode left, FeelAstNode right)
        {
            Op = op; Left = left; Right = right;
        }
    }

    public sealed class Between : FeelAstNode
    {
        public FeelAstNode Value { get; }
        public FeelAstNode Low { get; }
        public FeelAstNode High { get; }
        public Between(FeelAstNode value, FeelAstNode low, FeelAstNode high)
        {
            Value = value; Low = low; High = high;
        }
    }

    public sealed class InNode : FeelAstNode
    {
        public FeelAstNode Value { get; }
        public FeelAstNode Tests { get; }
        public InNode(FeelAstNode value, FeelAstNode tests)
        {
            Value = value; Tests = tests;
        }
    }

    public sealed class InstanceOf : FeelAstNode
    {
        public FeelAstNode Value { get; }
        public string TypeName { get; }
        public InstanceOf(FeelAstNode value, string typeName)
        {
            Value = value; TypeName = typeName;
        }
    }

    // ==================== Logic ====================

    public sealed class Conjunction : FeelAstNode
    {
        public IReadOnlyList<FeelAstNode> Operands { get; }
        public Conjunction(IReadOnlyList<FeelAstNode> operands) => Operands = operands;
    }

    public sealed class Disjunction : FeelAstNode
    {
        public IReadOnlyList<FeelAstNode> Operands { get; }
        public Disjunction(IReadOnlyList<FeelAstNode> operands) => Operands = operands;
    }

    // ==================== Control Flow ====================

    public sealed class IfNode : FeelAstNode
    {
        public FeelAstNode Condition { get; }
        public FeelAstNode ThenBranch { get; }
        public FeelAstNode ElseBranch { get; }
        public IfNode(FeelAstNode condition, FeelAstNode thenBranch, FeelAstNode elseBranch)
        {
            Condition = condition; ThenBranch = thenBranch; ElseBranch = elseBranch;
        }
    }

    public sealed class ForNode : FeelAstNode
    {
        public IReadOnlyList<IterationContext> Iterations { get; }
        public FeelAstNode ReturnExpression { get; }
        public ForNode(IReadOnlyList<IterationContext> iterations, FeelAstNode returnExpression)
        {
            Iterations = iterations; ReturnExpression = returnExpression;
        }
    }

    public sealed class IterationContext
    {
        public string VariableName { get; }
        public FeelAstNode ListExpression { get; }
        public FeelAstNode RangeEnd { get; } // null if not a range
        public IterationContext(string variableName, FeelAstNode listExpression, FeelAstNode rangeEnd = null)
        {
            VariableName = variableName; ListExpression = listExpression; RangeEnd = rangeEnd;
        }
    }

    public enum QuantifierType { Some, Every }

    public sealed class QuantifiedNode : FeelAstNode
    {
        public QuantifierType Quantifier { get; }
        public IReadOnlyList<IterationContext> Iterations { get; }
        public FeelAstNode Satisfies { get; }
        public QuantifiedNode(QuantifierType quantifier, IReadOnlyList<IterationContext> iterations, FeelAstNode satisfies)
        {
            Quantifier = quantifier; Iterations = iterations; Satisfies = satisfies;
        }
    }

    // ==================== Collections ====================

    public sealed class ListNode : FeelAstNode
    {
        public IReadOnlyList<FeelAstNode> Elements { get; }
        public ListNode(IReadOnlyList<FeelAstNode> elements) => Elements = elements;
    }

    public sealed class ContextNode : FeelAstNode
    {
        public IReadOnlyList<ContextEntryNode> Entries { get; }
        public ContextNode(IReadOnlyList<ContextEntryNode> entries) => Entries = entries;
    }

    public sealed class ContextEntryNode
    {
        public string Key { get; }
        public FeelAstNode Value { get; }
        public ContextEntryNode(string key, FeelAstNode value) { Key = key; Value = value; }
    }

    public sealed class RangeNode : FeelAstNode
    {
        public FeelAstNode Low { get; }
        public FeelAstNode High { get; }
        public bool LowInclusive { get; }
        public bool HighInclusive { get; }
        public RangeNode(FeelAstNode low, bool lowInclusive, FeelAstNode high, bool highInclusive)
        {
            Low = low; LowInclusive = lowInclusive; High = high; HighInclusive = highInclusive;
        }
    }

    public sealed class FilterNode : FeelAstNode
    {
        public FeelAstNode Source { get; }
        public FeelAstNode Filter { get; }
        public FilterNode(FeelAstNode source, FeelAstNode filter) { Source = source; Filter = filter; }
    }

    public sealed class PathNode : FeelAstNode
    {
        public FeelAstNode Source { get; }
        public string Member { get; }
        public PathNode(FeelAstNode source, string member) { Source = source; Member = member; }
    }

    // ==================== Functions ====================

    public sealed class FunctionDefinitionNode : FeelAstNode
    {
        public IReadOnlyList<string> Parameters { get; }
        public FeelAstNode Body { get; }
        public bool IsExternal { get; }
        public FunctionDefinitionNode(IReadOnlyList<string> parameters, FeelAstNode body, bool isExternal = false)
        {
            Parameters = parameters; Body = body; IsExternal = isExternal;
        }
    }

    public sealed class FunctionInvocation : FeelAstNode
    {
        public FeelAstNode Function { get; }
        public IReadOnlyList<FeelAstNode> Arguments { get; }
        public FunctionInvocation(FeelAstNode function, IReadOnlyList<FeelAstNode> arguments)
        {
            Function = function; Arguments = arguments;
        }
    }

    public sealed class NamedFunctionInvocation : FeelAstNode
    {
        public FeelAstNode Function { get; }
        public IReadOnlyList<(string Name, FeelAstNode Value)> Arguments { get; }
        public NamedFunctionInvocation(FeelAstNode function, IReadOnlyList<(string Name, FeelAstNode Value)> arguments)
        {
            Function = function; Arguments = arguments;
        }
    }

    // ==================== Unary Tests ====================

    public sealed class UnaryTests : FeelAstNode
    {
        public IReadOnlyList<FeelAstNode> Tests { get; }
        public bool IsNegated { get; }
        public UnaryTests(IReadOnlyList<FeelAstNode> tests, bool isNegated = false)
        {
            Tests = tests; IsNegated = isNegated;
        }
    }

    /// <summary>
    /// The dash '-' in a decision table input entry means "any value matches"
    /// </summary>
    public sealed class Dash : FeelAstNode
    {
        public static readonly Dash Instance = new();
        public override string ToString() => "-";
    }
}

