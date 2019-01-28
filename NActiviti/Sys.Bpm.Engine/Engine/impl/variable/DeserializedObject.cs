﻿/* Licensed under the Apache License, Version 2.0 (the "License");
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

    using org.activiti.engine.impl.persistence.entity;
    using System;
    using System.Collections;

    /// 
    /// 
    public class DeserializedObject
    {

        protected internal SerializableType type;
        protected internal object deserializedObject;
        protected internal byte[] originalBytes;
        protected internal IVariableInstanceEntity variableInstanceEntity;

        public DeserializedObject(SerializableType type, object deserializedObject, byte[] serializedBytes, IVariableInstanceEntity variableInstanceEntity)
        {
            this.type = type;
            this.deserializedObject = deserializedObject;
            this.originalBytes = serializedBytes;
            this.variableInstanceEntity = variableInstanceEntity;
        }

        public virtual void verifyIfBytesOfSerializedObjectChanged()
        {
            // this first check verifies if the variable value was not overwritten with another object
            if (deserializedObject == variableInstanceEntity.CachedValue && !variableInstanceEntity.Deleted)
            {
                byte[] bytes = type.serialize(deserializedObject, variableInstanceEntity);
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(originalBytes, bytes))
                {

                    // Add an additional check to prevent byte differences due to JDK changes etc
                    object originalObject = type.deserialize(originalBytes, variableInstanceEntity);
                    byte[] refreshedOriginalBytes = type.serialize(originalObject, variableInstanceEntity);

                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(refreshedOriginalBytes, bytes))
                    {
                        variableInstanceEntity.Bytes = bytes;
                    }
                }
            }
        }
    }

}