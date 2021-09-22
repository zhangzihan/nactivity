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
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using Spring.Util;

namespace Spring.Reflection.Dynamic
{
    #region IDynamicMethod interface

    /// <summary>
    /// Defines methods that dynamic method class has to implement.
    /// </summary>
    public interface IDynamicMethod
    {
        /// <summary>
        /// Invokes dynamic method on the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to invoke method on.
        /// </param>
        /// <param name="arguments">
        /// Method arguments.
        /// </param>
        /// <returns>
        /// A method return value.
        /// </returns>
        object Invoke(object target, params object[] arguments);
    }

    #endregion

    #region Safe wrapper

    class IdentityTable : Hashtable
    {
        public IdentityTable()
        { }

        protected override int GetHash(object key)
        {
            return key.GetHashCode();
        }

        protected override bool KeyEquals(object item, object key)
        {
            return ReferenceEquals(item, key);
        }
    }

    class SafeDyanmicObjectMethod : IDynamicMethod
    {
        private readonly dynamic context;
        private readonly string methodName;

        public SafeDyanmicObjectMethod(dynamic context, string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentException($"“{nameof(methodName)}”不能为 null 或空。", nameof(methodName));
            }

            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.methodName = methodName;
        }

        public object Invoke(object target, params object[] arguments)
        {
            dynamic member = GetMember();

            if (member is MulticastDelegate delegateMethod)
            {
                var method = delegateMethod.Method;
                if (method.ReturnParameter.Name != "Void")
                {
                    return method.Invoke(delegateMethod.Target, arguments);
                }
                else
                {
                    method.Invoke(delegateMethod.Target, arguments);
                    return null;
                }
            }
            else if (member is MethodInfo)
            {
                var method = member;
                if (method.ReturnParameter.Name != "Void")
                {
                    return method.Invoke(context, arguments);
                }
                else
                {
                    method.hod.Invoke(context, arguments);
                    return null;
                }
            }

            return null;
        }

        internal dynamic GetMember()
        {
            var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                CSharpBinderFlags.None,
                methodName,
                context.GetType(),
                new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            var member = callsite.Target(callsite, context);
            return member;
        }
    }

    class SafeDelegateMethod : IDynamicMethod
    {
        private readonly Delegate delegateMethod;

        public SafeDelegateMethod(Delegate delegateMethod)
        {
            this.delegateMethod = delegateMethod ?? throw new ArgumentNullException(nameof(delegateMethod));
        }

        public object Invoke(object target, params object[] arguments)
        {
            if (this.delegateMethod.Method.ReturnParameter.Name != "Void")
            {
                return this.delegateMethod.DynamicInvoke(arguments);
            }
            else
            {
                this.delegateMethod.DynamicInvoke(arguments);
                return null;
            }
        }
    }

    /// <summary>
    /// Safe wrapper for the dynamic method.
    /// </summary>
    /// <remarks>
    /// <see cref="SafeMethod"/> will attempt to use dynamic
    /// method if possible, but it will fall back to standard
    /// reflection if necessary.
    /// </remarks>
    public class SafeMethod : IDynamicMethod
    {
        private readonly MethodInfo methodInfo;

        /// <summary>
        /// Gets the class, that declares this method
        /// </summary>
        public Type DeclaringType
        {
            get { return methodInfo.DeclaringType; }
        }

        #region Generated Function Cache

        private class SafeMethodState
        {
            public readonly FunctionDelegate method;
            public readonly object[] nullArguments;

            public SafeMethodState(FunctionDelegate method, object[] nullArguments)
            {
                this.method = method;
                this.nullArguments = nullArguments;
            }
        }

        private static readonly Hashtable stateCache = new IdentityTable();

        #endregion

        private readonly SafeMethodState state;

        /// <summary>
        /// Creates a new instance of the safe method wrapper.
        /// </summary>
        /// <param name="methodInfo">Method to wrap.</param>
        public SafeMethod(MethodInfo methodInfo)
        {
            AssertUtils.ArgumentNotNull(methodInfo, "You cannot create a dynamic method for a null value.");

            state = (SafeMethodState)stateCache[methodInfo];
            if (state is null)
            {
                SafeMethodState newState = new(DynamicReflectionManager.CreateMethod(methodInfo),
                new object[methodInfo.GetParameters().Length]
                );

                lock (stateCache.SyncRoot)
                {
                    state = (SafeMethodState)stateCache[methodInfo];
                    if (state is null)
                    {
                        state = newState;
                        stateCache[methodInfo] = state;
                    }
                }
            }

            this.methodInfo = methodInfo;
        }

        /// <summary>
        /// Invokes dynamic method.
        /// </summary>
        /// <param name="target">
        /// Target object to invoke method on.
        /// </param>
        /// <param name="arguments">
        /// Method arguments.
        /// </param>
        /// <returns>
        /// A method return value.
        /// </returns>
        public object Invoke(object target, params object[] arguments)
        {
            // special case - when calling Invoke(null,null) it is undecidible if the second null is an argument or the argument array
            object[] nullArguments = state.nullArguments;
            if (arguments is null && nullArguments.Length == 1) arguments = nullArguments;
            int arglen = (arguments is null ? 0 : arguments.Length);
            if (nullArguments.Length != arglen)
            {
                throw new ArgumentException(string.Format("Invalid number of arguments passed into method {0} - expected {1}, but was {2}", methodInfo.Name, nullArguments.Length, arglen));
            }

            return this.state.method(target, arguments);
        }
    }

    #endregion

    /// <summary>
    /// Factory class for dynamic methods.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicMethod : BaseDynamicMember
    {
        /// <summary>
        /// Creates dynamic method instance for the specified <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="method">Method info to create dynamic method for.</param>
        /// <returns>Dynamic method for the specified <see cref="MethodInfo"/>.</returns>
        public static IDynamicMethod Create(MethodInfo method)
        {
            AssertUtils.ArgumentNotNull(method, "You cannot create a dynamic method for a null value.");

            IDynamicMethod dynamicMethod = new SafeMethod(method);
            return dynamicMethod;
        }
    }
}
