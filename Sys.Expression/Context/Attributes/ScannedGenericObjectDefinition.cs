#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
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
 */

#endregion

using Microsoft.Extensions.Logging;
using Spring.Core;
using Spring.Objects;
using Spring.Objects.Factory.Attributes;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Stereotype;
using System;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// A GenericObjectDefinition that provides attribute driven propulation
    /// of properties like LazyInit, Scope or Qualifier
    /// </summary>
    [Serializable]
    public class ScannedGenericObjectDefinition : GenericObjectDefinition
    {
        private static readonly ILogger Log = LogManager.GetLogger<ScannedGenericObjectDefinition>();

        /// <summary>
        /// Name provided by the Component Attribute
        /// </summary>
        private string _componentName;

        /// <summary>
        /// Creates a GenericObjectDefinition that applies the default values provided
        /// in the XML Spring config document. Additionally parses the specific class
        /// attributesthat allows the definition of LazyInit, Scope or Qualifier
        /// </summary>
        /// <param name="typeOfObject">Type of scanned component</param>
        /// <param name="defaults">Defualts provided in Spring Config document</param>
        public ScannedGenericObjectDefinition(Type typeOfObject, DocumentDefaultsDefinition defaults)
        {
            ObjectType = typeOfObject;

            ParseName();
            ApplyDefaults(defaults);
            ParseLazyAttribute();
            ParseScopeAttribute();
            ParseQualifierAttribute();

            Log.LogDebug($"ComponentName: {_componentName}; {ToString()}");
        }

        private void ParseName()
        {
            var attr = Attribute.GetCustomAttribute(ObjectType, typeof(ComponentAttribute), true) as ComponentAttribute;
            if (attr is not null && !string.IsNullOrEmpty(attr.Name))
                _componentName = attr.Name;
        }

        private void ApplyDefaults(DocumentDefaultsDefinition defaults)
        {
            if (defaults is null)
                return;

            bool.TryParse(defaults.LazyInit, out bool lazyInit);
            IsLazyInit = lazyInit;

            if (!String.IsNullOrEmpty(defaults.Autowire))
            {
                AutowireMode = GetAutowireMode(defaults.Autowire);
            }
        }

        private AutoWiringMode GetAutowireMode(string value)
        {
            AutoWiringMode autoWiringMode;
            autoWiringMode = (AutoWiringMode)Enum.Parse(typeof(AutoWiringMode), value, true);
            return autoWiringMode;
        }


        private void ParseScopeAttribute()
        {
            var attr = Attribute.GetCustomAttribute(ObjectType, typeof(ScopeAttribute), true) as ScopeAttribute;
            if (attr is not null)
            {
                Scope = attr.ObjectScope.ToString().ToLower();

                if (attr.ObjectScope == ObjectScope.Request || attr.ObjectScope == ObjectScope.Session)
                {
                    IsLazyInit = true;
                }
            }
        }

        private void ParseLazyAttribute()
        {
            var attr = Attribute.GetCustomAttribute(ObjectType, typeof(LazyAttribute), true) as LazyAttribute;
            if (attr is not null)
                IsLazyInit = attr.LazyInitialize;
        }

        private void ParseQualifierAttribute()
        {
            var attr = Attribute.GetCustomAttribute(ObjectType, typeof(QualifierAttribute), true) as QualifierAttribute;
            if (attr is not null)
            {
                var qualifier = new AutowireCandidateQualifier(attr.GetType());

                if (!string.IsNullOrEmpty(attr.Value))
                    qualifier.SetAttribute(AutowireCandidateQualifier.VALUE_KEY, attr.Value);

                ParseQualifierProperties(attr, qualifier);

                AddQualifier(qualifier);
            }
        }

        private void ParseQualifierProperties(QualifierAttribute attr, AutowireCandidateQualifier qualifier)
        {
            foreach (var property in attr.GetType().GetProperties())
            {
                if (!property.Name.Equals("TypeId") && !property.Name.Equals("Value"))
                {
                    object value = property.GetValue(attr, null);
                    if (value is not null)
                    {
                        var attribute = new ObjectMetadataAttribute(property.Name, value);
                        qualifier.AddMetadataAttribute(attribute);
                    }
                }
            }
        }

        /// <summary>
        /// Provides the name of the object scanned
        /// </summary>
        /// <returns>return the provided attribute name of the full object type name</returns>
        public string ComponentName
        {
            get
            {
                return _componentName;
            }
        }
    }
}
