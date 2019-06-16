using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Persistence
{

    using Sys.Workflow.Engine.Impl.Persistence.Caches;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

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