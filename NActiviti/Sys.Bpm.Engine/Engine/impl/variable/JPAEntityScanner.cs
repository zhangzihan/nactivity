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

namespace Sys.Workflow.Engine.Impl.Variable
{


    /// <summary>
    /// Scans class and creates <seealso cref="EntityMetaData"/> based on it.
    /// 
    /// 
    /// </summary>
    public class JPAEntityScanner
    {

        public virtual EntityMetaData ScanClass(Type clazz)
        {
            EntityMetaData metaData = new EntityMetaData();
            // in case with JPA Enhancement
            // method should iterate over superclasses list
            // to find @Entity and @Id annotations
            while (clazz != null && !clazz.Equals(typeof(object)))
            {

                // Class should have @Entity annotation
                bool isEntity = IsEntityAnnotationPresent(clazz);

                if (isEntity)
                {
                    metaData.EntityClass = clazz;
                    metaData.JPAEntity = true;
                    // Try to find a field annotated with @Id
                    FieldInfo idField = GetIdField(clazz);
                    if (idField != null)
                    {
                        metaData.IdField = idField;
                    }
                    else
                    {
                        // Try to find a method annotated with @Id
                        MethodInfo idMethod = GetIdMethod(clazz);
                        if (idMethod != null)
                        {
                            metaData.IdMethod = idMethod;
                        }
                        else
                        {
                            throw new ActivitiException("Cannot find field or method with annotation @Id on class '" + clazz.FullName + "', only single-valued primary keys are supported on JPA-entities");
                        }
                    }
                    break;
                }
                clazz = clazz.BaseType;
            }
            return metaData;
        }

        private MethodInfo GetIdMethod(Type clazz)
        {
            MethodInfo idMethod = null;
            // Get all public declared methods on the class. According to spec, @Id
            // should only be
            // applied to fields and property get methods
            MethodInfo[] methods = clazz.GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute(typeof(IdAttribute)) is IdAttribute idAnnotation)
                {
                    idMethod = method;
                    break;
                }
            }
            return idMethod;
        }

        private FieldInfo GetIdField(Type clazz)
        {
            FieldInfo idField = null;
            FieldInfo[] fields = clazz.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttribute(typeof(IdAttribute)) is IdAttribute idAnnotation)
                {
                    idField = field;
                    break;
                }
            }

            if (idField == null)
            {
                // Check superClass for fields with @Id, since getDeclaredFields
                // does
                // not return superclass-fields.
                Type superClass = clazz.BaseType;
                if (superClass != null && !superClass.Equals(typeof(object)))
                {
                    // Recursively go up class hierarchy
                    idField = GetIdField(superClass);
                }
            }
            return idField;
        }

        private bool IsEntityAnnotationPresent(Type clazz)
        {
            return (clazz.GetCustomAttribute(typeof(EntityAttribute)) != null);
        }
    }

    public class EntityAttribute : Attribute
    {

    }

    public class IdAttribute : Attribute
    {
    }
}