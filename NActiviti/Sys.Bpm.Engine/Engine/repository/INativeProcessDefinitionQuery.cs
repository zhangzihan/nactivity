﻿namespace Sys.Workflow.engine.repository
{
	using Sys.Workflow.engine.query;

	/// <summary>
	/// Allows querying of <seealso cref="Sys.Workflow.engine.repository.IProcessDefinition"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeProcessDefinitionQuery : INativeQuery<INativeProcessDefinitionQuery, IProcessDefinition>
	{

	}
}