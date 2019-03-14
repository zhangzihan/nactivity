
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static org.activiti.api.runtime.shared.query.Sort;

namespace org.activiti.api.runtime.shared.query
{

    /// <summary>
    /// 排序
    /// </summary>
    public class Sort : ICollection<Order>
    {
        private IList<Order> orders = new List<Order>();


        /// <summary>
        /// 排序字段数
        /// </summary>
        public int Count => orders.Count;


        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly => orders.IsReadOnly;


        /// <summary>
        /// 
        /// </summary>
        public Sort()
        {

        }


        /// <summary>
        /// 
        /// </summary>
        //[JsonConstructor]
        public Sort([JsonProperty("Sort")]IEnumerable<Order> sorts)
        {
            orders = sorts?.ToList();
        }


        /// <summary>
        /// 排序列表
        /// </summary>
        public IList<Order> Orders { get => orders; set => orders = value ?? new List<Order>(); }


        /// <summary>
        /// 取消所有排序
        /// </summary>
        public static Sort unsorted()
        {
            return new Sort();
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerator<Order> GetEnumerator()
        {
            return orders.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>

        IEnumerator IEnumerable.GetEnumerator()
        {
            return orders.GetEnumerator();
        }

        /// <summary>
        /// 添加排序
        /// </summary>

        public void Add(Order item)
        {
            orders.Add(item);
        }

        /// <summary>
        /// 清除排序
        /// </summary>

        public void Clear()
        {
            orders.Clear();
        }

        /// <summary>
        /// 是否包含排序
        /// </summary>

        public bool Contains(Order item)
        {
            return orders.Contains(item);
        }

        /// <summary>
        /// 复制
        /// </summary>

        public void CopyTo(Order[] array, int arrayIndex)
        {
            orders.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 移除排序
        /// </summary>

        public bool Remove(Order item)
        {
            return orders.Remove(item);
        }

        /// <summary>
        /// 排序
        /// </summary>

        public class Order
        {

            /// <summary>
            /// 方向
            /// </summary>
            public Direction Direction { get; set; }


            /// <summary>
            /// 排序字段
            /// </summary>
            public string Property { get; set; }
        }


        /// <summary>
        /// 排序
        /// </summary>
        public enum Direction
        {

            /// <summary>
            /// 升序
            /// </summary>
            ASC,

            /// <summary>
            /// 降序
            /// </summary>
            DESC
        }


        /// <summary>
        /// 
        /// </summary>
        public enum NullHandling
        {


            /// <summary>
            /// 
            /// </summary>
            /**
             * Lets the data store decide what to do with nulls.
             */
            NATIVE,


            /// <summary>
            /// 
            /// </summary>
            /**
             * A hint to the used data store to order entries with null values before non null entries.
             */
            NULLS_FIRST,


            /// <summary>
            /// 
            /// </summary>
            /**
             * A hint to the used data store to order entries with null values after non null entries.
             */
            NULLS_LAST
        }
    }
}
