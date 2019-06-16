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
namespace Sys.Workflow.engine.impl.bpmn.parser.handler
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.bpmn.model;

    public class ServiceTaskParseHandler : AbstractActivityBpmnParseHandler<ServiceTask>
    {
        protected internal override Type HandledType
        {
            get
            {
                return typeof(ServiceTask);
            }
        }

        protected internal override void ExecuteParse(BpmnParse bpmnParse, ServiceTask serviceTask)
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

        protected internal virtual void CreateActivityBehaviorForServiceTaskType(BpmnParse bpmnParse, ServiceTask serviceTask)
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

        protected internal virtual void CreateMailActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateMailActivityBehavior(serviceTask);
        }

        protected internal virtual void CreateMuleActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateMuleActivityBehavior(serviceTask);
        }

        protected internal virtual void CreateCamelActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateCamelActivityBehavior(serviceTask);
        }

        protected internal virtual void CreateShellActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateShellActivityBehavior(serviceTask);
        }

        protected internal virtual void CreateActivityBehaviorForCustomServiceTaskType(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            logger.LogWarning("Invalid service task type: '" + serviceTask.Type + "' " + " for service task " + serviceTask.Id);
        }

        protected internal virtual void CreateClassDelegateServiceTask(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateClassDelegateServiceTask(serviceTask);
        }

        protected internal virtual void CreateServiceTaskDelegateExpressionActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateServiceTaskDelegateExpressionActivityBehavior(serviceTask);
        }

        protected internal virtual void CreateServiceTaskExpressionActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateServiceTaskExpressionActivityBehavior(serviceTask);
        }

        protected internal virtual void CreateWebServiceActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateWebServiceActivityBehavior(serviceTask);
        }

        protected internal virtual void CreateDefaultServiceTaskActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_CLASS;
            serviceTask.Implementation = ImplementationType.IMPLEMENTATION_TASK_SERVICE_DEFAULT;
            //修改默认ServiceTaskBehavior
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.CreateClassDelegateServiceTask(serviceTask);
            //serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createDefaultServiceTaskBehavior(serviceTask);
        }
    }
}