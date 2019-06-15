using System;
using System.Text;

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

namespace org.activiti.engine.impl.scripting
{
    using org.activiti.engine.impl.bpmn.data;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.util;

    /// <summary>
    /// ScriptEngine that used JUEL for script evaluation and compilation (JSR-223).
    /// 
    /// Uses EL 1.1 if available, to resolve expressions. Otherwise it reverts to EL 1.0, using <seealso cref="ExpressionFactoryResolver"/>.
    /// 
    /// 
    /// </summary>
    public class JuelScriptEngine : AbstractScriptEngine, Compilable
    {

        private ScriptEngineFactory scriptEngineFactory;
        private ExpressionFactory expressionFactory;

        public JuelScriptEngine(ScriptEngineFactory scriptEngineFactory)
        {
            this.scriptEngineFactory = scriptEngineFactory;
            // Resolve the ExpressionFactory
            expressionFactory = ExpressionFactoryResolver.resolveExpressionFactory();
        }

        public JuelScriptEngine() : this(null)
        {
        }

        public virtual CompiledScript compile(string script)
        {
            ValueExpression expr = parse(script, context);
            return new JuelCompiledScript(this, expr);
        }

        public virtual CompiledScript compile(Reader reader)
        {
            // Create a String based on the reader and compile it
            return compile(readFully(reader));
        }

        public virtual object eval(string script, ScriptContext scriptContext)
        {
            ValueExpression expr = parse(script, scriptContext);
            return evaluateExpression(expr, scriptContext);
        }

        public virtual object eval(Reader reader, ScriptContext scriptContext)
        {
            return eval(readFully(reader), scriptContext);
        }

        private object syncRoot = new object();

        public virtual ScriptEngineFactory Factory
        {
            get
            {
                lock (syncRoot)
                {
                    if (scriptEngineFactory == null)
                    {
                        scriptEngineFactory = new JuelScriptEngineFactory();
                    }
                }
                return scriptEngineFactory;
            }
        }

        public virtual Bindings createBindings()
        {
            return new SimpleBindings();
        }

        private object evaluateExpression(ValueExpression expr, ScriptContext ctx)
        {
            try
            {
                return expr.getValue(createElContext(ctx));
            }
            catch (ELException elexp)
            {
                throw new ScriptException(elexp);
            }
        }

        private ELResolver createElResolver()
        {
            CompositeELResolver compositeResolver = new CompositeELResolver();
            compositeResolver.add(new ArrayELResolver());
            compositeResolver.add(new ListELResolver());
            compositeResolver.add(new MapELResolver());
            compositeResolver.add(new JsonNodeELResolver());
            compositeResolver.add(new ResourceBundleELResolver());
            compositeResolver.add(new DynamicBeanPropertyELResolver(typeof(ItemInstance), "getFieldValue", "setFieldValue"));
            compositeResolver.add(new BeanELResolver());
            return new SimpleResolver(compositeResolver);
        }

        private string readFully(Reader reader)
        {
            char[] array = new char[8192];
            StringBuilder strBuffer = new StringBuilder();
            int count;
            try
            {
                while ((count = reader.read(array, 0, array.Length)) > 0)
                {
                    strBuffer.Append(array, 0, count);
                }
            }
            catch (IOException exp)
            {
                throw new ScriptException(exp);
            }
            return strBuffer.ToString();
        }

        private ValueExpression parse(string script, ScriptContext scriptContext)
        {
            try
            {
                return expressionFactory.createValueExpression(createElContext(scriptContext), script, typeof(object));
            }
            catch (ELException ele)
            {
                throw new ScriptException(ele);
            }
        }

        private ELContext createElContext(ScriptContext scriptCtx)
        {
            // Check if the ELContext is already stored on the ScriptContext
            object existingELCtx = scriptCtx.getAttribute("elcontext");
            if (existingELCtx is ELContext)
            {
                return (ELContext)existingELCtx;
            }

            scriptCtx.setAttribute("context", scriptCtx, ScriptContext.ENGINE_SCOPE);

            // Built-in function are added to ScriptCtx
            scriptCtx.setAttribute("out:print", PrintMethod, ScriptContext.ENGINE_SCOPE);

            SecurityManager securityManager = System.SecurityManager;
            if (securityManager == null)
            {
                scriptCtx.setAttribute("lang:import", ImportMethod, ScriptContext.ENGINE_SCOPE);
            }

            ELContext elContext = new ELContextAnonymousInnerClass(this, scriptCtx);
            // Store the elcontext in the scriptContext to be able to reuse
            scriptCtx.setAttribute("elcontext", elContext, ScriptContext.ENGINE_SCOPE);
            return elContext;
        }

        private class ELContextAnonymousInnerClass : ELContext
        {
            private readonly JuelScriptEngine outerInstance;

            private ScriptContext scriptCtx;

            public ELContextAnonymousInnerClass(JuelScriptEngine outerInstance, ScriptContext scriptCtx)
            {
                this.outerInstance = outerInstance;
                this.scriptCtx = scriptCtx;
                resolver = outerInstance.createElResolver();
                varMapper = new ScriptContextVariableMapper(outerInstance, scriptCtx);
                funcMapper = new ScriptContextFunctionMapper(outerInstance, scriptCtx);
            }


