using System;
using System.Reflection;

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
namespace org.activiti.engine.impl.util
{
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using Sys;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using org.activiti.engine;
    using Microsoft.Extensions.Logging;
    using System.IO;

    /// 
    public abstract class ReflectUtil
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<ReflectUtil>();

        private static readonly Regex GETTER_PATTERN = new Regex("(get|is)[A-Z].*");
        private static readonly Regex SETTER_PATTERN = new Regex("set[A-Z].*");

        public static ClassLoader ClassLoader
        {
            get
            {
                ClassLoader loader = CustomClassLoader;
                if (loader == null)
                {
                    loader = new ClassLoader();
                }
                return loader;
            }
        }

        public static Type loadClass(string className)
        {
            Type clazz = null;
            ClassLoader classLoader = CustomClassLoader;

            // First exception in chain of classloaders will be used as cause when
            // no class is found in any of them
            Exception throwable = null;

            if (classLoader != null)
            {
                try
                {
                    log.LogTrace($"Trying to load class with custom classloader: {className}");
                    clazz = loadClass(classLoader, className);
                }
                catch (Exception t)
                {
                    throwable = t;
                }
            }
            if (clazz == null)
            {
                try
                {
                    log.LogTrace($"Trying to load class with current thread context classloader: {className}");
                    //clazz = loadClass(Thread.CurrentThread.ContextClassLoader, className);
                }
                catch (Exception t)
                {
                    if (throwable == null)
                    {
                        throwable = t;
                    }
                }
                if (clazz == null)
                {
                    try
                    {
                        log.LogTrace($"Trying to load class with local classloader: {className}");
                        //clazz = loadClass(typeof(ReflectUtil).ClassLoader, className);
                    }
                    catch (Exception t)
                    {
                        if (throwable == null)
                        {
                            throwable = t;
                        }
                    }
                }
            }

            if (clazz == null)
            {
                throw new ActivitiClassLoadingException(className, throwable);
            }
            return clazz;
        }

        public static Stream getResourceAsStream(string name)
        {
            System.IO.Stream resourceStream = null;
            ClassLoader classLoader = CustomClassLoader;
            if (classLoader != null)
            {
                resourceStream = classLoader.getResourceAsStream(name);
            }

            if (resourceStream == null)
            {
                // Try the current Thread context classloader
                //classLoader = Thread.CurrentThread.ContextClassLoader;
                resourceStream = classLoader.getResourceAsStream(name);
            }
            return resourceStream;
        }

        public static Uri getResource(string name)
        {
            Uri url = null;
            ClassLoader classLoader = CustomClassLoader;
            if (classLoader != null)
            {
                url = classLoader.getResource(name);
            }
            if (url == null)
            {
                // Try the current Thread context classloader
                //classLoader = Thread.CurrentThread.ContextClassLoader;
                url = classLoader.getResource(name);
            }

            return url;
        }

        public static object instantiate(string className)
        {
            try
            {
                Type clazz = loadClass(className);
                return Activator.CreateInstance(clazz);
            }
            catch (Exception e)
            {
                throw new ActivitiException("couldn't instantiate class " + className, e);
            }
        }

        public static object invoke(object target, string methodName, object[] args)
        {
            try
            {
                Type clazz = target.GetType();
                MethodInfo method = findMethod(clazz, methodName, args);
                //method.Accessible = true;
                return method.Invoke(target, args);
            }
            catch (Exception e)
            {
                throw new ActivitiException("couldn't invoke " + methodName + " on " + target, e);
            }
        }

        /// <summary>
        /// Returns the field of the given object or null if it doesn't exist.
        /// </summary>
        public static FieldInfo getField(string fieldName, object @object)
        {
            return getField(fieldName, @object.GetType());
        }

        /// <summary>
        /// Returns the field of the given class or null if it doesn't exist.
        /// </summary>
        public static FieldInfo getField(string fieldName, Type clazz)
        {
            FieldInfo field = null;
            try
            {
                field = clazz.GetField(fieldName);
            }
            catch (NotSupportedException)
            {
                // for some reason getDeclaredFields doesn't search superclasses
                // (which getFields() does ... but that gives only public fields)
                Type superClass = clazz.BaseType;
                if (superClass != null)
                {
                    return getField(fieldName, superClass);
                }
            }
            catch (Exception)
            {
                throw new ActivitiException("not allowed to access field " + field + " on class " + clazz.FullName);
            }
            return field;
        }

        public static void setField(FieldInfo field, object @object, object value)
        {
            try
            {
                field.SetValue(@object, value);
            }
            catch (ArgumentException e)
            {
                throw new ActivitiException("Could not set field " + field.ToString(), e);
            }
            catch (FieldAccessException e)
            {
                throw new ActivitiException("Could not set field " + field.ToString(), e);
            }
        }

