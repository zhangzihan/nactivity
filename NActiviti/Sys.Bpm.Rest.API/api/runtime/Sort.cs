
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static org.activiti.api.runtime.shared.query.Sort;

namespace org.activiti.api.runtime.shared.query
{
    public class Sort : ICollection<Order>
    {
        private IList<Order> orders = new List<Order>();

        public int Count => orders.Count;

        public bool IsReadOnly => orders.IsReadOnly;

        public Sort()
        {

        }

        //[JsonConstructor]
        public Sort([JsonProperty("Sort")]IEnumerable<Order> sorts)
        {
            orders = sorts?.ToList();
        }

        public IList<Order> Orders { get => orders; set => orders = value ?? new List<Order>(); }

        public static Sort unsorted()
        {
            return new Sort();
        }

        public IEnumerator<Order> GetEnumerator()
        {
            return orders.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return orders.GetEnumerator();
        }

        public void Add(Order item)
        {
            orders.Add(item);
        }

        public void Clear()
        {
            orders.Clear();
        }

        public bool Contains(Order item)
        {
            return orders.Contains(item);
        }

        public void CopyTo(Order[] array, int arrayIndex)
        {
            orders.CopyTo(array, arrayIndex);
        }

        public bool Remove(Order item)
        {
            return orders.Remove(item);
        }

        public class Order
        {
            public Direction Direction { get; set; }

            public string Property { get; set; }
        }

        public enum Direction
        {
            ASC,
            DESC
        }

        public enum NullHandling
        {

            /**
             * Lets the data store decide what to do with nulls.
             */
            NATIVE,

            /**
             * A hint to the used data store to order entries with null values before non null entries.
             */
            NULLS_FIRST,

            /**
             * A hint to the used data store to order entries with null values after non null entries.
             */
            NULLS_LAST
        }
    }
}
