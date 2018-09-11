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
namespace org.activiti.engine.impl
{
	using org.activiti.engine.runtime;

	public class DataObjectImpl : IDataObject
	{
	  private string name;
	  private object value;
	  private string description;
	  private string localizedName;
	  private string localizedDescription;
	  private string dataObjectDefinitionKey;

	  private string type;

	  public DataObjectImpl(string name, object value, string description, string type, string localizedName, string localizedDescription, string dataObjectDefinitionKey)
	  {

		this.name = name;
		this.value = value;
		this.type = type;
		this.description = description;
		this.localizedName = localizedName;
		this.localizedDescription = localizedDescription;
		this.dataObjectDefinitionKey = dataObjectDefinitionKey;
	  }

	  public virtual string Name
	  {
		  get
		  {
			return name;
		  }
		  set
		  {
			this.name = value;
		  }
	  }


	  public virtual string LocalizedName
	  {
		  get
		  {
			if (!string.ReferenceEquals(localizedName, null) && localizedName.Length > 0)
			{
			  return localizedName;
			}
			else
			{
			  return name;
			}
		  }
		  set
		  {
			this.localizedName = value;
		  }
	  }


	  public virtual string Description
	  {
		  get
		  {
			if (!string.ReferenceEquals(localizedDescription, null) && localizedDescription.Length > 0)
			{
			  return localizedDescription;
			}
			else
			{
			  return description;
			}
		  }
		  set
		  {
			this.description = value;
		  }
	  }


	  public virtual object Value
	  {
		  get
		  {
			return value;
		  }
		  set
		  {
			this.value = value;
		  }
	  }


	  public virtual string Type
	  {
		  get
		  {
			return type;
		  }
		  set
		  {
			this.type = value;
		  }
	  }



	  public virtual string DataObjectDefinitionKey
	  {
		  get
		  {
			return dataObjectDefinitionKey;
		  }
		  set
		  {
			this.dataObjectDefinitionKey = value;
		  }
	  }


	}
}