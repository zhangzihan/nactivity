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
    /// Implementation of the BPMN 2.0 'dataInput' and 'dataOutput'
    /// 
    /// 
    /// </summary>
    public class Data
	{

	  protected internal string id;

	  protected internal string name;

	  protected internal ItemDefinition definition;

	  public Data(string id, string name, ItemDefinition definition)
	  {
		this.id = id;
		this.name = name;
		this.definition = definition;
	  }

	  public virtual string Id
	  {
		  get
		  {
			return this.id;
		  }
	  }

	  public virtual string Name
	  {
		  get
		  {
			return this.name;
		  }
	  }

	  public virtual ItemDefinition Definition
	  {
		  get
		  {
			return this.definition;
		  }
	  }
	}

}