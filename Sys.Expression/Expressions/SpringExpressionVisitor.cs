using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Spring.Context.Support;
using Spring.Expressions.Parser.Antlr4Gen;

namespace Spring.Expressions
{
    /// <summary>
    /// Visitor that converts ANTLR4 parse tree into Spring Expression AstNode hierarchy.
    /// Replaces the ANTLR2 AST construction that was previously done by the parser + ASTFactory.
    /// </summary>
    internal class SpringExpressionVisitor : ExpressionBaseVisitor<AstNode>
    {
        /// <summary>Creates a node of the specified type with the given text.</summary>
        private static T CreateNode<T>(string text) where T : AstNode, new()
        {
            T node = new T();
            node.setText(text);
            return node;
        }

        /// <summary>Builds a left-associative binary operator chain.</summary>
        private AstNode BuildBinaryChain(AstNode first, List<AstNode> operators, List<AstNode> operands)
        {
            AstNode result = first;
            for (int i = 0; i < operators.Count; i++)
            {
                operators[i].addChild(result);
                operators[i].addChild(operands[i]);
                result = operators[i];
            }
            return result;
        }

        // ========== Top-level rules ==========

        public override AstNode VisitExpr([NotNull] ExpressionParser.ExprContext context)
        {
            return Visit(context.expression());
        }

        public override AstNode VisitExprList([NotNull] ExpressionParser.ExprListContext context)
        {
            var node = CreateNode<ExpressionListNode>("expressionList");
            foreach (var expr in context.expression())
            {
                node.addChild(Visit(expr));
            }
            return node;
        }

        // ========== Expression rules ==========

        public override AstNode VisitExpression([NotNull] ExpressionParser.ExpressionContext context)
        {
            var logicalOrs = context.logicalOrExpression();

            if (context.ASSIGN() != null)
            {
                // a = b
                var assign = CreateNode<AssignNode>("=");
                assign.addChild(Visit(logicalOrs[0]));
                assign.addChild(Visit(logicalOrs[1]));
                return assign;
            }
            else if (context.DEFAULT() != null)
            {
                // a ?? b
                var defaultNode = CreateNode<DefaultNode>("??");
                defaultNode.addChild(Visit(logicalOrs[0]));
                defaultNode.addChild(Visit(logicalOrs[1]));
                return defaultNode;
            }
            else if (context.QMARK() != null)
            {
                // a ? b : c
                var exprs = context.expression();
                var ternary = CreateNode<TernaryNode>("?");
                ternary.addChild(Visit(logicalOrs[0]));
                ternary.addChild(Visit(exprs[0]));
                ternary.addChild(Visit(exprs[1]));
                return ternary;
            }

            // simple expression
            return Visit(logicalOrs[0]);
        }

        public override AstNode VisitParenExpr([NotNull] ExpressionParser.ParenExprContext context)
        {
            return Visit(context.expression());
        }

        public override AstNode VisitLogicalOrExpression([NotNull] ExpressionParser.LogicalOrExpressionContext context)
        {
            var items = context.logicalXorExpression();
            var first = Visit(items[0]);
            var ors = context.OR();

            if (ors.Length == 0) return first;

            var operators = new List<AstNode>();
            var operands = new List<AstNode>();
            for (int i = 0; i < ors.Length; i++)
            {
                operators.Add(CreateNode<OpOR>("or"));
                operands.Add(Visit(items[i + 1]));
            }
            return BuildBinaryChain(first, operators, operands);
        }

        public override AstNode VisitLogicalXorExpression([NotNull] ExpressionParser.LogicalXorExpressionContext context)
        {
            var items = context.logicalAndExpression();
            var first = Visit(items[0]);
            var xors = context.XOR();

            if (xors.Length == 0) return first;

            var operators = new List<AstNode>();
            var operands = new List<AstNode>();
            for (int i = 0; i < xors.Length; i++)
            {
                operators.Add(CreateNode<OpXOR>("xor"));
                operands.Add(Visit(items[i + 1]));
            }
            return BuildBinaryChain(first, operators, operands);
        }

