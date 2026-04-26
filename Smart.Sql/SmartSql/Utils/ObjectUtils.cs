using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SmartSql.Utils
{
    public static class ObjectUtils
    {
        private static ConcurrentDictionary<string, Func<object, Dictionary<string, object>>> _cachedConvert = new ConcurrentDictionary<string, Func<object, Dictionary<string, object>>>();

        public static Dictionary<string, object> ToDictionary(object sourceObj, bool ignorePropNameCase)
        {
            if (sourceObj is null)
            {
                var comparer = ignorePropNameCase ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture;
                return new Dictionary<string, object>(comparer);
            }

            if (sourceObj is IDictionary s)
            {
                var comparer = ignorePropNameCase ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture;
                Dictionary<string, object> dict = new Dictionary<string, object>(comparer);
                foreach (var key in s.Keys)
                {
                    if (key is null)
                    {
                        throw new ArgumentException("IDictionary contains a null key.", nameof(sourceObj));
                    }

                    var propName = key.ToString();
                    if (string.IsNullOrWhiteSpace(propName))
                    {
                        throw new ArgumentException("IDictionary contains an empty key.", nameof(sourceObj));
                    }

                    dict[propName] = s[key];
                }
                return dict;
            }
            var dicConvert = GetDictionaryConvert(sourceObj.GetType(), ignorePropNameCase);
            try
            {
                return dicConvert(sourceObj);
            }
            catch (InvalidProgramException ex)
            {
                throw new InvalidProgramException($"ObjectUtils.ToDictionary failed for type: {sourceObj.GetType().FullName}", ex);
            }
        }

        public static Func<object, Dictionary<string, object>> GetDictionaryConvert(Type sourceType, bool ignorePropNameCase)
        {
            string key = $"{sourceType.AssemblyQualifiedName}_{ignorePropNameCase}";
            if (!_cachedConvert.TryGetValue(key, out var obj))
            {
                obj = CreateDictionaryConvertConvertImpl(sourceType, ignorePropNameCase);
                _cachedConvert.TryAdd(key, obj);
            }
            return obj;
        }

        private static readonly Type _dicType = typeof(Dictionary<string, object>);
        private static readonly ConstructorInfo _dicCtor = _dicType.GetConstructor(new Type[] { typeof(int), typeof(IEqualityComparer<string>) });
        private static readonly MethodInfo _addItemDicMentod = _dicType.GetMethod("Add");
        private static readonly MethodInfo _get_CurrentCultureIgnoreCase_StringComparer = typeof(StringComparer).GetMethod("get_CurrentCultureIgnoreCase");
        private static readonly MethodInfo _get_CurrentCulture_StringComparer = typeof(StringComparer).GetMethod("get_CurrentCulture");

        private static Func<object, Dictionary<string, object>> CreateDictionaryConvertConvertImpl(Type sourceType, bool ignorePropNameCase)
        {
            Type returnType = _dicType;
            var addItemMethod = _addItemDicMentod;
            var sourceProps = GetReadableInstanceProperties(sourceType);
            var dynamicMethod = new DynamicMethod("ObjToDic" + Guid.NewGuid().ToString("N"), returnType, new[] { typeof(object) }, sourceType, true);
            var iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.DeclareLocal(returnType);
            int propsCount = sourceProps.Length;
            LocalBuilder valueTypeLocal = null;
            if (sourceType.IsValueType)
            {
                valueTypeLocal = iLGenerator.DeclareLocal(sourceType);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Unbox_Any, sourceType);
                iLGenerator.Emit(OpCodes.Stloc, valueTypeLocal);
            }

            EmitUtils.LoadInt32(iLGenerator, propsCount);
            iLGenerator.Emit(OpCodes.Call, ignorePropNameCase ? _get_CurrentCultureIgnoreCase_StringComparer : _get_CurrentCulture_StringComparer);
            iLGenerator.Emit(OpCodes.Newobj, _dicCtor);
            iLGenerator.Emit(OpCodes.Stloc_0);
            foreach (var prop in sourceProps)
            {
                var getMethod = prop.GetGetMethod(true);
                if (getMethod is null)
                {
                    continue;
                }

                iLGenerator.Emit(OpCodes.Ldloc_0); //[dic]
                iLGenerator.Emit(OpCodes.Ldstr, prop.Name);//[dic][prop-name]
                if (sourceType.IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Ldloca, valueTypeLocal);
                    iLGenerator.Emit(OpCodes.Call, getMethod);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);//[dic][prop-name][sourceObj]
                    iLGenerator.Emit(OpCodes.Castclass, sourceType);
                    iLGenerator.Emit(OpCodes.Callvirt, getMethod);//[dic][prop-name][prop-value]
                }
                if (prop.PropertyType.IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Box, prop.PropertyType);
                }
                iLGenerator.Emit(OpCodes.Call, addItemMethod);//[empty]
            }
            iLGenerator.Emit(OpCodes.Ldloc_0);
            iLGenerator.Emit(OpCodes.Ret);
            var funcType = System.Linq.Expressions.Expression.GetFuncType(typeof(object), returnType);
            return (Func<object, Dictionary<string, object>>)dynamicMethod.CreateDelegate(funcType);
        }

        private static PropertyInfo[] GetReadableInstanceProperties(Type sourceType)
        {
            var properties = new List<PropertyInfo>();
            var seenNames = new HashSet<string>(StringComparer.Ordinal);
            for (Type t = sourceType; t is not null && t != typeof(object); t = t.BaseType)
            {
                var declaredProps = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var p in declaredProps)
                {
                    if (p.CanRead == false)
                    {
                        continue;
                    }

                    if (p.GetIndexParameters().Length != 0)
                    {
                        continue;
                    }

                    if (p.PropertyType.IsByRefLike)
                    {
                        continue;
                    }

                    var getMethod = p.GetGetMethod(true);
                    if (getMethod is null || getMethod.IsStatic)
                    {
                        continue;
                    }

                    if (seenNames.Add(p.Name) == false)
                    {
                        continue;
                    }

                    properties.Add(p);
                }
            }

            return properties.ToArray();
        }

    }
}
