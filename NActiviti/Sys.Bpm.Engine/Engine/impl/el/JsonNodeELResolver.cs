using Newtonsoft.Json.Linq;
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
namespace org.activiti.engine.impl.el
{
    /// <summary>
    /// Defines property resolution behavior on JsonNodes.
    /// </summary>
    /// <seealso cref= CompositeELResolver </seealso>
    /// <seealso cref= ELResolver </seealso>
    public class JsonNodeELResolver : ELResolver
    {

        private readonly bool readOnly;

        /// <summary>
        /// Creates a new read/write BeanELResolver.
        /// </summary>
        public JsonNodeELResolver() : this(false)
        {
        }

        /// <summary>
        /// Creates a new BeanELResolver whose read-only status is determined by the given parameter.
        /// </summary>
        public JsonNodeELResolver(bool readOnly)
        {
            this.readOnly = readOnly;
        }

        /// <summary>
        /// If the base object is not null, returns the most general type that this resolver accepts for
        /// the property argument. Otherwise, returns null. Assuming the base is not null, this method
        /// will always return Object.class. This is because any object is accepted as a key and is
        /// coerced into a string.
        /// </summary>
        /// <param name="context">
        ///            The context of this evaluation. </param>
        /// <param name="base">
        ///            The bean to analyze. </param>
        /// <returns> null if base is null; otherwise Object.class. </returns>
        public override Type GetCommonPropertyType(ELContext context, object @base)
        {
            return IsResolvable(@base) ? typeof(object) : null;
        }

        /// <summary>
        /// If the base object is a map, returns the most general acceptable type for a value in this
        /// map. If the base is a Map, the propertyResolved property of the ELContext object must be set
        /// to true by this resolver, before returning. If this property is not true after this method is
        /// called, the caller should ignore the return value. Assuming the base is a Map, this method
        /// will always return Object.class. This is because Maps accept any object as the value for a
        /// given key.
        /// </summary>
        /// <param name="context">
        ///            The context of this evaluation. </param>
        /// <param name="base">
        ///            The map to analyze. Only bases of type Map are handled by this resolver. </param>
        /// <param name="property">
        ///            The key to return the acceptable type for. Ignored by this resolver. </param>
        /// <returns> If the propertyResolved property of ELContext was set to true, then the most general
        ///         acceptable type; otherwise undefined. </returns>
        /// <exception cref="NullPointerException">
        ///             if context is null </exception>
        /// <exception cref="ELException">
        ///             if an exception was thrown while performing the property or variable resolution.
        ///             The thrown exception must be included as the cause property of this exception, if
        ///             available. </exception>
        public override Type GetType(ELContext context, object @base, object property)
        {
            if (context == null)
            {
                throw new System.NullReferenceException("context is null");
            }
            Type result = null;
            if (IsResolvable(@base))
            {
                result = typeof(object);
                context.IsPropertyResolved = true;
            }
            return result;
        }

        /// <summary>
        /// If the base object is a map, returns the value associated with the given key, as specified by
        /// the property argument. If the key was not found, null is returned. If the base is a Map, the
        /// propertyResolved property of the ELContext object must be set to true by this resolver,
        /// before returning. If this property is not true after this method is called, the caller should
        /// ignore the return value. Just as in java.util.Map.get(Object), just because null is returned
        /// doesn't mean there is no mapping for the key; it's also possible that the Map explicitly maps
        /// the key to null.
        /// </summary>
        /// <param name="context">
        ///            The context of this evaluation. </param>
        /// <param name="base">
        ///            The map to analyze. Only bases of type Map are handled by this resolver. </param>
        /// <param name="property">
        ///            The key to return the acceptable type for. Ignored by this resolver. </param>
        /// <returns> If the propertyResolved property of ELContext was set to true, then the value
        ///         associated with the given key or null if the key was not found. Otherwise, undefined. </returns>
        /// <exception cref="ClassCastException">
        ///             if the key is of an inappropriate type for this map (optionally thrown by the
        ///             underlying Map). </exception>
        /// <exception cref="NullPointerException">
        ///             if context is null, or if the key is null and this map does not permit null keys
        ///             (the latter is optionally thrown by the underlying Map). </exception>
        /// <exception cref="ELException">
        ///             if an exception was thrown while performing the property or variable resolution.
        ///             The thrown exception must be included as the cause property of this exception, if
        ///             available. </exception>
        public override object GetValue(ELContext context, object @base, object property)
        {
            if (context == null)
            {
                throw new System.NullReferenceException("context is null");
            }
            object result = null;
            if (IsResolvable(@base))
            {
                JToken resultNode = ((JToken)@base)[property.ToString()];
                if (resultNode != null)
                {
                    //object res = resultNode.ToObject();

                    //if (resultNode.Boolean)
                    //{
                    //    result = resultNode.asBoolean();
                    //}
                    //else if (resultNode.Long)
                    //{
                    //    result = resultNode.asLong();
                    //}
                    //else if (resultNode.BigDecimal || resultNode.Double)
                    //{
                    //    result = resultNode.asDouble();
                    //}
                    //else if (resultNode.Textual)
                    //{
                    //    result = resultNode.asText();
                    //}
                    //else
                    //{
                    //    result = resultNode.ToString();
                    //}

                }
                else
                {
                    result = resultNode;
                }
                context.IsPropertyResolved = true;
            }
            return result;
        }

