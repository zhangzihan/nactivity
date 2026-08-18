using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Sys.Rules.DmnEngine.Engine.Decisions;
using Sys.Rules.DmnEngine.Utils;
using Sys.Rules.DmnEngine.Engine.Definition;
using Sys.Rules.DmnEngine.Engine.Execution.Result;
using System.Text.RegularExpressions;
using Sys.Rules.DmnEngine.Feel;
using Sys.Rules.DmnEngine.Feel.Ast;
using Sys.Rules.DmnEngine.Feel.Eval;
using Sys.Rules.DmnEngine.Feel.Parsing;
using Sys.Rules.DmnEngine.Feel.Types;

namespace Sys.Rules.DmnEngine.Engine.Execution.Context
{
    /// <summary>
    /// Context where is the DMN model executed
    /// </summary>
    public class DmnExecutionContext
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Parsed (pre-processed) expressions cache (Global and Definition)
        /// </summary>
        protected static readonly ConcurrentDictionary<string, FeelAstNode> ParsedExpressionsCache =
            new ConcurrentDictionary<string, FeelAstNode>();

        /// <summary>
        /// 静态解析表达式缓存容量上限；超过上限后直接放弃缓存，避免静态字典无限增长（内存泄漏）
        /// </summary>
        private const int ParsedExpressionsCacheMaxSize = 5000;

        /// <summary>
        /// Parsed (pre-processed) expressions cache (Context and Execution)
        /// </summary>
        protected readonly ConcurrentDictionary<string, FeelAstNode> ParsedExpressionsInstanceCache =
            new ConcurrentDictionary<string, FeelAstNode>();

        /// <summary>
        /// Shared FEEL engine instance
        /// </summary>
        protected static readonly FeelEngine FeelEngine = new();

        /// <summary>
        /// Unique identifier of the execution context (set at CTOR)
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// DMN Model definition
        /// </summary>
        public DmnDefinition Definition { get; }

        /// <summary>
        /// Variables used while executing the DMN model - can be used within the Decision Tables and/or Expressions
        /// In general, it holds the Input Data of DMN model and outputs from Decision Tables and/or Expressions
        /// </summary>
        public IReadOnlyDictionary<string, DmnExecutionVariable> Variables { get; }

        /// <summary>
        /// Dictionary of available decisions by name
        /// </summary>
        public IReadOnlyDictionary<string, IDmnDecision> Decisions { get; }


        /// <summary>
        /// Snapshots of the context state during the execution
        /// </summary>
        public DmnExecutionSnapshots Snapshots { get; } = new DmnExecutionSnapshots();

        /// <summary>
        /// Execution context options
        /// </summary>
        private readonly DmnExecutionContextOptions options = new DmnExecutionContextOptions();

        /// <summary>
        /// Execution context options
        /// </summary>
        public IDmnExecutionContextOptions Options => options;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="definition">DMN Model definition</param>
        /// <param name="variables">Variables used while executing the DMN model</param>
        /// <param name="decisions">Dictionary of available decisions by name</param>
        /// <param name="configure">Optional configuration action</param>
        /// <exception cref="ArgumentNullException">Any of the parameters is null</exception>
        public DmnExecutionContext(
            DmnDefinition definition,
            IReadOnlyDictionary<string, DmnExecutionVariable> variables,
            IReadOnlyDictionary<string, IDmnDecision> decisions,
            Action<DmnExecutionContextOptions> configure = null)
        {
            Id = Guid.NewGuid().ToString();

            Definition = definition ?? throw Logger.Fatal<ArgumentNullException>($"{nameof(definition)} is null");
            Variables = variables ?? throw Logger.Fatal<ArgumentNullException>($"{nameof(variables)} is null");
            Decisions = decisions ?? throw Logger.Fatal<ArgumentNullException>($"{nameof(decisions)} is null");

            configure?.Invoke(options);
        }

        /// <summary>
        /// Resets the DMN execution context - clears all variables except the input parameters (sets them to null)
        /// and clears the snapshots
        /// </summary>
        /// <returns><see cref="DmnExecutionContext"/></returns>
        public virtual DmnExecutionContext Reset()
        {
            if (Variables == null) return this;
            foreach (var variable in Variables.Values.Where(i => !i.IsInputParameter))
            {
                variable.Value = null;
            }

            Snapshots.Reset();
            return this;
        }

