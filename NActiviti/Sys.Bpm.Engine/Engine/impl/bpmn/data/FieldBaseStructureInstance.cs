using System.Collections.Generic;

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
	/// An instance of <seealso cref="IFieldBaseStructureDefinition"/>
	/// 
	/// 
	/// </summary>
	public class FieldBaseStructureInstance : IStructureInstance
	{

	  protected internal IFieldBaseStructureDefinition structureDefinition;

	  protected internal IDictionary<string, object> fieldValues;

	  public FieldBaseStructureInstance(IFieldBaseStructureDefinition structureDefinition)
	  {
		this.structureDefinition = structureDefinition;
		this.fieldValues = new Dictionary<string, object>();
	  }

	  public virtual object getFieldValue(string fieldName)
	  {
		return this.fieldValues[fieldName];
	  }

	  public virtual void setFieldValue(string fieldName, object value)
	  {
		this.fieldValues[fieldName] = value;
	  }

	  public virtual int FieldSize
	  {
		  get
		  {
			return this.structureDefinition.FieldSize;
		  }
	  }

	  public virtual string getFieldNameAt(int index)
	  {
		return this.structureDefinition.getFieldNameAt(index);
	  }

	  public virtual object[] toArray()
	  {
		int fieldSize = this.FieldSize;
		object[] arguments = new object[fieldSize];
		for (int i = 0; i < fieldSize; i++)
		{
		  string fieldName = this.getFieldNameAt(i);
		  object argument = this.getFieldValue(fieldName);
		  arguments[i] = argument;
		}
		return arguments;
	  }

	  public virtual void loadFrom(object[] array)
	  {
		int fieldSize = this.FieldSize;
		for (int i = 0; i < fieldSize; i++)
		{
		  string fieldName = this.getFieldNameAt(i);
		  object fieldValue = array[i];
		  this.setFieldValue(fieldName, fieldValue);
		}
	  }
	}

}