        /// <summary>
        /// If the base object is a map, returns whether a call to
        /// <seealso cref="#setValue(ELContext, Object, Object, Object)"/> will always fail. If the base is a Map,
        /// the propertyResolved property of the ELContext object must be set to true by this resolver,
        /// before returning. If this property is not true after this method is called, the caller should
        /// ignore the return value. If this resolver was constructed in read-only mode, this method will
        /// always return true. If a Map was created using java.util.Collections.unmodifiableMap(Map),
        /// this method must return true. Unfortunately, there is no Collections API method to detect
        /// this. However, an implementation can create a prototype unmodifiable Map and query its
        /// runtime type to see if it matches the runtime type of the base object as a workaround.
        /// </summary>
        /// <param name="context">
        ///            The context of this evaluation. </param>
        /// <param name="base">
        ///            The map to analyze. Only bases of type Map are handled by this resolver. </param>
        /// <param name="property">
        ///            The key to return the acceptable type for. Ignored by this resolver. </param>
        /// <returns> If the propertyResolved property of ELContext was set to true, then true if calling
        ///         the setValue method will always fail or false if it is possible that such a call may
        ///         succeed; otherwise undefined. </returns>
        /// <exception cref="NullPointerException">
        ///             if context is null. </exception>
        /// <exception cref="ELException">
        ///             if an exception was thrown while performing the property or variable resolution.
        ///             The thrown exception must be included as the cause property of this exception, if
        ///             available. </exception>
        public override bool IsReadOnly(ELContext context, object @base, object property)
        {
            if (context == null)
            {
                throw new System.NullReferenceException("context is null");
            }
            if (IsResolvable(@base))
            {
                context.IsPropertyResolved = true;
            }
            return readOnly;
        }

        /// <summary>
        /// If the base object is a map, attempts to set the value associated with the given key, as
        /// specified by the property argument. If the base is a Map, the propertyResolved property of
        /// the ELContext object must be set to true by this resolver, before returning. If this property
        /// is not true after this method is called, the caller can safely assume no value was set. If
        /// this resolver was constructed in read-only mode, this method will always throw
        /// PropertyNotWritableException. If a Map was created using
        /// java.util.Collections.unmodifiableMap(Map), this method must throw
        /// PropertyNotWritableException. Unfortunately, there is no Collections API method to detect
        /// this. However, an implementation can create a prototype unmodifiable Map and query its
        /// runtime type to see if it matches the runtime type of the base object as a workaround.
        /// </summary>
        /// <param name="context">
        ///            The context of this evaluation. </param>
        /// <param name="base">
        ///            The map to analyze. Only bases of type Map are handled by this resolver. </param>
        /// <param name="property">
        ///            The key to return the acceptable type for. Ignored by this resolver. </param>
        /// <param name="value">
        ///            The value to be associated with the specified key. </param>
        /// <exception cref="ClassCastException">
        ///             if the class of the specified key or value prevents it from being stored in this
        ///             map. </exception>
        /// <exception cref="NullPointerException">
        ///             if context is null, or if this map does not permit null keys or values, and the
        ///             specified key or value is null. </exception>
        /// <exception cref="IllegalArgumentException">
        ///             if some aspect of this key or value prevents it from being stored in this map. </exception>
        /// <exception cref="PropertyNotWritableException">
        ///             if this resolver was constructed in read-only mode, or if the put operation is
        ///             not supported by the underlying map. </exception>
        /// <exception cref="ELException">
        ///             if an exception was thrown while performing the property or variable resolution.
        ///             The thrown exception must be included as the cause property of this exception, if
        ///             available. </exception>
        public override void SetValue(ELContext context, object @base, object property, object value)
        {
            if (context == null)
            {
                throw new System.NullReferenceException("context is null");
            }
            if (@base is JToken)
            {
                if (readOnly)
                {
                    throw new Exception("resolver is read-only");
                }
                JToken node = (JToken)@base;
                if (value is decimal)
                {
                    node[property.ToString()] = (decimal)value;

                }
                else if (value is bool?)
                {
                    node[property.ToString()] = (bool?)value;

                }
                else if (value is long?)
                {
                    node[property.ToString()] = (long?)value;

                }
                else if (value is double?)
                {
                    node[property.ToString()] = (double?)value;

                }
                else if (value != null)
                {
                    node[property.ToString()] = value.ToString();

                }
                else
                {
                    node[property.ToString()] = property.ToString();
                }
                context.IsPropertyResolved = true;
            }
        }

        /// <summary>
        /// Test whether the given base should be resolved by this ELResolver.
        /// </summary>
        /// <param name="base">
        ///            The bean to analyze. </param>
        /// <param name="property">
        ///            The name of the property to analyze. Will be coerced to a String. </param>
        /// <returns> base != null </returns>
        private bool IsResolvable(object @base)
        {
            return @base is JToken;
        }
    }

}