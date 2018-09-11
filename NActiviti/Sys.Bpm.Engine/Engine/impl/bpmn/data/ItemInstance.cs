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
	/// An instance of <seealso cref="ItemDefinition"/>
	/// 
	/// 
	/// </summary>
	public class ItemInstance
	{

	  protected internal ItemDefinition item;

	  protected internal IStructureInstance structureInstance;

	  public ItemInstance(ItemDefinition item, IStructureInstance structureInstance)
	  {
		this.item = item;
		this.structureInstance = structureInstance;
	  }

	  public virtual ItemDefinition Item
	  {
		  get
		  {
			return this.item;
		  }
	  }

	  public virtual IStructureInstance StructureInstance
	  {
		  get
		  {
			return this.structureInstance;
		  }
	  }

	  private FieldBaseStructureInstance FieldBaseStructureInstance
	  {
		  get
		  {
			return (FieldBaseStructureInstance) this.structureInstance;
		  }
	  }

	  public virtual object getFieldValue(string fieldName)
	  {
		return this.FieldBaseStructureInstance.getFieldValue(fieldName);
	  }

	  public virtual void setFieldValue(string fieldName, object value)
	  {
		this.FieldBaseStructureInstance.setFieldValue(fieldName, value);
	  }
	}

}