using System.Collections.Generic;

namespace Sys.Workflow.bpmn.model
{

    /// <summary>
    /// interface for accessing Element attributes.
    /// 
    /// </summary>
    public interface IHasExtensionAttributes
    {
        /// <summary>
        /// get element's attributes </summary>
        IDictionary<string, IList<ExtensionAttribute>> Attributes { get; set; }

        /// <summary>
        /// return value of the attribute from given namespace with given name.
        /// </summary>
        /// <param name="namespace"> </param>
        /// <param name="name"> </param>
        /// <returns> attribute value or null in case when attribute was not found </returns>
        string GetAttributeValue(string @namespace, string name);

        /// <summary>
        /// add attribute to the object </summary>
        void AddAttribute(ExtensionAttribute attribute);

    }

}