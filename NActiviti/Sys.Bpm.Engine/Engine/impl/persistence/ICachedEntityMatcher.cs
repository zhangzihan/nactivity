using System.Collections.Generic;

namespace org.activiti.engine.impl.persistence
{
    using org.activiti.engine.impl.persistence.cache;
    using org.activiti.engine.impl.persistence.entity;

    /// <summary>
    /// Interface to express a condition whether or not a cached entity should be used in the return result of a query.
    /// 
    /// 
    /// </summary>
    public interface ICachedEntityMatcher<EntityImpl> where EntityImpl : IEntity
    {
        /// <summary>
        /// Returns true if an entity from the cache should be retained (i.e. used as return result for a query).
        /// 
        /// Most implementations of this interface probably don't need this method,
        /// and should extend the simpler <seealso cref="CachedEntityMatcherAdapter"/>, which hides this method.
        /// 
        /// Note that the databaseEntities collection can be null, in case only the cache is checked.
        /// </summary>
        bool IsRetained(ICollection<EntityImpl> databaseEntities, ICollection<CachedEntity> cachedEntities, EntityImpl entity, object param);
    }
}