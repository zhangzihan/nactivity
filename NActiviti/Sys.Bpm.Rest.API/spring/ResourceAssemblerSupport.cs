using System;
using System.Collections.Generic;

namespace org.springframework.hateoas.mvc
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class ResourceAssemblerSupport<T, D> : IResourceAssembler<T, D> where D : ResourceSupport
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


        /// <summary>
        /// 
        /// </summary>
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


        /// <summary>
        /// 
        /// </summary>
        public abstract D toResource(T entity);
    }
}
