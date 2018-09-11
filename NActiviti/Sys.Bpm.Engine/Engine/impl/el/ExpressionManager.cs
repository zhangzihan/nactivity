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
namespace org.activiti.engine.impl.el
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.@delegate.invocation;
    using org.activiti.engine.impl.persistence.entity;
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
        //ExpressionContext expressionContext = new ExpressionContext();

        protected internal ExpressionFactory expressionFactory;
        // Default implementation (does nothing)
        protected internal ELContext parsingElContext = new ParsingElContext();
        protected internal IDictionary<object, object> beans;

        public ExpressionManager() : this(null)
        {
            //expressionContext = new ExpressionContext();

            //expressionContext.Imports.AddType(typeof(Math));
        }

        public ExpressionManager(bool initFactory) : this(null, initFactory)
        {
        }

        public ExpressionManager(IDictionary<object, object> beans) : this(beans, true)
        {
        }

        public ExpressionManager(IDictionary<object, object> beans, bool initFactory)
        {
            // Use the ExpressionFactoryImpl in activiti build in version of juel,
            // with parametrised method expressions enabled
            if (initFactory)
            {
                //expressionFactory = new ExpressionFactoryImpl();
            }
            this.beans = beans;
        }

        public virtual engine.@delegate.IExpression createExpression(string expression)
        {
            ValueExpression valueExpression = createValueExpression(expression.Trim(), typeof(object));

            var expr = new JuelExpression(valueExpression, expression);

            return expr;
        }

        private ValueExpression createValueExpression(string expression, Type type)
        {
            return new ValueExpression(expression, type);
        }

        public virtual ExpressionFactory ExpressionFactory
        {
            set
            {
                this.expressionFactory = value;
            }
        }

        public virtual ELContext getElContext(IVariableScope variableScope)
        {
            ELContext elContext = null;
            if (variableScope is VariableScopeImpl)
            {
                VariableScopeImpl variableScopeImpl = (VariableScopeImpl)variableScope;
                elContext = variableScopeImpl.CachedElContext;
            }

            if (elContext == null)
            {
                elContext = createElContext(variableScope as IVariableScope);
                if (variableScope is VariableScopeImpl)
                {
                    ((VariableScopeImpl)variableScope).CachedElContext = elContext;
                }
            }

            return elContext;
        }

        protected internal virtual ActivitiElContext createElContext(IVariableScope variableScope)
        {
            ELResolver elResolver = createElResolver(variableScope);
            return new ActivitiElContext(elResolver);
        }

        protected internal virtual ELResolver createElResolver(IVariableScope variableScope)
        {
            CompositeELResolver elResolver = new CompositeELResolver();
            elResolver.add(new VariableScopeElResolver(variableScope));

            if (beans != null)
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

    public class ExpressionFactory
    {
    }
}