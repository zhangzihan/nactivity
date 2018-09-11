using System.Collections.Generic;

namespace org.activiti.engine.impl.persistence
{

	using org.activiti.engine.impl.persistence.cache;
	using org.activiti.engine.impl.persistence.entity;

	/// 
	public abstract class CachedEntityMatcherAdapter<EntityImpl> : ICachedEntityMatcher<EntityImpl> where EntityImpl : org.activiti.engine.impl.persistence.entity.IEntity
	{

	  public virtual bool isRetained(ICollection<EntityImpl> databaseEntities, ICollection<CachedEntity> cachedEntities, EntityImpl entity, object param)
	  {
		return isRetained(entity, param);
	  }

	  public abstract bool isRetained(EntityImpl entity, object param);

	}


}