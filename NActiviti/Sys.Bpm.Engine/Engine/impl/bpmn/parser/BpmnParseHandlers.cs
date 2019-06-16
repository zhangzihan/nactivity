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
namespace Sys.Workflow.engine.impl.bpmn.parser
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.parse;
    using Sys.Workflow;

    /// 
    public class BpmnParseHandlers
    {
        protected internal IDictionary<Type, IList<IBpmnParseHandler>> parseHandlers;
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<BpmnParseHandlers>();

        public BpmnParseHandlers()
        {
            this.parseHandlers = new Dictionary<Type, IList<IBpmnParseHandler>>();
        }

        public virtual IList<IBpmnParseHandler> GetHandlersFor(Type clazz)
        {
            return parseHandlers[clazz];
        }

        public virtual void AddHandlers(IList<IBpmnParseHandler> bpmnParseHandlers)
        {
            foreach (IBpmnParseHandler bpmnParseHandler in bpmnParseHandlers)
            {
                AddHandler(bpmnParseHandler);
            }
        }

        public virtual void AddHandler(IBpmnParseHandler bpmnParseHandler)
        {
            foreach (Type type in bpmnParseHandler.HandledTypes)
            {
                if (parseHandlers.TryGetValue(type, out IList<IBpmnParseHandler> handlers) == false)
                {
                    handlers = new List<IBpmnParseHandler>();
                    parseHandlers[type] = handlers;
                }
                handlers.Add(bpmnParseHandler);
            }
        }

        public virtual void ParseElement(BpmnParse bpmnParse, BaseElement element)
        {
            if (element is DataObject)
            {
                // ignore DataObject elements because they are processed on Process
                // and Sub process level
                return;
            }

            if (element is FlowElement)
            {
                bpmnParse.CurrentFlowElement = (FlowElement)element;
            }

            // Execute parse handlers
            parseHandlers.TryGetValue(element.GetType(), out IList<IBpmnParseHandler> handlers);

            if (handlers == null)
            {
                logger.LogWarning("Could not find matching parse handler for + " + element.Id + " this is likely a bug.");
            }
            else
            {
                foreach (IBpmnParseHandler handler in handlers)
                {
                    handler.Parse(bpmnParse, element);
                }
            }
        }
    }
}