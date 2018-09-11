using System.Text;

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
namespace org.activiti.validation
{
	public class ValidationError
	{

	  protected internal string validatorSetName;

	  protected internal string problem;

	  // Default description in english.
	  // Other languages can map the validatorSetName/validatorName to the
	  // translated version.
	  protected internal string defaultDescription;

	  protected internal string processDefinitionId;

	  protected internal string processDefinitionName;

	  protected internal int xmlLineNumber;

	  protected internal int xmlColumnNumber;

	  protected internal string activityId;

	  protected internal string activityName;

	  protected internal bool isWarning;

	  public virtual string ValidatorSetName
	  {
		  get
		  {
			return validatorSetName;
		  }
		  set
		  {
			this.validatorSetName = value;
		  }
	  }


	  public virtual string Problem
	  {
		  get
		  {
			return problem;
		  }
		  set
		  {
			this.problem = value;
		  }
	  }


	  public virtual string DefaultDescription
	  {
		  get
		  {
			return defaultDescription;
		  }
		  set
		  {
			this.defaultDescription = value;
		  }
	  }


	  public virtual string ProcessDefinitionId
	  {
		  get
		  {
			return processDefinitionId;
		  }
		  set
		  {
			this.processDefinitionId = value;
		  }
	  }


	  public virtual string ProcessDefinitionName
	  {
		  get
		  {
			return processDefinitionName;
		  }
		  set
		  {
			this.processDefinitionName = value;
		  }
	  }


	  public virtual int XmlLineNumber
	  {
		  get
		  {
			return xmlLineNumber;
		  }
		  set
		  {
			this.xmlLineNumber = value;
		  }
	  }


	  public virtual int XmlColumnNumber
	  {
		  get
		  {
			return xmlColumnNumber;
		  }
		  set
		  {
			this.xmlColumnNumber = value;
		  }
	  }


	  public virtual string ActivityId
	  {
		  get
		  {
			return activityId;
		  }
		  set
		  {
			this.activityId = value;
		  }
	  }


	  public virtual string ActivityName
	  {
		  get
		  {
			return activityName;
		  }
		  set
		  {
			this.activityName = value;
		  }
	  }


	  public virtual bool Warning
	  {
		  get
		  {
			return isWarning;
		  }
		  set
		  {
			this.isWarning = value;
		  }
	  }


	  public override string ToString()
	  {
		StringBuilder strb = new StringBuilder();
		strb.Append("[Validation set: '" + validatorSetName + "' | Problem: '" + problem + "'] : ");
		strb.Append(defaultDescription);
		strb.Append(" - [Extra info : ");
		bool extraInfoAlreadyPresent = false;
		if (!string.ReferenceEquals(processDefinitionId, null))
		{
		  strb.Append("processDefinitionId = " + processDefinitionId);
		  extraInfoAlreadyPresent = true;
		}
		if (!string.ReferenceEquals(processDefinitionName, null))
		{
		  if (extraInfoAlreadyPresent)
		  {
			strb.Append(" | ");
		  }
		  strb.Append("processDefinitionName = " + processDefinitionName + " | ");
		  extraInfoAlreadyPresent = true;
		}
		if (!string.ReferenceEquals(activityId, null))
		{
		  if (extraInfoAlreadyPresent)
		  {
			strb.Append(" | ");
		  }
		  strb.Append("id = " + activityId + " | ");
		  extraInfoAlreadyPresent = true;
		}
		if (!string.ReferenceEquals(activityName, null))
		{
		  if (extraInfoAlreadyPresent)
		  {
			strb.Append(" | ");
		  }
		  strb.Append("activityName = " + activityName + " | ");
		  extraInfoAlreadyPresent = true;
		}
		strb.Append("]");
		if (xmlLineNumber > 0 && xmlColumnNumber > 0)
		{
		  strb.Append(" ( line: " + xmlLineNumber + ", column: " + xmlColumnNumber + ")");
		}
		return strb.ToString();
	  }

	}

}