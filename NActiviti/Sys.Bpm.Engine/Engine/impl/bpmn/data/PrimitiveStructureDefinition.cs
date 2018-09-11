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
namespace org.activiti.engine.impl.bpmn.data
{
	/// <summary>
	/// Represents a structure based on a primitive class
	/// 
	/// 
	/// </summary>
	public class PrimitiveStructureDefinition : IStructureDefinition
	{

	  protected internal string id;

	  protected internal Type primitiveClass;

	  public PrimitiveStructureDefinition(string id, Type primitiveClass)
	  {
		this.id = id;
		this.primitiveClass = primitiveClass;
	  }

	  public virtual string Id
	  {
		  get
		  {
			return this.id;
		  }
	  }

	  public virtual Type PrimitiveClass
	  {
		  get
		  {
			return primitiveClass;
		  }
	  }

	  public virtual IStructureInstance createInstance()
	  {
		return new PrimitiveStructureInstance(this);
	  }
	}

}