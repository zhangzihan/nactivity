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

namespace org.activiti.engine.impl
{

    using org.activiti.engine.query;

    /// <summary>
    /// Contains the possible properties that can be used in a <seealso cref="IDeploymentQuery"/>.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class DeploymentQueryProperty : IQueryProperty
    {

        private const long serialVersionUID = 1L;

        private static readonly IDictionary<string, DeploymentQueryProperty> properties = new Dictionary<string, DeploymentQueryProperty>();

        public static readonly DeploymentQueryProperty DEPLOYMENT_ID = new DeploymentQueryProperty("RES.ID_");
        public static readonly DeploymentQueryProperty DEPLOYMENT_NAME = new DeploymentQueryProperty("RES.NAME_");
        public static readonly DeploymentQueryProperty DEPLOYMENT_TENANT_ID = new DeploymentQueryProperty("RES.TENANT_ID_");
        public static readonly DeploymentQueryProperty DEPLOY_TIME = new DeploymentQueryProperty("RES.DEPLOY_TIME_");

        private string name;

        public DeploymentQueryProperty(string name)
        {
            this.name = name;
            properties[name] = this;
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public static DeploymentQueryProperty findByName(string propertyName)
        {
            return properties[propertyName];
        }

    }

}