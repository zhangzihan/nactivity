using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sys.Rules.DmnEngine.Engine.Decisions;
using Sys.Rules.DmnEngine.Engine.Definition;
using Sys.Rules.DmnEngine.Parser.Dto;
// 区分二义性：System.Linq.Expressions.Expression 与 Sys.Rules.DmnEngine.Parser.Dto.Expression 同名，
// 本文件仅使用前者构建编译构造委托，固定别名避免 CS0104。
using Expression = System.Linq.Expressions.Expression;

namespace Sys.Rules.DmnEngine.Engine.Execution.Context
{
    /// <summary>
    /// Factory creating the DMN Model execution context from <see cref="DmnModel"/> or <see cref="DmnDefinition"/>
    /// </summary>
    public static class DmnExecutionContextFactory
    {
        /// <summary>
        /// 构造委托缓存：按执行上下文类型缓存编译后的构造委托，
        /// 避免每次 DMN 执行都走 Activator.CreateInstance 反射构造（含构造器查找与参数装箱）。
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<DmnDefinition, IReadOnlyDictionary<string, DmnExecutionVariable>, IReadOnlyDictionary<string, IDmnDecision>, Action<DmnExecutionContextOptions>, DmnExecutionContext>> _ctorDelegateCache
            = new ConcurrentDictionary<Type, Func<DmnDefinition, IReadOnlyDictionary<string, DmnExecutionVariable>, IReadOnlyDictionary<string, IDmnDecision>, Action<DmnExecutionContextOptions>, DmnExecutionContext>>();

        /// <summary>
        /// Creates the execution context from <paramref name="sourceModel"/>
        /// </summary>
        /// <param name="sourceModel">Source model to create the execution context for</param>
        /// <param name="configure">Optional configuration action</param>
        /// <returns><paramref name="sourceModel"/> execution context</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sourceModel"/> is null</exception>
        public static DmnExecutionContext CreateExecutionContext(
            DmnModel sourceModel,
            Action<DmnExecutionContextOptions> configure = null)
        {
            return CreateCustomExecutionContext<DmnExecutionContext>(sourceModel, configure);
        }

        /// <summary>
        /// Creates the execution context from <paramref name="sourceDefinition"/>
        /// </summary>
        /// <param name="sourceDefinition">Source model definition  to create the execution context for</param>
        /// <param name="configure">Optional configuration action</param>
        /// <returns><paramref name="sourceDefinition"/> execution context</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sourceDefinition"/> is null</exception>
        public static DmnExecutionContext CreateExecutionContext(
            DmnDefinition sourceDefinition,
            Action<DmnExecutionContextOptions> configure = null)
        {
            return CreateCustomExecutionContext<DmnExecutionContext>(sourceDefinition, configure);
        }

        /// <summary>
        /// Creates the execution context from <paramref name="sourceModel"/>
        /// </summary>
        /// <param name="sourceModel">Source model to create the execution context for</param>
        /// <param name="configure">Optional configuration action</param>
        /// <typeparam name="T">Type of the execution context to be created. Must be <see cref="DmnExecutionContext"/> or its descendant</typeparam>
        /// <returns><paramref name="sourceModel"/> execution context</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sourceModel"/> is null</exception>
        public static DmnExecutionContext CreateCustomExecutionContext<T>(
            DmnModel sourceModel,
            Action<DmnExecutionContextOptions> configure = null) 
            where T : DmnExecutionContext
        {
            if (sourceModel == null) throw new ArgumentNullException(nameof(sourceModel));

            var definition = DmnDefinitionFactory.CreateDmnDefinition(sourceModel);
            return CreateCustomExecutionContext<T>(definition, configure);
        }

        /// <summary>
        /// Creates the execution context from <paramref name="sourceDefinition"/>
        /// </summary>
        /// <param name="sourceDefinition">Source model definition  to create the execution context for</param>
        /// <param name="configure">Optional configuration action</param>
        /// <typeparam name="T">Type of the execution context to be created. Must be <see cref="DmnExecutionContext"/> or its descendant</typeparam>
        /// <returns><paramref name="sourceDefinition"/> execution context</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sourceDefinition"/> is null</exception>
        public static DmnExecutionContext CreateCustomExecutionContext<T>(
            DmnDefinition sourceDefinition,
            Action<DmnExecutionContextOptions> configure = null) 
            where T:DmnExecutionContext
        {
            if (sourceDefinition == null) throw new ArgumentNullException(nameof(sourceDefinition));
            if (sourceDefinition.Decisions == null || sourceDefinition.Decisions.Count < 1) throw new ArgumentException("No decisions in the sourceDefinition definition");
            if (sourceDefinition.Variables == null || sourceDefinition.Variables.Count < 1) throw new ArgumentException("No variables in the sourceDefinition definition");

            //Init runtime (execution) variables
            var variables = sourceDefinition
                  .Variables
                  .Values
                  .Select(sourceVariable => new DmnExecutionVariable(sourceVariable))
                  .ToDictionary(variable => variable.Name);

            // Decisions reference - Dictionary of available decisions by name
            var decisions = sourceDefinition
                .Decisions
                .ToDictionary(
                    sourceDecision => sourceDecision.Key,
                    sourceDecision => sourceDecision.Value);

            // 使用按类型缓存的编译构造委托，首次编译后永久复用，消除每次执行的反射开销
            var ctor = _ctorDelegateCache.GetOrAdd(typeof(T), BuildCtorDelegate);
            return (T)ctor(sourceDefinition, variables, decisions, configure);
        }

        /// <summary>
        /// 为执行上下文类型构建编译后的构造委托（参数与 <see cref="DmnExecutionContext"/> 的公开构造器一致）
        /// </summary>
        private static Func<DmnDefinition, IReadOnlyDictionary<string, DmnExecutionVariable>, IReadOnlyDictionary<string, IDmnDecision>, Action<DmnExecutionContextOptions>, DmnExecutionContext> BuildCtorDelegate(Type contextType)
        {
            var defParam = Expression.Parameter(typeof(DmnDefinition), "definition");
            var varsParam = Expression.Parameter(typeof(IReadOnlyDictionary<string, DmnExecutionVariable>), "variables");
            var decsParam = Expression.Parameter(typeof(IReadOnlyDictionary<string, IDmnDecision>), "decisions");
            var cfgParam = Expression.Parameter(typeof(Action<DmnExecutionContextOptions>), "configure");

            var ctor = contextType.GetConstructor(new[]
            {
                typeof(DmnDefinition),
                typeof(IReadOnlyDictionary<string, DmnExecutionVariable>),
                typeof(IReadOnlyDictionary<string, IDmnDecision>),
                typeof(Action<DmnExecutionContextOptions>)
            }) ?? throw new InvalidOperationException($"类型 {contextType.FullName} 缺少与 DmnExecutionContext 一致的公开构造器");

            Expression body = Expression.New(ctor, defParam, varsParam, decsParam, cfgParam);
            if (contextType != typeof(DmnExecutionContext))
            {
                body = Expression.Convert(body, typeof(DmnExecutionContext));
            }

            return Expression.Lambda<Func<DmnDefinition, IReadOnlyDictionary<string, DmnExecutionVariable>, IReadOnlyDictionary<string, IDmnDecision>, Action<DmnExecutionContextOptions>, DmnExecutionContext>>(
                body, defParam, varsParam, decsParam, cfgParam).Compile();
        }
    }
}

