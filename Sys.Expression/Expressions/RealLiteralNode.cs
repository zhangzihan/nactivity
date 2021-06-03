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
using System.Globalization;
using System.Runtime.Serialization;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed real literal node.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class RealLiteralNode : BaseNode
    {
        private object nodeValue;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public RealLiteralNode() : base()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected RealLiteralNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private object syncRoot = new();

        /// <summary>
        /// Returns a value for the real literal node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            if (nodeValue is null)
            {
                lock (syncRoot)
                {
                    if (nodeValue is null)
                    {
                        string n = this.getText();
                        char lastChar = n.ToLower()[n.Length - 1];
                        if (char.IsDigit(lastChar))
                        {
                            nodeValue = double.Parse(n, NumberFormatInfo.InvariantInfo);
                        }
                        else
                        {
                            n = n.Substring(0, n.Length - 1);
                            if (lastChar == 'm')
                            {
                                nodeValue = decimal.Parse(n, NumberFormatInfo.InvariantInfo);
                            }
                            else if (lastChar == 'f')
                            {
                                nodeValue = float.Parse(n, NumberFormatInfo.InvariantInfo);
                            }
                            else
                            {
                                nodeValue = double.Parse(n, NumberFormatInfo.InvariantInfo);
                            }
                        }
                    }
                }
            }

            return nodeValue;
        }
    }
}