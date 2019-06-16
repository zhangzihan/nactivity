using System;
using System.Collections.Generic;

namespace Sys.Workflow.Hateoas.Mvc
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class ResourceAssemblerSupport<T, D> : IResourceAssembler<T, D> where D : ResourceSupport
    {
        private readonly Type controllerClass;
        private readonly Type resourceType;

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
        public virtual IList<D> ToResources(IEnumerable<T> entities)
        {
            IList<D> result = new List<D>();

            foreach (T entity in entities)
            {
                result.Add(ToResource(entity));
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
        protected D CreateResourceWithId(object id, T entity)
        {
            return CreateResourceWithId(id, entity, new object[0]);
        }


        /// <summary>
        /// 
        /// </summary>
        protected D CreateResourceWithId(object id, T entity, params object[] parameters)
        {
            D instance = InstantiateResource(entity);
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
        protected D InstantiateResource(T entity)
        {
            return default; //BeanUtils.instantiateClass(resourceType);
        }


        /// <summary>
        /// 
        /// </summary>
        public abstract D ToResource(T entity);
    }
}
