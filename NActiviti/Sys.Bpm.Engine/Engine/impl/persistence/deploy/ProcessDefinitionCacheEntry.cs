using System;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sys.Workflow.engine.impl.persistence.deploy
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.repository;

    /// 
    [Serializable]
	public class ProcessDefinitionCacheEntry
	{

	  private const long serialVersionUID = 6833801933658529070L;

	  protected internal IProcessDefinition processDefinition;
	  protected internal BpmnModel bpmnModel;
	  protected internal Process process;

	  public ProcessDefinitionCacheEntry(IProcessDefinition processDefinition, BpmnModel bpmnModel, Process process)
	  {
		this.processDefinition = processDefinition;
		this.bpmnModel = bpmnModel;
		this.process = process;
	  }

	  public virtual IProcessDefinition ProcessDefinition
	  {
		  get
		  {
			return processDefinition;
		  }
		  set
		  {
			this.processDefinition = value;
		  }
	  }


	  public virtual BpmnModel BpmnModel
	  {
		  get
		  {
			return bpmnModel;
		  }
		  set
		  {
			this.bpmnModel = value;
		  }
	  }


	  public virtual Process Process
	  {
		  get
		  {
			return process;
		  }
		  set
		  {
			this.process = value;
		  }
	  }


	}

}