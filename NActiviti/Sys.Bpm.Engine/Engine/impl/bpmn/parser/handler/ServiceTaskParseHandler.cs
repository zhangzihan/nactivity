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
namespace Sys.Workflow.Engine.Impl.Bpmn.Parser.Handlers
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;

    public class ServiceTaskParseHandler : AbstractActivityBpmnParseHandler<ServiceTask>
    {
        public override Type HandledType
        {
            get
            {
                return typeof(ServiceTask);
            }
        }

        public override void ExecuteParse(BpmnParse bpmnParse, ServiceTask serviceTask)
        {

            if (!string.IsNullOrWhiteSpace(serviceTask.Type))
            {
                CreateActivityBehaviorForServiceTaskType(bpmnParse, serviceTask);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                CreateClassDelegateServiceTask(bpmnParse, serviceTask);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                CreateServiceTaskDelegateExpressionActivityBehavior(bpmnParse, serviceTask);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                CreateServiceTaskExpressionActivityBehavior(bpmnParse, serviceTask);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrWhiteSpace(serviceTask.OperationRef))
            {
                CreateWebServiceActivityBehavior(bpmnParse, serviceTask);
            }
            else
            {
                CreateDefaultServiceTaskActivityBehavior(bpmnParse, serviceTask);
            }

        }

        public virtual void CreateActivityBehaviorForServiceTaskType(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            if (serviceTask.Type.Equals("mail", StringComparison.CurrentCultureIgnoreCase))
            {
                CreateMailActivityBehavior(bpmnParse, serviceTask);
            }
            else if (serviceTask.Type.Equals("mule", StringComparison.CurrentCultureIgnoreCase))
            {
                CreateMuleActivityBehavior(bpmnParse, serviceTask);
            }
            else if (serviceTask.Type.Equals("camel", StringComparison.CurrentCultureIgnoreCase))
            {
                CreateCamelActivityBehavior(bpmnParse, serviceTask);
            }
            else if (serviceTask.Type.Equals("shell", StringComparison.CurrentCultureIgnoreCase))
            {
                CreateShellActivityBehavior(bpmnParse, serviceTask);
            }
            else
            {
                CreateActivityBehaviorForCustomServiceTaskType(bpmnParse, serviceTask);
            }
        }

        public virtual void CreateMailActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateMailActivityBehavior(serviceTask);
        }

        public virtual void CreateMuleActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateMuleActivityBehavior(serviceTask);
        }

        public virtual void CreateCamelActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateCamelActivityBehavior(serviceTask);
        }

        public virtual void CreateShellActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateShellActivityBehavior(serviceTask);
        }

        public virtual void CreateActivityBehaviorForCustomServiceTaskType(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            logger.LogWarning("Invalid service task type: '" + serviceTask.Type + "' " + " for service task " + serviceTask.Id);
        }

        public virtual void CreateClassDelegateServiceTask(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateClassDelegateServiceTask(serviceTask);
        }

        public virtual void CreateServiceTaskDelegateExpressionActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateServiceTaskDelegateExpressionActivityBehavior(serviceTask);
        }

        public virtual void CreateServiceTaskExpressionActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateServiceTaskExpressionActivityBehavior(serviceTask);
        }

        public virtual void CreateWebServiceActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateWebServiceActivityBehavior(serviceTask);
        }

        public virtual void CreateDefaultServiceTaskActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_CLASS;
            serviceTask.Implementation = ImplementationType.IMPLEMENTATION_TASK_SERVICE_DEFAULT;
            //修改默认ServiceTaskBehavior
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateClassDelegateServiceTask(serviceTask);
            //serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createDefaultServiceTaskBehavior(serviceTask);
        }
    }
}