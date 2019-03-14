using org.activiti.engine.repository;
using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
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
 *
 */

namespace org.activiti.cloud.services.api.model.converter
{

    /// <summary>
    /// 
    /// </summary>
    public class DeploymentConverter : IModelConverter<IDeployment, Deployment>
    {
        private readonly ListConverter listConverter;


        /// <summary>
        /// 
        /// </summary>
        public DeploymentConverter(ListConverter listConverter)
        {
            this.listConverter = listConverter;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual Deployment from(IDeployment source)
        {
            Deployment deployment = null;
            if (source != null)
            {
                deployment = new Deployment(source.Id,
                    source.Name,
                    source.Category,
                    source.TenantId,
                    source.BusinessKey,
                    source.DeploymentTime);
            }
            return deployment;
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual IList<Deployment> from(IList<IDeployment> deployments)
        {
            return listConverter.from(deployments, this);
        }
    }
}