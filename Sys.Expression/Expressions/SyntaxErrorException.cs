#region License

/*
 * Copyright ?2002-2011 the original author or authors.
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

using System;
using System.Runtime.Serialization;

namespace Spring.Expressions
{
	/// <summary>
	/// Exception thrown when detecting invalid SpEL syntax
	/// </summary>
    /// <author>Erich Eichinger</author>
	[Serializable]
	internal class SyntaxErrorException : Exception, ISerializable
	{
        private readonly string _expression;
        private readonly int _line;
        private readonly int _column;

	    ///<summary>
	    ///</summary>
	    public int Line
	    {
	        get { return _line; }
	    }

	    ///<summary>
	    ///</summary>
	    public int Column
	    {
	        get { return _column; }
	    }

	    ///<summary>
	    ///Gets a message that provides details on the syntax error.
	    ///</summary>
	    public override string Message
        {
            get
            {
                return string.Format("Syntax Error on line {0}, column {1}: {2} in expression{3}'{4}'", Line, Column, base.Message, Environment.NewLine, _expression );
            }
        }

	    ///<summary>
	    /// The expression that caused the error
	    ///</summary>
	    public string Expression
	    {
	        get { return _expression; }
	    }

	    #region Public Instance Constructors

        /// <summary>
        /// TODO
        /// </summary>
		public SyntaxErrorException(string message, int line, int column, string expression)
			: base(message)
        {
            _line = line;
            _column = column;
            _expression = expression;
        }

		#endregion Public Instance Constructors

		#region Protected Instance Constructors

        /// <summary>
        /// TODO
        /// </summary>
		protected SyntaxErrorException(SerializationInfo info, StreamingContext context)
            : base(info.GetString("Message"), null)
        {
            _line = info.GetInt32("Line");
            _column = info.GetInt32("Column");
            _expression = info.GetString("Expression");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData(info, context);
            info.AddValue("Line", _line);
            info.AddValue("Column", _column);
            info.AddValue("Expression", this._expression);
        }

		#endregion Protected Instance Constructors
	}
}