        public override AstNode VisitLogicalAndExpression([NotNull] ExpressionParser.LogicalAndExpressionContext context)
        {
            var items = context.relationalExpression();
            var first = Visit(items[0]);
            var ands = context.AND();

            if (ands.Length == 0) return first;

            var operators = new List<AstNode>();
            var operands = new List<AstNode>();
            for (int i = 0; i < ands.Length; i++)
            {
                operators.Add(CreateNode<OpAND>("and"));
                operands.Add(Visit(items[i + 1]));
            }
            return BuildBinaryChain(first, operators, operands);
        }

        public override AstNode VisitRelationalExpression([NotNull] ExpressionParser.RelationalExpressionContext context)
        {
            var sums = context.sumExpr();
            var left = Visit(sums[0]);

            if (context.relationalOperator() != null)
            {
                string opText = context.relationalOperator().GetText();
                AstNode opNode = CreateRelationalOperatorNode(opText);
                opNode.addChild(left);
                opNode.addChild(Visit(sums[1]));
                return opNode;
            }

            return left;
        }

        private static AstNode CreateRelationalOperatorNode(string op)
        {
            return op switch
            {
                "==" => CreateNode<OpEqual>("=="),
                "!=" => CreateNode<OpNotEqual>("!="),
                "<" => CreateNode<OpLess>("<"),
                "<=" => CreateNode<OpLessOrEqual>("<="),
                ">" => CreateNode<OpGreater>(">"),
                ">=" => CreateNode<OpGreaterOrEqual>(">="),
                "in" => CreateNode<OpIn>("in"),
                "is" => CreateNode<OpIs>("is"),
                "between" => CreateNode<OpBetween>("between"),
                "like" => CreateNode<OpLike>("like"),
                "matches" => CreateNode<OpMatches>("matches"),
                _ => throw new ArgumentException($"Node type for operator '{op}' is not defined.")
            };
        }

        public override AstNode VisitSumExpr([NotNull] ExpressionParser.SumExprContext context)
        {
            var prods = context.prodExpr();
            var first = Visit(prods[0]);

            if (prods.Length == 1) return first;

            var operators = new List<AstNode>();
            var operands = new List<AstNode>();

            // Determine operator type from the children between prodExpr nodes
            for (int i = 1; i < prods.Length; i++)
            {
                // Find the operator token between prods[i-1] and prods[i]
                int prevStop = prods[i - 1].Stop.TokenIndex;
                int currStart = prods[i].Start.TokenIndex;

                bool isPlus = false;
                foreach (var t in context.PLUS())
                {
                    if (t.Symbol.TokenIndex > prevStop && t.Symbol.TokenIndex < currStart)
                    { isPlus = true; break; }
                }

                if (isPlus)
                    operators.Add(CreateNode<OpADD>("+"));
                else
                    operators.Add(CreateNode<OpSUBTRACT>("-"));

                operands.Add(Visit(prods[i]));
            }
            return BuildBinaryChain(first, operators, operands);
        }

