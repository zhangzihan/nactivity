using Spring.Core.TypeResolution;
using System;
using System.Collections.Concurrent;
using static Spring.Expressions.PropertyOrFieldNode;

namespace Spring.Expressions
{
    public static class ValueAccessorResolver
    {
        private static readonly ConcurrentDictionary<Type, Type> items = new();

        public static IValueAccessor GetValueAccessor(Type targetType, string memberName)
        {
            if (targetType is null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentException($"“{nameof(memberName)}”不能为 null 或空。", nameof(memberName));
            }

            if (TypeResolutionUtils.TryResolveType(memberName, out var type) && type is not null)
            {
                return new TypeValueAccessor(type);
            }

            _ = items.TryGetValue(targetType, out var accessorType);
            if (accessorType is null)
            {
                foreach (var key in items.Keys)
                {
                    if (key.IsAssignableFrom(targetType))
                    {
                        accessorType = items[key];
                        break;
                    }
                }
                if (accessorType is null)
                {
                    return null;
                }
            }

            if (accessorType.IsAssignableFrom(typeof(IValueAccessor)))
            {
                return null;
            }

            return Activator.CreateInstance(accessorType, new object[] { memberName }) as IValueAccessor;
        }

        public static void Register(Type targetType, Type accessorType)
        {
            if (targetType is null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            if (accessorType is null)
            {
                throw new ArgumentNullException(nameof(accessorType));
            }

            if (accessorType.IsAssignableFrom(typeof(IValueAccessor)))
            {
                throw new NotSupportedException($"{accessorType.FullName}必须实现Spring.Expressions.IValueAccessor接口");
            }

            _ = items.AddOrUpdate(targetType, accessorType, (key, old) => accessorType);
        }
    }
}
