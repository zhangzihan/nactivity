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
namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow;
    using System.Collections.Generic;
    using System.Threading;

    /// 
    public class ValidateExecutionRelatedEntityCountCfgCmd : ICommand<object>
    {
        private ILogger<ValidateExecutionRelatedEntityCountCfgCmd> log = ProcessEngineServiceProvider.LoggerService<ValidateExecutionRelatedEntityCountCfgCmd>();

        public static string PROPERTY_EXECUTION_RELATED_ENTITY_COUNT = "cfg.execution-related-entities-count";

        public virtual object Execute(ICommandContext commandContext)
        {
            /*
             * If execution related entity counting is on in config | Current property in database : Result
             * 
             *  A) true | not there : write new property with value 'true'
             *  B) true | true : all good
             *  C) true | false : the feature was disabled before, but it is enabled now. Old executions will have a local flag with false. 
             *                    It is now enabled. This is fine, will be handled in logic. Update the property.
             *  
             *  D) false | not there: write new property with value 'false'
             *  E) false | true : the feature was disabled before and enabled now. To guarantee data consistency, we need to remove the flag from all executions.
             *                    Update the property.
             *  F) false | false : all good
             *  
             * In case A and D (not there), the property needs to be written to the db
             * Only in case E something needs to be done explicitely, the others are okay.
             */

            IPropertyEntityManager propertyEntityManager = commandContext.PropertyEntityManager;

            bool configProperty = commandContext.ProcessEngineConfiguration.PerformanceSettings.EnableExecutionRelationshipCounts;
            IPropertyEntity propertyEntity = propertyEntityManager.FindById<PropertyEntityImpl>(new KeyValuePair<string, object>("name", PROPERTY_EXECUTION_RELATED_ENTITY_COUNT));

            if (propertyEntity == null)
            {

                // 'not there' case in the table above: easy, simply insert the value
                IPropertyEntity newPropertyEntity = propertyEntityManager.Create();
                newPropertyEntity.Name = PROPERTY_EXECUTION_RELATED_ENTITY_COUNT;
                newPropertyEntity.Value = Convert.ToString(configProperty);
                propertyEntityManager.Insert(newPropertyEntity);

            }
            else
            {

                bool propertyValue = Convert.ToBoolean(propertyEntity.Value.ToString().ToLower());
                if (!configProperty && propertyValue)
                {
                    if (log.IsEnabled(LogLevel.Information))
                    {
                        log.LogInformation("Configuration change: execution related entity counting feature was enabled before, but now disabled. " + "Updating all execution entities.");
                    }
                    commandContext.ProcessEngineConfiguration.ExecutionDataManager.UpdateAllExecutionRelatedEntityCountFlags(configProperty);
                }

                // Update property
                if (configProperty != propertyValue)
                {
                    propertyEntity.Value = Convert.ToString(configProperty);
                    propertyEntityManager.Update(propertyEntity);
                }

            }

            return commandContext.GetResult();
        }
    }
}