        public override AstNode VisitProdExpr([NotNull] ExpressionParser.ProdExprContext context)
        {
            var pows = context.powExpr();
            var first = Visit(pows[0]);

            if (pows.Length == 1) return first;

            var operators = new List<AstNode>();
            var operands = new List<AstNode>();

            for (int i = 1; i < pows.Length; i++)
            {
                int prevStop = pows[i - 1].Stop.TokenIndex;
                int currStart = pows[i].Start.TokenIndex;

                string opType = null;
                foreach (var t in context.STAR())
                {
                    if (t.Symbol.TokenIndex > prevStop && t.Symbol.TokenIndex < currStart)
                    { opType = "*"; break; }
                }
                if (opType == null)
                {
                    foreach (var t in context.DIV())
                    {
                        if (t.Symbol.TokenIndex > prevStop && t.Symbol.TokenIndex < currStart)
                        { opType = "/"; break; }
                    }
                }
                if (opType == null)
                {
                    foreach (var t in context.MOD())
                    {
                        if (t.Symbol.TokenIndex > prevStop && t.Symbol.TokenIndex < currStart)
                        { opType = "%"; break; }
                    }
                }

                operators.Add(opType switch
                {
                    "*" => CreateNode<OpMULTIPLY>("*"),
                    "/" => CreateNode<OpDIVIDE>("/"),
                    "%" => CreateNode<OpMODULUS>("%"),
                    _ => throw new InvalidOperationException("Unknown product operator")
                });
                operands.Add(Visit(pows[i]));
            }
            return BuildBinaryChain(first, operators, operands);
        }

        public override AstNode VisitPowExpr([NotNull] ExpressionParser.PowExprContext context)
        {
            var unaries = context.unaryExpression();
            var baseExpr = Visit(unaries[0]);
            if (unaries.Length > 1)
            {
                var pow = CreateNode<OpPOWER>("^");
                pow.addChild(baseExpr);
                pow.addChild(Visit(unaries[1]));
                return pow;
            }
            return baseExpr;
        }

        public override AstNode VisitUnaryExpression([NotNull] ExpressionParser.UnaryExpressionContext context)
        {
            if (context.PLUS() != null)
            {
                var op = CreateNode<OpUnaryPlus>("+");
                op.addChild(Visit(context.unaryExpression()));
                return op;
            }
            if (context.MINUS() != null)
            {
                var op = CreateNode<OpUnaryMinus>("-");
                op.addChild(Visit(context.unaryExpression()));
                return op;
            }
            if (context.BANG() != null)
            {
                var op = CreateNode<OpNOT>("!");
                op.addChild(Visit(context.unaryExpression()));
                return op;
            }
            return Visit(context.primaryExpression());
        }

        // ========== Primary expression ==========

        public override AstNode VisitPrimaryExpression([NotNull] ExpressionParser.PrimaryExpressionContext context)
        {
            var expr = CreateNode<Expression>("expression");
            expr.addChild(Visit(context.startNode()));

            foreach (var nodeCtx in context.node())
            {
                var visited = Visit(nodeCtx);
                if (visited != null)
                    expr.addChild(visited);
            }

            return expr;
        }

        public override AstNode VisitStartNode([NotNull] ExpressionParser.StartNodeContext context)
        {
            if (context.exprList() != null) return Visit(context.exprList());
            if (context.parenExpr() != null) return Visit(context.parenExpr());
            if (context.methodOrProperty() != null) return Visit(context.methodOrProperty());
            if (context.functionOrVar() != null) return Visit(context.functionOrVar());
            if (context.localFunctionOrVar() != null) return Visit(context.localFunctionOrVar());
            if (context.reference() != null) return Visit(context.reference());
            if (context.indexer() != null) return Visit(context.indexer());
            if (context.literal() != null) return Visit(context.literal());
            if (context.type() != null) return Visit(context.type());
            if (context.constructor() != null) return Visit(context.constructor());
            if (context.projection() != null) return Visit(context.projection());
            if (context.selection() != null) return Visit(context.selection());
            if (context.firstSelection() != null) return Visit(context.firstSelection());
            if (context.lastSelection() != null) return Visit(context.lastSelection());
            if (context.listInitializer() != null) return Visit(context.listInitializer());
            if (context.mapInitializer() != null) return Visit(context.mapInitializer());
            if (context.lambda() != null) return Visit(context.lambda());
            if (context.attribute() != null) return Visit(context.attribute());

            throw new InvalidOperationException("Unknown startNode alternative");
        }

