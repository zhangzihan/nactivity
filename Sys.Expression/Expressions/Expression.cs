#region License

/*
 * Copyright ?2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Spring.Core;
using Spring.Reflection.Dynamic;
using Spring.Util;
using StringUtils = Spring.Util.StringUtils;
using AntlrInputStream = global::Antlr4.Runtime.AntlrInputStream;
using CommonTokenStream = global::Antlr4.Runtime.CommonTokenStream;
using IParseTree = global::Antlr4.Runtime.Tree.IParseTree;
using ExpressionLexer = Spring.Expressions.Parser.Antlr4Gen.ExpressionLexer;
using ExpressionParser = Spring.Expressions.Parser.Antlr4Gen.ExpressionParser;

namespace Spring.Expressions
{
    /// <summary>
    /// Container object for the parsed expression.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Preparing this object once and reusing it many times for expression
    /// evaluation can result in significant performance improvements, as 
    /// expression parsing and reflection lookups are only performed once. 
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class Expression : BaseNode
    {
        /// <summary>
        /// Contains a list of reserved variable names.
        /// You must not use any variable names with the reserved prefix!
        /// </summary>
        public class ReservedVariableNames
        {
            /// <summary>
            /// Variable Names using this prefix are reserved for internal framework use
            /// </summary>
            public static readonly string RESERVEDPREFIX = "____spring_";

            /// <summary>
            /// variable name of the currently processed object factory, if any
            /// </summary>
            internal static readonly string CurrentObjectFactory = RESERVEDPREFIX + "CurrentObjectFactory";
        }

        static Expression()
        {
        }

        /// <summary>
        /// Helper to parse an expression string using ANTLR4.
        /// </summary>
        private static IExpression ParseWithAntlr4(string expression, Func<ExpressionParser, IParseTree> parseFunc)
        {
            var visitor = new SpringExpressionVisitor();
            var stream = new AntlrInputStream(expression);
            var lexer = new ExpressionLexer(stream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ExpressionParser(tokenStream);

            // Use BailErrorStrategy to throw on first error
            parser.ErrorHandler = new global::Antlr4.Runtime.BailErrorStrategy();

            try
            {
                var tree = parseFunc(parser);
                return (IExpression)visitor.Visit(tree);
            }
            catch (global::Antlr4.Runtime.Misc.ParseCanceledException ex)
            {
                // BailErrorStrategy wraps RecognitionException in ParseCanceledException
                if (ex.InnerException is global::Antlr4.Runtime.RecognitionException re)
                {
                    int line = 0, col = 0;
                    if (re.OffendingToken != null)
                    {
                        line = re.OffendingToken.Line;
                        col = re.OffendingToken.Column;
                    }
                    throw new SyntaxErrorException(re.Message, line, col, expression);
                }
                throw new SyntaxErrorException(ex.Message, 0, 0, expression);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class
        /// by parsing specified expression string.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        public static IExpression Parse(string expression)
        {
            if (StringUtils.HasText(expression))
            {
                return ParseWithAntlr4(expression, p => p.expr());
            }
            else
            {
                return new Expression();
            }
        }

        /// <summary>
        /// Registers lambda expression under the specified <paramref name="functionName"/>.
        /// </summary>
        /// <param name="functionName">Function name to register expression as.</param>
        /// <param name="lambdaExpression">Lambda expression to register.</param>
        /// <param name="variables">Variables dictionary that the function will be registered in.</param>
        public static void RegisterFunction(string functionName, string lambdaExpression, IDictionary variables)
        {
            AssertUtils.ArgumentHasText(functionName, "functionName");
            AssertUtils.ArgumentHasText(lambdaExpression, "lambdaExpression");

            var result = ParseWithAntlr4(lambdaExpression, p => p.lambda());
            variables[functionName] = result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class
        /// by parsing specified primary expression string.
        /// </summary>
        /// <param name="expression">Primary expression to parse.</param>
        internal static IExpression ParsePrimary(string expression)
        {
            if (StringUtils.HasText(expression))
            {
                return ParseWithAntlr4(expression, p => p.primaryExpression());
            }
            else
            {
                return new Expression();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class
        /// by parsing specified property expression string.
        /// </summary>
        /// <param name="expression">Property expression to parse.</param>
        internal static IExpression ParseProperty(string expression)
        {
            if (StringUtils.HasText(expression))
            {
                return ParseWithAntlr4(expression, p => p.property());
            }
            else
            {
                return new Expression();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// </summary>
        public Expression()
        { }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected Expression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Evaluates this expression for the specified root object and returns 
        /// value of the last node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Value of the last node.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            object result = context;

            if (this.getNumberOfChildren() > 0)
            {
                AstNode node = this.getFirstChild();
                while (node is object)
                {
                    result = GetValue(((BaseNode)node), result, evalContext);

                    node = node.getNextSibling();
                }
            }

            return result;
        }

        /// <summary>
        /// Evaluates this expression for the specified root object and sets 
        /// value of the last node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <param name="newValue">Value to set last node to.</param>
        /// <exception cref="NotSupportedException">If navigation expression is empty.</exception>
        protected override void Set(object context, EvaluationContext evalContext, object newValue)
        {
            object target = context;

            if (this.getNumberOfChildren() > 0)
            {
                AstNode node = this.getFirstChild();

                for (int i = 0; i < this.getNumberOfChildren() - 1; i++)
                {
                    try
                    {
                        target = GetValue(((BaseNode)node), target, evalContext);
                        node = node.getNextSibling();
                    }
                    catch (NotReadablePropertyException e)
                    {
                        throw new NotWritablePropertyException("Cannot read the value of '" + node.getText() + "' property in the expression.", e);
                    }
                }
                SetValue(((BaseNode)node), target, evalContext, newValue);
            }
            else
            {
                throw new NotSupportedException("You cannot set the value for an empty expression.");
            }
        }

        /// <summary>
        /// Evaluates this expression for the specified root object and returns 
        /// <see cref="PropertyInfo"/> of the last node, if possible.
        /// </summary>
        /// <param name="context">Context to evaluate expression against.</param>
        /// <param name="variables">Expression variables map.</param>
        /// <returns>Value of the last node.</returns>
        internal PropertyInfo GetPropertyInfo(object context, IDictionary<string, object> variables)
        {
            if (this.getNumberOfChildren() > 0)
            {
                object target = context;
                AstNode node = this.getFirstChild();

                for (int i = 0; i < this.getNumberOfChildren() - 1; i++)
                {
                    target = ((IExpression)node).GetValue(target, variables);
                    node = node.getNextSibling();
                }

                if (node is PropertyOrFieldNode propNode)
                {
                    return (PropertyInfo)propNode.GetMemberInfo(target);
                }
                else if (node is IndexerNode idxNode)
                {
                    return idxNode.GetPropertyInfo(target, variables);
                }
                else
                {
                    throw new FatalReflectionException("Cannot obtain PropertyInfo from an expression that does not resolve to a property or an indexer.");
                }
            }

            throw new FatalReflectionException("Cannot obtain PropertyInfo for empty property name.");
        }
    }
}