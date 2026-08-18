using System.Collections.Concurrent;
using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
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
namespace Sys.Workflow.Engine.Impl.EL
{
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Delegate.Invocation;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys;
    using System;

    /// <summary>
    /// <para>
    /// Central manager for all expressions.
    /// </para>
    /// <para>
    /// Process parsers will use this to build expression objects that are stored in the process definitions.
    /// </para>
    /// <para>
    /// Then also this class is used as an entry point for runtime evaluation of the expressions.
    /// </para>
    /// </summary>
    public class ExpressionManager
    {
        /// <summary>
        /// Default implementation (does nothing)
        /// </summary>
        protected internal ELContext parsingElContext = new ParsingElContext();
        /// <summary>
        /// 
        /// </summary>
        protected internal IDictionary<object, object> beans;

        /// <summary>
        /// 表达式对象缓存（key 为表达式字符串）。
        /// 表达式对象（<seealso cref="JuelExpression"/> 及其内部的 <seealso cref="IValueExpression"/>）创建后不可变，
        /// 求值状态均来自运行时传入的 ELContext，可安全地在多线程间共享复用。
        /// </summary>
        private readonly ConcurrentDictionary<string, IExpression> expressionCache = new ConcurrentDictionary<string, IExpression>();

        /// <summary>
        /// 缓存的表达式工厂，避免每次创建表达式都重复解析服务
        /// </summary>
        private IExpressionFactory expressionFactory;

        /// <summary>
        /// 
        /// </summary>
        public ExpressionManager() : this(null)
        {
            //expressionContext = new ExpressionContext();

            //expressionContext.Imports.AddType(typeof(Math));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beans"></param>
        public ExpressionManager(IDictionary<object, object> beans)
        {
            this.beans = beans;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual IExpression CreateExpression(string expression)
        {
            // 相同表达式字符串直接复用缓存的表达式对象，避免热路径上重复创建
            return expressionCache.GetOrAdd(expression, expr =>
            {
                IValueExpression valueExpression = ExpressionFactory.CreateValueExpression(parsingElContext, expr.Trim(), typeof(object));

                return new JuelExpression(valueExpression, expr);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExpressionFactory ExpressionFactory
        {
            get
            {
                // 解析结果缓存在实例字段上（ProcessEngineServiceProvider.ServiceProvider 为全局静态，
                // 但挂实例字段可兼容未来按引擎实例隔离的场景）
                return expressionFactory ??= ProcessEngineServiceProvider.Resolve<IExpressionFactory>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableScope"></param>
        /// <returns></returns>
        public virtual ELContext GetElContext(IVariableScope variableScope)
        {
            ELContext elContext = null;
            if (variableScope is VariableScopeImpl variableScopeImpl)
            {
                elContext = variableScopeImpl.CachedElContext;
            }

            if (elContext is null)
            {
                elContext = CreateElContext(variableScope);
                if (variableScope is VariableScopeImpl impl)
                {
                    impl.CachedElContext = elContext;
                }
            }

            return elContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableScope"></param>
        /// <returns></returns>
        protected internal virtual ActivitiElContext CreateElContext(IVariableScope variableScope)
        {
            ELResolver elResolver = CreateElResolver(variableScope);
            return new ActivitiElContext(elResolver);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableScope"></param>
        /// <returns></returns>
        protected internal virtual ELResolver CreateElResolver(IVariableScope variableScope)
        {
            CompositeELResolver elResolver = new CompositeELResolver();
            elResolver.Add(new VariableScopeElResolver(variableScope));

            if (beans is object)
            {
                // ACT-1102: Also expose all beans in configuration when using
                // standalone activiti, not
                // in spring-context
                //elResolver.add(new ReadOnlyMapELResolver(beans));
            }

            //elResolver.add(new ArrayELResolver());
            //elResolver.add(new ListELResolver());
            //elResolver.add(new MapELResolver());
            //elResolver.add(new JsonNodeELResolver());
            //elResolver.add(new DynamicBeanPropertyELResolver(typeof(ItemInstance), "getFieldValue", "setFieldValue")); // TODO: needs verification
            //elResolver.add(new BeanELResolver());
            return elResolver;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDictionary<object, object> Beans
        {
            get
            {
                return beans;
            }
            set
            {
                this.beans = value;
            }
        }
    }
}