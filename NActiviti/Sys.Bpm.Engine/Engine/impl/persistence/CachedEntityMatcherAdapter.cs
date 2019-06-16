using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.persistence
{

    using Sys.Workflow.engine.impl.persistence.cache;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    public abstract class CachedEntityMatcherAdapter<EntityImpl> : ICachedEntityMatcher<EntityImpl> where EntityImpl : IEntity
    {
        public virtual bool IsRetained(ICollection<EntityImpl> databaseEntities, ICollection<CachedEntity> cachedEntities, EntityImpl entity, object param)
        {
            return IsRetained(entity, param);
        }

        public abstract bool IsRetained(EntityImpl entity, object param);

    }


}