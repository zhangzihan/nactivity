using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Sys.Rules.DmnEngine.Parser.Dto
{
    /// <inheritdoc />
    /// <summary>
    /// Variable definition
    /// Used in <see cref="T:Sys.Rules.DmnEngine.Parser.Dto.Decision">expression decision definition</see> to define the output variable
    /// </summary>
    public class Variable : NamedElement
    {
        /// <summary>
        /// Data type of the variable
        /// </summary>
        [XmlAttribute("typeRef")]
        public string TypeRef { get; set; }

        /// <inheritdoc />
        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"{Name}{(string.IsNullOrWhiteSpace(TypeRef) ? "" : ":" + TypeRef)} ({Id})";
        }
    }
}

