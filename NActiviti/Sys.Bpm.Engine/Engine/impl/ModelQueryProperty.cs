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

namespace org.activiti.engine.impl
{

    using org.activiti.engine.query;
    using org.activiti.engine.repository;

    /// <summary>
    /// Contains the possible properties that can be used in a <seealso cref="IModelQuery"/>.
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ModelQueryProperty : IQueryProperty
    {

        private const long serialVersionUID = 1L;

        private static readonly IDictionary<string, ModelQueryProperty> properties = new Dictionary<string, ModelQueryProperty>();

        public static readonly ModelQueryProperty MODEL_CATEGORY = new ModelQueryProperty("RES.CATEGORY_");
        public static readonly ModelQueryProperty MODEL_ID = new ModelQueryProperty("RES.ID_");
        public static readonly ModelQueryProperty MODEL_VERSION = new ModelQueryProperty("RES.VERSION_");
        public static readonly ModelQueryProperty MODEL_NAME = new ModelQueryProperty("RES.NAME_");
        public static readonly ModelQueryProperty MODEL_CREATE_TIME = new ModelQueryProperty("RES.CREATE_TIME_");
        public static readonly ModelQueryProperty MODEL_LAST_UPDATE_TIME = new ModelQueryProperty("RES.LAST_UPDATE_TIME_");
        public static readonly ModelQueryProperty MODEL_KEY = new ModelQueryProperty("RES.KEY_");
        public static readonly ModelQueryProperty MODEL_TENANT_ID = new ModelQueryProperty("RES.TENANT_ID_");

        private string name;

        public ModelQueryProperty(string name)
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

        public static ModelQueryProperty FindByName(string propertyName)
        {
            return properties[propertyName];
        }

    }

}