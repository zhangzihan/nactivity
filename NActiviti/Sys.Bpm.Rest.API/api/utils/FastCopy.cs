using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sys.Workflow.cloud.services.api.utils
{
    /// <summary>
    /// 在两个不同的类型之间，快速的拷贝
    /// </summary>
    public static class FastCopy
    {
        static Action<S, T> CreateCopier<S, T>()
        {
            var target = Expression.Parameter(typeof(T));
            var source = Expression.Parameter(typeof(S));
            var props1 = typeof(S).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead).ToList();
            var props2 = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite).ToList();

            var props = props1.Where(x => props2.Where(y => y.Name == x.Name).Count() > 0);

            var block = Expression.Block(
                from p in props
                select Expression.Assign(Expression.Property(target, p.Name), Expression.Property(source, p.Name)));
            return Expression.Lambda<Action<S, T>>(block, source, target).Compile();
        }

        static ConcurrentDictionary<string, object> actions = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 快速的拷贝同名公共属性。忽略差异的字段。
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void Copy<S, T>(S from, T to)
        {
            string name = string.Format("{0}_{1}", typeof(S), typeof(T));

            object obj;
            if (!actions.TryGetValue(name, out obj))
            {
                var ff = CreateCopier<S, T>();
                actions.TryAdd(name, ff);
                obj = ff;
            }
            Action<S, T> act = (Action<S, T>)obj;
            act(from, to);
        }
    }
}
