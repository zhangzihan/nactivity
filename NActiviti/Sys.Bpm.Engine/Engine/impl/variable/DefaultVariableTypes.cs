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
namespace org.activiti.engine.impl.variable
{

    /// 
    [Serializable]
    public class DefaultVariableTypes : IVariableTypes
    {

        private const long serialVersionUID = 1L;

        private readonly IList<IVariableType> typesList = new List<IVariableType>();
        private readonly IDictionary<string, IVariableType> typesMap = new Dictionary<string, IVariableType>();

        public virtual IVariableTypes addType(IVariableType type)
        {
            return addType(type, typesList.Count);
        }

        public virtual IVariableTypes addType(IVariableType type, int index)
        {
            typesList.Insert(index, type);
            typesMap[type.TypeName] = type;

            return this;
        }

        public virtual IList<IVariableType> TypesList
        {
            set
            {
                this.typesList.Clear();
                ((List<IVariableType>)this.typesList).AddRange(value);
                this.typesMap.Clear();
                foreach (IVariableType type in value)
                {
                    typesMap[type.TypeName] = type;
                }
            }
        }

        public virtual IVariableType getVariableType(string typeName)
        {
            return typesMap[typeName];
        }

        public virtual IVariableType findVariableType(object value)
        {
            foreach (IVariableType type in typesList)
            {
                if (type.isAbleToStore(value))
                {
                    return type;
                }
            }
            throw new ActivitiException("couldn't find a variable type that is able to serialize " + value);
        }

        public virtual int getTypeIndex(IVariableType type)
        {
            return typesList.IndexOf(type);
        }

        public virtual int getTypeIndex(string typeName)
        {
            IVariableType type = typesMap[typeName];
            if (type != null)
            {
                return getTypeIndex(type);
            }
            else
            {
                return -1;
            }
        }

        public virtual IVariableTypes removeType(IVariableType type)
        {
            typesList.Remove(type);
            typesMap.Remove(type.TypeName);
            return this;
        }
    }

}