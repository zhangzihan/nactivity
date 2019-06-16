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
namespace Sys.Workflow.engine.impl.el
{
    using java;
    using Sys.Workflow.engine.impl.util;

    /// <summary>
    /// A <seealso cref="ELResolver"/> for dynamic bean properties
    /// 
    /// 
    /// </summary>
    public class DynamicBeanPropertyELResolver : ELResolver
    {

        protected internal Type subject;

        protected internal string readMethodName;

        protected internal string writeMethodName;

        protected internal bool readOnly;

        public DynamicBeanPropertyELResolver(bool readOnly, Type subject, string readMethodName, string writeMethodName)
        {
            this.readOnly = readOnly;
            this.subject = subject;
            this.readMethodName = readMethodName;
            this.writeMethodName = writeMethodName;
        }

        public DynamicBeanPropertyELResolver(Type subject, string readMethodName, string writeMethodName) : this(false, subject, readMethodName, writeMethodName)
        {
        }

        public virtual Type getCommonPropertyType(ELContext context, object @base)
        {
            if (this.subject.IsInstanceOfType(@base))
            {
                return typeof(object);
            }
            else
            {
                return null;
            }
        }

        public override IEnumerator<FeatureDescriptor> getFeatureDescriptors(ELContext context, object @base)
        {
            return null;
        }

        public override Type getType(ELContext context, object @base, object property)
        {
            if (@base == null || this.getCommonPropertyType(context, @base) == null)
            {
                return null;
            }

            context.PropertyResolved = true;
            return typeof(object);
        }

        public override object getValue(ELContext context, object @base, object property)
        {
            if (@base == null || this.getCommonPropertyType(context, @base) == null)
            {
                return null;
            }

            string propertyName = property.ToString();

            try
            {
                object value = ReflectUtil.invoke(@base, this.readMethodName, new object[] { propertyName });
                context.PropertyResolved = true;
                return value;
            }
            catch (Exception e)
            {
                throw new ELException(e);
            }
        }

        public override bool isReadOnly(ELContext context, object @base, object property)
        {
            return this.readOnly;
        }

        public override void setValue(ELContext context, object @base, object property, object value)
        {
            if (@base == null || this.getCommonPropertyType(context, @base) == null)
            {
                return;
            }

            string propertyName = property.ToString();
            try
            {
                ReflectUtil.invoke(@base, this.writeMethodName, new object[] { propertyName, value });
                context.PropertyResolved = true;
            }
            catch (Exception e)
            {
                throw new ELException(e);
            }
        }
    }

    public class FeatureDescriptor
    {
    }
}