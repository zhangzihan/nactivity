namespace org.activiti.engine.impl.@delegate
{
	using org.activiti.engine.impl.persistence.entity;

	/// <summary>
	/// If the behaviour of an element in a process implements this interface, it has a 'background job' functionality.
	/// 
	/// The instance will be called at the end of executing the engine operations for each <seealso cref="IExecutionEntity"/> that currently is at the activity AND is inactive.
	/// 
	/// 
	/// </summary>
	public interface IInactiveActivityBehavior
	{

	  void executeInactive(IExecutionEntity executionEntity);

	}

}