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
namespace org.activiti.engine.impl.bpmn.parser.handler
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;

    public class ServiceTaskParseHandler : AbstractActivityBpmnParseHandler<ServiceTask>
    {
        protected internal override Type HandledType
        {
            get
            {
                return typeof(ServiceTask);
            }
        }

        protected internal override void executeParse(BpmnParse bpmnParse, ServiceTask serviceTask)
        {

            if (!string.IsNullOrWhiteSpace(serviceTask.Type))
            {
                createActivityBehaviorForServiceTaskType(bpmnParse, serviceTask);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                createClassDelegateServiceTask(bpmnParse, serviceTask);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                createServiceTaskDelegateExpressionActivityBehavior(bpmnParse, serviceTask);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                createServiceTaskExpressionActivityBehavior(bpmnParse, serviceTask);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_WEBSERVICE.Equals(serviceTask.ImplementationType, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrWhiteSpace(serviceTask.OperationRef))
            {
                createWebServiceActivityBehavior(bpmnParse, serviceTask);
            }
            else
            {
                createDefaultServiceTaskActivityBehavior(bpmnParse, serviceTask);
            }

        }

        protected internal virtual void createActivityBehaviorForServiceTaskType(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            if (serviceTask.Type.Equals("mail", StringComparison.CurrentCultureIgnoreCase))
            {
                createMailActivityBehavior(bpmnParse, serviceTask);
            }
            else if (serviceTask.Type.Equals("mule", StringComparison.CurrentCultureIgnoreCase))
            {
                createMuleActivityBehavior(bpmnParse, serviceTask);
            }
            else if (serviceTask.Type.Equals("camel", StringComparison.CurrentCultureIgnoreCase))
            {
                createCamelActivityBehavior(bpmnParse, serviceTask);
            }
            else if (serviceTask.Type.Equals("shell", StringComparison.CurrentCultureIgnoreCase))
            {
                createShellActivityBehavior(bpmnParse, serviceTask);
            }
            else
            {
                createActivityBehaviorForCustomServiceTaskType(bpmnParse, serviceTask);
            }
        }

        protected internal virtual void createMailActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createMailActivityBehavior(serviceTask);
        }

        protected internal virtual void createMuleActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createMuleActivityBehavior(serviceTask);
        }

        protected internal virtual void createCamelActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createCamelActivityBehavior(serviceTask);
        }

        protected internal virtual void createShellActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createShellActivityBehavior(serviceTask);
        }

        protected internal virtual void createActivityBehaviorForCustomServiceTaskType(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            logger.LogWarning("Invalid service task type: '" + serviceTask.Type + "' " + " for service task " + serviceTask.Id);
        }

        protected internal virtual void createClassDelegateServiceTask(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createClassDelegateServiceTask(serviceTask);
        }

        protected internal virtual void createServiceTaskDelegateExpressionActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createServiceTaskDelegateExpressionActivityBehavior(serviceTask);
        }

        protected internal virtual void createServiceTaskExpressionActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createServiceTaskExpressionActivityBehavior(serviceTask);
        }

        protected internal virtual void createWebServiceActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createWebServiceActivityBehavior(serviceTask);
        }

        protected internal virtual void createDefaultServiceTaskActivityBehavior(BpmnParse bpmnParse, ServiceTask serviceTask)
        {
            serviceTask.Behavior = bpmnParse.ActivityBehaviorFactory.createDefaultServiceTaskBehavior(serviceTask);
        }
    }

}