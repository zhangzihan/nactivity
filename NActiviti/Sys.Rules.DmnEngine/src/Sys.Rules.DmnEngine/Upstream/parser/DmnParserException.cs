using System;

namespace Sys.Rules.DmnEngine.Parser
{
    /// <inheritdoc />
    /// <summary>
    /// Exception thrown while parsing the DMN Model
    /// </summary>
    public class DmnParserException : Exception
    {
        /// <inheritdoc />
        /// <summary>
        /// Creates <see cref="T:Sys.Rules.DmnEngine.Parser.DmnParserException" /> with given <paramref name="message" /> and optional <paramref name="innerException" />
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Optional inner exception</param>
        public DmnParserException(string message, Exception innerException=null) : base(message, innerException)
        {
        }
    }
}

