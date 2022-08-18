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
namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
{

    using Sys.Workflow.Engine.Impl.Bpmn.Parser;
    using Sys.Workflow.Engine.Impl.Util;
    using System.Reflection;

    /// 
    public class ClassDelegateUtil
    {

        public static object InstantiateDelegate(Type clazz, IList<FieldDeclaration> fieldDeclarations)
        {
            return InstantiateDelegate(clazz.FullName, fieldDeclarations);
        }

        public static object InstantiateDelegate(string className, IList<FieldDeclaration> fieldDeclarations)
        {
            object @object = ReflectUtil.Instantiate(className);
            ApplyFieldDeclaration(fieldDeclarations, @object);
            return @object;
        }

        public static void ApplyFieldDeclaration(IList<FieldDeclaration> fieldDeclarations, object target)
        {
            if (fieldDeclarations is object)
            {
                foreach (FieldDeclaration declaration in fieldDeclarations)
                {
                    ApplyFieldDeclaration(declaration, target);
                }
            }
        }

        public static void ApplyFieldDeclaration(FieldDeclaration declaration, object target)
        {
            MethodInfo setterMethod = ReflectUtil.GetSetter(declaration.Name, target.GetType(), declaration.Value.GetType());

            if (setterMethod is not null)
            {
                try
                {
                    setterMethod.Invoke(target, new object[] { declaration.Value });
                }
                catch (System.ArgumentException e)
                {
                    throw new ActivitiException("Error while invoking '" + declaration.Name + "' on class " + target.GetType().FullName, e);
                }
                catch (Exception e)
                {
                    throw new ActivitiException("Illegal access when calling '" + declaration.Name + "' on class " + target.GetType().FullName, e);
                }
                //catch (InvocationTargetException e)
                //{
                //    throw new ActivitiException("Exception while invoking '" + declaration.Name + "' on class " + target.GetType().FullName, e);
                //}
            }
            else
            {
                FieldInfo field = ReflectUtil.GetField(declaration.Name, target);
                if (field is null)
                {
                    throw new ActivitiIllegalArgumentException("Field definition uses non-existing field '" + declaration.Name + "' on class " + target.GetType().FullName);
                }
                // Check if the delegate field's type is correct
                if (!FieldTypeCompatible(declaration, field))
                {
                    throw new ActivitiIllegalArgumentException("Incompatible type set on field declaration '" + declaration.Name + "' for class " + target.GetType().FullName + ". Declared value has type " + declaration.Value.GetType().FullName + ", while expecting " + field.DeclaringType.Name);
                }
                ReflectUtil.SetField(field, target, declaration.Value);
            }
        }

        public static bool FieldTypeCompatible(FieldDeclaration declaration, FieldInfo field)
        {
            if (declaration.Value is not null)
            {
                return declaration.Value.GetType().IsAssignableFrom(field.DeclaringType);
            }
            else
            {
                // Null can be set any field type
                return true;
            }
        }
    }
}