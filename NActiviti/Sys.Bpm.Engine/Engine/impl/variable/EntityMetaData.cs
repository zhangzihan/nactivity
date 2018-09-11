using System;
using System.Reflection;

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

    /// <summary>
    /// Class containing meta-data about Entity-classes.
    /// 
    /// 
    /// </summary>
    public class EntityMetaData
    {

        private bool isJPAEntity;
        private Type entityClass;
        private MethodInfo idMethod;
        private FieldInfo idField;

        public virtual bool JPAEntity
        {
            get
            {
                return isJPAEntity;
            }
            set
            {
                this.isJPAEntity = value;
            }
        }


        public virtual Type EntityClass
        {
            get
            {
                return entityClass;
            }
            set
            {
                this.entityClass = value;
            }
        }


        public virtual MethodInfo IdMethod
        {
            get
            {
                return idMethod;
            }
            set
            {
                this.idMethod = value;
            }
        }


        public virtual FieldInfo IdField
        {
            get
            {
                return idField;
            }
            set
            {
                this.idField = value;
            }
        }


        public virtual Type IdType
        {
            get
            {
                Type idType = null;
                if (idField != null)
                {
                    idType = idField.FieldType;
                }
                else if (idMethod != null)
                {
                    idType = idMethod.ReturnType;
                }
                return idType;
            }
        }
    }

}