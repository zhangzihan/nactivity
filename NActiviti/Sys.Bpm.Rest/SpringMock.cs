using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.events.integration;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.services.connectors.model;
using org.springframework.messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static org.springframework.data.domain.Sort;

namespace org.springframework.messaging
{
    public interface SubscribableChannel { }
}

namespace org.springframework.context
{
    public interface ApplicationEventPublisher
    {
        void publishEvent(IntegrationRequestEvent @event);

        void publishEvent(Command signalCmd);
    }
}

namespace org.springframework.messaging
{
    public interface Message<T> { }

    public interface MessageChannel<T>
    {
        void send(Message<T> message);
    }
}

namespace org.springframework.messaging.support
{
    public class MessageBuilder<T>
    {
        public static MessageBuilder<T> withPayload(T value)
        {
            return new MessageBuilder<T>();
        }

        public Message<T> build()
        {
            return default(Message<T>);
        }

        public MessageBuilder<T> setHeader(string cONNECTOR_TYPE, string connectorType)
        {
            return this;
        }
    }
}

namespace org.springframework.cloud.stream.binding
{
    public interface BinderAwareChannelResolver
    {
        BinderAwareChannelResolver resolveDestination(string connectorType);
        void send<T>(Message<T> message);
    }
}

namespace org.springframework.data.domain
{
    public class Pageable
    {
        public Pageable()
        {

        }

        //[JsonConstructor]
        public Pageable([JsonProperty("Offset")]int offset, [JsonProperty("PageSize")]int pageSize, [JsonProperty("Sort")]Sort sort)
        {
            Offset = offset;
            PageSize = pageSize;
            Sort = sort;
        }

        public Sort Sort { get; set; }
        public int Offset { get; set; }
        public int PageSize { get; set; }
    }

    public class Sort : ICollection<Order>
    {
        private IList<Order> orders = new List<Order>();

        public int Count => orders.Count;

        public bool IsReadOnly => orders.IsReadOnly;

        private Sort()
        {

        }

        //[JsonConstructor]
        public Sort([JsonProperty("Sort")]IEnumerable<Order> sorts)
        {
            orders = sorts?.ToList();
        }

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

    public interface Page<T> : Slice<T>
    {

    }

    public interface Slice<T> : IEnumerable<T>
    {

    }

    public abstract class Chunk<T> : Slice<T>
    {
        protected IList<T> list;
        protected Pageable pageable;

        public Chunk(IList<T> list, Pageable pageable)
        {
            this.list = list;
            this.pageable = pageable;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }

    public class PageImpl<T> : Chunk<T>, Page<T>
    {
        private long count;

        public PageImpl(IList<T> list, Pageable pageable, long count) : base(list, pageable)
        {
            this.count = count;
        }
    }
}

namespace org.springframework.hateoas
{
    public abstract class ResourceSupport
    {
        protected IList<Link> links = new List<Link>();

        public ResourceSupport add(Link link)
        {
            this.links.Add(link);

            return this;
        }

        public IList<Link> Links
        {
            get => links;
            set => links = value;
        }
    }

    public class Resource<T> : ResourceSupport
    {
        private T content;

        protected Resource()
        {
            this.content = default(T);
        }

        public Resource(T content, IEnumerable<Link> links)
        {
            this.content = content;
            this.links = (links ?? new List<Link>()).ToList();
        }

        public T Content
        {
            get => content;
            set => content = value;
        }
    }

    public class Resources<T> : ResourceSupport, IEnumerable<T>
    {
        protected IList<T> resourcesList;

        public Resources(IList<T> resourcesList)
        {
            this.resourcesList = resourcesList;
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public interface ResourceAssembler<T, D> where D : ResourceSupport
    {

        /**
         * Converts the given entity into an {@link ResourceSupport}.
         * 
         * @param entity
         * @return
         */
        D toResource(T entity);
    }

    public class PagedResources<T> : Resources<T>
    {
        public PagedResources(IList<T> resourcesList) : base(resourcesList)
        {
        }
    }

    public class Link
    {

    }
}

namespace org.springframework.hateoas.mvc
{
    public abstract class ResourceAssemblerSupport<T, D> : ResourceAssembler<T, D> where D : ResourceSupport
    {
        private Type controllerClass;
        private Type resourceType;

        /**
         * Creates a new {@link ResourceAssemblerSupport} using the given controller class and resource type.
         * 
         * @param controllerClass must not be {@literal null}.
         * @param resourceType must not be {@literal null}.
         */
        public ResourceAssemblerSupport(Type controllerClass, Type resourceType)
        {
            this.controllerClass = controllerClass;
            this.resourceType = resourceType;
        }

        /**
         * Converts all given entities into resources.
         * 
         * @see #toResource(Object)
         * @param entities must not be {@literal null}.
         * @return
         */
        public virtual IList<D> toResources(IEnumerable<T> entities)
        {
            IList<D> result = new List<D>();

            foreach (T entity in entities)
            {
                result.Add(toResource(entity));
            }

            return result;
        }

        /**
         * Creates a new resource with a self link to the given id.
         * 
         * @param entity must not be {@literal null}.
         * @param id must not be {@literal null}.
         * @return
         */
        protected D createResourceWithId(object id, T entity)
        {
            return createResourceWithId(id, entity, new object[0]);
        }

        protected D createResourceWithId(object id, T entity, params object[] parameters)
        {
            D instance = instantiateResource(entity);
            //instance.add(linkTo(controllerClass, parameters).slash(id).withSelfRel());
            return instance;
        }

        /**
         * Instantiates the resource object. Default implementation will assume a no-arg constructor and use reflection but
         * can be overridden to manually set up the object instance initially (e.g. to improve performance if this becomes an
         * issue).
         * 
         * @param entity
         * @return
         */
        protected D instantiateResource(T entity)
        {
            return default(D); //BeanUtils.instantiateClass(resourceType);
        }

        public abstract D toResource(T entity);
    }
}
