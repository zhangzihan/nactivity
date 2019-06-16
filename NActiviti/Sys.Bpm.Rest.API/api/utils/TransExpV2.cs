using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sys.Workflow.Cloud.Services.Api.Utils
{
    /// <summary>
    /// 将数据类型从一种类型转为另外一种类型,复制公共属性
    /// </summary>
    /// <typeparam name="TIn">from</typeparam>
    /// <typeparam name="TOut">to</typeparam>
    public static class TransExpV2<TIn, TOut>
    {
        private static readonly Func<TIn, TOut> cache = GetFunc();
        private static Func<TIn, TOut> GetFunc()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");
            List<MemberBinding> memberBindingList = new List<MemberBinding>();

            foreach (var item in typeof(TOut).GetProperties())
            {
                if (!item.CanWrite)
                    continue;

                MemberExpression property = Expression.Property(parameterExpression, typeof(TIn).GetProperty(item.Name));
                MemberBinding memberBinding = Expression.Bind(item, property);
                memberBindingList.Add(memberBinding);
            }

            MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
            Expression<Func<TIn, TOut>> lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, new ParameterExpression[] { parameterExpression });

            return lambda.Compile();
        }


        /// <summary>
        /// 
        /// </summary>
        public static TOut Trans(TIn tIn)
        {
            return cache(tIn);
        }
    }
}