        public override AstNode VisitNode([NotNull] ExpressionParser.NodeContext context)
        {
            if (context.methodOrProperty() != null) return Visit(context.methodOrProperty());
            if (context.indexer() != null) return Visit(context.indexer());
            if (context.projection() != null) return Visit(context.projection());
            if (context.selection() != null) return Visit(context.selection());
            if (context.firstSelection() != null) return Visit(context.firstSelection());
            if (context.lastSelection() != null) return Visit(context.lastSelection());
            if (context.exprList() != null) return Visit(context.exprList());
            if (context.DOT() != null) return null; // DOT is consumed but no node created
            throw new InvalidOperationException("Unknown node alternative");
        }

        // ========== Function/Variable ==========

        public override AstNode VisitFunctionOrVar([NotNull] ExpressionParser.FunctionOrVarContext context)
        {
            if (context.function() != null) return Visit(context.function());
            return Visit(context.var());
        }

        public override AstNode VisitFunction([NotNull] ExpressionParser.FunctionContext context)
        {
            var node = CreateNode<FunctionNode>(context.ID().GetText());
            AddArgsFromMethodArgs(context.methodArgs(), node);
            return node;
        }

        public override AstNode VisitVar([NotNull] ExpressionParser.VarContext context)
        {
            return CreateNode<VariableNode>(context.ID().GetText());
        }

        public override AstNode VisitLocalFunctionOrVar([NotNull] ExpressionParser.LocalFunctionOrVarContext context)
        {
            if (context.localFunction() != null) return Visit(context.localFunction());
            return Visit(context.localVar());
        }

        public override AstNode VisitLocalFunction([NotNull] ExpressionParser.LocalFunctionContext context)
        {
            var node = CreateNode<LocalFunctionNode>(context.ID().GetText());
            AddArgsFromMethodArgs(context.methodArgs(), node);
            return node;
        }

        public override AstNode VisitLocalVar([NotNull] ExpressionParser.LocalVarContext context)
        {
            return CreateNode<LocalVariableNode>(context.ID().GetText());
        }

        // ========== Method/Property ==========

        public override AstNode VisitMethodOrProperty([NotNull] ExpressionParser.MethodOrPropertyContext context)
        {
            if (context.methodArgs() != null)
            {
                var node = CreateNode<MethodNode>(context.ID().GetText());
                AddArgsFromMethodArgs(context.methodArgs(), node);
                return node;
            }
            return Visit(context.property());
        }

        public override AstNode VisitMethodArgs([NotNull] ExpressionParser.MethodArgsContext context)
        {
            // Should not be called directly; use AddArgsFromMethodArgs instead
            throw new InvalidOperationException("Use AddArgsFromMethodArgs");
        }

        private void AddArgsFromMethodArgs(ExpressionParser.MethodArgsContext context, AstNode parent)
        {
            if (context == null) return;
            foreach (var arg in context.argument())
            {
                parent.addChild(Visit(arg));
            }
        }

        public override AstNode VisitProperty([NotNull] ExpressionParser.PropertyContext context)
        {
            return CreateNode<PropertyOrFieldNode>(context.ID().GetText());
        }

        // ========== Reference ==========

        public override AstNode VisitReference([NotNull] ExpressionParser.ReferenceContext context)
        {
            var node = CreateNode<ReferenceNode>("ref");
            var names = context.quotableName();
            if (names.Length == 2)
            {
                // ref with namespace: @(ns:name)
                node.addChild(Visit(names[0]));
                node.addChild(Visit(names[1]));
            }
            else
            {
                // local ref: @(name)
                node.addChild(null); // no namespace - placeholder
                node.addChild(Visit(names[0]));
            }
            return node;
        }

        // ========== Indexer ==========

        public override AstNode VisitIndexer([NotNull] ExpressionParser.IndexerContext context)
        {
            var node = CreateNode<IndexerNode>("[");
            foreach (var arg in context.argument())
            {
                node.addChild(Visit(arg));
            }
            return node;
        }

        // ========== Projection/Selection ==========

