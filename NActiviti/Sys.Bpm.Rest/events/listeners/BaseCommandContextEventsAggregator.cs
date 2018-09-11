using org.activiti.engine.impl.context;
using org.activiti.engine.impl.interceptor;
using System;
using System.Collections.Generic;

/*
 * Copyright 2018 Alfresco, Inc. and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace org.activiti.cloud.services.events.listeners
{

    public abstract class BaseCommandContextEventsAggregator<E, L> where L : ICommandContextCloseListener
    {

        public virtual void add(E element)
        {
            ICommandContext currentCommandContext = CurrentCommandContext;
            IList<E> attributes = currentCommandContext.getGenericAttribute<IList<E>>(AttributeKey);
            if (attributes == null)
            {
                attributes = new List<E>();
                currentCommandContext.addAttribute(AttributeKey, attributes);
            }
            attributes.Add(element);

            if (!currentCommandContext.hasCloseListener(CloseListenerClass))
            {
                currentCommandContext.addCloseListener(CloseListener);
            }

        }

        protected internal abstract Type CloseListenerClass { get; }

        protected internal abstract L CloseListener { get; }

        protected internal abstract string AttributeKey { get; }

        protected internal virtual ICommandContext CurrentCommandContext
        {
            get
            {
                return Context.CommandContext;
            }
        }

    }

}