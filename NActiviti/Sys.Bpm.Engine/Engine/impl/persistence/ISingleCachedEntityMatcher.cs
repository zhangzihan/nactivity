namespace org.activiti.engine.impl.persistence
{
	using org.activiti.engine.impl.persistence.entity;

	/// <summary>
	/// Interface to express a condition whether or not one specific cached entity should be used in the return result of a query.
	/// 
	/// 
	/// </summary>
	public interface ISingleCachedEntityMatcher<EntityImpl> where EntityImpl : org.activiti.engine.impl.persistence.entity.IEntity
	{

	  bool isRetained(EntityImpl entity, object param);

	}
}