        /// <summary>
        /// Sets the <paramref name="name">named</paramref> input parameter <paramref name="value"/>
        /// </summary>
        /// <remarks>
        /// Variable <see cref="DmnExecutionVariable.Value"/> setter doesn't allow to set the value for the input parameters to prevent the change of them,
        ///  so <see cref="DmnExecutionVariable.SetInputParameterValue"/> is to be used explicitly
        /// </remarks>
        /// <param name="name">Name of the input parameter</param>
        /// <param name="value">Value of the input parameter</param>
        /// <returns><see cref="DmnExecutionContext"/></returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is null or empty</exception>
        /// <exception cref="DmnExecutorException">Input parameter with given <paramref name="name"/> doesn't exist</exception>
        public virtual DmnExecutionContext WithInputParameter(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw Logger.Fatal<ArgumentException>($"{nameof(name)} is null or empty");

            name = DmnVariableDefinition.NormalizeVariableName(name);
            var variable = Variables?.Values.FirstOrDefault(i => i.IsInputParameter && i.Name == name);
            if (variable == null)
                throw Logger.Fatal<DmnExecutorException>($"WithInputParameter: {name} is not an input parameter");

            variable.SetInputParameterValue(value);
            Logger.Info($"Execution context input parameter {name} set to {value}");

            // Propagate value to alias variables (e.g. when Camunda exports use different
            // names in DRD input data vs. decision table expressions)
            if (options.ResolveInputAliases &&
                Definition.InputExpressionAliases.TryGetValue(name, out var aliases))
            {
                foreach (var aliasName in aliases)
                {
                    if (Variables.TryGetValue(aliasName, out var aliasVar))
                    {
                        aliasVar.Value = value;
                        Logger.Info($"Execution context alias variable {aliasName} set to {value} (alias of input {name})");
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Sets the input parameters from key-value collection (key=name of parameter, value=value to be set)
        /// </summary>
        /// <remarks>
        /// Variable <see cref="DmnExecutionVariable.Value"/> setter doesn't allow to set the value for the input parameters to prevent the change of them,
        ///  so <see cref="DmnExecutionVariable.SetInputParameterValue"/> is to be used explicitly
        /// </remarks>
        /// <param name="parameters">Collection of parameters - Key=name, Value=value</param>
        /// <returns><see cref="DmnExecutionContext"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="parameters"/> is null</exception>
        public virtual DmnExecutionContext WithInputParameters(
            IReadOnlyCollection<KeyValuePair<string, object>> parameters)
        {
            if (parameters == null) throw Logger.Fatal<ArgumentNullException>($"{nameof(parameters)} is null");

            foreach (var parameter in parameters)
            {
                WithInputParameter(parameter.Key, parameter.Value);
            }

            return this;
        }

        /// <summary>
        /// Executes (evaluates) decision with given <paramref name="decisionName"/>
        /// </summary>
        /// <param name="decisionName">Name of the decision to execute</param>
        /// <returns>Decision result</returns>
        /// <exception cref="ArgumentException"><paramref name="decisionName"/> is null or empty</exception>
        /// <exception cref="DmnExecutorException">Decision with <paramref name="decisionName"/> not found</exception>
        public DmnDecisionResult ExecuteDecision(string decisionName)
        {
            if (string.IsNullOrWhiteSpace(decisionName))
                throw Logger.Fatal<ArgumentException>($"{nameof(decisionName)} is null or empty");
            if (!Decisions.ContainsKey(decisionName))
                throw Logger.Fatal<DmnExecutorException>($"ExecuteDecision: - decision {decisionName} not found");

            var decision = Decisions[decisionName];
            return ExecuteDecision(decision);
        }

        /// <summary>
        /// Executes (evaluates) given <paramref name="decision"/>
        /// </summary>
        /// <param name="decision">Decision to execute</param>
        /// <returns>Decision result</returns>
        /// <exception cref="ArgumentNullException"><paramref name="decision"/> is null</exception>
        public virtual DmnDecisionResult ExecuteDecision(IDmnDecision decision)
        {
            if (decision == null) throw Logger.Fatal<ArgumentNullException>($"{nameof(decision)} is null");

            if (Options.RecordSnapshots)
            {
                //clear snapshots and create "initial" snapshot of the variables
                Snapshots.Reset();
                Snapshots.CreateSnapshot(this);
            }

            var correlationId = Guid.NewGuid().ToString();
            var result = decision.Execute(this, correlationId);
            //note: the decision-after-execute snapshot is created by decision (for each decision in "dependency graph")

            //clear execution cache
            PurgeExpressionCacheExecutionScope(correlationId);

            return result;
        }

        /// <summary>
        /// Creates the execution context snapshot - to be called by decision after the evaluation
        /// </summary>
        /// <param name="decision">Decision evaluated just before the snapshot</param>
        /// <param name="result"><paramref name="decision"/> result</param>
        internal virtual void CreateSnapshot(IDmnDecision decision, DmnDecisionResult result)
        {
            if (Options.RecordSnapshots)
                Snapshots.CreateSnapshot(this, decision, result);
        }

        /// <summary>
        /// Builds a FEEL evaluation context from the current execution variables.
        /// </summary>
        protected virtual FeelEvaluationContext BuildFeelEvaluationContext()
        {
            var evalContext = new FeelEvaluationContext();
            foreach (var variable in Variables.Values)
            {
                var value = variable.Value;
                // When value is null and the variable has a value type, use the default value
                // (replicates old DynamicExpresso behavior where null int 鈫?0, null bool 鈫?false, etc.)
                if (value == null && variable.Type != null && variable.Type.IsValueType)
                {
                    value = Activator.CreateInstance(variable.Type);
                }
                // When value is null and the variable type is string, use empty string
                // (replicates C# behavior where null string in concatenation = "")
                else if (value == null && variable.Type == typeof(string))
                {
                    value = "";
                }
                // Coerce CLR values to FEEL canonical types
                value = FeelTypeCoercion.CoerceToFeel(value);
                evalContext.SetVariable(variable.Name, value);
            }
            return evalContext;
        }

        /// <summary>
        /// Builds a FEEL scope with all known variable names for multi-word name resolution.
        /// </summary>
        protected virtual FeelScope BuildFeelScope()
        {
            var scope = new FeelScope();
            foreach (var variable in Variables.Values)
            {
                scope.AddName(variable.Name);
            }
            return scope;
        }

        /// <summary>
        /// Evaluates expression
        /// </summary>
        /// <param name="expression">Expression to evaluate</param>
        /// <param name="outputType">Output (result) type</param>
        /// <param name="executionId">Identifier of the execution run</param>
        /// <exception cref="ArgumentException"><paramref name="expression"/> is null or empty</exception>
        /// <exception cref="ArgumentNullException"><paramref name="outputType"/> is null</exception>
        /// <exception cref="DmnExecutorException">Exception while invoking the expression</exception>
        /// <exception cref="DmnExecutorException">Can't convert the expression result to <paramref name="outputType"/></exception>
        /// <returns>The expression result converted to <paramref name="outputType"/></returns>
        public virtual object EvalExpression(string expression, Type outputType, string executionId)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw Logger.Fatal<ArgumentException>($"{nameof(expression)} is null or empty");
            if (outputType == null) throw Logger.Fatal<ArgumentNullException>($"{nameof(outputType)} is null");

            expression = PreProcessExpression(expression);
            var scope = BuildFeelScope();

            // Check parsed expression cache
            var cacheKey = GetParsedExpressionCacheKey(executionId, expression, outputType);
            if (Options.ParsedExpressionCacheScope == ParsedExpressionCacheScopeEnum.None ||
                !GetParsedExpressionsFromCache(cacheKey, out var parsedAst))
            {
                try
                {
                    parsedAst = FeelEngine.ParseExpression(expression, scope);
                }
                catch (Exception exception)
                {
                    throw Logger.Fatal<DmnExecutorException>($"Exception while parsing the expression {expression}",
                        exception);
                }

                if (Options.ParsedExpressionCacheScope != ParsedExpressionCacheScopeEnum.None)
                    CacheParsedExpression(cacheKey, parsedAst);
            }

            // Evaluate expression
            var evalContext = BuildFeelEvaluationContext();
            object result;
            try
            {
                result = FeelEngine.Evaluate(parsedAst, evalContext);
            }
            catch (Exception exception)
            {
                throw Logger.Fatal<DmnExecutorException>($"Exception while invoking the expression {expression}",
                    exception);
            }

            // Convert the result to the expected output type
            object resultConverted;
            try
            {
                resultConverted = ConvertResult(result, outputType);
            }
            catch (Exception exception)
            {
                throw Logger.Fatal<DmnExecutorException>($"Can't convert the expression result to {outputType.Name}",
                    exception);
            }

            return resultConverted;
        }

        /// <summary>
        /// Evaluates a FEEL simple unary tests expression against the given input value.
        /// Used for decision table rule input evaluation.
        /// </summary>
        /// <param name="unaryTestsExpression">The unary tests expression (e.g. "> 5", "1..10", "\"hello\"")</param>
        /// <param name="inputValue">The input value to test against</param>
        /// <param name="executionId">Identifier of the execution run</param>
        /// <returns>True if the input value matches the unary tests</returns>
        public virtual bool EvalUnaryTests(string unaryTestsExpression, object inputValue, string executionId)
        {
            if (string.IsNullOrWhiteSpace(unaryTestsExpression))
                throw Logger.Fatal<ArgumentException>($"{nameof(unaryTestsExpression)} is null or empty");

            unaryTestsExpression = PreProcessExpression(unaryTestsExpression);
            var scope = BuildFeelScope();

            // Check parsed expression cache
            var cacheKey = GetParsedExpressionCacheKey(executionId, $"UT:{unaryTestsExpression}", typeof(bool));
            if (Options.ParsedExpressionCacheScope == ParsedExpressionCacheScopeEnum.None ||
                !GetParsedExpressionsFromCache(cacheKey, out var parsedAst))
            {
                try
                {
                    parsedAst = FeelEngine.ParseSimpleUnaryTests(unaryTestsExpression, scope);
                }
                catch
                {
                    // Fallback: Camunda and other tools allow full FEEL expressions
                    // (with or/and operators) in input entries, not just simple unary tests.
                    try
                    {
                        parsedAst = FeelEngine.ParseExpression(unaryTestsExpression, scope);
                    }
                    catch (Exception fallbackException)
                    {
                        throw Logger.Fatal<DmnExecutorException>(
                            $"Exception while parsing the unary tests expression {unaryTestsExpression}", fallbackException);
                    }
                }

                if (Options.ParsedExpressionCacheScope != ParsedExpressionCacheScopeEnum.None)
                    CacheParsedExpression(cacheKey, parsedAst);
            }

            // Evaluate
            var evalContext = BuildFeelEvaluationContext();
            evalContext.InputValue = FeelTypeCoercion.CoerceToFeel(inputValue);

            try
            {
                var evaluator = new FeelEvaluator(evalContext, Sys.Rules.DmnEngine.Feel.Functions.FeelBuiltInFunctions.Resolve);
                return evaluator.EvaluateAsUnaryTest(parsedAst);
            }
            catch (Exception exception)
            {
                throw Logger.Fatal<DmnExecutorException>(
                    $"Exception while evaluating the unary tests expression {unaryTestsExpression}", exception);
            }
        }

        /// <summary>
        /// Converts a FEEL evaluation result to the expected CLR output type.
        /// Handles FEEL-specific types (decimal, DateOnly, etc.) and standard .NET conversions.
        /// </summary>
        protected static object ConvertResult(object result, Type outputType)
        {
            if (result == null)
            {
                if (outputType.IsValueType)
                {
                    // For nullable value types, return null
                    if (Nullable.GetUnderlyingType(outputType) != null) return null;
                    return Activator.CreateInstance(outputType);
                }
                return null;
            }
            if (outputType == typeof(object)) return result;
            if (outputType.IsInstanceOfType(result)) return result;

            // Use FEEL type coercion first
            var coerced = FeelTypeCoercion.CoerceToClr(result, outputType);
            if (coerced != null) return coerced;

            // Fallback to Convert.ChangeType for standard types
            return Convert.ChangeType(result, outputType);
        }

        /// <summary>
        /// Tries to retrieve the <paramref name="parsedExpression"/> from the parsed expression cache using the <paramref name="cacheKey"/>.
        /// <see cref="ParsedExpressionCacheScopeEnum.Global"/> and <see cref="ParsedExpressionCacheScopeEnum.Definition"/> scopes use static <see cref="ParsedExpressionsCache"/>,
        /// otherwise the <see cref="ParsedExpressionsInstanceCache"/> is used.
        /// </summary>
        /// <param name="cacheKey">Retrieval key of the <paramref name="parsedExpression"/></param>
        /// <param name="parsedExpression">Parsed expression retrieved from cache if successful</param>
        /// <returns>True when the <paramref name="parsedExpression"/> has been retrieved from cache, otherwise false</returns>
        protected virtual bool GetParsedExpressionsFromCache(string cacheKey, out FeelAstNode parsedExpression)
        {

            if (Options.ParsedExpressionCacheScope == ParsedExpressionCacheScopeEnum.Global ||
                Options.ParsedExpressionCacheScope == ParsedExpressionCacheScopeEnum.Definition)
            {
                return ParsedExpressionsCache.TryGetValue(cacheKey, out parsedExpression);
            }

            return ParsedExpressionsInstanceCache.TryGetValue(cacheKey, out parsedExpression);
        }

        /// <summary>
        /// Store the <paramref name="parsedExpression"/> into parsed expression cache using the <paramref name="cacheKey"/>.
        /// <see cref="ParsedExpressionCacheScopeEnum.Global"/> and <see cref="ParsedExpressionCacheScopeEnum.Definition"/> scopes use static <see cref="ParsedExpressionsCache"/>,
        /// otherwise the <see cref="ParsedExpressionsInstanceCache"/> is used.
        /// </summary>
        /// <param name="cacheKey">Retrieval key of the <paramref name="parsedExpression"/></param>
        /// <param name="parsedExpression">Parsed expression to cache</param>
        protected virtual void CacheParsedExpression(string cacheKey, FeelAstNode parsedExpression)
        {
            if (Options.ParsedExpressionCacheScope == ParsedExpressionCacheScopeEnum.Global ||
                Options.ParsedExpressionCacheScope == ParsedExpressionCacheScopeEnum.Definition)
            {
                // 超过容量上限时直接放弃缓存（下一次执行重新解析即可），防止静态缓存只增不减
                if (ParsedExpressionsCache.Count >= ParsedExpressionsCacheMaxSize)
                    return;

                ParsedExpressionsCache[cacheKey] = parsedExpression;
            }
            else
            {
                ParsedExpressionsInstanceCache[cacheKey] = parsedExpression;
            }
        }

        /// <summary>
        /// Compose the parsed expression key (based on <see cref="DmnExecutionContextOptions.ParsedExpressionCacheScope"/>)
        /// </summary>
        /// <param name="executionId">Identifier of the execution run</param>
        /// <param name="expression">Unparsed (raw) expression</param>
        /// <param name="outputType">Expression output type</param>
        /// <returns>Parsed expression key</returns>
        protected virtual string GetParsedExpressionCacheKey(string executionId, string expression, Type outputType)
        {
            string prefix;
            switch (Options.ParsedExpressionCacheScope)
            {
                case ParsedExpressionCacheScopeEnum.None:
                    prefix = Guid.NewGuid().ToString(); //fallback, always unique key
                    break;
                case ParsedExpressionCacheScopeEnum.Execution:
                    prefix = executionId;
                    break;
                case ParsedExpressionCacheScopeEnum.Context:
                    prefix = Id;
                    break;
                case ParsedExpressionCacheScopeEnum.Definition:
                    // Definition.Id 在每次解析 DMN 时都会重新生成（Guid.NewGuid），不能作为稳定的缓存键前缀，
                    // 否则同一规则定义反复解析时旧条目永久失引用、静态缓存只增不减；
                    // DmnDefinition 上没有 Key/Version 等稳定标识，这里退化为空前缀
                    // （与 Global 等价，直接按表达式文本+输出类型去重）。
                    prefix = "";
                    break;
                case ParsedExpressionCacheScopeEnum.Global:
                    prefix = "";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"{prefix}||{expression}||{outputType.AssemblyQualifiedName}";
        }

        /// <summary>
        /// Purge all cached expressions belonging to given Execution <paramref name="executionId"/> scope
        /// </summary>
        /// <param name="executionId">Execution Id</param>
        public virtual void PurgeExpressionCacheExecutionScope(string executionId)
        {
            var keys = ParsedExpressionsInstanceCache.Keys.Where(k => k.StartsWith($"{executionId}||"));
            foreach (var key in keys)
            {
                ParsedExpressionsInstanceCache.TryRemove(key, out _);
            }
        }


        /// <summary>
        /// Purge all cached expressions belonging to any Execution scope
        /// </summary>
        public virtual void PurgeExpressionCacheExecutionScopeAll()
        {
            var keys = ParsedExpressionsInstanceCache.Keys.Where(k => !k.StartsWith($"{Id}||")); //anything that doesn't belongs to Context should be Execution scope (None scope is not stored)
            foreach (var key in keys)
            {
                ParsedExpressionsInstanceCache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Purge all cached expressions belonging to this Context scope
        /// </summary>
        public virtual void PurgeExpressionCacheContextScope()
        {
            var keys = ParsedExpressionsInstanceCache.Keys.Where(k => k.StartsWith($"{Id}||"));
            foreach (var key in keys)
            {
                ParsedExpressionsInstanceCache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Purge all cached expressions belonging to given Definition <paramref name="definitionId"/> scope
        /// </summary>
        /// <param name="definitionId">Definition Id</param>
        public static void PurgeExpressionCacheDefinitionScope(string definitionId)
        {
            var keys = ParsedExpressionsCache.Keys.Where(k => k.StartsWith($"{definitionId}||"));
            foreach (var key in keys)
            {
                ParsedExpressionsCache.TryRemove(key, out _);
            }
        }


        /// <summary>
        /// Purge all cached expressions belonging to any Definition scope
        /// </summary>
        public static void PurgeExpressionCacheDefinitionScopeAll()
        {
            var keys = ParsedExpressionsCache.Keys.Where(k => !k.StartsWith($"||")); //anything that doesn't belongs to Global should be Definition scope
            foreach (var key in keys)
            {
                ParsedExpressionsCache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Purge all cached expressions belonging to Global scope
        /// </summary>
        public static void PurgeExpressionCacheGlobalScope()
        {
            var keys = ParsedExpressionsCache.Keys.Where(k => k.StartsWith($"||"));
            foreach (var key in keys)
            {
                ParsedExpressionsCache.TryRemove(key, out _);
            }
        }
        /// <summary>
        /// Pre-processes a DMN expression to handle DMN-style date/time/duration constructor syntax.
        /// Converts unquoted date/time arguments to quoted strings:
        /// <c>date(2018-01-23)</c> 鈫?<c>date("2018-01-23")</c>
        /// <c>time(13:00)</c> 鈫?<c>time("13:00")</c>
        /// <c>duration(P3Y)</c> 鈫?<c>duration("P3Y")</c>
        /// String literals are preserved (not modified).
        /// </summary>
        protected static string PreProcessExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) return expression;

            // Single regex that matches EITHER a string literal (to skip it) OR a date/time/duration
            // constructor with an unquoted literal argument. The alternation ensures string contents
            // are never modified. Argument must start with a digit (dates/times), 'P' (durations),
            // or 'T' followed by a digit (time-only), to avoid matching variable names.
            expression = Regex.Replace(expression,
                @"""(?:[^""\\]|\\.)*""" +                                          // Alt 1: string literal
                @"|\b(date\s+and\s+time|date|time|duration)\((?!"")" +             // Alt 2: function(
                @"(\d[\d\-T:+.Z]*|P[\dYMDTHS.]+|T\d[\d:.+\-Z]*)\)",              // unquoted literal arg)
                match =>
                {
                    if (!match.Groups[1].Success) return match.Value; // string literal 鈥?keep as-is
                    return $"{match.Groups[1].Value}(\"{match.Groups[2].Value}\")";
                });

            return expression;
        }

        /// <summary>
        /// Evaluates expression
        /// </summary>
        /// <param name="expression">Expression to evaluate</param>
        /// <param name="executionId">Identifier of the execution run</param>
        /// <typeparam name="TOutputType">Output (result) type</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is null or empty</exception>
        /// <returns>The expression result converted to <typeparamref name="TOutputType"/></returns>
        public TOutputType EvalExpression<TOutputType>(string expression, string executionId)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw Logger.Fatal<ArgumentNullException>($"EvalExpression: - {nameof(expression)} is null or empty");

            var result = EvalExpression(expression, typeof(TOutputType), executionId);
            var resultConverted = (TOutputType)result;
            return resultConverted;
        }

        /// <summary>
        /// Gets the runtime (execution) variable with given <paramref name="name"/>
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <returns>Variable with given <paramref name="name"/></returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is null or empty</exception>
        /// <exception cref="DmnExecutorException">Variable not found</exception>
        public virtual DmnExecutionVariable GetVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw Logger.Fatal<ArgumentException>($"{nameof(name)} is null or empty");

            if (!Variables.ContainsKey(name))
            {
                throw Logger.Fatal<DmnExecutorException>($"GetVariable: - variable {name} not found");
            }

            return Variables[name];
        }

        /// <summary>
        /// Gets the runtime (execution) variable corresponding to its <paramref name="definition"/>
        /// </summary>
        /// <param name="definition">Name of the variable</param>
        /// <returns>Variable  corresponding to its <paramref name="definition"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is null</exception>
        public DmnExecutionVariable GetVariable(IDmnVariable definition)
        {
            if (definition == null)
                throw Logger.Fatal<ArgumentNullException>($"{nameof(definition)} is null");

            return GetVariable(definition.Name);
        }

    }
}

