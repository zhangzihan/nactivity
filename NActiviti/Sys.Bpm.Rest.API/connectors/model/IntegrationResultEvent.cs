using Newtonsoft.Json;
using System.Collections.Generic;

/*
 * Copyright 2017 Alfresco, Inc. and/or its affiliates.
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

namespace Sys.Workflow.Services.Connectors.Models
{

    /// <summary>
    /// 
    /// </summary>
    public class IntegrationResultEvent
    {
        private readonly string id;
        private readonly string executionId;
        private readonly string flowNodeId;
        private readonly IDictionary<string, object> variables;
        private readonly string appName;
        private readonly string appVersion;
        private readonly string serviceName;
        private readonly string serviceFullName;
        private readonly string serviceType;
        private readonly string serviceVersion;


        /// <summary>
        /// 
        /// </summary>
        //used by json deserialization
        public IntegrationResultEvent()
        {
            this.id = System.Guid.NewGuid().ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        //[JsonConstructor]
        public IntegrationResultEvent([JsonProperty("ExecutionId")]string executionId,
            [JsonProperty("Variables")]IDictionary<string, object> variables,
            [JsonProperty("AppName")]string appName,
            [JsonProperty("AppVersion")]string appVersion,
            [JsonProperty("ServiceName")]string serviceName,
            [JsonProperty("ServiceFullName")] string serviceFullName,
            [JsonProperty("ServiceType")]string serviceType,
            [JsonProperty("ServiceVersion")]string serviceVersion) : this()
        {
            this.executionId = executionId;
            this.variables = variables;
            this.appName = appName;
            this.appVersion = appVersion;
            this.serviceName = serviceName;
            this.serviceFullName = serviceFullName;
            this.serviceType = serviceType;
            this.serviceVersion = serviceVersion;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string Id
        {
            get
            {
                return id;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string FlowNodeId
        {
            get
            {
                return flowNodeId;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return variables;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string AppName
        {
            get
            {
                return appName;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string AppVersion
        {
            get
            {
                return appVersion;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string ServiceName
        {
            get
            {
                return serviceName;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string ServiceFullName
        {
            get
            {
                return serviceFullName;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string ServiceType
        {
            get
            {
                return serviceType;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual string ServiceVersion
        {
            get
            {
                return serviceVersion;
            }
        }
    }

}