        public override AstNode VisitProjection([NotNull] ExpressionParser.ProjectionContext context)
        {
            var node = CreateNode<ProjectionNode>("!{");
            node.addChild(Visit(context.expression()));
            return node;
        }

        public override AstNode VisitSelection([NotNull] ExpressionParser.SelectionContext context)
        {
            var node = CreateNode<SelectionNode>("?{");
            foreach (var expr in context.expression())
            {
                node.addChild(Visit(expr));
            }
            return node;
        }

        public override AstNode VisitFirstSelection([NotNull] ExpressionParser.FirstSelectionContext context)
        {
            var node = CreateNode<SelectionFirstNode>("^{");
            node.addChild(Visit(context.expression()));
            return node;
        }

        public override AstNode VisitLastSelection([NotNull] ExpressionParser.LastSelectionContext context)
        {
            var node = CreateNode<SelectionLastNode>("${");
            node.addChild(Visit(context.expression()));
            return node;
        }

        // ========== Type ==========

        public override AstNode VisitType([NotNull] ExpressionParser.TypeContext context)
        {
            var nameNode = Visit(context.qualifiedId());
            string typeName = nameNode.getText();
            var node = CreateNode<TypeNode>(typeName);
            node.addChild(nameNode);
            return node;
        }

        // ========== Attribute ==========

        public override AstNode VisitAttribute([NotNull] ExpressionParser.AttributeContext context)
        {
            var nameNode = Visit(context.qualifiedId());
            var node = CreateNode<AttributeNode>(nameNode.getText());
            node.addChild(nameNode);

            if (context.ctorArgs() != null)
            {
                AddArgsFromCtorArgs(context.ctorArgs(), node);
            }

            return node;
        }

        // ========== Lambda ==========

        public override AstNode VisitLambda([NotNull] ExpressionParser.LambdaContext context)
        {
            var node = CreateNode<LambdaExpressionNode>("lambda");

            if (context.argList() != null)
            {
                // Create an args wrapper node
                var argsNode = CreateNode<SpringAST>("args");
                foreach (var id in context.argList().ID())
                {
                    argsNode.addChild(CreateNode<QualifiedIdentifier>(id.GetText()));
                }
                node.addChild(argsNode);
            }

            node.addChild(Visit(context.expression()));

            return node;
        }

        // ========== Constructor ==========

        public override AstNode VisitConstructor([NotNull] ExpressionParser.ConstructorContext context)
        {
            var nameNode = Visit(context.qualifiedId());

            if (context.ctorArgs() != null)
            {
                // Object constructor: new Type(args)
                var node = CreateNode<ConstructorNode>(nameNode.getText());
                node.addChild(nameNode);
                AddArgsFromCtorArgs(context.ctorArgs(), node);
                return node;
            }
            else
            {
                // Array constructor: new Type[ranks]
                var node = CreateNode<ArrayConstructorNode>(nameNode.getText());
                node.addChild(Visit(context.arrayRank()));
                if (context.listInitializer() != null)
                {
                    node.addChild(Visit(context.listInitializer()));
                }
                return node;
            }
        }

        public override AstNode VisitArrayRank([NotNull] ExpressionParser.ArrayRankContext context)
        {
            var node = CreateNode<SpringAST>("[");
            foreach (var expr in context.expression())
            {
                node.addChild(Visit(expr));
            }
            return node;
        }

        // ========== Initializers ==========

        public override AstNode VisitListInitializer([NotNull] ExpressionParser.ListInitializerContext context)
        {
            var node = CreateNode<ListInitializerNode>("{");
            foreach (var expr in context.expression())
            {
                node.addChild(Visit(expr));
            }
            return node;
        }

        public override AstNode VisitMapInitializer([NotNull] ExpressionParser.MapInitializerContext context)
        {
            var node = CreateNode<MapInitializerNode>("#{");
            foreach (var entry in context.mapEntry())
            {
                node.addChild(Visit(entry));
            }
            return node;
        }