            internal ELResolver resolver;
            internal VariableMapper varMapper;
            internal FunctionMapper funcMapper;

            public override ELResolver ELResolver
            {
                get
                {
                    return resolver;
                }
            }

            public override VariableMapper VariableMapper
            {
                get
                {
                    return varMapper;
                }
            }

            public override FunctionMapper FunctionMapper
            {
                get
                {
                    return funcMapper;
                }
            }
        }

        private static Method PrintMethod
        {
            get
            {
                try
                {
                    return typeof(JuelScriptEngine).GetMethod("print", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.Instance, Type.DefaultBinder, Type.DefaultBinder, new Type[] { typeof(object) }, null);
                }
                catch (Exception)
                {
                    // Will never occur
                    return null;
                }
            }
        }

        public static void print(object @object)
        {
            Console.Write(@object);
        }

        private static Method ImportMethod
        {
            get
            {
                try
                {
                    return typeof(JuelScriptEngine).GetMethod("importFunctions", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.Instance, Type.DefaultBinder, new Type[] { typeof(ScriptContext), typeof(string), typeof(object) } null);
                }
                catch (Exception)
                {
                    // Will never occur
                    return null;
                }
            }
        }

        public static void importFunctions(ScriptContext ctx, string @namespace, object obj)
        {
            Type clazz = null;
            if (obj is Type)
            {
                clazz = (Type)obj;
            }
            else if (obj is string)
            {
                try
                {
                    clazz = ReflectUtil.loadClass((string)obj);
                }
                catch (ActivitiException ae)
                {
                    throw new ELException(ae);
                }
            }
            else
            {
                throw new ELException("Class or class name is missing");
            }
            Method[] methods = clazz.GetMethods();
            foreach (Method m in methods)
            {
                int mod = m.Modifiers;
                if (Modifier.isStatic(mod) && Modifier.isPublic(mod))
                {
                    string name = @namespace + ":" + m.Name;
                    ctx.setAttribute(name, m, ScriptContext.ENGINE_SCOPE);
                }
            }
        }

        /// <summary>
        /// Class representing a compiled script using JUEL.
        /// </summary>
        private class JuelCompiledScript : CompiledScript
        {
            private readonly JuelScriptEngine outerInstance;


            internal ValueExpression valueExpression;

            internal JuelCompiledScript(JuelScriptEngine outerInstance, ValueExpression valueExpression)
            {
                this.outerInstance = outerInstance;
                this.valueExpression = valueExpression;
            }

            public virtual ScriptEngine Engine
            {
                get
                {
                    // Return outer class instance
                    return outerInstance;
                }
            }

            public virtual object eval(ScriptContext ctx)
            {
                return outerInstance.evaluateExpression(valueExpression, ctx);
            }
        }

        /// <summary>
        /// ValueMapper that uses the ScriptContext to get variable values or value expressions.
        /// </summary>
        private class ScriptContextVariableMapper : VariableMapper
        {
            private readonly JuelScriptEngine outerInstance;


            internal ScriptContext scriptContext;

            internal ScriptContextVariableMapper(JuelScriptEngine outerInstance, ScriptContext scriptCtx)
            {
                this.outerInstance = outerInstance;
                this.scriptContext = scriptCtx;
            }

            public override ValueExpression resolveVariable(string variableName)
            {
                int scope = scriptContext.getAttributesScope(variableName);
                if (scope != -1)
                {
                    object value = scriptContext.getAttribute(variableName, scope);
                    if (value is ValueExpression)
                    {
                        // Just return the existing ValueExpression
                        return (ValueExpression)value;
                    }
                    else
                    {
                        // Create a new ValueExpression based on the variable value
                        return outerInstance.expressionFactory.createValueExpression(value, typeof(object));
                    }
                }
                return null;
            }

            public override ValueExpression setVariable(string name, ValueExpression value)
            {
                ValueExpression previousValue = resolveVariable(name);
                scriptContext.setAttribute(name, value, ScriptContext.ENGINE_SCOPE);
                return previousValue;
            }
        }

        /// <summary>
        /// FunctionMapper that uses the ScriptContext to resolve functions in EL.
        /// </summary>
        private class ScriptContextFunctionMapper : FunctionMapper
        {
            private readonly JuelScriptEngine outerInstance;


            internal ScriptContext scriptContext;

            internal ScriptContextFunctionMapper(JuelScriptEngine outerInstance, ScriptContext ctx)
            {
                this.outerInstance = outerInstance;
                this.scriptContext = ctx;
            }

            internal virtual string getFullFunctionName(string prefix, string localName)
            {
                return prefix + ":" + localName;
            }

            public override Method resolveFunction(string prefix, string localName)
            {
                string functionName = getFullFunctionName(prefix, localName);
                int scope = scriptContext.getAttributesScope(functionName);
                if (scope != -1)
                {
                    // Methods are added as variables in the ScriptScope
                    object attributeValue = scriptContext.getAttribute(functionName);
                    return (attributeValue is Method) ? (Method)attributeValue : null;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}