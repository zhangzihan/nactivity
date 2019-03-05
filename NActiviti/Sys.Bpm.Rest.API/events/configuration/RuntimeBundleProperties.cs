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

namespace org.activiti.cloud.services.events.configuration
{
    public class RuntimeBundleProperties
    {
        private string rbSpringAppName;
        private string serviceType;
        private string serviceVersion;
        private string appName;
        private string appVersion;

        private RuntimeBundleEventsProperties eventsProperties = new RuntimeBundleEventsProperties();

        public virtual string RbSpringAppName
        {
            get
            {
                return rbSpringAppName;
            }
            set
            {
                this.rbSpringAppName = value;
            }
        }

        public virtual string ServiceFullName
        {
            get
            {
                //if we change this then we also have to change integration-result-stream.properties
                return rbSpringAppName;
            }
        }

        // a level of indirection here as we may change this to use its own property
        public virtual string ServiceName
        {
            get
            {
                return RbSpringAppName;
            }
        }


        public virtual string ServiceType
        {
            get
            {
                return serviceType;
            }
            set
            {
                this.serviceType = value;
            }
        }


        public virtual string ServiceVersion
        {
            get
            {
                return serviceVersion;
            }
            set
            {
                this.serviceVersion = value;
            }
        }


        public virtual string AppName
        {
            get
            {
                return appName;
            }
            set
            {
                this.appName = value;
            }
        }


        public virtual string AppVersion
        {
            get
            {
                return appVersion;
            }
            set
            {
                this.appVersion = value;
            }
        }


        public virtual RuntimeBundleEventsProperties EventsProperties
        {
            get
            {
                return eventsProperties;
            }
            set
            {
                this.eventsProperties = value;
            }
        }


        public class RuntimeBundleEventsProperties
        {

            internal bool integrationAuditEventsEnabled;

            public virtual bool IntegrationAuditEventsEnabled
            {
                get
                {
                    return integrationAuditEventsEnabled;
                }
                set
                {
                    this.integrationAuditEventsEnabled = value;
                }
            }

        }
    }

}