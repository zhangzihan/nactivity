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
    /// <summary>
    /// An <seealso cref="ELResolver"/> that exposed object values in the map, under the name of the entry's key. The values in the map are only returned when requested property has no 'base', meaning it's a
    /// root-object.
    /// 
    /// 
    /// </summary>
    public class ReadOnlyMapELResolver : ELResolver
    {

        protected internal IDictionary<object, object> wrappedMap;

        public ReadOnlyMapELResolver(IDictionary<object, object> map)
        {
            this.wrappedMap = map;
        }

        public override object GetValue(ELContext context, object @base, object property)
        {
            if (@base == null)
            {
                if (wrappedMap.ContainsKey(property))
                {
                    context.IsPropertyResolved = true;
                    return wrappedMap[property];
                }
            }
            return null;
        }

        public override bool IsReadOnly(ELContext context, object @base, object property)
        {
            return true;
        }

        public override void SetValue(ELContext context, object @base, object property, object value)
        {
            if (@base == null)
            {
                if (wrappedMap.ContainsKey(property))
                {
                    throw new ActivitiException("Cannot set value of '" + property + "', it's readonly!");
                }
            }
        }

        public override Type GetCommonPropertyType(ELContext context, object arg)
        {
            return typeof(object);
        }

        public override Type GetType(ELContext context, object arg1, object arg2)
        {
            return typeof(object);
        }
    }

}