        /// <summary>
        /// Returns the setter-method for the given field name or null if no setter exists.
        /// </summary>
        public static MethodInfo getSetter(string fieldName, Type clazz, Type fieldType)
        {
            //string setterName = "set" + Character.toTitleCase(fieldName[0]) + fieldName.Substring(1, fieldName.Length - 1);
            try
            {
                var prop = clazz.GetProperty(fieldName);

                return prop.SetMethod;

                // Using getMethods(), getMethod(...) expects exact parameter type
                // matching and ignores inheritance-tree.
                //Method[] methods = clazz.GetMethods();
                //foreach (Method method in methods)
                //{
                //    if (method.Name.Equals(setterName))
                //    {
                //        Type[] paramTypes = method.ParameterTypes;
                //        if (paramTypes != null && paramTypes.Length == 1 && paramTypes[0].IsAssignableFrom(fieldType))
                //        {
                //            return method;
                //        }
                //    }
                //}
                //return null;
            }
            catch (Exception)
            {
                throw new ActivitiException("Not allowed to access method " + fieldName + "on class " + clazz.FullName);
            }
        }

        private static MethodInfo findMethod(Type clazz, string methodName, object[] args)
        {
            foreach (MethodInfo method in clazz.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                // TODO add parameter matching
                if (method.Name.Equals(methodName) && matches(method.GetParameters().Select(x => x.ParameterType).ToArray(), args))
                {
                    return method;
                }
            }
            Type superClass = clazz.BaseType;
            if (superClass != null)
            {
                return findMethod(superClass, methodName, args);
            }
            return null;
        }

        public static object instantiate(string className, object[] args)
        {
            Type clazz = loadClass(className);
            ConstructorInfo constructor = findMatchingConstructor(clazz, args);
            if (constructor == null)
            {
                throw new ActivitiException("couldn't find constructor for " + className + " with args " + args?.ToList());
            }
            try
            {
                return constructor.Invoke(args);
            }
            catch (Exception e)
            {
                throw new ActivitiException("couldn't find constructor for " + className + " with args " + args?.ToList(), e);
            }
        }

        private static ConstructorInfo findMatchingConstructor(Type clazz, object[] args)
        {
            foreach (ConstructorInfo constructor in clazz.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            { // cannot
              // use
              // <?>
              // or
              // <T>
              // due
              // to
              // JDK
              // 5/6
              // incompatibility
                if (matches(constructor.GetParameters().Select(x => x.ParameterType).ToArray(), args))
                {
                    return constructor;
                }
            }
            return null;
        }

        private static bool matches(Type[] parameterTypes, object[] args)
        {
            if ((parameterTypes == null) || (parameterTypes.Length == 0))
            {
                return ((args == null) || (args.Length == 0));
            }
            if ((args == null) || (parameterTypes.Length != args.Length))
            {
                return false;
            }
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if ((args[i] != null) && (!parameterTypes[i].IsAssignableFrom(args[i].GetType())))
                {
                    return false;
                }
            }
            return true;
        }

        private static ClassLoader CustomClassLoader
        {
            get
            {
                ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
                if (processEngineConfiguration != null)
                {
                    ClassLoader classLoader = processEngineConfiguration.ClassLoader;
                    if (classLoader != null)
                    {
                        return classLoader;
                    }
                }
                return new ClassLoader();
            }
        }

        private static Type loadClass(ClassLoader classLoader, string className)
        {
            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            bool useClassForName = processEngineConfiguration == null || processEngineConfiguration.UseClassForNameClassLoading;
            return Type.GetType(className, true);
        }

        public static bool isGetter(MethodInfo method)
        {
            string name = method.Name;
            Type type = method.ReturnType;
            Type[] @params = method.GetParameters().Select(x => x.ParameterType).ToArray();

            if (!GETTER_PATTERN.IsMatch(name))
            {
                return false;
            }

            // special for isXXX boolean
            if (name.StartsWith("is", StringComparison.Ordinal))
            {
                return @params.Length == 0 && type.Name.Equals("boolean", StringComparison.CurrentCultureIgnoreCase);
            }

            return @params.Length == 0 && !type.Equals(typeof(void));
        }

        public static bool isSetter(MethodInfo method, bool allowBuilderPattern)
        {
            string name = method.Name;
            Type type = method.ReturnType;
            Type[] @params = method.GetParameters().Select(x => x.ParameterType).ToArray();

            if (!SETTER_PATTERN.IsMatch(name))
            {
                return false;
            }

            return @params.Length == 1 && (type.Equals(typeof(void)) || (allowBuilderPattern && type.IsAssignableFrom(method.DeclaringType)));
        }

        public static bool isSetter(MethodInfo method)
        {
            return isSetter(method, false);
        }

        public static string getGetterShorthandName(MethodInfo method)
        {
            if (!isGetter(method))
            {
                return method.Name;
            }

            string name = method.Name;
            if (name.StartsWith("get", StringComparison.Ordinal))
            {
                name = name.Substring(3);
                name = name.Substring(0, 1).ToLower(new CultureInfo("en")) + name.Substring(1);
            }
            else if (name.StartsWith("is", StringComparison.Ordinal))
            {
                name = name.Substring(2);
                name = name.Substring(0, 1).ToLower(new CultureInfo("en")) + name.Substring(1);
            }

            return name;
        }

        public static string getSetterShorthandName(MethodInfo method)
        {
            if (!isSetter(method))
            {
                return method.Name;
            }

            string name = method.Name;
            if (name.StartsWith("set", StringComparison.Ordinal))
            {
                name = name.Substring(3);
                name = name.Substring(0, 1).ToLower(new CultureInfo("en")) + name.Substring(1);
            }

            return name;
        }
    }
}