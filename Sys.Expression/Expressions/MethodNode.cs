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
using System.Reflection;
using System.Runtime.Serialization;
using Spring.Expressions.Processors;
using Spring.Util;
using Spring.Reflection.Dynamic;
using System.Dynamic;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed method node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class MethodNode : NodeWithArguments
    {
        private const BindingFlags BINDING_FLAGS
            = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static
            | BindingFlags.IgnoreCase;

        private static readonly IDictionary collectionProcessorMap = new Hashtable();
        private static readonly IDictionary extensionMethodProcessorMap = new Hashtable();

        private bool initialized = false;
        private bool cachedIsParamArray = false;
        private Type paramArrayType;
        private int argumentCount;
        private IDynamicMethod cachedInstanceMethod;
        private int cachedInstanceMethodHash;

        /// <summary>
        /// Static constructor. Initializes a map of special collection processor methods.
        /// </summary>
        static MethodNode()
        {
            collectionProcessorMap.Add("count", new CountAggregator());
            collectionProcessorMap.Add("sum", new SumAggregator());
            collectionProcessorMap.Add("max", new MaxAggregator());
            collectionProcessorMap.Add("min", new MinAggregator());
            collectionProcessorMap.Add("average", new AverageAggregator());
            collectionProcessorMap.Add("sort", new SortProcessor());
            collectionProcessorMap.Add("orderBy", new OrderByProcessor());
            collectionProcessorMap.Add("distinct", new DistinctProcessor());
            collectionProcessorMap.Add("nonNull", new NonNullProcessor());
            collectionProcessorMap.Add("reverse", new ReverseProcessor());
            collectionProcessorMap.Add("convert", new ConversionProcessor());

            extensionMethodProcessorMap.Add("date", new DateConversionProcessor());
            extensionMethodProcessorMap.Add("isnull", new IsNullValueConversionProcessor());

            extensionMethodProcessorMap.Add("IIF", new Func<bool, object, object, object>((b, x, y) =>
            {
                return b ? x : y;
            }));
            extensionMethodProcessorMap.Add("IF", new Func<bool, object, object, object>((b, x, y) =>
            {
                return b ? x : y;
            }));
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        public MethodNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected MethodNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private readonly object syncRoot = new();

        /// <summary>
        /// Returns node's value for the given context.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            string methodName = this.getText();
            object[] argValues = ResolveArguments(evalContext);
            ICollectionProcessor localCollectionProcessor = null;
            IMethodCallProcessor methodCallProcessor = null;

            // resolve method, if necessary
            lock (syncRoot)
            {
                // check if it is a collection and the methodname denotes a collection processor
                if ((context is null || context is ICollection))
                {
                    // predefined collection processor?
                    localCollectionProcessor = (ICollectionProcessor)collectionProcessorMap[methodName];

                    // user-defined collection processor?
                    if (localCollectionProcessor is null && evalContext.Variables is object)
                    {
                        evalContext.Variables.TryGetValue(methodName, out object temp);
                        localCollectionProcessor = temp as ICollectionProcessor;
                    }
                }

                var method = extensionMethodProcessorMap[methodName];
                // try extension methods
                if (method is IMethodCallProcessor)
                {
                    methodCallProcessor = method as IMethodCallProcessor;
                    // user-defined extension method processor?
                    if (methodCallProcessor is null && evalContext.Variables is object)
                    {
                        evalContext.Variables.TryGetValue(methodName, out object temp);
                        methodCallProcessor = temp as IMethodCallProcessor;
                    }
                }
                else if (method is Delegate delegateMethod)
                {
                    cachedInstanceMethod = new SafeDelegateMethod(delegateMethod);
                }

                // try instance method
                if (context is not null && cachedInstanceMethod is null)
                {
                    // calculate checksum, if the cached method matches the current context
                    if (initialized)
                    {
                        int calculatedHash = CalculateMethodHash(context.GetType(), argValues);
                        initialized = (calculatedHash == cachedInstanceMethodHash);
                    }

                    if (!initialized)
                    {
                        Initialize(methodName, argValues, context);
                        initialized = true;
                    }
                }
            }

            if (localCollectionProcessor is object)
            {
                return localCollectionProcessor.Process((ICollection)context, argValues);
            }
            else if (methodCallProcessor is object)
            {
                return methodCallProcessor.Process(context, argValues);
            }
            else if (cachedInstanceMethod is object)
            {
                object[] paramValues = (cachedIsParamArray)
                                        ? ReflectionUtils.PackageParamArray(argValues, argumentCount, paramArrayType)
                                        : argValues;
                return cachedInstanceMethod.Invoke(context, paramValues);
            }
            else
            {
                throw new ArgumentException(string.Format("Method '{0}' with the specified number and types of arguments does not exist.", methodName));
            }
        }

        private int CalculateMethodHash(Type contextType, object[] argValues)
        {
            HashCode hash = new HashCode();
            hash.Add(contextType.GetHashCode());
            for (int i = 0; i < argValues.Length; i++)
            {
                object arg = argValues[i];
                if (arg is not null)
                {
                    hash.Add(arg.GetType().GetHashCode());
                }
            }
            return hash.ToHashCode();
        }

        private void Initialize(string methodName, object[] argValues, object context)
        {
            Type contextType = (context is Type ? context as Type : context.GetType());

            // check the context type first
            MethodInfo mi = GetBestMethod(contextType, methodName, BINDING_FLAGS, argValues);

            // if not found, probe the Type's type          
            if (mi is null)
            {
                mi = GetBestMethod(typeof(Type), methodName, BINDING_FLAGS, argValues);
            }

            if (mi is null)
            {
                var property = contextType.GetProperty(methodName);
                if (property is not null && property.GetValue(context) is Delegate methodDelegate)
                {
                    mi = methodDelegate.Method;

                    SetParameters(mi);

                    cachedInstanceMethod = new SafeDelegateMethod(methodDelegate);
                    cachedInstanceMethodHash = CalculateMethodHash(contextType, argValues);

                    return;
                }
            }

            //if (mi is null && context is DynamicObject)
            //{
            //    var method = new SafeDyanmicObjectMethod(context, methodName);
            //    var member = method.GetMember();

            //    if (member is Delegate methodDelegate)
            //    {
            //        SetParameters(methodDelegate.Method);
            //    }
            //    else if (member is MethodInfo)
            //    {
            //        SetParameters(member);
            //    }

            //    cachedInstanceMethod = method;
            //    cachedInstanceMethodHash = CalculateMethodHash(contextType, argValues);

            //    return;
            //}

            if (mi is null)
            {
                return;
            }
            else
            {
                SetParameters(mi);

                cachedInstanceMethod = new SafeMethod(mi);
                cachedInstanceMethodHash = CalculateMethodHash(contextType, argValues);
            }

            void SetParameters(MethodInfo mi)
            {
                ParameterInfo[] parameters = mi.GetParameters();
                if (parameters.Length > 0)
                {
                    ParameterInfo lastParameter = parameters[^1];
                    cachedIsParamArray = lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
                    if (cachedIsParamArray)
                    {
                        paramArrayType = lastParameter.ParameterType.GetElementType();
                        argumentCount = parameters.Length;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the best method given the name, argument values, for a given type.
        /// </summary>
        /// <param name="type">The type on which to search for the method.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="argValues">The arg values.</param>
        /// <returns>Best matching method or null if none found.</returns>
        public static MethodInfo GetBestMethod(Type type, string methodName, BindingFlags bindingFlags, object[] argValues)
        {
            MethodInfo mi = null;
            try
            {
                mi = type.GetMethod(methodName, bindingFlags | BindingFlags.FlattenHierarchy);
            }
            catch (AmbiguousMatchException)
            {

                IList<MethodInfo> overloads = GetCandidateMethods(type, methodName, bindingFlags, argValues.Length);
                if (overloads.Count > 0)
                {
                    mi = ReflectionUtils.GetMethodByArgumentValues(overloads, argValues);
                }
            }
            return mi;
        }



        private static IList<MethodInfo> GetCandidateMethods(Type type, string methodName, BindingFlags bindingFlags, int argCount)
        {
            MethodInfo[] methods = type.GetMethods(bindingFlags | BindingFlags.FlattenHierarchy);
            List<MethodInfo> matches = new();

            foreach (MethodInfo method in methods)
            {
                if (method.Name == methodName)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == argCount)
                    {
                        matches.Add(method);
                    }
                    else if (parameters.Length > 0)
                    {
                        ParameterInfo lastParameter = parameters[^1];
                        if (lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                        {
                            matches.Add(method);
                        }
                    }
                }
            }

            return matches;
        }
    }
}
