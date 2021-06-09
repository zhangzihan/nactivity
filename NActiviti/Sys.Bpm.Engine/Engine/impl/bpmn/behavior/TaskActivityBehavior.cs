using Newtonsoft.Json.Linq;
using System;
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
namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    /// <summary>
    /// Parent class for all BPMN 2.0 task types such as ServiceTask, ScriptTask, UserTask, etc.
    /// 
    /// When used on its own, it behaves just as a pass-through activity.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class TaskActivityBehavior : AbstractBpmnActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal virtual string GetActiveValue(string originalValue, string propertyName, JToken taskElementProperties)
        {
            string activeValue = originalValue;
            if (taskElementProperties is object)
            {
                JToken overrideValueNode = taskElementProperties[propertyName];
                if (overrideValueNode is null)
                {
                    activeValue = null;
                }
                else
                {
                    activeValue = overrideValueNode.ToString();
                }
            }
            return activeValue;
        }

        protected internal virtual IList<string> GetActiveValueList(IList<string> originalValues, string propertyName, JToken taskElementProperties)
        {
            IList<string> activeValues = originalValues;
            if (taskElementProperties is object)
            {
                JToken overrideValuesNode = taskElementProperties[propertyName];
                if (overrideValuesNode is object)
                {
                    if (overrideValuesNode is null || !(overrideValuesNode is JArray))
                    {
                        activeValues = null;
                    }
                    else
                    {
                        activeValues = new List<string>();
                        foreach (JToken valueNode in overrideValuesNode)
                        {
                            activeValues.Add(valueNode.ToString());
                        }
                    }
                }
            }
            return activeValues;
        }
    }

}