        public override AstNode VisitMapEntry([NotNull] ExpressionParser.MapEntryContext context)
        {
            var node = CreateNode<MapEntryNode>("entry");
            var exprs = context.expression();
            node.addChild(Visit(exprs[0]));
            node.addChild(Visit(exprs[1]));
            return node;
        }

        // ========== Arguments ==========

        public override AstNode VisitCtorArgs([NotNull] ExpressionParser.CtorArgsContext context)
        {
            throw new InvalidOperationException("Use AddArgsFromCtorArgs instead");
        }

        private void AddArgsFromCtorArgs(ExpressionParser.CtorArgsContext context, AstNode parent)
        {
            if (context == null) return;
            foreach (var arg in context.namedArgument())
            {
                parent.addChild(VisitNamedArgument(arg));
            }
        }

        public override AstNode VisitArgument([NotNull] ExpressionParser.ArgumentContext context)
        {
            return Visit(context.expression());
        }

        public override AstNode VisitNamedArgument([NotNull] ExpressionParser.NamedArgumentContext context)
        {
            if (context.ASSIGN() != null)
            {
                var node = CreateNode<NamedArgumentNode>(context.ID().GetText());
                node.addChild(Visit(context.expression()));
                return node;
            }
            return Visit(context.argument());
        }

        // ========== Qualified ID ==========

        public override AstNode VisitQualifiedId([NotNull] ExpressionParser.QualifiedIdContext context)
        {
            var ids = context.ID();
            if (ids.Length == 1)
            {
                return CreateNode<QualifiedIdentifier>(ids[0].GetText());
            }

            var node = CreateNode<QualifiedIdentifier>(ids[0].GetText());
            for (int i = 1; i < ids.Length; i++)
            {
                node.addChild(CreateNode<QualifiedIdentifier>(ids[i].GetText()));
            }
            return node;
        }

        public override AstNode VisitQuotableName([NotNull] ExpressionParser.QuotableNameContext context)
        {
            if (context.STRING_LITERAL() != null)
            {
                string raw = context.STRING_LITERAL().GetText();
                string content = raw.Substring(1, raw.Length - 2).Replace("''", "'");
                return CreateNode<QualifiedIdentifier>(content);
            }
            return Visit(context.qualifiedId());
        }

        // ========== Literals ==========

        public override AstNode VisitLiteral([NotNull] ExpressionParser.LiteralContext context)
        {
            if (context.NULL_LITERAL() != null)
                return CreateNode<NullLiteralNode>(context.NULL_LITERAL().GetText());
            if (context.INTEGER_LITERAL() != null)
                return CreateNode<IntLiteralNode>(context.INTEGER_LITERAL().GetText());
            if (context.HEXADECIMAL_INTEGER_LITERAL() != null)
                return CreateNode<HexLiteralNode>(context.HEXADECIMAL_INTEGER_LITERAL().GetText());
            if (context.REAL_LITERAL() != null)
                return CreateNode<RealLiteralNode>(context.REAL_LITERAL().GetText());
            if (context.STRING_LITERAL() != null)
            {
                string raw = context.STRING_LITERAL().GetText();
                // Strip surrounding quotes and unescape doubled quotes
                string content = raw.Substring(1, raw.Length - 2).Replace("''", "'");
                return CreateNode<StringLiteralNode>(content);
            }
            if (context.boolLiteral() != null)
                return Visit(context.boolLiteral());

            throw new InvalidOperationException("Unknown literal type");
        }

        public override AstNode VisitBoolLiteral([NotNull] ExpressionParser.BoolLiteralContext context)
        {
            return CreateNode<BooleanLiteralNode>(context.GetText());
        }

        public override AstNode VisitRelationalOperator([NotNull] ExpressionParser.RelationalOperatorContext context)
        {
            throw new InvalidOperationException("Use CreateRelationalOperatorNode instead");
        }
    }
}
